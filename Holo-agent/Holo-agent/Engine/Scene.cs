using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Engine.Utilities;
using Engine.Components;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Engine
{
    [DataContract]
    public struct Room
    {
        [DataMember(Order = 0)]
        public GameObject go;
        [DataMember(Order = 1)]
        public List<GameObject> contents;
    }

    [DataContract]
    public class Scene
    {
        private List<GameObject> objectsToDestroy = new List<GameObject>();
        [DataMember(Order = 0)]
        private Graph<Room,BoundingBox> roomGraph;
        [DataMember(Order = 1)]
        private GameObject activeCamera;
        [DataMember(Order = 2)]
        private float sceneTime = 0.0f;
        private List<GameObject> objectsToRespace = new List<GameObject>();

        public List<GameObject> GetAllObjects()
        {
            HashSet<GameObject> allObjects = new HashSet<GameObject>();
            foreach(GraphNode<Room,BoundingBox> node in roomGraph)
            {
                allObjects.Add(node.Value.go);
                foreach (GameObject go2 in node.Value.contents)
                    allObjects.Add(go2);

                foreach (GraphNode<Room, BoundingBox> neighbour in node.Neighbours)
                {
                    allObjects.Add(neighbour.Value.go);
                    foreach (GameObject go2 in neighbour.Value.contents)
                        allObjects.Add(go2);
                }
            }
            return new List<GameObject>(allObjects);
        }

        public List<GameObject> GetNearbyObjects(GameObject go)
        {
            HashSet<GameObject> nearbyObjects = new HashSet<GameObject>();
            foreach (GraphNode<Room, BoundingBox> node in roomGraph)
            {
                if (node.Value.contents.Contains(go))
                {
                    foreach(GameObject go2 in node.Value.contents)
                        nearbyObjects.Add(go2);

                    foreach (GraphNode<Room, BoundingBox> neighbour in node.Neighbours)
                    {
                        foreach (GameObject go2 in neighbour.Value.contents)
                            nearbyObjects.Add(go2);
                    }
                    break;
                }
            }
            return new List<GameObject>(nearbyObjects);
        }

        public void Respace(GameObject go)
        {
            if (objectsToRespace == null) return;
            if(!objectsToRespace.Contains(go)) objectsToRespace.Add(go);
        }

        public void AddRoom(GameObject room)
        {
            Room r;
            r.go = room;
            r.contents = new List<GameObject>();
            roomGraph.AddNode(new GraphNode<Room,BoundingBox>(r));
        }

        public void AddRoomConnection(GameObject room1, GameObject room2, BoundingBox portal)
        {
            GraphNode<Room, BoundingBox> node1 = roomGraph.Nodes.Find(x => x.Value.go.Equals(room1));
            GraphNode<Room, BoundingBox> node2 = roomGraph.Nodes.Find(x => x.Value.go.Equals(room2));
            if (node1 != null && node2 != null) roomGraph.AddUndirectedEdge(node1, node2, portal);
        }

        public void AddObject(GameObject go)
        {
            foreach(GraphNode<Room,BoundingBox> node in roomGraph)
            {
                Room room = node.Value;
                if (go.Bound.Intersects(room.go.Bound))
                {
                    room.contents.Add(go);
                }
            }
        }

        public void RemoveObject(GameObject go)
        {
            foreach (GraphNode<Room, BoundingBox> node in roomGraph)
            {
                Room room = node.Value;
                room.contents.Remove(go);
            }
        }

        [IgnoreDataMember]
        public GameObject Camera
        {
            get
            {
                return activeCamera;
            }
            set
            {
                if (value != null) activeCamera = value;
            }
        }

        [IgnoreDataMember]
        public float SceneTime
        {
            get { return sceneTime; }
        }

        public GameObject FindRoomContaining(GameObject gameObject)
        {
            if (gameObject.Parent == null) return null;

            foreach(GraphNode<Room, BoundingBox> room in roomGraph)
            {
                if(room.Value.go.Bound.Intersects(gameObject.Bound))
                {
                    return room.Value.go;
                }
            }

            return null;
        }

        public void Destroy(GameObject gameObject)
        {
            objectsToDestroy.Add(gameObject);
        }

        public void Update(GameTime gameTime)
        {
            sceneTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach(GraphNode<Room,BoundingBox> node in roomGraph)
            {
                Room room = node.Value;
                room.go.Update(gameTime);
            }

            foreach(GameObject go in objectsToDestroy)
            {
                go.Destroy();
            }
            objectsToDestroy.Clear();

            foreach(GameObject go in objectsToRespace)
            {
                RemoveObject(go);

                foreach (GraphNode<Room, BoundingBox> node in roomGraph)
                {
                    if (node.Value.go.Bound.Intersects(go.Bound))
                    {
                        node.Value.contents.Add(go);
                    }
                }
            }
            objectsToRespace.Clear();

            foreach(GraphNode<Room, BoundingBox> node in roomGraph)
            {
                foreach(GameObject go in node.Value.contents)
                {
                    Collider goCol = go.GetComponent<Collider>();
                    Rigidbody rig = go.GetComponent<Rigidbody>();
                    if (goCol != null && rig != null)
                    {
                        bool grounded = false;
                        foreach(GameObject go2 in node.Value.contents)
                        {
                            Collider go2Col = go2.GetComponent<Collider>();
                            if(go2Col != null && !go.Equals(go2))
                            {
                                Bounding_Volumes.CollisionResult collision = goCol.Collide(go2Col);
                                if(collision.CollisionDetected)
                                {
                                    float lostVel = Vector3.Dot(collision.CollisionPlane.Value.Normal, rig.Velocity);
                                    if (lostVel < 0.0f)
                                    {
                                        Vector3 lostVelocity = -collision.CollisionPlane.Value.Normal * lostVel;
                                        rig.AddVelocityChange(lostVelocity);
                                    }
                                    if (Vector3.Dot(collision.CollisionPlane.Value.Normal, Vector3.Up) > 0.9f)
                                    {
                                        rig.IsGrounded = true;
                                        grounded = true;
                                    }
                                }
                            }
                        }

                        foreach(GraphNode<Room, BoundingBox> neighbour in node.Neighbours)
                        {
                            foreach(GameObject go2 in neighbour.Value.contents)
                            {
                                Collider go2Col = go2.GetComponent<Collider>();
                                if (go2Col != null && !go.Equals(go2))
                                {
                                    Bounding_Volumes.CollisionResult collision = goCol.Collide(go2Col);
                                    if (collision.CollisionDetected)
                                    {
                                        float lostVel = Vector3.Dot(collision.CollisionPlane.Value.Normal, rig.Velocity);
                                        if (lostVel < 0.0f)
                                        {
                                            Vector3 lostVelocity = -collision.CollisionPlane.Value.Normal * lostVel;
                                            rig.AddVelocityChange(lostVelocity);
                                        }
                                        if (Vector3.Dot(collision.CollisionPlane.Value.Normal, Vector3.Up) > 0.9f)
                                        {
                                            rig.IsGrounded = true;
                                            grounded = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (!grounded) rig.IsGrounded = false;
                    }
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            HashSet<GameObject> objectsToRender = new HashSet<GameObject>();

            foreach(GraphNode<Room,BoundingBox> node in roomGraph)
            {
                if(node.Value.contents.Contains(activeCamera))
                {
                    foreach(GameObject go in node.Value.contents) objectsToRender.Add(go);

                    foreach(GraphNode<Room,BoundingBox> neighbour in node.Neighbours)
                    {
                        foreach (GameObject go in neighbour.Value.contents) objectsToRender.Add(go);
                    }
                    break;
                }
            }

            foreach (GameObject go in objectsToRender)
            {
                go.Draw(gameTime);
            }
        }

        public Scene()
        {
            activeCamera = null;
            roomGraph = new Graph<Room, BoundingBox>();
            objectsToDestroy = new List<GameObject>();
            objectsToRespace = new List<GameObject>();
        }

        public void DrawDebug(GameTime gameTime, GraphicsDeviceManager graphicsDevice)
        {
            List<Room> roomsToRender = new List<Room>();

            HashSet<GameObject> objectsToRender = new HashSet<GameObject>();

            foreach (GraphNode<Room, BoundingBox> node in roomGraph)
            {
                objectsToRender.Add(node.Value.go);
                if (node.Value.contents.Contains(activeCamera))
                {
                    foreach (GameObject go in node.Value.contents) objectsToRender.Add(go);

                    foreach (GraphNode<Room, BoundingBox> neighbour in node.Neighbours)
                    {
                        foreach (GameObject go in neighbour.Value.contents) objectsToRender.Add(go);
                    }
                }
            }

            foreach (GameObject go in objectsToRender)
            {
                go.DrawDebug(gameTime, graphicsDevice);
            }
        }

        [OnDeserialized]
        private void InitializeAfterLoading(StreamingContext context)
        {
            objectsToDestroy = new List<GameObject>();
            objectsToRespace = new List<GameObject>();
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Engine.Utilities;
using Engine.Components;

namespace Engine
{
    public struct Room
    {
        public GameObject go;
        public List<GameObject> contents;
    }

    public class Scene
    {
        private List<GameObject> objectsToDestroy;
        private Graph<Room,BoundingBox> roomGraph;
        private GameObject activeCamera;
        private List<GameObject> objectsToRespace;

        public List<GameObject> GetObjects()
        {
            List<GameObject> allObjects = new List<GameObject>();
            foreach(GraphNode<Room,BoundingBox> node in roomGraph)
            {
                Room room = node.Value;
                PopulateGameObjectList(ref allObjects, room.go);
            }
            return allObjects;
        }

        private void PopulateGameObjectList(ref List<GameObject> list, GameObject go)
        {
            list.Add(go);
            List<GameObject> children = go.GetChildren();
            foreach (GameObject child in children)
            {
                PopulateGameObjectList(ref list, child);
            }
        }

        public void Respace(GameObject go)
        {
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
                foreach(GraphNode<Room,BoundingBox> node in roomGraph)
                {
                    if (node.Value.contents.Contains(go))
                    {
                        node.Value.contents.Remove(go);
                        break;
                    }
                }

                foreach (GraphNode<Room, BoundingBox> node in roomGraph)
                {
                    if (node.Value.go.Bound.Intersects(go.Bound))
                    {
                        node.Value.contents.Add(go);
                    }
                }
            }
            objectsToRespace.Clear();

            /*foreach(GraphNode<Room, BoundingBox> node in roomGraph)
            {
                foreach(GameObject go in node.Value.contents)
                {
                    Collider goCol = go.GetComponent<Collider>();
                    if(goCol != null && go.GetComponent<Rigidbody>() != null)
                    {
                        foreach(GameObject go2 in node.Value.contents)
                        {
                            Collider go2Col = go2.GetComponent<Collider>();
                            if(!go.Equals(go2) && go2Col != null)
                            {
                                int collision = goCol.Collide(go2Col);
                                if(collision > 0)
                                {
                                    // Temporary
                                    Rigidbody rig = go.GetComponent<Rigidbody>();
                                    rig.Velocity = -rig.Velocity;
                                }
                            }
                        }

                        foreach(GraphNode<Room, BoundingBox> neighbour in node.Neighbours)
                        {
                            foreach(GameObject go2 in neighbour.Value.contents)
                            {
                                Collider go2Col = go2.GetComponent<Collider>();
                                if (!go.Equals(go2) && go2Col != null)
                                {
                                    int collision = goCol.Collide(go2Col);
                                    if (collision > 0)
                                    {
                                        // Temporary
                                        Rigidbody rig = go.GetComponent<Rigidbody>();
                                        rig.Velocity = -rig.Velocity;
                                    }
                                }
                            }
                        }
                    }
                }
            }*/
        }

        public void Draw(GameTime gameTime)
        {
            List<Room> roomsToRender = new List<Room>();

            foreach(GraphNode<Room,BoundingBox> node in roomGraph)
            {
                if(node.Value.contents.Contains(activeCamera))
                {
                    roomsToRender.Add(node.Value);
                    foreach(GraphNode<Room,BoundingBox> neighbour in node.Neighbours)
                    {
                        roomsToRender.Add(neighbour.Value);
                    }
                }
            }

            foreach (Room room in roomsToRender)
            {
                room.go.Draw(gameTime);
            }
        }

        public Scene()
        {
            activeCamera = null;
            roomGraph = new Graph<Room, BoundingBox>();
            objectsToDestroy = new List<GameObject>();
            objectsToRespace = new List<GameObject>();
        }
    }
}

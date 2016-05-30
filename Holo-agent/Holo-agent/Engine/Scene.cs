using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Engine.Utilities;

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

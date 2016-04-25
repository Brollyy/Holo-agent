using Engine.Components;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Engine
{
    public class Scene
    {
        private List<GameObject> objectsToDestroy;
        private List<GameObject> gameObjects;
        private GameObject activeCamera;

        public List<GameObject> GetObjects()
        {
            List<GameObject> allObjects = new List<GameObject>();
            foreach(GameObject go in gameObjects)
            {
                PopulateGameObjectList(ref allObjects, go);
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

        public void AddObject(GameObject go)
        {
            gameObjects.Add(go);
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
            foreach(GameObject go in gameObjects)
            {
                go.Update(gameTime);
            }

            foreach(GameObject go in objectsToDestroy)
            {
                go.Destroy();
                gameObjects.Remove(go);
            }
            objectsToDestroy.Clear();
        }

        public void Draw(GameTime gameTime)
        {
            foreach (GameObject go in gameObjects)
            {
                go.Draw(gameTime);
            }
        }

        public Scene()
        {
            activeCamera = null;
            gameObjects = new List<GameObject>();
            objectsToDestroy = new List<GameObject>();
        }
    }
}

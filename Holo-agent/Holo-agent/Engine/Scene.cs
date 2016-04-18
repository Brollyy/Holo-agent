using Engine.Components;
using System;
using System.Collections.Generic;

namespace Engine
{
    public class Scene
    {
        private List<GameObject> gameObjects;
        private GameObject activeCamera;

        public List<GameObject> GetObjects()
        {
            return gameObjects;
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
                if (value != null && value.GetComponent<Camera>() != null) activeCamera = value;
            }
        }

        public Scene()
        {
            activeCamera = null;
            gameObjects = new List<GameObject>();
        }
    }
}

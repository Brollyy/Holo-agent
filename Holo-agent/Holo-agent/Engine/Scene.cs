using System;
using System.Collections.Generic;

namespace Engine
{
    public class Scene
    {
        private List<GameObject> gameObject;
        private GameObject activeCamera;

        public GameObject Camera
        {
            get
            {
                return activeCamera;
            }
        }

        public Scene(GameObject camera)
        {
            activeCamera = camera;
        }
    }
}

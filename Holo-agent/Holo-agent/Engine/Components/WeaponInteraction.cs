using System.Collections.Generic;

namespace Engine.Components
{
    class WeaponInteraction : Interaction
    {
        public override void Interact(GameObject go)
        {
            List<GameObject> objects = Owner.Scene.GetObjects();
            foreach(GameObject gameObject in objects)
            {
                if (gameObject.GetComponent<PlayerController>() != null)
                {
                    gameObject.GetComponent<PlayerController>().addWeapon(go);
                }
            }
        }
    }
}

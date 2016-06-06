using System.Collections.Generic;

namespace Engine.Components
{
    class WeaponInteraction : Interaction
    {
        public override void Interact(GameObject go)
        {
            if(go.GetComponent<PlayerController>() != null)
            {
                go.GetComponent<PlayerController>().addWeapon(Owner);
            }
        }
    }
}

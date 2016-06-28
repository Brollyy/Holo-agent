using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    class WeaponInteraction : Interaction
    {
        public override void Interact(GameObject go)
        {
            if (go.GetComponent<PlayerController>() != null)
            {
                go.GetComponent<PlayerController>().addWeapon(Owner);
            }
        }
    }
}

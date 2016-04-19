using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine;
using Engine.Components;
using Engine.Utilities;

namespace Engine.Components
{
    class DoorInteraction : Interaction 
    {
        public override void Interact(GameObject go)
        {
            Owner.GlobalRotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(225), MathHelper.ToRadians(270), 0);
        }

    }
}

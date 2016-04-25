using Microsoft.Xna.Framework;

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

using Microsoft.Xna.Framework;

namespace Engine.Components
{
    class Rigidbody : Component
    {
        private float freeFallTime;
        private Vector3 velocity;
        public Rigidbody()
        {
            freeFallTime = 0;
            velocity = Vector3.Zero;
        }
        public override void Update(GameTime gameTime)
        {
            freeFallTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            FreeFall(freeFallTime);
        }
        private void FreeFall(float freeFallTime)
        {
            float height = Owner.GlobalPosition.Y - ((Physics.GravitationalAcceleration * 0.1f) * (freeFallTime * freeFallTime) * 0.5f);
            Owner.GlobalPosition = new Vector3(Owner.GlobalPosition.X, height, Owner.GlobalPosition.Z);
        }
    }
}

using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class Rigidbody : Component
    {
        private float velocityThreshhold;

        private float invertMass;
        private Vector3 velocity;
        private Vector3 force;
        private Vector3 impulseForce;
        private float dragCoefficient;
        private float groundDrag = 0.1f;

        public float Mass
        {
            get { return 1.0f / invertMass; }
            set { invertMass = 1.0f / value; }
        }

        public bool IsGrounded
        {
            get; set;
        } = false;

        public bool GravityEnabled
        {
            get; set;
        } = true;

        public bool IsSleeping
        {
            get; set;
        } = false;

        public Vector3 Velocity
        {
            get { return velocity; }
        }

        public Rigidbody(float mass = 1.0f, float drag = 0.0f)
        {
            this.invertMass = 1.0f / mass;
            velocity = Vector3.Zero;
            force = Vector3.Zero;
            impulseForce = Vector3.Zero;
            dragCoefficient = drag;
            velocityThreshhold = invertMass * 0.002f;
        }

        public Rigidbody(Rigidbody other)
        {
            this.invertMass = other.invertMass;
            velocity = other.velocity;
            force = other.force;
            impulseForce = other.impulseForce;
            dragCoefficient = other.dragCoefficient;
            velocityThreshhold = other.velocityThreshhold;
            IsSleeping = other.IsSleeping;
            IsGrounded = other.IsGrounded;
            GravityEnabled = other.GravityEnabled;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!IsSleeping)
            {
                if (IsGrounded || !GravityEnabled) velocity *= (1 - groundDrag);
                velocity += ((force + 
                             impulseForce / deltaTime -
                             (velocity.LengthSquared() > 0.00001f ? velocity.LengthSquared() * dragCoefficient * Vector3.Normalize(velocity) : Vector3.Zero)) * invertMass +
                            (!IsGrounded && GravityEnabled ? Physics.GravitationalAcceleration : Vector3.Zero)) * deltaTime;
                Owner.GlobalPosition += velocity * deltaTime;
                force = Vector3.Zero;
                impulseForce = Vector3.Zero;
                IsSleeping = velocity.LengthSquared() < velocityThreshhold;
            }
        }

        public void AddForce(Vector3 force)
        {
            this.force += force;
            if (force.LengthSquared() > 0.00001f) IsSleeping = false;
        }

        public void AddRelativeForce(Vector3 relativeForce)
        {
            Vector3 newForce = Vector3.Transform(relativeForce, Owner.LocalToWorldMatrix);
            this.force += newForce;
            if (newForce.LengthSquared() > 0.00001f) IsSleeping = false;
        }

        public void AddImpulseForce(Vector3 impulse)
        {
            this.impulseForce += impulse;
            if (impulse.LengthSquared() > 0.00001f) IsSleeping = false;
        }

        public void AddRelativeImpulseForce(Vector3 relativeImpulse)
        {
            Vector3 newImpulse = Vector3.Transform(relativeImpulse, Owner.LocalToWorldMatrix);
            this.impulseForce += newImpulse;
            if (newImpulse.LengthSquared() > 0.00001f) IsSleeping = false;
        }

        public void AddVelocityChange(Vector3 velocityChange)
        {
            velocity += velocityChange;
            if (velocityChange.LengthSquared() > 0.00001f) IsSleeping = false;
        }

        public void AddRelativeVelocityChange(Vector3 relativeVelocityChange)
        {
            Vector3 newVelocityChange = Vector3.Transform(relativeVelocityChange, Owner.LocalToWorldMatrix);
            velocity += newVelocityChange;
            if (newVelocityChange.LengthSquared() > 0.00001f) IsSleeping = false;
        }
    }
}

using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class Rigidbody : Component
    {
        private float mass, deltaTime;
        private Vector3 initialPosition, velocity, initialVelocity, motionTime, acceleration;
        private bool isFalling;
        public bool GravityEnabled
        {
            get; set;
        } = true;
        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
        }
        public Rigidbody() { }
        public void Initialize(float mass)
        {
            this.mass = mass;
            motionTime = Vector3.Zero;
            velocity = Vector3.Zero;
            acceleration = Vector3.Zero;
            isFalling = false;
            initialPosition = Owner.GlobalPosition;
            initialVelocity = Vector3.Zero;
            deltaTime = 0;
        }
        public override void Update(GameTime gameTime)
        {
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (GravityEnabled)
            {
                if (!isGrounded())
                {
                    if (!isFalling)
                    {
                        AddForce(Vector3.UnitY * Physics.GravitationalAcceleration, initialVelocity);
                        isFalling = true;
                    }
                }
                else
                {
                    if (isFalling)
                    {
                        acceleration.Y = 0;
                        initialVelocity.Y = 0;
                        isFalling = false;
                    }
                    if (Owner.GetComponent<PlayerController>() == null) //Temporary
                    {
                        if (Owner.GlobalPosition.Y < 0) //Temporary
                            Owner.GlobalPosition = new Vector3(Owner.GlobalPosition.X, 0, Owner.GlobalPosition.Z);
                    }
                    else
                    {
                        if (Owner.GlobalPosition.Y < 18) //Temporary
                            Owner.GlobalPosition = new Vector3(Owner.GlobalPosition.X, 18, Owner.GlobalPosition.Z);
                    }
                }
            }
            UpdateVelocity();
            UpdateMotionTime(gameTime);
            UpdatePosition();
        }
        private void UpdateMotionTime(GameTime gameTime)
        {
            if (velocity.X != 0 || acceleration.X != 0)
                motionTime.X += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                motionTime.X = 0;
                initialPosition.X = Owner.GlobalPosition.X;
            }
            if (velocity.Y != 0 || acceleration.Y != 0)
                motionTime.Y += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                motionTime.Y = 0;
                initialPosition.Y = Owner.GlobalPosition.Y;
            }
            if (velocity.Z != 0 || acceleration.Z != 0)
                motionTime.Z += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                motionTime.Z = 0;
                initialPosition.Z = Owner.GlobalPosition.Z;
            }
        }
        private void UpdatePosition()
        {
            Owner.LocalPosition = initialPosition + ((initialVelocity * motionTime) + (acceleration * (motionTime * motionTime) * 0.5f)) * Physics.MeterScale;
        }
        private void UpdateVelocity()
        {
            velocity = initialVelocity + (acceleration * motionTime);
        }
        public bool isGrounded()
        {
            if (Owner.GetComponent<CharacterController>() == null) //Temporary
            {
                if (Owner.GlobalPosition.Y <= 0) //Temporary
                    return true;
                else
                    return false;
            }
            else
            {
                if (Owner.GlobalPosition.Y <= 18) //Temporary
                    return true;
                else
                    return false;
            }
        }
        public void AddForce(Vector3 acceleration, Vector3 initialVelocity)
        {
            if (acceleration.X != 0 || initialVelocity.X != this.initialVelocity.X)
            {
                if (acceleration.X != 0)
                {
                    initialVelocity.X = velocity.X;
                    this.acceleration.X += acceleration.X;
                }
                if (initialVelocity.X != this.initialVelocity.X)
                {
                    this.initialVelocity.X = initialVelocity.X;
                }
                motionTime.X = 0;
                initialPosition.X = Owner.GlobalPosition.X;
            }
            if (acceleration.Y != 0 || initialVelocity.Y != this.initialVelocity.Y)
            {
                if (acceleration.Y != 0)
                {
                    initialVelocity.Y = velocity.Y;
                    this.acceleration.Y += acceleration.Y;
                }
                if (initialVelocity.Y != this.initialVelocity.Y)
                {
                    this.initialVelocity.Y = initialVelocity.Y;
                }
                motionTime.Y = 0;
                initialPosition.Y = Owner.GlobalPosition.Y;
            }
            if (acceleration.Z != 0 || initialVelocity.Z != this.initialVelocity.Z)
            {
                if (acceleration.Z != 0)
                {
                    initialVelocity.Z = velocity.Z;
                    this.acceleration.Z += acceleration.Z;
                }
                if (initialVelocity.Z != this.initialVelocity.Z)
                {
                    this.initialVelocity.Z = initialVelocity.Z;
                }
                motionTime.Z = 0;
                initialPosition.Z = Owner.GlobalPosition.Z;
            }
        }
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Components
{
    public abstract class CharacterController : Component
    {
        protected float walkSpeed;
        protected float walkVolume;
        protected float runSpeed;
        protected float runVolume;
        protected float crouchSpeed;
        protected float crouchVolume;
        protected Movement movement;

        protected Vector3 lastPosition, lastPosition2;
        protected Quaternion lastRotation, lastRotation2;

        public Movement Movement { get { return movement; } }

        private float health = 100;
        private float maxHealth = 100;
        private float regenRate = 20;

        public virtual void DealDamage(float amount, Weapon causer)
        {
            health -= amount;
            if (health <= 0) HandleDeath();
        }

        protected virtual void HandleDeath()
        {
            Owner.IsVisible = false;
            Owner.Scene.Destroy(Owner);
        }

        public void Revert()
        {
            Owner.LocalPosition = lastPosition2;
            Owner.LocalQuaternionRotation = lastRotation2;
            lastPosition = lastPosition2;
            lastRotation = lastRotation2;
        }

        public override void Update(GameTime gameTime)
        {
            lastPosition2 = lastPosition;
            lastPosition = Owner.LocalPosition;
            lastRotation2 = lastRotation;
            lastRotation = Owner.LocalQuaternionRotation;

            if (health < maxHealth)
            {
                health += (float)gameTime.ElapsedGameTime.TotalSeconds * regenRate;
                if (health > maxHealth) health = maxHealth;
            }
        }

        public CharacterController() : 
            this(75.0f, 0.5f, 125.0f, 1.0f, 35.0f, 0.0f)
        {
        }

        public CharacterController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume)
        {
            this.walkSpeed = walkSpeed;
            this.walkVolume = walkVolume;
            this.runSpeed = runSpeed;
            this.runVolume = runVolume;
            this.crouchSpeed = crouchSpeed;
            this.crouchVolume = crouchVolume;
            this.movement = Movement.WALK;
        }
    }

    public enum Movement
    {
        WALK = 0,
        RUN,
        CROUCH,
        IDLE
    }
}

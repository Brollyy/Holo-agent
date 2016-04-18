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

        public CharacterController() : 
            this(75.0f, 0.5f, 125.0f, 1.0f, 50.0f, 0.0f)
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
        CROUCH
    }
}

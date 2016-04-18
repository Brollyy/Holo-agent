using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class PlayerController : CharacterController
    {
        private float turnSpeed;

        private void Turn(GameTime gameTime)
        {
            Vector3 rot = Owner.LocalEulerRotation;
            if (Input.MOUSE_AXIS_X < 0)
            {
                rot.X += (float)(turnSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.MOUSE_AXIS_X > 0)
            {
                rot.X -= (float)(turnSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.MOUSE_AXIS_Y < 0)
            {
                if (rot.Y < 75f || rot.Y > 285f)
                {
                    float newRot = rot.Y + (float)(turnSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                    if (newRot < 75f || newRot > 285f)
                        rot.Y += (float)(turnSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
            if (Input.MOUSE_AXIS_Y > 0)
            {
                if (rot.Y < 75f || rot.Y > 285f)
                {
                    float newRot = rot.Y - (float)(turnSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                    if (newRot < 75f || newRot > 285f)
                        rot.Y -= (float)(turnSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
            Owner.LocalEulerRotation = Vector3.Lerp(Owner.LocalEulerRotation, rot, 0.75f);
        }

        private void Move(GameTime gameTime)
        {
            float speed;
            switch(movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }
            if (Input.KEY_STATE.IsKeyDown(Input.MOVE_FORWARD))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(speed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.KEY_STATE.IsKeyDown(Input.MOVE_BACKWARD))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(speed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.KEY_STATE.IsKeyDown(Input.STRAFE_LEFT))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Left, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(speed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.KEY_STATE.IsKeyDown(Input.STRAFE_RIGHT))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Right, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(speed * gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public void UpdateMovementState()
        {
            // TODO: Needs more complicated logic for jumping and movement.
            // Movement state will be the base for animation (possibly plus actual speed of the character).
            if (Input.KEY_STATE.IsKeyDown(Input.JUMP))
            {
                //Jump.
            }

            // Check if previous state ended.
            if (movement == Movement.CROUCH && Input.KEY_STATE.IsKeyUp(Input.CROUCH)) movement = Movement.WALK;
            if (movement == Movement.RUN && Input.KEY_STATE.IsKeyUp(Input.RUN)) movement = Movement.WALK;

            // Check if new state occured.
            if (movement == Movement.WALK && Input.KEY_STATE.IsKeyDown(Input.CROUCH))
            {
                movement = Movement.CROUCH;
                return;
            }
            if(movement == Movement.WALK && Input.KEY_STATE.IsKeyDown(Input.RUN))
            {
                movement = Movement.RUN;
                return;
            }
        }

        public override void Update(GameTime gameTime)
        {
            UpdateMovementState();
            Turn(gameTime);
            Move(gameTime);
        }

        public PlayerController() : 
            this(75.0f, 0.5f, 125.0f, 1.0f, 50.0f, 0.0f, 200.0f)
        {
        }

        public PlayerController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume, float turnSpeed):
            base(walkSpeed, walkVolume, runSpeed, runVolume, crouchSpeed, crouchVolume)
        {
            this.turnSpeed = turnSpeed;
        }
    }
}

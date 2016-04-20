using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Engine.Utilities;

namespace Engine.Components
{
    public class PlayerController : CharacterController
    {
        private float turnSpeed;

        private void Turn(float xMove, float yMove, GameTime gameTime)
        {
            Vector3 rot = Owner.LocalEulerRotation;
            rot.X -= (float)(turnSpeed * xMove * gameTime.ElapsedGameTime.TotalMilliseconds);
            if (rot.Y < 75f || rot.Y > 285f)
            {
                float newRot = rot.Y - (float)(turnSpeed * yMove * gameTime.ElapsedGameTime.TotalMilliseconds);
                if (newRot < 75f || newRot > 285f)
                    rot.Y = newRot;
            }
            Owner.LocalEulerRotation = Vector3.Lerp(Owner.LocalEulerRotation, rot, 0.75f);
        }

        private void MoveForward(PressingActionArgs args)
        {
            float speed;
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }

            Owner.LocalPosition += Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                   (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void MoveBackward(PressingActionArgs args)
        {
            float speed;
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }

            Owner.LocalPosition += Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                   (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void MoveLeft(PressingActionArgs args)
        {
            float speed;
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }

            Owner.LocalPosition += Vector3.Transform(Vector3.Left, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                   (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void MoveRight(PressingActionArgs args)
        {
            float speed;
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }

            Owner.LocalPosition += Vector3.Transform(Vector3.Right, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                   (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void Jump(PressedActionArgs args)
        {
            // TODO: Needs logic for jumping and movement.
            // Movement state will be the base for animation (possibly plus actual speed of the character).
        }

        private void Crouch(PressedActionArgs args)
        {
            if (movement == Movement.WALK)
            {
                movement = Movement.CROUCH;
                Owner.LocalPosition = Owner.LocalPosition - new Vector3(0, 9, 0);
            }
        }

        private void StopCrouching(ReleasedActionArgs args)
        {
            if (movement == Movement.CROUCH)
            {
                movement = Movement.WALK;
                Owner.LocalPosition = Owner.LocalPosition + new Vector3(0, 9, 0);
            }
        }

        private void Run(PressedActionArgs args)
        {
            if (movement == Movement.WALK)
            {
                movement = Movement.RUN;
            }
        }

        private void StopRunning(ReleasedActionArgs args)
        { 
            if (movement == Movement.RUN) movement = Movement.WALK;
        }

        private void Interact(PressedActionArgs args)
        {
            // Interaction ray.
            Raycast ray = new Raycast(Owner.GlobalPosition, Owner.LocalToWorldMatrix.Forward, 100.0f);
            List<GameObject> objects = Owner.Scene.GetObjects();
            float? closest = null;
            GameObject closestGo = null;
            foreach (GameObject go in objects)
            {
                if (go == Owner) continue;
                Collider col = go.GetComponent<Collider>();
                if (col != null)
                {
                    float? distance = ray.Intersect(col.bound);
                    if (distance != null)
                    {
                        if (closest == null || distance < closest)
                        {
                            closest = distance;
                            closestGo = go;
                        }
                    }
                }
            }
            
            if (closestGo != null)
            {
                Interaction interact = closestGo.GetComponent<Interaction>();
                if (interact != null) interact.Interact(Owner);
                System.Console.WriteLine(closestGo.Name + " " + closest);
            }
            else
            {
                System.Console.WriteLine("No object found in radius.");
            }
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public PlayerController() : 
            this(75.0f, 0.5f, 125.0f, 1.0f, 50.0f, 0.0f, 20.0f)
        {
        }

        public PlayerController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume, float turnSpeed):
            base(walkSpeed, walkVolume, runSpeed, runVolume, crouchSpeed, crouchVolume)
        {
            this.turnSpeed = turnSpeed;

            // Bind actions to input.
            Input.BindActionContinuousPress(GameAction.MOVE_FORWARD, MoveForward);
            Input.BindActionContinuousPress(GameAction.MOVE_BACKWARD, MoveBackward);
            Input.BindActionContinuousPress(GameAction.STRAFE_LEFT, MoveLeft);
            Input.BindActionContinuousPress(GameAction.STRAFE_RIGHT, MoveRight);
            Input.BindActionPress(GameAction.INTERACT, Interact);
            Input.BindActionPress(GameAction.CROUCH, Crouch);
            Input.BindActionRelease(GameAction.CROUCH, StopCrouching);
            Input.BindActionPress(GameAction.RUN, Run);
            Input.BindActionRelease(GameAction.RUN, StopRunning);
            Input.BindMouseMovement(Turn);
        }
    }
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Engine.Utilities;

namespace Engine.Components
{
    public class PlayerController : CharacterController
    {
        private bool interactPressed;
        private float turnSpeed;

        private void Turn(GameTime gameTime)
        {
            Vector3 rot = Owner.LocalEulerRotation;
            rot.X -= (float)(turnSpeed * Input.MOUSE_AXIS_X * gameTime.ElapsedGameTime.TotalMilliseconds);
            if (rot.Y < 75f || rot.Y > 285f)
            {
                float newRot = rot.Y - (float)(turnSpeed * Input.MOUSE_AXIS_Y * gameTime.ElapsedGameTime.TotalMilliseconds);
                if (newRot < 75f || newRot > 285f)
                    rot.Y = newRot;
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
            if (movement == Movement.CROUCH && Input.KEY_STATE.IsKeyUp(Input.CROUCH))
            {
                movement = Movement.WALK;
                Owner.LocalPosition = Owner.LocalPosition + new Vector3(0, 9, 0);
            }
            if (movement == Movement.RUN && Input.KEY_STATE.IsKeyUp(Input.RUN)) movement = Movement.WALK;

            // Check if new state occured.
            if (movement == Movement.WALK && Input.KEY_STATE.IsKeyDown(Input.CROUCH))
            {
                movement = Movement.CROUCH;
                Owner.LocalPosition = Owner.LocalPosition - new Vector3(0, 9, 0);
                return;
            }
            if(movement == Movement.WALK && Input.KEY_STATE.IsKeyDown(Input.RUN))
            {
                movement = Movement.RUN;
                return;
            }
        }
        public void Ray(ref GameObject closestGameObject, ref float? closest, float length)
        {
            Raycast ray = new Raycast(Owner.GlobalPosition, Owner.LocalToWorldMatrix.Forward, length);
            List<GameObject> objects = Owner.Scene.GetObjects();
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
                            closestGameObject = go;
                        }
                    }
                }
            }
        }
        public override void Update(GameTime gameTime)
        {
            UpdateMovementState();
            Turn(gameTime);
            Move(gameTime);
            if(Owner.GetComponents<Weapon>() != null)
            {
                Weapon weapon = Owner.GetChild("Pistol").GetComponent<Weapon>();
                if (Input.IsButtonPressed(Input.FIRE))
                {
                    weapon.shoot(gameTime);
                }
                else
                {
                    if (weapon.getGunfire() == false)
                        weapon.unlockWeapon();
                }
                if (Input.IsButtonPressed(Input.ZOOM))
                {

                }
                if (Input.IsButtonReleased(Input.ZOOM))
                {

                }
                if (Input.KEY_STATE.IsKeyDown(Input.RELOAD))
                {
                    weapon.reload();
                }
            }
            if (Input.KEY_STATE.IsKeyDown(Input.INTERACT))
            {
                float? closest = null;
                GameObject closestGameObject = null;
                if (!interactPressed)
                {
                    interactPressed = true;
                    Ray(ref closestGameObject, ref closest, 100);
                    if(closestGameObject != null)
                    {
                        Interaction interact = closestGameObject.GetComponent<Interaction>();
                        if (interact != null) interact.Interact(Owner);
                        System.Console.WriteLine(closestGameObject.Name + " " + closest);
                    }
                    else
                    {
                        System.Console.WriteLine("No object found in radius.");
                    }
                }
            }
            else if (interactPressed) interactPressed = false;
        }

        public PlayerController() : 
            this(75.0f, 0.5f, 125.0f, 1.0f, 50.0f, 0.0f, 20.0f)
        {
        }

        public PlayerController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume, float turnSpeed):
            base(walkSpeed, walkVolume, runSpeed, runVolume, crouchSpeed, crouchVolume)
        {
            this.turnSpeed = turnSpeed;
            interactPressed = false;
        }
    }
}

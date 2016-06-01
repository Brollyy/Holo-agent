using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Engine.Utilities;
using System;
using System.Linq;

namespace Engine.Components
{
    public class PlayerController : CharacterController
    {
        private float turnSpeed;
        private bool hologramRecording;
        private bool hologramPlaying;
        private HologramPath? recordedPath;
        private GameObject player;
        private Vector3 playerCameraPosition;
        private Quaternion playerCameraRotation;
        private Vector3 playerCameraScale;
        private Vector3 lastPosition, lastPosition2;
        private Quaternion lastRotation, lastRotation2;
        private float? closestObjectDistance;
        private GameObject closestObject;
        private Color crosshairColor;
        private GameObject[] weapons;
        public GameObject ClosestObject
        {
            get
            {
                return closestObject;
            }
        }
        public float? ClosestObjectDistance
        {
            get
            {
                return closestObjectDistance;
            }
        }
        public Color CrosshairColor
        {
            get
            {
                return crosshairColor;
            }
        }
        public MeshInstance PlayerMesh
        {
            get;
            set;
        }
        public void addWeapon(GameObject weapon)
        {
            int index = Array.FindIndex(weapons, i => i == null);
            int otherWeaponIndex = Array.FindIndex(weapons, i => i != null && i.GetComponent<Weapon>().getWeaponType() == weapon.GetComponent<Weapon>().getWeaponType());
            Weapon component = weapon.GetComponent<Weapon>();
            if (index != -1 && otherWeaponIndex == -1 && component != null && weapons.Contains(weapon) == false)
            {
                weapons[index] = weapon;
                weapons[index].GetComponent<Weapon>().Collision = false;
                arm(weapon);
            }
        }
        private void removeWeapon(GameObject weapon)
        {
            int index = Array.IndexOf(weapons, weapon);
            int otherWeaponIndex = Array.FindIndex(weapons, i => i != null && i != weapon);
            if (index != -1)
            {
                if (otherWeaponIndex != -1)
                    arm(weapons[otherWeaponIndex]);
                //Remove later (after adding gravity and fixing collisions)............//
                weapons[index].LocalPosition -= Vector3.UnitZ * (15);                  //
                weapons[index].GlobalRotation = Owner.LocalQuaternionRotation;         //
                //.....................................................................//
                weapons[index].GetComponent<Weapon>().Collision = true;
                weapons[index].IsVisible = true;
                weapons[index].Parent = null;
                weapons[index] = null;
                weapon.GlobalScale = Vector3.One;
            }
        }
        private void arm(GameObject weapon)
        {
            int index = Array.FindIndex(weapons, i => i != null && i != weapon && i.GetComponent<Weapon>().IsArmed == true);
            if (index != -1)
            {
                weapons[index].GetComponent<Weapon>().IsArmed = false;
                weapons[index].IsVisible = false;
            }
            weapon.GetComponent<Weapon>().IsArmed = true;
            weapon.IsVisible = true;
            if (weapon.Parent == null)
            {
                weapon.Parent = Owner;
                weapon.LocalScale = Vector3.One;
                weapon.LocalQuaternionRotation = Quaternion.Identity;
                weapon.LocalPosition = weapon.GetComponent<Weapon>().AsChildPosition;
            }
        }
        private void changeWeapon(MouseWheelStates state)
        {
            if (weapons.Where(i => i != null).Count() > 1)
            {
                if (state == MouseWheelStates.Up)
                {
                    int nextWeaponIndex = (Array.IndexOf(weapons, getWeapon().Owner) + 1);
                    if (nextWeaponIndex != -1 && weapons[nextWeaponIndex] != null)
                    {
                        arm(weapons[nextWeaponIndex]);
                    }
                }
                if (state == MouseWheelStates.Down)
                {
                    int previousWeaponIndex = (Array.IndexOf(weapons, getWeapon().Owner) - 1);
                    if (previousWeaponIndex != -1 && weapons[previousWeaponIndex] != null)
                    {
                        arm(weapons[previousWeaponIndex]);
                    }
                }
            }
        }
        public Weapon getWeapon()
        {
            int index = Array.FindIndex(weapons, i => i != null && i.GetComponent<Weapon>().IsArmed == true);
            if (index != -1)
            {
                return weapons[index].GetComponent<Weapon>();
            }
            else
            {
                return null;
            }
        }
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
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }
            if (rigidbody != null && rigidbody.isGrounded())
            {
                Vector3 initialVelocity = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) * (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
                rigidbody.AddForce(Vector3.Zero, initialVelocity.X, initialVelocity.Y, initialVelocity.Z);
            }
        }
        private void StayForward(ReleasedActionArgs args)
        {
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.AddForce(Vector3.Zero, 0, rigidbody.Velocity.Y, 0);
        }
        private void MoveBackward(PressingActionArgs args)
        {
            float speed;
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }
            if (rigidbody != null && rigidbody.isGrounded())
            {
                Vector3 initialVelocity = Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) * (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
                rigidbody.AddForce(Vector3.Zero, initialVelocity.X, initialVelocity.Y, initialVelocity.Z);
            }
        }
        private void StayBackward(ReleasedActionArgs args)
        {
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.AddForce(Vector3.Zero, 0, rigidbody.Velocity.Y, 0);
        }
        private void MoveLeft(PressingActionArgs args)
        {
            float speed;
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }
            if (rigidbody != null && rigidbody.isGrounded())
            {
                Vector3 initialVelocity = Vector3.Transform(Vector3.Left, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) * (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
                rigidbody.AddForce(Vector3.Zero, initialVelocity.X, initialVelocity.Y, initialVelocity.Z);
            }
        }
        private void StayLeft(ReleasedActionArgs args)
        {
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.AddForce(Vector3.Zero, 0, rigidbody.Velocity.Y, 0);
        }
        private void MoveRight(PressingActionArgs args)
        {
            float speed;
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }
            if (rigidbody != null && rigidbody.isGrounded())
            {
                Vector3 initialVelocity = Vector3.Transform(Vector3.Right, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) * (float)(speed * args.gameTime.ElapsedGameTime.TotalSeconds);
                rigidbody.AddForce(Vector3.Zero, initialVelocity.X, initialVelocity.Y, initialVelocity.Z);
            }
        }
        private void StayRight(ReleasedActionArgs args)
        {
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.AddForce(Vector3.Zero, 0, rigidbody.Velocity.Y, 0);
        }
        private void Jump(PressedActionArgs args)
        {
            // TODO: Needs logic for jumping and movement.
            // Movement state will be the base for animation (possibly plus actual speed of the character).
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null && rigidbody.isGrounded())
            {
                rigidbody.AddForce(Vector3.Zero, 0, 3.5f, 0);
            }
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

        public void Ray(ref GameObject closestGameObject, ref float? closest, float maxDistance)
        {
            Raycast ray = new Raycast(Owner.GlobalPosition, Owner.LocalToWorldMatrix.Forward, maxDistance);
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
        private bool canInteract(ref GameObject closestGo, ref float? closest)
        {
            if (closest <= 100.0f && closestGo.GetComponent<Interaction>() != null)
                return true;
            else
                return false;
        }
        private void Interact(PressedActionArgs args)
        {
            if (closestObject != null)
            {
                Interaction interact = closestObject.GetComponent<Interaction>();
                if (canInteract(ref closestObject, ref closestObjectDistance))
                    interact.Interact(closestObject);
                System.Console.WriteLine(closestObject.Name + " " + closestObjectDistance);
            }
            else
            {
                System.Console.WriteLine("No object found in radius.");
            }
        }

        private void Fire(PressingActionArgs args)
        {
            if (hologramRecording) return;
            Weapon weapon = getWeapon();
            if (weapon == null) return;
            weapon.shoot(args.gameTime);
        }

        private void UnlockFire(ReleasedActionArgs args)
        {
            if (hologramRecording) return;
            Weapon weapon = getWeapon();
            if (weapon == null) return;
            weapon.unlockWeapon();
        }

        private void Reload(PressedActionArgs args)
        {
            if (hologramRecording) return;
            Weapon weapon = getWeapon();
            if (weapon == null) return;
            weapon.reload();
        }
        private void dropWeapon(PressedActionArgs args)
        {
            if (getWeapon() != null)
                removeWeapon(getWeapon().Owner);
        }
        private void RecordingButton(PressedActionArgs args)
        {
            if (!hologramRecording && !hologramPlaying)
            {
                GameObject hologramRecording = new GameObject("HologramRecorder", Owner.LocalPosition, 
                                                              Owner.LocalQuaternionRotation, Owner.LocalScale, Owner.Scene, Owner.Parent);
                hologramRecording.AddComponent(new HologramRecorder(5.0f, 100, StopRecording));
                hologramRecording.AddNewComponent<Rigidbody>();
                hologramRecording.GetComponent<Rigidbody>().Initialize(80);
                hologramRecording.GetComponent<Rigidbody>().GravityEnabled = false;
                MeshInstance mesh = Owner.GetComponent<MeshInstance>();
                if(mesh != null) hologramRecording.AddComponent(new MeshInstance(mesh));
                if(PlayerMesh != null) Owner.AddComponent(PlayerMesh);
                player = Owner;
                playerCameraPosition = Owner.Scene.Camera.GlobalPosition;
                playerCameraRotation = Owner.Scene.Camera.GlobalRotation;
                playerCameraScale = Owner.Scene.Camera.GlobalScale;
                Owner.RemoveComponent(this);
                hologramRecording.AddComponent(this);
                Owner.Scene.Camera.Parent = hologramRecording;
                this.hologramRecording = true;
            }
        }

        private void StopRecording(HologramPath path)
        {
            this.hologramRecording = false;
            recordedPath = path;
            Owner.RemoveComponent(this);
            if (player != null)
            {
                player.AddComponent(this);
                if(PlayerMesh != null) player.RemoveComponent(PlayerMesh);
            }
            Owner.Scene.Camera.Parent = Owner;
            Owner.Scene.Camera.GlobalPosition = playerCameraPosition;
            Owner.Scene.Camera.GlobalRotation = playerCameraRotation;
            Owner.Scene.Camera.GlobalScale = playerCameraScale;
        }

        private void PlaybackButton(PressedActionArgs args)
        {
            if (!hologramRecording && !hologramPlaying && recordedPath != null)
            {
                GameObject hologramPlayback = new GameObject("HologramPlayback", Owner.LocalPosition, Owner.LocalQuaternionRotation,
                                                              Owner.LocalScale, Owner.Scene, Owner.Parent);
                hologramPlayback.AddComponent(new HologramPlayback(recordedPath.Value, StopPlayback));
                if (PlayerMesh != null)
                {
                    hologramPlayback.AddComponent(PlayerMesh);
                    AnimationController anim = hologramPlayback.AddNewComponent<AnimationController>();
                    anim.BindAnimation("run", PlayerMesh.Model.Clips[1], true);
                    anim.BindAnimation("idle", null, false);
                    anim.SetBindPose(PlayerMesh.Model.Clips[0]);
                }
                this.hologramPlaying = true;
            }
        }

        private void StopPlayback()
        {
            this.hologramPlaying = false;
            if(PlayerMesh != null) PlayerMesh.Owner.RemoveComponent(PlayerMesh);
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
            closestObjectDistance = null;
            closestObject = null;
            Ray(ref closestObject, ref closestObjectDistance, 1000.0f);
            if (canInteract(ref closestObject, ref closestObjectDistance))
                crosshairColor = Color.Lime;
            else
                crosshairColor = Color.Orange;
            changeWeapon(Input.getMouseWheelState());
        }
        public override void Destroy()
        {
            Input.UnbindActionContinuousPress(GameAction.MOVE_FORWARD, MoveForward);
            Input.UnbindActionRelease(GameAction.MOVE_FORWARD, StayForward);
            Input.UnbindActionContinuousPress(GameAction.MOVE_BACKWARD, MoveBackward);
            Input.UnbindActionRelease(GameAction.MOVE_BACKWARD, StayBackward);
            Input.UnbindActionContinuousPress(GameAction.STRAFE_LEFT, MoveLeft);
            Input.UnbindActionRelease(GameAction.STRAFE_LEFT, StayLeft);
            Input.UnbindActionContinuousPress(GameAction.STRAFE_RIGHT, MoveRight);
            Input.UnbindActionRelease(GameAction.STRAFE_RIGHT, StayRight);
            Input.UnbindActionPress(GameAction.JUMP, Jump);
            Input.UnbindActionPress(GameAction.INTERACT, Interact);
            Input.UnbindActionPress(GameAction.CROUCH, Crouch);
            Input.UnbindActionRelease(GameAction.CROUCH, StopCrouching);
            Input.UnbindActionPress(GameAction.RUN, Run);
            Input.UnbindActionRelease(GameAction.RUN, StopRunning);
            Input.UnbindActionPress(GameAction.RECORD_HOLOGRAM, RecordingButton);
            Input.UnbindActionPress(GameAction.PLAY_HOLOGRAM, PlaybackButton);
            Input.UnbindActionPress(GameAction.RELOAD, Reload);
            Input.UnbindActionContinuousPress(GameAction.FIRE, Fire);
            Input.UnbindActionRelease(GameAction.FIRE, UnlockFire);
            Input.UnbindActionPress(GameAction.DROP_WEAPON, dropWeapon);
            Input.UnbindMouseMovement(Turn);
        }

        public PlayerController(PlayerController other):
            this(other.walkSpeed, other.walkVolume, other.runSpeed, other.runVolume, 
                 other.crouchSpeed, other.crouchVolume, other.turnSpeed)
        {
            movement = other.movement;
        }

        public PlayerController() : 
            this(200f, 0.5f, 350.0f, 1.0f, 150.0f, 0.0f, 20.0f)
        {
        }

        public PlayerController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume, float turnSpeed):
            base(walkSpeed, walkVolume, runSpeed, runVolume, crouchSpeed, crouchVolume)
        {
            this.turnSpeed = turnSpeed;
            recordedPath = null;
            hologramRecording = false;
            hologramPlaying = false;
            player = null;
            lastPosition = lastPosition2 = Vector3.Zero;
            lastRotation = lastRotation2 = Quaternion.Identity;
            closestObjectDistance = null;
            closestObject = null;
            crosshairColor = Color.Orange;
            weapons = new GameObject[3];
            // Bind actions to input.
            Input.BindActionContinuousPress(GameAction.MOVE_FORWARD, MoveForward);
            Input.BindActionRelease(GameAction.MOVE_FORWARD, StayForward);
            Input.BindActionContinuousPress(GameAction.MOVE_BACKWARD, MoveBackward);
            Input.BindActionRelease(GameAction.MOVE_BACKWARD, StayBackward);
            Input.BindActionContinuousPress(GameAction.STRAFE_LEFT, MoveLeft);
            Input.BindActionRelease(GameAction.STRAFE_LEFT, StayLeft);
            Input.BindActionContinuousPress(GameAction.STRAFE_RIGHT, MoveRight);
            Input.BindActionRelease(GameAction.STRAFE_RIGHT, StayRight);
            Input.BindActionPress(GameAction.JUMP, Jump);
            Input.BindActionPress(GameAction.INTERACT, Interact);
            Input.BindActionPress(GameAction.CROUCH, Crouch);
            Input.BindActionRelease(GameAction.CROUCH, StopCrouching);
            Input.BindActionPress(GameAction.RUN, Run);
            Input.BindActionRelease(GameAction.RUN, StopRunning);
            Input.BindActionPress(GameAction.RECORD_HOLOGRAM, RecordingButton);
            Input.BindActionPress(GameAction.PLAY_HOLOGRAM, PlaybackButton);
            Input.BindActionPress(GameAction.RELOAD, Reload);
            Input.BindActionContinuousPress(GameAction.FIRE, Fire);
            Input.BindActionRelease(GameAction.FIRE, UnlockFire);
            Input.BindActionPress(GameAction.DROP_WEAPON, dropWeapon);
            Input.BindMouseMovement(Turn);
        }
    }
}

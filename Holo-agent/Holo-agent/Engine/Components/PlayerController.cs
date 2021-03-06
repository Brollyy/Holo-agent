﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Engine.Utilities;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Audio;
using System.Resources;

namespace Engine.Components
{
    [DataContract]
    public class PlayerController : CharacterController
    {
        [DataMember]
        private const float hologramCooldown = 5.0f;
        [DataMember]
        private float turnSpeed;
        private bool hologramRecording;
        private bool hologramPlaying;
        private bool hologramPreview;
        private GameObject preview;
        private GameObject hologramPlayback;
        [DataMember]
        private Pair<HologramPath?,float>[] recordedPaths;
        [DataMember]
        private int selectedPath;
        private int playingPath;
        private GameObject player;
        private Vector3 playerCameraPosition;
        private Quaternion playerCameraRotation;
        private Vector3 playerCameraScale;
        private Quaternion playerRotation;
        private Color crosshairColor;
        private List<SoundEffectInstance> stepsSounds;
        [DataMember]
        private GameObject[] weapons;
        [DataMember]
        private bool isCrouching;
        [DataMember]
        private bool isRunning;
        [DataMember]
        private int isMoving;

        public List<SoundEffectInstance> StepsSounds
        {
            set
            {
                stepsSounds = value;
            }
        }

        public int SelectedPath
        {
            get { return selectedPath; }
        }

        public int PlayingPath
        {
            get { return playingPath; }
        }

        public bool HologramPreviewing
        {
            get { return hologramPreview; }
        }

        public bool HologramPlaying
        {
            get { return hologramPlaying; }
        }
        public bool HologramRecording
        {
            get
            {
                return hologramRecording;
            }
        }
        public bool IsPathRecorded(int index)
        {
            if (index < 0 || index >= recordedPaths.Length) return false;
            else return recordedPaths[index].First.HasValue;
        }

        public float PathCooldown(int index)
        {
            if (!IsPathRecorded(index)) return float.PositiveInfinity;
            return recordedPaths[index].Second;
        }

        public Color CrosshairColor
        {
            get
            {
                return crosshairColor;
            }
        }
        [DataMember]
        public MeshInstance PlayerMesh
        {
            get;
            set;
        }
        [DataMember]
        public MeshInstance HologramMesh
        {
            get;
            set;
        }
        [DataMember]
        public MeshInstance PreviewMesh
        {
            get;
            set;
        }
        protected override void HandleDeath()
        {
            
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
                weapons[index].Parent = Owner.Scene.FindRoomContaining(weapon);
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
            if (weapon.Parent != Owner.Scene.Camera)
            {
                weapon.Parent = Owner.Scene.Camera;
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
            Vector3 cameraRot = Owner.Scene.Camera.LocalEulerRotation;
            rot.X -= (float)(turnSpeed * xMove * gameTime.ElapsedGameTime.TotalMilliseconds);
            if (cameraRot.Y < 75f || cameraRot.Y > 285f)
            {
                float newRot = cameraRot.Y - (float)(turnSpeed * yMove * gameTime.ElapsedGameTime.TotalMilliseconds);
                if (newRot < 75f || newRot > 285f)
                    cameraRot.Y = newRot;
            }
            Owner.LocalEulerRotation = Vector3.Lerp(Owner.LocalEulerRotation, rot, 0.75f);
            Owner.Scene.Camera.LocalEulerRotation = Vector3.Lerp(Owner.Scene.Camera.LocalEulerRotation, cameraRot, 0.75f);
        }

        private void StartMoving(PressedActionArgs args)
        {
            isMoving++;
        }

        private void Move(PressingActionArgs args)
        {
            float speed;
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            float delta = (float)args.gameTime.ElapsedGameTime.TotalSeconds;
            switch (movement)
            {
                case Movement.WALK: speed = walkSpeed; break;
                case Movement.RUN: speed = runSpeed; break;
                case Movement.CROUCH: speed = crouchSpeed; break;
                default: speed = 0.0f; break;
            }

            Vector3 direction;
            switch(args.action)
            {
                case GameAction.MOVE_FORWARD: direction = Owner.LocalToWorldMatrix.Forward; break;
                case GameAction.MOVE_BACKWARD: direction = Owner.LocalToWorldMatrix.Backward; break;
                case GameAction.STRAFE_LEFT: direction = Owner.LocalToWorldMatrix.Left; break;
                case GameAction.STRAFE_RIGHT: direction = Owner.LocalToWorldMatrix.Right; break;
                default: direction = Vector3.Zero; break;
            }
            direction.Y = 0; direction.Normalize();

            if (rigidbody != null && (rigidbody.IsGrounded || !rigidbody.GravityEnabled) && Vector3.Dot(rigidbody.Velocity, direction) < speed)
            {
                rigidbody.AddForce(rigidbody.Mass * 5*(speed - rigidbody.Velocity.Length()) * direction);
            }
        }
        private void Stay(ReleasedActionArgs args)
        {
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null && (rigidbody.IsGrounded || !rigidbody.GravityEnabled))
            {
                Vector3 direction;
                if (args == null)
                {
                    rigidbody.AddVelocityChange(-rigidbody.Velocity);
                }
                else
                {
                    switch (args.action)
                    {
                        case GameAction.MOVE_FORWARD: direction = Owner.LocalToWorldMatrix.Forward; break;
                        case GameAction.MOVE_BACKWARD: direction = Owner.LocalToWorldMatrix.Backward; break;
                        case GameAction.STRAFE_LEFT: direction = Owner.LocalToWorldMatrix.Left; break;
                        case GameAction.STRAFE_RIGHT: direction = Owner.LocalToWorldMatrix.Right; break;
                        default: direction = Vector3.Zero; break;
                    }
                    direction.Y = 0; direction.Normalize();

                    float vel = Vector3.Dot(direction, rigidbody.Velocity);
                    if (vel > 0.0f)
                    {
                        rigidbody.AddVelocityChange(-vel * direction);
                    }
                }
                isMoving--;
            }
        }
        
        private void Jump(PressedActionArgs args)
        {
            // TODO: Needs logic for jumping and movement.
            // Movement state will be the base for animation (possibly plus actual speed of the character).
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null && rigidbody.IsGrounded)
            {
                rigidbody.AddImpulseForce(rigidbody.Mass*40*Vector3.Up);
            }
        }

        private void Crouch(PressedActionArgs args)
        {
            isCrouching = true;
            movement = Movement.CROUCH;
            Owner.Scene.Camera.LocalPosition = Owner.Scene.Camera.LocalPosition - new Vector3(0, 9, 0);
            playerCameraPosition -= 9 * Vector3.Up;
        }

        private void StopCrouching(ReleasedActionArgs args)
        {
            isCrouching = false;
            if (movement == Movement.CROUCH)
            {
                movement = isRunning ? Movement.RUN : Movement.WALK;
                Owner.Scene.Camera.LocalPosition = Owner.Scene.Camera.LocalPosition + new Vector3(0, 9, 0);
                playerCameraPosition += 9 * Vector3.Up;
            }
        }

        private void Run(PressedActionArgs args)
        {
            isRunning = true;
            if (movement == Movement.WALK || movement == Movement.IDLE)
            {
                movement = Movement.RUN;
            }
        }

        private void StopRunning(ReleasedActionArgs args)
        {
            isRunning = false;
            if (movement == Movement.RUN) movement = Movement.WALK;
        }

        private bool canInteract()
        {
            if (closestObjectDistance <= 100.0f && closestObject.GetComponent<Interaction>() != null)
            {
                Weapon weapon = closestObject.GetComponent<Weapon>();
                return (weapon == null || weapon.Collision);
            }
            else
                return false;
        }
        private void Interact(PressedActionArgs args)
        {
            if (closestObject != null)
            {
                Interaction interact = closestObject.GetComponent<Interaction>();
                if (canInteract())
                    interact.Interact(Owner);
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
            weapon.shoot();
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
            Rigidbody rig = Owner.GetComponent<Rigidbody>();
            if (!hologramRecording && !hologramPlaying && (rig == null || rig.IsGrounded || !rig.GravityEnabled))
            {
                GameObject hologramRecording = new GameObject("HologramRecorder", Owner.LocalPosition, 
                                                              Owner.LocalQuaternionRotation, Owner.LocalScale, Owner.Scene, Owner.Parent);
                hologramRecording.AddComponent(new HologramRecorder(5.0f, 100, StopRecording));
                hologramRecording.AddComponent(new Rigidbody(Owner.GetComponent<Rigidbody>()));
                Collider holCol = hologramRecording.AddNewComponent<Collider>();
                Bounding_Volumes.BoundingBox bound = (Owner.GetComponent<Collider>().bound as Bounding_Volumes.BoundingBox);
                holCol.bound = new Bounding_Volumes.BoundingBox(holCol, bound.Center, new Vector3(bound.HalfLength, bound.HalfHeight, bound.HalfWidth));
                MeshInstance mesh = Owner.GetComponent<MeshInstance>();
                if(mesh != null) hologramRecording.AddComponent(new MeshInstance(mesh));
                if (PlayerMesh != null)
                {
                    Owner.AddComponent(PlayerMesh);
                    AnimationController contr = Owner.AddNewComponent<AnimationController>();
                    contr.BindAnimation("idle", isCrouching ? 7 : 6, true);
                    contr.PlayAnimation("idle");
                }
                Stay(null);
                if (getWeapon() != null)
                {
                    getWeapon().Owner.IsVisible = false;
                }
                player = Owner;
                playerRotation = Owner.LocalQuaternionRotation;
                Vector3 rotation = Owner.LocalEulerRotation;
                Owner.RemoveComponent(this);
                hologramRecording.AddComponent(this);
                Owner.Scene.Camera.Parent = hologramRecording;
                rotation.Y = 0; rotation.Z = 0;
                player.LocalEulerRotation = rotation;
                this.hologramRecording = true;
            }
        }

        private void StopRecording(HologramPath path)
        {
            this.hologramRecording = false;
            recordedPaths[selectedPath].First = path;
            Minimap.Hologram = path.StartGlobalPosition;
            Owner.RemoveComponent(this);
            if (player != null)
            {
                player.AddComponent(this);
                if (PlayerMesh != null)
                {
                    player.RemoveComponent(PlayerMesh);
                    player.RemoveComponent(player.GetComponent<AnimationController>());
                }
            }

            player.LocalQuaternionRotation = playerRotation;
            Owner.Scene.Camera.Parent = Owner;
            Owner.Scene.Camera.LocalPosition = playerCameraPosition;
            Owner.Scene.Camera.LocalQuaternionRotation = playerCameraRotation;
            Owner.Scene.Camera.LocalScale = playerCameraScale;
            if(getWeapon() != null)
            {
                getWeapon().Owner.IsVisible = true;
            }
        }

        private void PlaybackButton(PressedActionArgs args)
        { 
            if (!hologramRecording && !hologramPlaying && recordedPaths[selectedPath].First.HasValue && !(recordedPaths[selectedPath].Second > 0.0f) )
            {
                StopPreview();

                hologramPlayback = new GameObject("HologramPlayback", Owner.LocalPosition, Owner.LocalQuaternionRotation,
                                                              Owner.LocalScale, Owner.Scene, Owner.Parent);
                Minimap.Hologram = hologramPlayback;
                hologramPlayback.AddComponent(new HologramPlayback(recordedPaths[selectedPath].First.Value, StopPlayback));
                recordedPaths[selectedPath].Second = recordedPaths[selectedPath].First.Value.Duration;
                if (HologramMesh != null)
                {
                    hologramPlayback.AddComponent(HologramMesh);
                    AnimationController anim = hologramPlayback.AddNewComponent<AnimationController>();
                    anim.BindAnimation("runForward", 1, true);
                    anim.BindAnimation("runBackward", 1, true);
                    anim.BindAnimation("runLeft", 1, true);
                    anim.BindAnimation("runRight", 1, true);
                    anim.BindAnimation("walkForward", 2, true);
                    anim.BindAnimation("walkBackward", 2, true);
                    anim.BindAnimation("walkLeft", 2, true);
                    anim.BindAnimation("walkRight", 2, true);
                    anim.BindAnimation("death", 3);
                    anim.BindAnimation("jump", 4);
                    anim.BindAnimation("crouchForward", 5, true);
                    anim.BindAnimation("crouchBackward", 5, true);
                    anim.BindAnimation("crouchLeft", 5, true);
                    anim.BindAnimation("crouchRight", 5, true);
                    anim.SetBindPose(isCrouching ? HologramMesh.Model.Clips[5] : HologramMesh.Model.Clips[3]);
                }
                this.hologramPlaying = true;
                playingPath = selectedPath;
            }
        }

        private void StopPlayback()
        {
            hologramPlayback.IsVisible = false;
            this.hologramPlaying = false;
            if(HologramMesh != null) HologramMesh.Owner.RemoveComponent(PlayerMesh);
            recordedPaths[playingPath].Second = hologramCooldown;
            if (recordedPaths[selectedPath].First.HasValue) Minimap.Hologram = recordedPaths[selectedPath].First.Value.StartGlobalPosition;
            else Minimap.Hologram = null;
        }

        private void SelectHologramPath(PressedActionArgs args)
        {
            if (hologramRecording) return;

            if (hologramPreview) StopPreview();

            switch(args.action)
            {
                case GameAction.SELECT_FIRST_HOLOGRAM: selectedPath = 0; break;
                case GameAction.SELECT_SECOND_HOLOGRAM: selectedPath = 1; break;
                case GameAction.SELECT_THIRD_HOLOGRAM: selectedPath = 2; break;
            }

            if(!hologramPlaying)
            {
                if (recordedPaths[selectedPath].First.HasValue) Minimap.Hologram = recordedPaths[selectedPath].First.Value.StartGlobalPosition;
                else Minimap.Hologram = null;
            }
        }

        private void PreviewHologram(PressedActionArgs args)
        {
            if(hologramPreview)
            {
                StopPreview();
                return;
            }

            if (!hologramRecording && !hologramPlaying && recordedPaths[selectedPath].First.HasValue)
            {
                preview = new GameObject("HologramPreview", Owner.LocalPosition, Owner.LocalQuaternionRotation,
                                                              Owner.LocalScale, Owner.Scene, Owner.Parent);
                Minimap.Hologram = preview;
                preview.AddComponent(new HologramPlayback(recordedPaths[selectedPath].First.Value, StopPreview));
                if (PreviewMesh != null)
                {
                    preview.AddComponent(PreviewMesh);
                    AnimationController anim = preview.AddNewComponent<AnimationController>();
                    anim.BindAnimation("runForward", 1, true);
                    anim.BindAnimation("runBackward", 1, true);
                    anim.BindAnimation("runLeft", 1, true);
                    anim.BindAnimation("runRight", 1, true);
                    anim.BindAnimation("walkForward", 2, true);
                    anim.BindAnimation("walkBackward", 2, true);
                    anim.BindAnimation("walkLeft", 2, true);
                    anim.BindAnimation("walkRight", 2, true);
                    anim.BindAnimation("death", 3);
                    anim.BindAnimation("jump", 4);
                    anim.BindAnimation("crouchForward", 5, true);
                    anim.BindAnimation("crouchBackward", 5, true);
                    anim.BindAnimation("crouchLeft", 5, true);
                    anim.BindAnimation("crouchRight", 5, true);
                    anim.SetBindPose(isCrouching ? PreviewMesh.Model.Clips[5] : PreviewMesh.Model.Clips[3]);
                }
                hologramPreview = true;
            }
        }

        private void StopPreview()
        {
            if (hologramPreview)
            {
                if (recordedPaths[selectedPath].First.HasValue) Minimap.Hologram = recordedPaths[selectedPath].First.Value.StartGlobalPosition;
                else Minimap.Hologram = null;
                hologramPreview = false;
                Owner.Scene.Destroy(preview);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for(int i = 0; i < recordedPaths.Length; ++i)
            {
                recordedPaths[i].Second -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (recordedPaths[i].Second < 0.0f) recordedPaths[i].Second = 0.0f;
            }

            Ray(1000.0f, Owner.Scene.GetNearbyObjects(Owner), Owner.Scene.Camera.LocalToWorldMatrix.Forward);

            if (canInteract())
                crosshairColor = Color.Lime;
            else if (closestObject != null && closestObject.GetComponent<EnemyController>() != null)
                crosshairColor = Color.Red;
            else
                crosshairColor = Color.Orange;
            changeWeapon(Input.getMouseWheelState());
            if (stepsSounds != null && stepsSounds.Count > 0 && !hologramRecording)
            {
                if (isMoving > 0)
                {
                    int index = stepsSounds.FindIndex(step => step.State.Equals(SoundState.Playing));
                    if (index != -1 && index != (movement.Equals(Movement.RUN) ? 1 : 0))
                        stepsSounds[index].Stop();
                    stepsSounds[(movement.Equals(Movement.RUN) ? 1 : 0)].Play();
                }

                if(isMoving <= 0 || hologramRecording || movement.Equals(Movement.CROUCH))
                {
                    int index = stepsSounds.FindIndex(step => step.State.Equals(SoundState.Playing));
                    if (index != -1)
                        stepsSounds[index].Stop();
                }
            }
        }

        public override void Destroy()
        {
            Input.UnbindActionPress(GameAction.MOVE_FORWARD, StartMoving);
            Input.UnbindActionContinuousPress(GameAction.MOVE_FORWARD, Move);
            Input.UnbindActionRelease(GameAction.MOVE_FORWARD, Stay);
            Input.UnbindActionPress(GameAction.MOVE_BACKWARD, StartMoving);
            Input.UnbindActionContinuousPress(GameAction.MOVE_BACKWARD, Move);
            Input.UnbindActionRelease(GameAction.MOVE_BACKWARD, Stay);
            Input.UnbindActionPress(GameAction.STRAFE_LEFT, StartMoving);
            Input.UnbindActionContinuousPress(GameAction.STRAFE_LEFT, Move);
            Input.UnbindActionRelease(GameAction.STRAFE_LEFT, Stay);
            Input.UnbindActionPress(GameAction.STRAFE_RIGHT, StartMoving);
            Input.UnbindActionContinuousPress(GameAction.STRAFE_RIGHT, Move);
            Input.UnbindActionRelease(GameAction.STRAFE_RIGHT, Stay);
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
            Input.UnbindActionPress(GameAction.PREVIEW_HOLOGRAM, PreviewHologram);
            Input.UnbindActionContinuousPress(GameAction.GO_UP, UpTemp);
            Input.UnbindActionContinuousPress(GameAction.GO_DOWN, DownTemp);
            Input.UnbindMouseMovement(Turn);
        }

        public PlayerController(PlayerController other):
            this(other.walkSpeed, other.walkVolume, other.runSpeed, other.runVolume, 
                 other.crouchSpeed, other.crouchVolume, other.turnSpeed)
        {
            movement = other.movement;
        }

        public PlayerController() : 
            this(80f, 0.5f, 150f, 1.0f, 40f, 0.0f, 20.0f)
        {
        }

        public PlayerController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume, float turnSpeed):
            base(walkSpeed, walkVolume, runSpeed, runVolume, crouchSpeed, crouchVolume)
        {
            isCrouching = false;
            isRunning = false;
            playerCameraPosition = Vector3.Zero;
            playerCameraRotation = Quaternion.Identity;
            playerCameraScale = Vector3.One;
            this.turnSpeed = turnSpeed;
            recordedPaths = new Pair<HologramPath?,float>[3] 
            {
                new Pair<HologramPath?,float>(null, 0),
                new Pair<HologramPath?, float>(null, 0),
                new Pair<HologramPath?,float>(null, 0)
            };
            selectedPath = 0;
            hologramRecording = false;
            hologramPlaying = false;
            player = null;
            closestObjectDistance = null;
            closestObject = null;
            crosshairColor = Color.Orange;
            weapons = new GameObject[3];
            movement = Movement.WALK;
            InitializeInput(new StreamingContext());
        }

        private void DownTemp(PressingActionArgs args)
        {
            Owner.LocalPosition = Owner.LocalPosition - 100 * (float)args.gameTime.ElapsedGameTime.TotalSeconds * Vector3.UnitY;
        }

        private void UpTemp(PressingActionArgs args)
        {
            Owner.LocalPosition = Owner.LocalPosition + 100 * (float)args.gameTime.ElapsedGameTime.TotalSeconds * Vector3.UnitY;
        }

        [OnDeserialized]
        private void InitializeInput(StreamingContext context)
        {
            // Bind actions to input.
            Input.BindActionPress(GameAction.MOVE_FORWARD, StartMoving);
            Input.BindActionContinuousPress(GameAction.MOVE_FORWARD, Move);
            Input.BindActionRelease(GameAction.MOVE_FORWARD, Stay);
            Input.BindActionPress(GameAction.MOVE_BACKWARD, StartMoving);
            Input.BindActionContinuousPress(GameAction.MOVE_BACKWARD, Move);
            Input.BindActionRelease(GameAction.MOVE_BACKWARD, Stay);
            Input.BindActionPress(GameAction.STRAFE_LEFT, StartMoving);
            Input.BindActionContinuousPress(GameAction.STRAFE_LEFT, Move);
            Input.BindActionRelease(GameAction.STRAFE_LEFT, Stay);
            Input.BindActionPress(GameAction.STRAFE_RIGHT, StartMoving);
            Input.BindActionContinuousPress(GameAction.STRAFE_RIGHT, Move);
            Input.BindActionRelease(GameAction.STRAFE_RIGHT, Stay);
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
            Input.BindActionPress(GameAction.SELECT_FIRST_HOLOGRAM, SelectHologramPath);
            Input.BindActionPress(GameAction.SELECT_SECOND_HOLOGRAM, SelectHologramPath);
            Input.BindActionPress(GameAction.SELECT_THIRD_HOLOGRAM, SelectHologramPath);
            Input.BindActionPress(GameAction.PREVIEW_HOLOGRAM, PreviewHologram);
            Input.BindActionContinuousPress(GameAction.GO_UP, UpTemp);
            Input.BindActionContinuousPress(GameAction.GO_DOWN, DownTemp);
            Input.BindMouseMovement(Turn);
        }
    }
}

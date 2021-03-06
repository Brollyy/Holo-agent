﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Engine.Utilities;
using System;
using System.Runtime.Serialization;

namespace Engine.Components
{
    public delegate void FinalizeHologramRecording(HologramPath path);

    [DataContract]
    public class HologramRecorder : Component
    {
        private FinalizeHologramRecording handler;
        private HologramPath path;
        private float sampleTime;
        private float time;

        private void StopRecording(PressedActionArgs args)
        {
            foreach (Pair<Pair<float, float?>, Pair<GameAction, bool>> action in path.Actions)
            {
                if (action.First.Second == null)
                {
                    action.First.Second = sampleTime * path.LocalPositions.Count + time;
                }
            }
            if (handler != null) handler(path);
            Owner.Scene.Destroy(Owner);
        }

        public override void Update(GameTime gameTime)
        {
            if (path.LocalPositions.Count < path.NumberOfSteps)
            {
                time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (time > sampleTime)
                {
                    time = 0.0f;
                    path.LocalPositions.Add(Owner.LocalPosition);
                    path.LocalRotations.Add(Owner.LocalEulerRotation.X);
                }
            }
            else
            {
                StopRecording(null);
            }
        }

        public override void Destroy()
        {
            Input.UnbindActionPress(GameAction.RECORD_HOLOGRAM, StopRecording);
            Input.UnbindActionPress(GameAction.MOVE_FORWARD, Move);
            Input.UnbindActionRelease(GameAction.MOVE_FORWARD, StopMoving);
            Input.UnbindActionPress(GameAction.MOVE_BACKWARD, Move);
            Input.UnbindActionRelease(GameAction.MOVE_BACKWARD, StopMoving);
            Input.UnbindActionPress(GameAction.STRAFE_LEFT, Move);
            Input.UnbindActionRelease(GameAction.STRAFE_LEFT, StopMoving);
            Input.UnbindActionPress(GameAction.STRAFE_RIGHT, Move);
            Input.UnbindActionRelease(GameAction.STRAFE_RIGHT, StopMoving);
            Input.UnbindActionPress(GameAction.CROUCH, Crouch);
            Input.UnbindActionRelease(GameAction.CROUCH, StopCrouching);
            Input.UnbindActionPress(GameAction.RUN, Run);
            Input.UnbindActionRelease(GameAction.RUN, StopRunning);
            Input.UnbindActionPress(GameAction.JUMP, Jump);
        }

        protected override void InitializeNewOwner(GameObject newOwner)
        {
            base.InitializeNewOwner(newOwner);
            path.StartGlobalPosition = Owner.GlobalPosition;
            path.StartGlobalRotation = Owner.GlobalRotation;
        }


        public HologramRecorder() : this(5.0f, 50, null)
        {

        }

        public HologramRecorder(float recordingTime, int numberOfSamples, FinalizeHologramRecording handler)
        {
            this.handler = handler;
            Input.BindActionPress(GameAction.RECORD_HOLOGRAM, StopRecording);
            Input.BindActionPress(GameAction.MOVE_FORWARD, Move);
            Input.BindActionRelease(GameAction.MOVE_FORWARD, StopMoving);
            Input.BindActionPress(GameAction.MOVE_BACKWARD, Move);
            Input.BindActionRelease(GameAction.MOVE_BACKWARD, StopMoving);
            Input.BindActionPress(GameAction.STRAFE_LEFT, Move);
            Input.BindActionRelease(GameAction.STRAFE_LEFT, StopMoving);
            Input.BindActionPress(GameAction.STRAFE_RIGHT, Move);
            Input.BindActionRelease(GameAction.STRAFE_RIGHT, StopMoving);
            Input.BindActionPress(GameAction.CROUCH, Crouch);
            Input.BindActionRelease(GameAction.CROUCH, StopCrouching);
            Input.BindActionPress(GameAction.RUN, Run);
            Input.BindActionRelease(GameAction.RUN, StopRunning);
            Input.BindActionPress(GameAction.JUMP, Jump);
            if (recordingTime > 0.0f) path.Duration = recordingTime;
            else path.Duration = 5.0f;
            time = 0.0f;
            path.NumberOfSteps = numberOfSamples;
            this.sampleTime = recordingTime / (float)(numberOfSamples);
            path.Actions = new List<Pair<Pair<float, float?>, Pair<GameAction, bool>>>();
            path.LocalPositions = new List<Vector3>();
            path.LocalRotations = new List<float>();
        }

        private void StopRunning(ReleasedActionArgs args)
        {
            int i = path.Actions.FindLastIndex(x => x.Second.First == GameAction.RUN);
            if (i >= 0) path.Actions[i].First.Second = path.LocalPositions.Count * sampleTime + time;
            else path.Actions.Add(new Pair<Pair<float, float?>, Pair<GameAction, bool>>(
                new Pair<float, float?>(0, path.LocalPositions.Count * sampleTime + time),
                new Pair<GameAction, bool>(GameAction.RUN, false)));
        }

        private void Run(PressedActionArgs args)
        {
            float timer = path.LocalPositions.Count * sampleTime + time;
            path.Actions.Add(new Pair<Pair<float, float?>, Pair<GameAction, bool>>(new Pair<float, float?>(timer, null), 
                                                                                   new Pair<GameAction, bool>(GameAction.RUN, false)));
        }

        private void StopCrouching(ReleasedActionArgs args)
        {
            int i = path.Actions.FindLastIndex(x => x.Second.First == GameAction.CROUCH);
            if (i >= 0) path.Actions[i].First.Second = path.LocalPositions.Count * sampleTime + time;
            else path.Actions.Add(new Pair<Pair<float, float?>, Pair<GameAction, bool>>(
                new Pair<float, float?>(0, path.LocalPositions.Count * sampleTime + time),
                new Pair<GameAction, bool>(GameAction.CROUCH, false)));
        }

        private void Crouch(PressedActionArgs args)
        {
            float timer = path.LocalPositions.Count * sampleTime + time;
            path.Actions.Add(new Pair<Pair<float, float?>, Pair<GameAction, bool>>(new Pair<float, float?>(timer, null),
                                                                                   new Pair<GameAction, bool>(GameAction.CROUCH, false)));
        }

        private void StopMoving(ReleasedActionArgs args)
        {
            int i = path.Actions.FindLastIndex(x => x.Second.First == args.action);
            if (i >= 0) path.Actions[i].First.Second = path.LocalPositions.Count * sampleTime + time;
            else path.Actions.Add(new Pair<Pair<float, float?>, Pair<GameAction, bool>>(
                new Pair<float, float?>(0, path.LocalPositions.Count * sampleTime + time),
                new Pair<GameAction, bool>(args.action, false)));
        }

        private void Move(PressedActionArgs args)
        {
            float timer = path.LocalPositions.Count * sampleTime + time;
            path.Actions.Add(new Pair<Pair<float, float?>, Pair<GameAction, bool>>(new Pair<float, float?>(timer, null),
                                                                                   new Pair<GameAction, bool>(args.action, false)));
        }

        private void Jump(PressedActionArgs args)
        {
            float timer = path.LocalPositions.Count * sampleTime + time;
            path.Actions.Add(new Pair<Pair<float, float?>, Pair<GameAction, bool>>(new Pair<float, float?>(timer, null),
                                                                                   new Pair<GameAction, bool>(args.action, false)));
        }
    }

    public delegate void FinalizeHologramPlayback();

    [DataContract]
    public class HologramPlayback : Component
    {
        private FinalizeHologramPlayback handler;

        private HologramPath path;
        private float durationOfStep;
        private float overallTime;
        private float time;
        private int index;

        private bool isCrouching;
        private bool isRunning;

        public bool IsCrouching { get { return isCrouching; } }

        public void StopPlayback(PressedActionArgs args)
        {
            if (handler != null) handler();
            Owner.Scene.Destroy(Owner);
        }

        private void SwitchMovement(bool isCrouching, bool isRunning)
        {
            AnimationController contr = Owner.GetComponent<AnimationController>();
            string mov1 = (this.isCrouching ? "crouch" : (this.isRunning ? "run" : "walk"));
            this.isCrouching = isCrouching; this.isRunning = isRunning;
            string mov2 = (isCrouching ? "crouch" : (isRunning ? "run" : "walk"));

            if (contr.StopAnimation(mov1 + "Forward", 0.2f)) contr.PlayAnimation(mov2 + "Forward", 1, 0.2f);
            if (contr.StopAnimation(mov1 + "Backward", 0.2f)) contr.PlayAnimation(mov2 + "Backward", 1, 0.2f);
            if (contr.StopAnimation(mov1 + "Left", 0.2f)) contr.PlayAnimation(mov2 + "Left", 1, 0.2f);
            if (contr.StopAnimation(mov1 + "Right", 0.2f)) contr.PlayAnimation(mov2 + "Right", 1, 0.2f);
            if (isCrouching) contr.SetBindPose("crouchForward", 0.2f);
            else contr.SetBindPose("death", 0.2f);
        }

        public override void Update(GameTime gameTime)
        {
            overallTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (overallTime < path.Duration)
            {
                List<Pair<Pair<float,float?>,Pair<GameAction,bool>>> currentActions = path.Actions.FindAll(x => (x.First.First < overallTime && x.First.Second > overallTime));
                foreach(Pair<Pair<float, float?>, Pair<GameAction, bool>> action in path.Actions)
                {
                    if(action.First.First < overallTime && action.First.Second > overallTime && !action.Second.Second)
                    {
                        action.Second.Second = true;
                        AnimationController contr = Owner.GetComponent<AnimationController>();
                        if (contr != null)
                        {
                            string mov = (isCrouching ? "crouch" : (isRunning ? "run" : "walk"));
                            switch (action.Second.First)
                            {
                                case GameAction.CROUCH: SwitchMovement(true, isRunning); break;
                                case GameAction.JUMP: if(!isCrouching) contr.PlayAnimation("jump", 100, 0.2f); break;
                                case GameAction.RUN: SwitchMovement(isCrouching, true); break;
                                case GameAction.MOVE_FORWARD: contr.PlayAnimation(mov + "Forward", 1, 0.2f); break;
                                case GameAction.MOVE_BACKWARD: contr.PlayAnimation(mov + "Backward", 1, 0.2f); break;
                                case GameAction.STRAFE_LEFT: contr.PlayAnimation(mov + "Left", 1, 0.2f); break;
                                case GameAction.STRAFE_RIGHT: contr.PlayAnimation(mov + "Right", 1, 0.2f); break;
                            }
                        }
                    }

                    if(action.First.Second < overallTime && action.Second.Second)
                    {
                        action.Second.Second = false;
                        AnimationController contr = Owner.GetComponent<AnimationController>();
                        if (contr != null)
                        {
                            string mov = (isCrouching ? "crouch" : (isRunning ? "run" : "walk"));
                            switch (action.Second.First)
                            {
                                case GameAction.CROUCH: SwitchMovement(false, isRunning); break;
                                case GameAction.JUMP: break;
                                case GameAction.RUN: SwitchMovement(isCrouching, false); break;
                                case GameAction.MOVE_FORWARD: contr.StopAnimation(mov + "Forward", 0.2f); break;
                                case GameAction.MOVE_BACKWARD: contr.StopAnimation(mov + "Backward", 0.2f); break;
                                case GameAction.STRAFE_LEFT: contr.StopAnimation(mov + "Left", 0.2f); break;
                                case GameAction.STRAFE_RIGHT: contr.StopAnimation(mov + "Right", 0.2f); break;
                            }
                        }
                    }
                }

                if (index < path.LocalPositions.Count - 1)
                {
                    time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Owner.LocalPosition = Vector3.LerpPrecise(path.LocalPositions[index], path.LocalPositions[index + 1], time / durationOfStep);
                    float rot1 = path.LocalRotations[index];
                    float rot2 = path.LocalRotations[index + 1];
                    if(Math.Abs(rot1 - rot2) > 180)
                    {
                        if (rot2 < 180) rot2 += 360;
                        else rot2 -= 360;
                    }
                    float newRot = MathHelper.Lerp(rot1, rot2, time / durationOfStep);
                    if (newRot > 360) newRot -= 360;
                    if (newRot < 0) newRot += 360;
                    Owner.LocalEulerRotation = Vector3.UnitX * newRot;
                    if (time > durationOfStep)
                    {
                        time = 0.0f;
                        index++;
                    }
                }
            }
            else
            {
                StopPlayback(null);
            }
        }

        public override void Destroy()
        {
            Input.UnbindActionPress(GameAction.PLAY_HOLOGRAM, StopPlayback);
        }

        protected override void InitializeNewOwner(GameObject newOwner)
        {
            base.InitializeNewOwner(newOwner);
            Owner.GlobalPosition = path.StartGlobalPosition;
            Owner.GlobalRotation = path.StartGlobalRotation;
        }

        public HologramPlayback(HologramPath path, FinalizeHologramPlayback handler = null)
        {
            isRunning = false; isCrouching = false;
            Input.BindActionPress(GameAction.PLAY_HOLOGRAM, StopPlayback);
            this.path = new HologramPath(path);
            index = 0;
            time = 0.0f;
            overallTime = 0.0f;
            durationOfStep = path.Duration / (float)path.NumberOfSteps;
            this.handler = handler;
        }
    }

    [DataContract]
    public struct HologramPath
    {
        [DataMember]
        public float Duration;
        [DataMember]
        public int NumberOfSteps;
        [DataMember]
        public Vector3 StartGlobalPosition;
        [DataMember]
        public Quaternion StartGlobalRotation;
        [DataMember]
        public List<Vector3> LocalPositions;
        [DataMember]
        public List<float> LocalRotations;
        [DataMember]
        public List<Pair<Pair<float,float?>,Pair<GameAction,bool>>> Actions;

        public HologramPath(HologramPath other)
        {
            Duration = other.Duration;
            NumberOfSteps = other.NumberOfSteps;
            StartGlobalPosition = other.StartGlobalPosition;
            StartGlobalRotation = other.StartGlobalRotation;
            LocalPositions = new List<Vector3>(other.LocalPositions);
            LocalRotations = new List<float>(other.LocalRotations);
            Actions = new List<Pair<Pair<float, float?>, Pair<GameAction, bool>>>();
            foreach(Pair<Pair<float, float?>, Pair<GameAction, bool>> action in other.Actions)
            {
                Pair<Pair<float, float?>, Pair<GameAction, bool>> newAction = new Pair<Pair<float, float?>, Pair<GameAction, bool>>(
                    new Pair<float, float?>(action.First.First, action.First.Second), 
                    new Pair<GameAction, bool>(action.Second.First, action.Second.Second));

                Actions.Add(newAction);
            }
        }


    }
}

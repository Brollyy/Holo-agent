using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Engine.Utilities;
using System;

namespace Engine.Components
{
    public delegate void FinalizeHologramRecording(HologramPath path);

    public class HologramRecorder : Component
    {
        private FinalizeHologramRecording handler;
        private HologramPath path;
        private float sampleTime;
        private float time;

        private void StopRecording(PressedActionArgs args)
        {
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
            if (recordingTime > 0.0f) path.Duration = recordingTime;
            else path.Duration = 5.0f;
            time = 0.0f;
            path.NumberOfSteps = numberOfSamples;
            this.sampleTime = recordingTime / (float)(numberOfSamples);
            path.Actions = new List<Pair<Pair<float, float?>, GameAction>>();
            path.LocalPositions = new List<Vector3>();
            path.LocalRotations = new List<float>();
        }

        private void StopRunning(ReleasedActionArgs args)
        {
            int i = path.Actions.FindLastIndex(x => x.Second == GameAction.RUN);
            path.Actions[i].First.Second = path.LocalPositions.Count * sampleTime + time;
        }

        private void Run(PressedActionArgs args)
        {
            float timer = path.LocalPositions.Count * sampleTime + time;
            path.Actions.Add(new Pair<Pair<float, float?>, GameAction>(new Pair<float, float?>(timer, null), GameAction.RUN));
        }

        private void StopCrouching(ReleasedActionArgs args)
        {
            int i = path.Actions.FindLastIndex(x => x.Second == GameAction.CROUCH);
            path.Actions[i].First.Second = path.LocalPositions.Count * sampleTime + time;
        }

        private void Crouch(PressedActionArgs args)
        {
            float timer = path.LocalPositions.Count * sampleTime + time;
            path.Actions.Add(new Pair<Pair<float, float?>, GameAction>(new Pair<float, float?>(timer, null), GameAction.CROUCH));
        }

        private void StopMoving(ReleasedActionArgs args)
        {
            int i = path.Actions.FindLastIndex(x => x.Second == GameAction.MOVE_FORWARD);
            path.Actions[i].First.Second = path.LocalPositions.Count * sampleTime + time;
        }

        private void Move(PressedActionArgs args)
        {
            float timer = path.LocalPositions.Count * sampleTime + time;
            path.Actions.Add(new Pair<Pair<float, float?>, GameAction>(new Pair<float, float?>(timer, null), GameAction.MOVE_FORWARD));
        }
    }

    public delegate void FinalizeHologramPlayback();

    public class HologramPlayback : Component
    {
        private FinalizeHologramPlayback handler;

        private HologramPath path;
        private float durationOfStep;
        private float overallTime;
        private float time;
        private int index;
        private string currentAnimation = "idle";

        private void StopPlayback(PressedActionArgs args)
        {
            if (handler != null) handler();
            Owner.Scene.Destroy(Owner);
        }

        public override void Update(GameTime gameTime)
        {
            overallTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (overallTime < path.Duration)
            {
                Pair<Pair<float,float?>,GameAction> action = path.Actions.Find(x => (x.First.First < overallTime && x.First.Second > overallTime));
                string animationName = "idle";
                if (action != null)
                {
                    switch (action.Second)
                    {
                        case GameAction.CROUCH: break;
                        case GameAction.JUMP: break;
                        case GameAction.RUN: animationName = "run"; break;
                        case GameAction.MOVE_FORWARD: animationName = "run"; break;
                    }
                }

                if(!animationName.Equals(currentAnimation))
                {
                    currentAnimation = animationName;
                    AnimationController contr = Owner.GetComponent<AnimationController>();
                    if (contr != null) contr.Blend(currentAnimation, 0.2f);
                }

                if (index < path.LocalPositions.Count - 1)
                {
                    time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Owner.LocalPosition = Vector3.LerpPrecise(path.LocalPositions[index], path.LocalPositions[index + 1], time / durationOfStep);
                    Owner.LocalEulerRotation = Vector3.UnitX*MathHelper.Lerp(path.LocalRotations[index], path.LocalRotations[index + 1], time / durationOfStep);
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
            Input.BindActionPress(GameAction.PLAY_HOLOGRAM, StopPlayback);
            this.path = path;
            index = 0;
            time = 0.0f;
            overallTime = 0.0f;
            durationOfStep = path.Duration / (float)path.NumberOfSteps;
            this.handler = handler;
        }
    }

    public struct HologramPath
    {
        public float Duration;
        public int NumberOfSteps;
        public Vector3 StartGlobalPosition;
        public Quaternion StartGlobalRotation;
        public List<Vector3> LocalPositions;
        public List<float> LocalRotations;
        public List<Pair<Pair<float,float?>,GameAction>> Actions;
    }
}

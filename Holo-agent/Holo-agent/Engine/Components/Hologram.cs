using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Engine.Utilities;

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
            if (recordingTime > 0.0f) path.Duration = recordingTime;
            else path.Duration = 5.0f;
            time = 0.0f;
            path.NumberOfSteps = numberOfSamples;
            this.sampleTime = recordingTime / (float)(numberOfSamples);
            path.Actions = new List<Pair<Pair<float, float?>, GameAction>>();
            path.LocalPositions = new List<Vector3>();
            path.LocalRotations = new List<float>();
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

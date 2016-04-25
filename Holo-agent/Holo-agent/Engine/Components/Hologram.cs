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
            if (path.GlobalPositions.Count < path.NumberOfSteps)
            {
                time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (time > sampleTime)
                {
                    time = 0.0f;
                    path.GlobalPositions.Add(Owner.GlobalPosition);
                    path.GlobalRotations.Add(Owner.GlobalRotation);
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
            path.GlobalPositions = new List<Vector3>();
            path.GlobalRotations = new List<Quaternion>();
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
                if (index < path.GlobalPositions.Count - 1)
                {
                    time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Owner.GlobalPosition = Vector3.LerpPrecise(path.GlobalPositions[index], path.GlobalPositions[index + 1], time / durationOfStep);
                    Owner.GlobalRotation = Quaternion.Lerp(path.GlobalRotations[index], path.GlobalRotations[index + 1], time / durationOfStep);
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
        public List<Vector3> GlobalPositions;
        public List<Quaternion> GlobalRotations;
        public List<Pair<Pair<float,float?>,GameAction>> Actions;
    }
}

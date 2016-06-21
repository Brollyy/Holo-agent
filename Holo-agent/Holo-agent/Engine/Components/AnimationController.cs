using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animation;
using Engine.Utilities;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    public class AnimationController : Component
    {
        [DataMember]
        private Dictionary<string, int> clipIndex = new Dictionary<string, int>();
        [DataMember]
        private List<Pair<AnimationClip, bool>> clips = new List<Pair<AnimationClip, bool>>();
        private Dictionary<string, Pair<Pair<AnimationPlayer, float>, Pair<float, float>>> blendingInAnimations = new Dictionary<string, Pair<Pair<AnimationPlayer, float>, Pair<float, float>>>();
        private Dictionary<string, Pair<AnimationPlayer, float>> playingAnimations = new Dictionary<string, Pair<AnimationPlayer, float>>();
        private Dictionary<string, Pair<Pair<AnimationPlayer, float>, Pair<float, float>>> blendingOutAnimations = new Dictionary<string, Pair<Pair<AnimationPlayer, float>, Pair<float, float>>>();
        [DataMember]
        private int bindPoseIndex = -1;
        private AnimationPlayer BIND_POSE = null;
        private AnimationPlayer currentAnimation = null;

        public bool IsPlayingAnimations()
        {
            return (blendingInAnimations.Count > 0 || playingAnimations.Count > 0 || blendingOutAnimations.Count > 0);
        }

        public bool BindAnimation(string name, AnimationClip clip, bool looping = false)
        {
            if (clip == null)
            {
                clipIndex.Add(name, -1);
                return true;
            }

            if(!clips.Exists(x => x.Equals(clip)))
            {
                clips.Add(new Pair<AnimationClip,bool>(clip, looping));
                clipIndex.Add(name, clips.Count - 1);
                return true;
            }
            return false;
        }

        public void SetBindPose(AnimationClip clip)
        {
            if (clip == null)
            {
                BIND_POSE = null;
                bindPoseIndex = -1;
            }
            else
            {
                BIND_POSE = new AnimationPlayer(clip, Owner.GetComponent<MeshInstance>().Model);
                bindPoseIndex = clips.FindIndex(x => x.First == clip);
            }
        }

        public void SetBindPose(string name, float blendOutTime = 0, float positionNormalized = 0)
        {
            int index = clipIndex[name];
            if(index >= 0)
            {
                if (BIND_POSE != null) blendingOutAnimations["bind"] = new Pair<Pair<AnimationPlayer, float>, Pair<float, float>>(
                     new Pair<AnimationPlayer, float>(BIND_POSE, 1), new Pair<float, float>(0, blendOutTime));
                BIND_POSE = new AnimationPlayer(clips[index].First, Owner.GetComponent<MeshInstance>().Model);
                BIND_POSE.Position = positionNormalized * BIND_POSE.Duration;
                bindPoseIndex = index;
            }
        }

        public void PlayAnimation(string name, float weight = 1, float blendingTime = 0)
        {
            int animIndex = clipIndex[name];
            if (animIndex >= 0)
            {
                AnimationPlayer animPlayer = new AnimationPlayer(clips[animIndex].First, Owner.GetComponent<MeshInstance>().Model);
                animPlayer.Looping = clips[animIndex].Second;
                blendingInAnimations[name] = new Pair<Pair<AnimationPlayer,float>, Pair<float,float>>(new Pair<AnimationPlayer, float>(animPlayer, weight),
                                                                                                      new Pair<float, float>(0,blendingTime));
            }
        }

        public bool StopAnimation(string name, float blendOutTime = 0)
        {
            if(playingAnimations.ContainsKey(name))
            {
                blendingOutAnimations[name] = new Pair<Pair<AnimationPlayer, float>, Pair<float, float>>(playingAnimations[name], new Pair<float, float>(0, blendOutTime));
                playingAnimations.Remove(name);
                return true;
            }
            if (blendingInAnimations.ContainsKey(name))
            {
                float t = blendOutTime * (1.0f - blendingInAnimations[name].Second.First / blendingInAnimations[name].Second.Second);
                blendingOutAnimations[name] = new Pair<Pair<AnimationPlayer, float>, Pair<float, float>>(blendingInAnimations[name].First, new Pair<float, float>(t, blendOutTime));
                blendingInAnimations.Remove(name);
                return true;
            }
            return false;
        }

        public void StopAllAnimations(float blendOutTime = 0)
        {
            List<string> copyBlendInKeys = new List<string>(blendingInAnimations.Keys);
            foreach(string name in copyBlendInKeys)
            {
                float t = blendOutTime * (1.0f - blendingInAnimations[name].Second.First / blendingInAnimations[name].Second.Second);
                blendingOutAnimations[name] = new Pair<Pair<AnimationPlayer, float>, Pair<float, float>>(blendingInAnimations[name].First, new Pair<float, float>(t, blendOutTime));
                blendingInAnimations.Remove(name);
            }

            List<string> copyKeys = new List<string>(playingAnimations.Keys);
            foreach (string name in copyKeys)
            {
                blendingOutAnimations[name] = new Pair<Pair<AnimationPlayer, float>, Pair<float, float>>(playingAnimations[name], new Pair<float, float>(0, blendOutTime));
                playingAnimations.Remove(name);
            }
        }

        public override void Update(GameTime gameTime)
        {
            List<string> copyBlendInKeys = new List<string>(blendingInAnimations.Keys);
            foreach (string animName in copyBlendInKeys)
            {
                Pair<Pair<AnimationPlayer, float>, Pair<float, float>> anim = blendingInAnimations[animName];
                anim.Second.First += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(anim.Second.First >= anim.Second.Second)
                {
                    playingAnimations[animName] = anim.First;
                    blendingInAnimations.Remove(animName);
                }
                else
                {
                    anim.First.First.Update(gameTime);
                }
            }

            List<string> copyKeys = new List<string>(playingAnimations.Keys);
            foreach (string animName in copyKeys)
            {
                Pair<AnimationPlayer, float> anim = playingAnimations[animName];
                anim.First.Update(gameTime);
                if (!anim.First.Looping && anim.First.Position >= anim.First.Duration)
                    playingAnimations.Remove(animName);
            }

            List<string> copyBlendOutKeys = new List<string>(blendingOutAnimations.Keys);
            foreach (string animName in copyBlendOutKeys)
            {
                Pair<Pair<AnimationPlayer, float>, Pair<float, float>> anim = blendingOutAnimations[animName];
                
                anim.Second.First += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (anim.Second.First >= anim.Second.Second) blendingOutAnimations.Remove(animName);
            }

            if (IsPlayingAnimations())
            {
                List<Pair<AnimationPlayer, float>> animations = new List<Pair<AnimationPlayer, float>>();

                foreach(Pair<Pair<AnimationPlayer, float>, Pair<float, float>> anim in blendingInAnimations.Values)
                {
                    float w = anim.First.Second * anim.Second.First / anim.Second.Second;
                    animations.Add(new Pair<AnimationPlayer, float>(anim.First.First, w));
                }

                foreach(Pair<AnimationPlayer, float> anim in playingAnimations.Values)
                {
                    animations.Add(anim);
                }

                foreach (Pair<Pair<AnimationPlayer, float>, Pair<float, float>> anim in blendingOutAnimations.Values)
                {
                    float w = anim.First.Second * (1.0f - anim.Second.First / anim.Second.Second);
                    animations.Add(new Pair<AnimationPlayer, float>(anim.First.First, w));
                }

                AnimationPlayer lerpedPlayer = AnimationPlayer.LerpAndSet(animations);

                if (playingAnimations.Count == 0)
                {
                    float t = 0.0f;
                    float sumW = 0;
                    foreach (string animName in blendingInAnimations.Keys)
                    {
                        Pair<Pair<AnimationPlayer, float>, Pair<float, float>> anim = blendingInAnimations[animName];
                        sumW += anim.First.Second;
                        t += anim.First.Second * anim.Second.First / anim.Second.Second;
                    }
                    foreach (string animName in blendingOutAnimations.Keys)
                    {
                        Pair<Pair<AnimationPlayer, float>, Pair<float, float>> anim = blendingOutAnimations[animName];
                        sumW += anim.First.Second;
                        t += anim.First.Second * (1.0f - anim.Second.First / anim.Second.Second);
                    }
                    currentAnimation = AnimationPlayer.LerpAndSet(BIND_POSE, lerpedPlayer, t / sumW);
                }
                else currentAnimation = lerpedPlayer;
            }

            else if(BIND_POSE != null)
            {
                AnimationPlayer.Set(BIND_POSE);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animation;
using Engine.Utilities;
using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class AnimationController : Component
    {
        private Dictionary<string, int> clipIndex = new Dictionary<string, int>();
        private List<Pair<AnimationClip, bool>> clips = new List<Pair<AnimationClip, bool>>();
        private float blendingTime = 0.0f;
        private float blendingT = 0.0f;
        private bool isBlending = false;
        private AnimationPlayer animPlayer = null, blendAnimPlayer = null;
        private AnimationPlayer BIND_POSE = null;

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
            bool s = animPlayer == BIND_POSE;
            if (clip == null) BIND_POSE = null;
            else BIND_POSE = new AnimationPlayer(clip, Owner.GetComponent<MeshInstance>().Model);
            if (s) animPlayer = BIND_POSE;
        }

        public void Blend(string name, float time)
        {
            if(isBlending)
            {
                animPlayer = blendAnimPlayer;
            }

            int blendingIndex = clipIndex[name];
            if (blendingIndex >= 0)
            {
                blendAnimPlayer = new AnimationPlayer(clips[blendingIndex].First, Owner.GetComponent<MeshInstance>().Model);
                blendAnimPlayer.Looping = clips[blendingIndex].Second;
            }
            else blendAnimPlayer = BIND_POSE;
            blendingTime = time;
            blendingT = 0.0f;
            isBlending = true;
        }

        public void PlayAnimation(string name)
        {
            int animIndex = clipIndex[name];
            if (animIndex >= 0)
            {
                animPlayer = new AnimationPlayer(clips[animIndex].First, Owner.GetComponent<MeshInstance>().Model);
                animPlayer.Looping = clips[animIndex].Second;
            }
            else
            {
                animPlayer = BIND_POSE;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (animPlayer != null) animPlayer.Update(gameTime);
            if (isBlending)
            {
                if (blendAnimPlayer != null) blendAnimPlayer.Update(gameTime);
                blendingT += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(blendingT < blendingTime)
                {
                    if (animPlayer != null && blendAnimPlayer != null)
                    {
                        AnimationPlayer.LerpAndSet(animPlayer, blendAnimPlayer, blendingT / blendingTime);
                    }
                }
                else
                {
                    isBlending = false;
                    animPlayer = blendAnimPlayer;
                    blendAnimPlayer = null;
                }
            }
        }
    }
}

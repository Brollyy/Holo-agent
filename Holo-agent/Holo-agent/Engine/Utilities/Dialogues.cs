using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utilities
{
    public static class Dialogues
    {
        private static List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>> dialogueQuery = 
            new List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>>();

        public static Color DefaultColor = Color.Orange;

        public static void PlayDialogue(string message, float timeOffset = 0, Color? color = null, bool forceText = false)
        {
            PlayDialogue(message, timeOffset, float.PositiveInfinity, null, color, forceText);
        }

        public static void PlayDialogue(string message, float timeOffset, float duration, Color? color = null, bool forceText = false)
        {
            PlayDialogue(message, timeOffset, duration, null, color, forceText);
        }

        public static void PlayDialogue(SoundEffect messageAudio, float timeOffset = 0)
        {
            if (messageAudio == null) return;
            PlayDialogue(null, timeOffset, (float)messageAudio.Duration.TotalSeconds, messageAudio, null, false);
        }

        public static void PlayDialogue(string message, SoundEffect messageAudio, Color? color = null, bool forceText = false)
        {
            if (messageAudio == null) return;
            PlayDialogue(message, 0, (float)messageAudio.Duration.TotalSeconds, messageAudio, color, forceText);
        }

        public static void PlayDialogue(string message, float timeOffset, SoundEffect messageAudio, Color? color = null, bool forceText = false)
        {
            if (messageAudio == null) return;
            PlayDialogue(message, timeOffset, (float)messageAudio.Duration.TotalSeconds, messageAudio, color, forceText);
        }

        public static void PlayDialogue(string message, float timeOffset, float duration, SoundEffect messageAudio, Color? color, bool forceText)
        {
            if (message == null && messageAudio == null) return;
            dialogueQuery.Add(new Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>(
                new Pair<Pair<string, Color>, bool>(new Pair<string, Color>(message, (color.HasValue ? color.Value : DefaultColor)), forceText), 
                new Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>(
                    new Pair<float, float>(-timeOffset, duration), 
                    new Pair<SoundEffectInstance, string>((messageAudio == null ? null : messageAudio.CreateInstance()), (messageAudio == null ? null : messageAudio.Name)))));
        }

        public static void StopDialogue(string message)
        {
            List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>> dialoguesCopy =
                new List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>>(dialogueQuery);
            foreach(Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>> dialogue in dialoguesCopy)
            {
                if(dialogue.First.First.First == message)
                {
                    dialogue.Second.Second.First.Stop();
                    dialogueQuery.Remove(dialogue);
                }
            }
        }

        public static void StopDialogue(SoundEffect messageAudio)
        {
            List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>> dialoguesCopy =
                new List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>>(dialogueQuery);
            foreach (Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>> dialogue in dialoguesCopy)
            {
                if (dialogue.Second.Second.Second == messageAudio.Name)
                {
                    dialogue.Second.Second.First.Stop();
                    dialogueQuery.Remove(dialogue);
                }
            }
        }

        public static void StopCurrentDialogue()
        {
            Pair<string, bool> message = new Pair<string, bool>(null, false);
            foreach (Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>> dialogue in dialogueQuery)
            {
                if (dialogue.Second.First.First >= 0.0f)
                {
                    if (dialogue.First != null)
                    {
                        if (message.First == null || (!message.Second && dialogue.First.Second))
                        {
                            message.First = dialogue.First.First.First;
                            message.Second = dialogue.First.Second;
                        }
                    }
                }
            }
            if (message.First != null)
            {
                StopDialogue(message.First);
                return;
            }

            List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>> dialoguesCopy =
                new List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>>(dialogueQuery);
            foreach (Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>> dialogue in dialoguesCopy)
            {
                if (dialogue.Second.Second.First.State == SoundState.Playing)
                {
                    dialogue.Second.Second.First.Stop();
                    dialogueQuery.Remove(dialogue);
                    break;
                }
            }
        }

        public static void Draw(ref SpriteBatch spriteBatch, SpriteFont font, Point size, GameTime gameTime)
        {
            Pair<Pair<string, Color>,bool> message = new Pair<Pair<string, Color>, bool>(new Pair<string, Color>(null, Color.White), false);
            List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>> dialogueCopy = new List<Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>>>(dialogueQuery);
            foreach(Pair<Pair<Pair<string, Color>, bool>, Pair<Pair<float, float>, Pair<SoundEffectInstance, string>>> dialogue in dialogueCopy)
            {
                dialogue.Second.First.First += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if(dialogue.Second.First.First > dialogue.Second.First.Second)
                {
                    dialogueQuery.Remove(dialogue);
                }
                else if(dialogue.Second.First.First >= 0.0f)
                {
                    if(dialogue.Second.Second.First != null && !(dialogue.Second.Second.First.State == SoundState.Playing))
                    {
                        dialogue.Second.Second.First.Play();
                    }

                    if (dialogue.First != null)
                    {
                        if (message.First.First == null || (!message.Second && dialogue.First.Second))
                        {
                            message.First.First = dialogue.First.First.First;
                            message.First.Second = dialogue.First.First.Second;
                            message.Second = dialogue.First.Second;
                        }
                    }
                }
            }

            if(message.First.First != null)
            {
                Vector2 messageSize = 0.2f * font.MeasureString(message.First.First);
                spriteBatch.DrawString(font, message.First.First, new Vector2(size.X / 2 - messageSize.X / 2, size.Y * 0.85f - messageSize.Y / 2), 
                                       message.First.Second, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }
        }
    }
}

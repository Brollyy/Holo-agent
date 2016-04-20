using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Engine.Utilities;

namespace Engine
{
    public delegate void ProcessInputAction(ActionArgs args);
    public delegate void ProcessPressedAction(PressedActionArgs args);
    public delegate void ProcessReleasedAction(ReleasedActionArgs args);
    public delegate void ProcessPressingAction(PressingActionArgs args);
    public delegate void ProcessMouseMove(float xMove, float yMove, GameTime gameTime);
    public static class Input
    {
        private static Dictionary<GameAction, Pair<bool, float>> actionStates;
        private static int inversionFactorX, inversionFactorY;
        private static Dictionary<GameAction, InputSource> bindings;
        private static Dictionary<GameAction, List<ProcessPressedAction>> pressedDelegates;
        private static Dictionary<GameAction, List<ProcessReleasedAction>> releasedDelegates;
        private static Dictionary<GameAction, List<ProcessPressingAction>> pressingDelegates;
        private static List<ProcessMouseMove> mouseDelegates;
        /// <summary>
        /// Initializes controls.
        /// </summary>
        public static void Initialize(/*Load bindings from file*/)
        {
            bindings = new Dictionary<GameAction, InputSource>();
            bindings.Add(GameAction.MOVE_FORWARD, new KeyboardInputSource(Keys.W));
            bindings.Add(GameAction.MOVE_BACKWARD, new KeyboardInputSource(Keys.S));
            bindings.Add(GameAction.STRAFE_LEFT, new KeyboardInputSource(Keys.A));
            bindings.Add(GameAction.STRAFE_RIGHT, new KeyboardInputSource(Keys.D));
            bindings.Add(GameAction.RUN, new KeyboardInputSource(Keys.LeftShift));
            bindings.Add(GameAction.JUMP, new KeyboardInputSource(Keys.Space));
            bindings.Add(GameAction.CROUCH, new KeyboardInputSource(Keys.C));
            bindings.Add(GameAction.INTERACT, new KeyboardInputSource(Keys.E));
            bindings.Add(GameAction.FIRE, new MouseInputSource(MouseButtons.Left));
            bindings.Add(GameAction.ZOOM, new MouseInputSource(MouseButtons.Right));
            pressedDelegates = new Dictionary<GameAction, List<ProcessPressedAction>>();
            releasedDelegates = new Dictionary<GameAction, List<ProcessReleasedAction>>();
            pressingDelegates = new Dictionary<GameAction, List<ProcessPressingAction>>();
            actionStates = new Dictionary<GameAction, Pair<bool, float>>();
            foreach(GameAction ga in bindings.Keys)
            {
                pressedDelegates.Add(ga, new List<ProcessPressedAction>());
                releasedDelegates.Add(ga, new List<ProcessReleasedAction>());
                pressingDelegates.Add(ga, new List<ProcessPressingAction>());
                actionStates.Add(ga, new Pair<bool, float>(false, 0.0f));
            }
            mouseDelegates = new List<ProcessMouseMove>();

            inversionFactorX = 1;
            inversionFactorY = 1;
        }
        /// <summary>
        /// Gets keyboard and mouse states.
        /// </summary>
        public static void Update(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            foreach(GameAction ga in bindings.Keys)
            {
                InputSource source = bindings[ga];

                bool pressed = false;
                if (source is KeyboardInputSource)
                {
                    pressed = keyState[(source as KeyboardInputSource).key] == KeyState.Down;
                }
                else if(source is MouseInputSource)
                {
                    MouseButtons button = (source as MouseInputSource).button;
                    switch(button)
                    {
                        case MouseButtons.Left: pressed = (mouseState.LeftButton == ButtonState.Pressed); break;
                        case MouseButtons.Right: pressed = (mouseState.RightButton == ButtonState.Pressed); break;
                        case MouseButtons.Middle: pressed = (mouseState.MiddleButton == ButtonState.Pressed); break;
                    }
                }

                if(pressed == actionStates[ga].First)
                {
                    actionStates[ga].Second = actionStates[ga].Second + gameTime.ElapsedGameTime.Seconds;
                    if (pressed)
                    {
                        PressingActionArgs args = new PressingActionArgs(actionStates[ga].Second);
                        args.gameTime = gameTime;
                        foreach (ProcessPressingAction proc in pressingDelegates[ga])
                        {
                            proc(args);
                        }
                    }
                }
                else
                {
                    float time = actionStates[ga].Second;
                    actionStates[ga].Second = 0.0f;
                    actionStates[ga].First = pressed;

                    if(pressed)
                    {
                        PressedActionArgs args = new PressedActionArgs(time);
                        args.gameTime = gameTime;
                        foreach(ProcessPressedAction proc in pressedDelegates[ga])
                        {
                            proc(args);
                        }
                    }
                    else
                    {
                        ReleasedActionArgs args = new ReleasedActionArgs(time);
                        args.gameTime = gameTime;
                        foreach(ProcessReleasedAction proc in releasedDelegates[ga])
                        {
                            proc(args);
                        }
                    }
                }
            }

            float xMove = (float)(mouseState.X - graphics.PreferredBackBufferWidth / 2) / (float)graphics.PreferredBackBufferWidth * inversionFactorX;
            float yMove = (float)(mouseState.Y - graphics.PreferredBackBufferHeight / 2) / (float)graphics.PreferredBackBufferHeight * inversionFactorY;

            if(xMove*xMove + yMove*yMove > 0.0000001f)
            {
                foreach(ProcessMouseMove proc in mouseDelegates)
                {
                    proc(xMove, yMove, gameTime);
                }
            }
            Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
        }
   
        public static void BindAction(GameAction action, ProcessInputAction processFunction)
        {
            if(pressedDelegates.ContainsKey(action))
            {
                pressedDelegates[action].Add(new ProcessPressedAction(processFunction));
            }

            if(releasedDelegates.ContainsKey(action))
            {
                releasedDelegates[action].Add(new ProcessReleasedAction(processFunction));
            }

            if(pressingDelegates.ContainsKey(action))
            {
                pressingDelegates[action].Add(new ProcessPressingAction(processFunction));
            }
        }

        public static void BindActionPress(GameAction action, ProcessPressedAction processFunction)
        {
            if (pressedDelegates.ContainsKey(action))
            {
                pressedDelegates[action].Add(new ProcessPressedAction(processFunction));
            }
        }

        public static void BindActionRelease(GameAction action, ProcessReleasedAction processFunction)
        {
            if (releasedDelegates.ContainsKey(action))
            {
                releasedDelegates[action].Add(new ProcessReleasedAction(processFunction));
            }
        }

        public static void BindActionContinuousPress(GameAction action, ProcessPressingAction processFunction)
        {
            if (pressingDelegates.ContainsKey(action))
            {
                pressingDelegates[action].Add(new ProcessPressingAction(processFunction));
            }
        }

        public static void BindMouseMovement(ProcessMouseMove processFunction)
        {
            mouseDelegates.Add(processFunction);
        }

        /// <summary>
        /// Allows to invert mouse axis.
        /// Usage example: Input.InvertAxis('Y');
        /// </summary>
        public static void InvertAxis(char axis)
        {
            if (axis.Equals('X'))
            {
                if (inversionFactorX.Equals(1))
                    inversionFactorX = -1;
                else
                    inversionFactorX = 1;
            }
            if (axis.Equals('Y'))
            {
                if (inversionFactorY.Equals(1))
                    inversionFactorY = -1;
                else
                    inversionFactorY = 1;
            }
        }
    }
    public enum MouseButtons
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }

    public enum GameAction
    {
        MOVE_FORWARD,
        MOVE_BACKWARD,
        STRAFE_LEFT,
        STRAFE_RIGHT,
        RUN,
        JUMP,
        CROUCH,
        INTERACT,
        FIRE,
        ZOOM,
        RECORD_HOLOGRAM,
        START_HOLOGRAM
    }

    internal abstract class InputSource
    { 
    }

    internal class KeyboardInputSource : InputSource
    {
        public Keys key;

        public KeyboardInputSource(Keys key)
        {
            this.key = key;
        }
    }

    internal class MouseInputSource : InputSource
    {
        public MouseButtons button;

        public MouseInputSource(MouseButtons button)
        {
            this.button = button;
        }
    }

    public abstract class ActionArgs
    {
        public GameTime gameTime;
    }

    public class PressedActionArgs : ActionArgs
    {
        public float TimeSinceLastPress;

        internal PressedActionArgs(float time)
        {
            TimeSinceLastPress = time;
        }
    }

    public class ReleasedActionArgs : ActionArgs
    {
        public float DurationOfPress;

        internal ReleasedActionArgs(float time)
        {
            DurationOfPress = time;
        }
    }

    public class PressingActionArgs : ActionArgs
    {
        public float DurationSincePress;

        internal PressingActionArgs(float time)
        {
            DurationSincePress = time;
        }
    }
}
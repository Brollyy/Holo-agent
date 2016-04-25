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
            bindings.Add(GameAction.RELOAD, new KeyboardInputSource(Keys.R));
            bindings.Add(GameAction.RECORD_HOLOGRAM, new KeyboardInputSource(Keys.Q));
            bindings.Add(GameAction.PLAY_HOLOGRAM, new KeyboardInputSource(Keys.F));
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
        /// Updates the input state and activates handlers binded to specific actions.
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
                        List<ProcessPressingAction> copyPressingDelegates = new List<ProcessPressingAction>(pressingDelegates[ga]);
                        foreach (ProcessPressingAction proc in copyPressingDelegates)
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
                        List<ProcessPressedAction> copyPressedDelegates = new List<ProcessPressedAction>(pressedDelegates[ga]);
                        foreach(ProcessPressedAction proc in copyPressedDelegates)
                        {
                            proc(args);
                        }
                    }
                    else
                    {
                        ReleasedActionArgs args = new ReleasedActionArgs(time);
                        args.gameTime = gameTime;
                        List<ProcessReleasedAction> copyReleasedDelegates = new List<ProcessReleasedAction>(releasedDelegates[ga]);
                        foreach(ProcessReleasedAction proc in copyReleasedDelegates)
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
                List<ProcessMouseMove> copyMouseDelegates = new List<ProcessMouseMove>(mouseDelegates);
                foreach(ProcessMouseMove proc in copyMouseDelegates)
                {
                    proc(xMove, yMove, gameTime);
                }
            }
            Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
        }

        public static void BindActionHandler(GameAction action, ProcessInputAction processFunction)
        {
            BindActionPress(action, new ProcessPressedAction(processFunction));
            BindActionRelease(action, new ProcessReleasedAction(processFunction));
            BindActionContinuousPress(action, new ProcessPressingAction(processFunction));
        }

        public static void BindActionPress(GameAction action, ProcessPressedAction processFunction)
        {
            if (pressedDelegates.ContainsKey(action) && !pressedDelegates[action].Contains(processFunction))
            {
                pressedDelegates[action].Add(processFunction);
            }
        }

        public static void BindActionRelease(GameAction action, ProcessReleasedAction processFunction)
        {
            if (releasedDelegates.ContainsKey(action) && !releasedDelegates[action].Contains(processFunction))
            {
                releasedDelegates[action].Add(processFunction);
            }
        }

        public static void BindActionContinuousPress(GameAction action, ProcessPressingAction processFunction)
        {
            if (pressingDelegates.ContainsKey(action) && !pressingDelegates[action].Contains(processFunction))
            {
                pressingDelegates[action].Add(processFunction);
            }
        }

        public static void BindMouseMovement(ProcessMouseMove processFunction)
        {
            if(!mouseDelegates.Contains(processFunction)) mouseDelegates.Add(processFunction);
        }

        public static void UnbindActionHandler(GameAction action, ProcessInputAction processFunction)
        {
            UnbindActionPress(action, new ProcessPressedAction(processFunction));
            UnbindActionRelease(action, new ProcessReleasedAction(processFunction));
            UnbindActionContinuousPress(action, new ProcessPressingAction(processFunction));
        }

        public static void UnbindActionPress(GameAction action, ProcessPressedAction processFunction)
        {
            if (pressedDelegates.ContainsKey(action) && pressedDelegates[action].Contains(processFunction))
            {
                pressedDelegates[action].Remove(processFunction);
            }
        }

        public static void UnbindActionRelease(GameAction action, ProcessReleasedAction processFunction)
        {
            if (releasedDelegates.ContainsKey(action))
            {
                releasedDelegates[action].Remove(processFunction);
            }
        }

        public static void UnbindActionContinuousPress(GameAction action, ProcessPressingAction processFunction)
        {
            if (pressingDelegates.ContainsKey(action))
            {
                pressingDelegates[action].Remove(processFunction);
            }
        }

        public static void UnbindMouseMovement(ProcessMouseMove processFunction)
        {
            mouseDelegates.Remove(processFunction);
        }

        /// <summary>
        /// Allows to invert X mouse axis.
        /// </summary>
        public static void InvertXAxis()
        {
            inversionFactorX = -inversionFactorX;
        }

        public static void InvertYAxis()
        {
            inversionFactorY = -inversionFactorY;
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
        RELOAD,
        RECORD_HOLOGRAM,
        PLAY_HOLOGRAM
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
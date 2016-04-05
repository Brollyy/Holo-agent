using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class Input
    {
        private static Dictionary<InputAction, >
        public static void StartTrackingAction
    }

    public enum InputAction
    {
        Forward,
        Backwards,
        StrafeLeft,
        StrafeRight,
        Fire,
        Jump,
        CameraMovement
    }

    public class InputEventArgs: EventArgs
    {
        public InputEventArgs()
        {
            
        }
    }
}

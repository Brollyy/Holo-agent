using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class Input
    {
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

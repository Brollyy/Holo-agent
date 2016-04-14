using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public static class Input
    {
        public static KeyboardState KEY_STATE;
        public static MouseState MOUSE_STATE;
        public static ButtonState BUTTON_PRESSED, BUTTON_RELEASED;
        public static Keys MOVE_FORWARD, MOVE_BACKWARD, STRAFE_LEFT, STRAFE_RIGHT, RUN, JUMP, CROUCH;
        public static int MOUSE_ROTATION_X, MOUSE_ROTATION_Y;
        /// <summary>
        /// Initializes controls (from file in future).
        /// </summary>
        public static void Initialize(/*Load bindings from file*/)
        {
            MOVE_FORWARD = Keys.W;
            MOVE_BACKWARD = Keys.S;
            STRAFE_LEFT = Keys.A;
            STRAFE_RIGHT = Keys.D;
            RUN = Keys.LeftShift;
            //JUMP
            //CROUCH
            //...
            BUTTON_PRESSED = ButtonState.Pressed;
            BUTTON_RELEASED = ButtonState.Released;
        }
        /// <summary>
        /// Gets keyboard and mouse states.
        /// </summary>
        public static void Update()
        {
            KEY_STATE = Keyboard.GetState();
            MouseState oldMouseState = MOUSE_STATE;
            MOUSE_STATE = Mouse.GetState();
            MOUSE_ROTATION_X = MOUSE_STATE.X - oldMouseState.X;
            MOUSE_ROTATION_Y = MOUSE_STATE.Y - oldMouseState.Y;
        }
        /// <summary>
        /// Allows to change key binding.
        /// Usage example: Input.Bind(ref Input.MOVE_FORWARD, Keys.Up);
        /// </summary>
        public static void Bind(ref Keys actionName, Keys keyName)
        {
            actionName = keyName;
        }
    }
}

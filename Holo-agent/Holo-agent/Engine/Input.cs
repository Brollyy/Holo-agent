using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public static class Input
    {
        public static KeyboardState KEY_STATE;
        public static MouseState MOUSE_STATE;
        public static ButtonState BUTTON_PRESSED, BUTTON_RELEASED;
        public static MouseButtons FIRE, ZOOM;
        public static Keys MOVE_FORWARD, MOVE_BACKWARD, STRAFE_LEFT, STRAFE_RIGHT, RUN, JUMP, CROUCH;
        public static int MOUSE_AXIS_X, MOUSE_AXIS_Y;
        private static int inversionFactorX, inversionFactorY;
        /// <summary>
        /// Initializes controls.
        /// </summary>
        public static void Initialize(/*Load bindings from file*/)
        {
            MOVE_FORWARD = Keys.W;
            MOVE_BACKWARD = Keys.S;
            STRAFE_LEFT = Keys.A;
            STRAFE_RIGHT = Keys.D;
            RUN = Keys.LeftShift;
            JUMP = Keys.Space;
            CROUCH = Keys.C;
            FIRE = MouseButtons.Left;
            ZOOM = MouseButtons.Right;
            BUTTON_PRESSED = ButtonState.Pressed;
            BUTTON_RELEASED = ButtonState.Released;
            inversionFactorX = 1;
            inversionFactorY = 1;
        }
        /// <summary>
        /// Gets keyboard and mouse states.
        /// </summary>
        public static void Update()
        {
            KEY_STATE = Keyboard.GetState();
            MouseState oldMouseState = MOUSE_STATE;
            MOUSE_STATE = Mouse.GetState();
            MOUSE_AXIS_X = (MOUSE_STATE.X - oldMouseState.X) * inversionFactorX;
            MOUSE_AXIS_Y = (MOUSE_STATE.Y - oldMouseState.Y) * inversionFactorY;
        }
        /// <summary>
        /// Allows to change key binding.
        /// Usage example: Input.BindKey(ref Input.MOVE_FORWARD, Keys.Up);
        /// </summary>
        public static void BindKey(ref Keys actionName, Keys keyName)
        {
            actionName = keyName;
        }
        /// <summary>
        /// Allows to change mouse button binding.
        /// Usage example: Input.BindMouseButton(ref Input.FIRE, MouseButtons.Middle);
        /// </summary>
        public static void BindMouseButton(ref MouseButtons actionName, MouseButtons buttonName)
        {
            actionName = buttonName;
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
        /// <summary>
        /// Checks if mouse button is pressed.
        /// Usage example: Input.IsButtonPressed(Input.FIRE)
        /// </summary>
        public static bool IsButtonPressed(MouseButtons mouseButton)
        {
            if (MOUSE_STATE.LeftButton.Equals(BUTTON_PRESSED) && mouseButton.Equals(MouseButtons.Left))
            {
                return true;
            }
            if (MOUSE_STATE.MiddleButton.Equals(BUTTON_PRESSED) && mouseButton.Equals(MouseButtons.Middle))
            {
                return true;
            }
            if (MOUSE_STATE.RightButton.Equals(BUTTON_PRESSED) && mouseButton.Equals(MouseButtons.Right))
            {
                return true;
            }
            return false;
        }
    }
    public enum MouseButtons
    {
        Left = 0,
        Middle = 1,
        Right = 2
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreedyKid
{
    public enum InputDeviceState
    {
        Empty,
        GamePad,
        Keyboard,
        Hybrid
    }

    public static class InputManager
    {
        public static IInputsHandler PlayerDevice;

        public static bool HasGamepad = false;
        public static bool HasKeyboard = false;

        private static bool _previousKeyPress = false;

        public static void CheckNewGamepad()
        {            
            // check if new gamepad
            if (PlayerDevice != null && !HasGamepad)
            {
                for (int i = 0; i < 4; i++)
                {
                    GamePadState gamePadState = GamePad.GetState((PlayerIndex)i);

                    if (gamePadState.IsConnected)
                    {
                        HasGamepad = true;
                        HasKeyboard = true;
                        PlayerDevice = new HybridMouseGamePadHandler((PlayerIndex)i);
                    }
                }
            }            
        }

        public static bool CheckEngagement()
        {
            if (!CheckKeypress())
                return false;

            HasGamepad = false;
            HasKeyboard = false;

            for (int i = 0; i < 4; i++)
            {
                GamePadState gamePadState = GamePad.GetState((PlayerIndex)i);

                if (gamePadState.Buttons.Start == ButtonState.Pressed || gamePadState.Buttons.A == ButtonState.Pressed)
                {
                    HasGamepad = true;

                    if (Program.RunningOnConsole)
                    {
                        PlayerDevice = new GamePadInputsHandler((PlayerIndex)i);
                    }
                    else
                    {
                        HasKeyboard = true;
                        PlayerDevice = new HybridMouseGamePadHandler((PlayerIndex)i);
                    }

                    return true;
                }
            }

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed)
            {
                HasKeyboard = true;

                for (int i = 0; i < 4; i++)
                {
                    if (GamePad.GetState((PlayerIndex)i).IsConnected)
                    {
                        HasGamepad = true;
                        PlayerDevice = new HybridMouseGamePadHandler((PlayerIndex)i);
                        return true;
                    }
                }

                PlayerDevice = new MouseKeyboardInputsHandler(PlayerIndex.One);

                return true;
            }

            PlayerDevice = null;
            return false;
        }

        public static bool CheckKeypress()
        {
            bool keypress = false;

            for (int i = 0; i < 4; i++)
            {
                GamePadState gamePadState = GamePad.GetState((PlayerIndex)i);

                if (gamePadState.Buttons.Start == ButtonState.Pressed || gamePadState.Buttons.A == ButtonState.Pressed)
                {
                    keypress = true;
                    break;
                }
            }

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (!keypress && (keyboardState.IsKeyDown(Keys.Enter) || mouseState.LeftButton == ButtonState.Pressed))
            {
                keypress = true;
            }

            bool ret = (!_previousKeyPress && keypress);

            _previousKeyPress = keypress;

            return ret;
        }
    }
}

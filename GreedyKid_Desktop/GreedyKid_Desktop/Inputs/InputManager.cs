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

        public static bool CheckEngagement()
        {
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

            if (keyboardState.IsKeyDown(Keys.Enter))
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
    }
}

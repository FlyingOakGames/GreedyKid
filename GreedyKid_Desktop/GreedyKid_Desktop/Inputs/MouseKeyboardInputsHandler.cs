using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GreedyKid
{
    public sealed class MouseKeyboardInputsHandler : IInputsHandler
    {
        private PlayerIndex _playerIndex;
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        public MouseKeyboardInputsHandler(PlayerIndex playerIndex)
        {
            _playerIndex = playerIndex;
            _previousMouseState = Mouse.GetState();
            _previousKeyboardState = Keyboard.GetState();
        }

        public InputsHandlerTypes InputType
        {
            get { return InputsHandlerTypes.MouseKeyboard; }
        }

        public bool IsConnected
        {
            get { return true; }
        }

        public void HandleIngameInputs(BuildingManager manager)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            // inputs

            _previousKeyboardState = keyboardState;
            _previousMouseState = mouseState;
        }
    }
}

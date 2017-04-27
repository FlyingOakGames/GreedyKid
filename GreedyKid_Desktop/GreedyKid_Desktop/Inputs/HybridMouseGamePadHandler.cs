using Microsoft.Xna.Framework;

namespace GreedyKid
{
    public sealed class HybridMouseGamePadHandler : IInputsHandler
    {
        private MouseKeyboardInputsHandler _keyboardHandler;
        private GamePadInputsHandler _gamepadHandler;

        public static InputsHandlerTypes SelectedInputType = InputsHandlerTypes.HybridMouseGamePad;

        public HybridMouseGamePadHandler(PlayerIndex playerIndex)
        {
            _keyboardHandler = new MouseKeyboardInputsHandler(playerIndex);
            _gamepadHandler = new GamePadInputsHandler(playerIndex);
        }

        public InputsHandlerTypes InputType
        {
            get { return SelectedInputType; }
        }

        public bool IsConnected
        {
            get { return _gamepadHandler.IsConnected; }
        }

        public void HandleTitleInputs(TitleScreenManager manager)
        {
            _keyboardHandler.HandleTitleInputs(manager);
            _gamepadHandler.HandleTitleInputs(manager);
        }

        public void HandleIngameInputs(GameplayManager manager)
        {
            _keyboardHandler.HandleIngameInputs(manager);
            _gamepadHandler.HandleIngameInputs(manager);
        }


    }
}

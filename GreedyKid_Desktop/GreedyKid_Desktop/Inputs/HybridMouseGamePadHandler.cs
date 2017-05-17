using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public bool DetectKeyPress(out Microsoft.Xna.Framework.Input.Keys key, out MouseKeyboardInputsHandler.MouseButton mouseButton, out MouseKeyboardInputsHandler.MappingType type)
        {
            return _keyboardHandler.DetectKeyPress(out key, out mouseButton, out type);
        }

        public void ClearKeyPress()
        {
            _keyboardHandler.ClearKeyPress();
        }

        public void Update(float gameTime)
        {
            _keyboardHandler.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _keyboardHandler.Draw(spriteBatch);
            _gamepadHandler.Draw(spriteBatch);
        }
    }
}

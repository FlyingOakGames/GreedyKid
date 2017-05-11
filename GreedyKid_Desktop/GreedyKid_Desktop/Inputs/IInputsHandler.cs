namespace GreedyKid
{
    public enum InputsHandlerTypes
    {
        GamePad,
        MouseKeyboard,
        HybridMouseGamePad,
        Touch
    }

    public interface IInputsHandler
    {
        void HandleIngameInputs(GameplayManager manager);

        void HandleTitleInputs(TitleScreenManager manager);

        InputsHandlerTypes InputType { get; }

        bool IsConnected { get; }

        bool DetectKeyPress(out Microsoft.Xna.Framework.Input.Keys key, out MouseKeyboardInputsHandler.MouseButton mouseButton, out MouseKeyboardInputsHandler.MappingType type);
    }
}

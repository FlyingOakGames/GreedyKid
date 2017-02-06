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
        void HandleIngameInputs(Player player);

        InputsHandlerTypes InputType { get; }

        bool IsConnected { get; }
    }
}

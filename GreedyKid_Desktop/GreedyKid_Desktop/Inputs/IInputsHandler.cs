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
        void HandleIngameInputs(BuildingManager manager);        

        InputsHandlerTypes InputType { get; }

        bool IsConnected { get; }
    }
}

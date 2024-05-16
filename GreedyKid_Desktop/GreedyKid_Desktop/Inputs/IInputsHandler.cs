// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using Microsoft.Xna.Framework.Graphics;

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
        void HandleIntroInputs(IntroScreenManager manager);

        void HandleEndingInputs(EndingScreenManager manager);

        void HandleIngameInputs(GameplayManager manager);

        void HandleTitleInputs(TitleScreenManager manager);

        InputsHandlerTypes InputType { get; }

        bool IsConnected { get; }

        bool DetectKeyPress(out Microsoft.Xna.Framework.Input.Keys key, out MouseKeyboardInputsHandler.MouseButton mouseButton, out MouseKeyboardInputsHandler.MappingType type);

        void ClearKeyPress();

        void Update(float gameTime);

        void Draw(SpriteBatch spriteBatch);
    }
}

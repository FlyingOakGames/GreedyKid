using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GreedyKid
{
    public enum ButtonType
    {
        PlayStation,
        Xbox
    }

    public sealed class GamePadInputsHandler : IInputsHandler
    {
        private const float _stickDZ = 0.05f;
        private const float _triggerDZ = 0.25f;

        private PlayerIndex _playerIndex;
        private bool _isConnected = false;

        private GamePadState _previousGamePadState;

#if PLAYSTATION4
        public static ButtonType PreferredButtonType = ButtonType.PlayStation;
#else
        public static ButtonType PreferredButtonType = ButtonType.Xbox;
#endif      

        public GamePadInputsHandler(PlayerIndex playerIndex)
        {
            _playerIndex = playerIndex;
            _previousGamePadState = GamePad.GetState(playerIndex, GamePadDeadZone.IndependentAxes);
        }

        public InputsHandlerTypes InputType
        {
            get { return InputsHandlerTypes.GamePad; }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
        }

        public void HandleIngameInputs(Player player)
        {
            GamePadState currentState = GamePad.GetState(_playerIndex, GamePadDeadZone.IndependentAxes);
            _isConnected = currentState.IsConnected;

            // moving
            if (currentState.DPad.Left == ButtonState.Pressed || currentState.ThumbSticks.Left.X < 0.0f)
                player.MoveLeft();
            else if (currentState.DPad.Right == ButtonState.Pressed || currentState.ThumbSticks.Left.X > 0.0f)
                player.MoveRight();

            // rolling
            if (currentState.Buttons.X == ButtonState.Pressed && _previousGamePadState.Buttons.X == ButtonState.Released)
                player.Roll();

            // action
            if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                player.Action();

            _previousGamePadState = currentState;
        }
    }
}

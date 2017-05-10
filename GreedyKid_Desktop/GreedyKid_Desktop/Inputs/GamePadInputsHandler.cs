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
        private const float _stickDZ = 0.5f;

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

        public void HandleTitleInputs(TitleScreenManager manager)
        {
            GamePadState currentState = GamePad.GetState(_playerIndex, GamePadDeadZone.IndependentAxes);
            _isConnected = currentState.IsConnected;

            if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                manager.PushSelect();
            else if (currentState.Buttons.B == ButtonState.Pressed && _previousGamePadState.Buttons.B == ButtonState.Released)
                manager.PushBack();

            if ((currentState.DPad.Up == ButtonState.Pressed && _previousGamePadState.DPad.Up == ButtonState.Released) ||
                (currentState.ThumbSticks.Left.Y >= _stickDZ && _previousGamePadState.ThumbSticks.Left.Y < _stickDZ))
                manager.PushUp();
            else if ((currentState.DPad.Down == ButtonState.Pressed && _previousGamePadState.DPad.Down == ButtonState.Released) ||
                (currentState.ThumbSticks.Left.Y <= -_stickDZ && _previousGamePadState.ThumbSticks.Left.Y > -_stickDZ))
                manager.PushDown();

            if ((currentState.DPad.Right == ButtonState.Pressed && _previousGamePadState.DPad.Right == ButtonState.Released) ||
                (currentState.ThumbSticks.Left.X >= _stickDZ && _previousGamePadState.ThumbSticks.Left.X < _stickDZ))
                manager.PushRight();
            else if ((currentState.DPad.Left == ButtonState.Pressed && _previousGamePadState.DPad.Left == ButtonState.Released) ||
                (currentState.ThumbSticks.Left.X <= -_stickDZ && _previousGamePadState.ThumbSticks.Left.X > -_stickDZ))
                manager.PushLeft();

            _previousGamePadState = currentState;
        }

        public void HandleIngameInputs(GameplayManager manager)
        {
            GamePadState currentState = GamePad.GetState(_playerIndex, GamePadDeadZone.IndependentAxes);
            _isConnected = currentState.IsConnected;

            if (manager.Player != null && !manager.Pause)
            {
                // moving
                if (currentState.DPad.Left == ButtonState.Pressed || currentState.ThumbSticks.Left.X < 0.0f)
                    manager.Player.MoveLeft();
                else if (currentState.DPad.Right == ButtonState.Pressed || currentState.ThumbSticks.Left.X > 0.0f)
                    manager.Player.MoveRight();

                // rolling
                if (currentState.Buttons.X == ButtonState.Pressed && _previousGamePadState.Buttons.X == ButtonState.Released)
                    manager.Player.Roll();

                // action
                if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                    manager.Player.Action();

                // shouting
                if (currentState.Buttons.B == ButtonState.Pressed)
                    manager.Player.Shout();

                // taunting
                if (currentState.Buttons.Y == ButtonState.Pressed)
                    manager.Player.Taunt();

                // pause
                if (currentState.Buttons.Start == ButtonState.Pressed && _previousGamePadState.Buttons.Start == ButtonState.Released)
                    manager.RequestPause();
            }
            else if (manager.Player != null && manager.Pause)
            {
                if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                    manager.PauseSelect();
                else if (currentState.Buttons.B == ButtonState.Pressed && _previousGamePadState.Buttons.B == ButtonState.Released)
                    manager.PauseCancel();

                if ((currentState.DPad.Up == ButtonState.Pressed && _previousGamePadState.DPad.Up == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y >= _stickDZ && _previousGamePadState.ThumbSticks.Left.Y < _stickDZ))
                    manager.PauseUp();
                else if ((currentState.DPad.Down == ButtonState.Pressed && _previousGamePadState.DPad.Down == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y <= -_stickDZ && _previousGamePadState.ThumbSticks.Left.Y > -_stickDZ))
                    manager.PauseDown();

                if ((currentState.DPad.Right == ButtonState.Pressed && _previousGamePadState.DPad.Right == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.X >= _stickDZ && _previousGamePadState.ThumbSticks.Left.X < _stickDZ))
                    manager.PauseRight();
                else if ((currentState.DPad.Left == ButtonState.Pressed && _previousGamePadState.DPad.Left == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.X <= -_stickDZ && _previousGamePadState.ThumbSticks.Left.X > -_stickDZ))
                    manager.PauseLeft();
            }
            else
            {
                // go
                if ((currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released) ||
                    (currentState.Buttons.Start == ButtonState.Pressed && _previousGamePadState.Buttons.Start == ButtonState.Released))
                    manager.DisappearTransition();
            }

            _previousGamePadState = currentState;
        }
    }
}

// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GreedyKid
{
    public enum ButtonType
    {
        PlayStation,
        Xbox
    }

    public sealed class GamePadInputsHandler : IInputsHandler
    {
        private const float _stickDZ = 0.8f;

        private PlayerIndex _playerIndex;
        private bool _isConnected = false;

        private GamePadState _previousGamePadState;

        public static ButtonType PreferredButtonType = ButtonType.Xbox;

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
            if (_isConnected && !currentState.IsConnected)
                manager.RequestPause(true);
            else if (!_isConnected && currentState.IsConnected)
                manager.ResetDisconnection();
            _isConnected = currentState.IsConnected;

            if (manager.Player != null && !manager.Pause && !manager.Gameover)
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
                if ((currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released) ||
                    (currentState.DPad.Up == ButtonState.Pressed && _previousGamePadState.DPad.Up == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y >= _stickDZ && _previousGamePadState.ThumbSticks.Left.Y < _stickDZ) ||
                    (currentState.DPad.Down == ButtonState.Pressed && _previousGamePadState.DPad.Up == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y <= -_stickDZ && _previousGamePadState.ThumbSticks.Left.Y > -_stickDZ))
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
            else if (manager.Player != null && manager.Gameover)
            {
                if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                    manager.PauseSelect();

                if ((currentState.DPad.Up == ButtonState.Pressed && _previousGamePadState.DPad.Up == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y >= _stickDZ && _previousGamePadState.ThumbSticks.Left.Y < _stickDZ))
                    manager.PauseUp();
                else if ((currentState.DPad.Down == ButtonState.Pressed && _previousGamePadState.DPad.Down == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y <= -_stickDZ && _previousGamePadState.ThumbSticks.Left.Y > -_stickDZ))
                    manager.PauseDown();
            }
            // inter level
            else
            {
                if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                    manager.PauseSelect();

                if ((currentState.DPad.Up == ButtonState.Pressed && _previousGamePadState.DPad.Up == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y >= _stickDZ && _previousGamePadState.ThumbSticks.Left.Y < _stickDZ))
                    manager.PauseUp();
                else if ((currentState.DPad.Down == ButtonState.Pressed && _previousGamePadState.DPad.Down == ButtonState.Released) ||
                    (currentState.ThumbSticks.Left.Y <= -_stickDZ && _previousGamePadState.ThumbSticks.Left.Y > -_stickDZ))
                    manager.PauseDown();
            }

            _previousGamePadState = currentState;
        }

        public void HandleIntroInputs(IntroScreenManager manager)
        {
            GamePadState currentState = GamePad.GetState(_playerIndex, GamePadDeadZone.IndependentAxes);
            _isConnected = currentState.IsConnected;

            if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                manager.Skip();
            else if (currentState.Buttons.B == ButtonState.Pressed && _previousGamePadState.Buttons.B == ButtonState.Released)
                manager.Skip();
            else if (currentState.Buttons.Start == ButtonState.Pressed && _previousGamePadState.Buttons.Start == ButtonState.Released)
                manager.Skip();

            _previousGamePadState = currentState;
        }

        public void HandleEndingInputs(EndingScreenManager manager)
        {
            GamePadState currentState = GamePad.GetState(_playerIndex, GamePadDeadZone.IndependentAxes);
            _isConnected = currentState.IsConnected;

            if (currentState.Buttons.A == ButtonState.Pressed && _previousGamePadState.Buttons.A == ButtonState.Released)
                manager.Skip();
            else if (currentState.Buttons.B == ButtonState.Pressed && _previousGamePadState.Buttons.B == ButtonState.Released)
                manager.Skip();
            else if (currentState.Buttons.Start == ButtonState.Pressed && _previousGamePadState.Buttons.Start == ButtonState.Released)
                manager.Skip();

            _previousGamePadState = currentState;
        }

        public bool DetectKeyPress(out Microsoft.Xna.Framework.Input.Keys key, out MouseKeyboardInputsHandler.MouseButton mouseButton, out MouseKeyboardInputsHandler.MappingType type)
        {
            key = Keys.A;
            mouseButton = MouseKeyboardInputsHandler.MouseButton.LeftButton;
            type = MouseKeyboardInputsHandler.MappingType.Keyboard;
            return false;
        }

        public void ClearKeyPress()
        {

        }

        public void Update(float gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}

using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Text;

namespace GreedyKid
{
    public sealed class MouseKeyboardInputsHandler : IInputsHandler
    {
        public enum MappingType
        {
            Keyboard,
            Mouse
        }

        public enum MouseButton
        {
            LeftButton,
            RightButton,
            XButton1,
            XButton2,
            MiddleButton
        }

        public struct Mapping
        {
            private MappingType _type;
            private Keys _keyboardKey;
            private MouseButton _mouseButton;
            public string Name;

            public Mapping(Keys key)
            {
                _type = MappingType.Keyboard;
                _keyboardKey = key;
                _mouseButton = MouseButton.MiddleButton;

                Name = Enum.GetName(typeof(Keys), key);
                ToHumanReadable();
            }

            public Mapping(MouseButton mouseButton)
            {
                _type = MappingType.Mouse;
                _keyboardKey = Keys.A;
                _mouseButton = mouseButton;

                Name = Enum.GetName(typeof(MouseButton), mouseButton);
                ToHumanReadable();
            }

            private void ToHumanReadable()
            {
                StringBuilder builder = new StringBuilder();
                foreach (char c in Name)
                {
                    if (Char.IsUpper(c) && builder.Length > 0) builder.Append(' ');
                    builder.Append(c);
                }
                Name = builder.ToString();
                Name = Name.ToUpperInvariant();
            }

            public bool IsPressed(MouseState mouseState, KeyboardState keyboardState)
            {
                if (_type == MappingType.Keyboard)
                    return keyboardState.IsKeyDown(_keyboardKey);
                else if (_mouseButton == MouseButton.MiddleButton)
                    return mouseState.MiddleButton == ButtonState.Pressed;
                else if (_mouseButton == MouseButton.XButton1)
                    return mouseState.XButton1 == ButtonState.Pressed;
                else if (_mouseButton == MouseButton.XButton2)
                    return mouseState.XButton2 == ButtonState.Pressed;
                else if (_mouseButton == MouseButton.LeftButton)
                    return mouseState.LeftButton == ButtonState.Pressed;
                else if (_mouseButton == MouseButton.RightButton)
                    return mouseState.RightButton == ButtonState.Pressed;

                return false;
            }

            public bool IsReleased(MouseState mouseState, KeyboardState keyboardState)
            {
                if (_type == MappingType.Keyboard)
                    return keyboardState.IsKeyUp(_keyboardKey);
                else if (_mouseButton == MouseButton.MiddleButton)
                    return mouseState.MiddleButton == ButtonState.Released;
                else if (_mouseButton == MouseButton.XButton1)
                    return mouseState.XButton1 == ButtonState.Released;
                else if (_mouseButton == MouseButton.XButton2)
                    return mouseState.XButton2 == ButtonState.Released;
                else if (_mouseButton == MouseButton.LeftButton)
                    return mouseState.LeftButton == ButtonState.Released;
                else if (_mouseButton == MouseButton.RightButton)
                    return mouseState.RightButton == ButtonState.Released;

                return false;
            }

            public bool Equals(Keys key)
            {
                return _type == MappingType.Keyboard && _keyboardKey == key;
            }

            public bool Equals(MouseButton button)
            {
                return _type == MappingType.Mouse && _mouseButton == button;
            }

            public void Read(BinaryReader reader)
            {
                _type = (MappingType)reader.ReadInt32();
                _keyboardKey = (Keys)reader.ReadInt32();
                _mouseButton = (MouseButton)reader.ReadInt32();

                if (_type == MappingType.Mouse)
                    Name = Enum.GetName(typeof(MouseButton), _mouseButton);
                else
                    Name = Enum.GetName(typeof(Keys), _keyboardKey);
                ToHumanReadable();
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write((int)_type);
                writer.Write((int)_keyboardKey);
                writer.Write((int)_mouseButton);
            }
        }

        private PlayerIndex _playerIndex;
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        private MouseState _previousRebindMouseState;
        private KeyboardState _previousRebindKeyboardState;

        // default keys
        public static Mapping UpKey;
        public static Mapping DownKey;
        public static Mapping LeftKey;
        public static Mapping RightKey;

        public static Mapping ActionKey;
        public static Mapping ShoutKey;
        public static Mapping RollKey;
        public static Mapping TauntKey;
        
        private static Mapping _backKey = new Mapping(Keys.Escape);
        private static Mapping _selectKey = new Mapping(Keys.Enter);

        public static bool HasRemapped = false;

        private static string _mappingPath = "Keyboard";

        // default mapping
        private const Keys _defaultUpKey = Keys.W;
        private const Keys _defaultdownKey = Keys.S;
        private const Keys _defaultLeftKey = Keys.A;
        private const Keys _defaultRightKey = Keys.D;

        private const Keys _defaultActionKey = Keys.E;
        private const Keys _defaultShoutKey = Keys.Space;
        private const Keys _defaultRollKey = Keys.Q;
        private const Keys _defaultTauntKey = Keys.LeftShift;

        private Rectangle _mouseRectangle = new Rectangle(85, 1921, 11, 11);
        private Vector2 _mouseCenter = new Vector2(5.0f, 5.0f);

        public static void RestoreDefault()
        {
            HasRemapped = false;
            UpKey = new Mapping(_defaultUpKey);
            DownKey = new Mapping(_defaultdownKey);
            LeftKey = new Mapping(_defaultLeftKey);
            RightKey = new Mapping(_defaultRightKey);
            ActionKey = new Mapping(_defaultActionKey);
            ShoutKey = new Mapping(_defaultShoutKey);
            RollKey = new Mapping(_defaultRollKey);
            TauntKey = new Mapping(_defaultTauntKey);
        }

        public MouseKeyboardInputsHandler(PlayerIndex playerIndex)
        {
            _playerIndex = playerIndex;
            _previousMouseState = Mouse.GetState();
            _previousKeyboardState = Keyboard.GetState();
            _previousRebindMouseState = _previousMouseState;
            _previousRebindKeyboardState = _previousKeyboardState;
            RestoreDefault();

            _mappingPath = Path.Combine(SettingsManager.Instance.SaveDirectory, _mappingPath);

            // try to load, if loaded set has mapped to true
            try
            {
                if (File.Exists(_mappingPath))
                {
                    using (FileStream fileStream = new FileStream(_mappingPath, FileMode.Open))
                    {
                        using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                        {
                            using (BinaryReader reader = new BinaryReader(gzipStream))
                            {
                                UpKey.Read(reader);
                                DownKey.Read(reader);
                                LeftKey.Read(reader);
                                RightKey.Read(reader);
                                ActionKey.Read(reader);
                                ShoutKey.Read(reader);
                                RollKey.Read(reader);
                                TauntKey.Read(reader);
                            }
                        }
                    }
                    HasRemapped = true;
                }
            }
            catch (Exception)
            {
                // could not load, restore default
                RestoreDefault();
            }
        }

        public static void SaveMapping()
        {
            // check if default
            if (
                UpKey.Equals(_defaultUpKey) &&
                DownKey.Equals(_defaultdownKey) &&
                LeftKey.Equals(_defaultLeftKey) &&
                RightKey.Equals(_defaultRightKey) &&
                ActionKey.Equals(_defaultActionKey) &&
                ShoutKey.Equals(_defaultShoutKey) &&
                RollKey.Equals(_defaultRollKey) &&
                TauntKey.Equals(_defaultTauntKey)
                )
            {
                HasRemapped = false;
                // try to delete mapping file if exists
                try
                {
                    if (File.Exists(_mappingPath))
                        File.Delete(_mappingPath);
                }
                catch (Exception)
                {
                    // could not delete, but that's alright
                }
            }
            else
            {
#if DESKTOP
                // save
                using (FileStream fileStream = new FileStream(_mappingPath, FileMode.Create))
                {
                    using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
                    {
                        using (BinaryWriter writer = new BinaryWriter(gzipStream))
                        {
                            UpKey.Write(writer);
                            DownKey.Write(writer);
                            LeftKey.Write(writer);
                            RightKey.Write(writer);
                            ActionKey.Write(writer);
                            ShoutKey.Write(writer);
                            RollKey.Write(writer);
                            TauntKey.Write(writer);

                            writer.Flush();
                            gzipStream.Flush();
                            fileStream.Flush();
                        }
                    }
                }
#endif
                HasRemapped = true;
            }
        }

        public InputsHandlerTypes InputType
        {
            get { return InputsHandlerTypes.MouseKeyboard; }
        }

        public bool IsConnected
        {
            get { return true; }
        }

        public void HandleTitleInputs(TitleScreenManager manager)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                manager.PushSelect();
            else if (keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
                manager.PushBack();

            if (keyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up) ||
                UpKey.IsPressed(mouseState, keyboardState) && UpKey.IsReleased(_previousMouseState, _previousKeyboardState))
                manager.PushUp();
            else if (keyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down) ||
                DownKey.IsPressed(mouseState, keyboardState) && DownKey.IsReleased(_previousMouseState, _previousKeyboardState))
                manager.PushDown();

            if (keyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left) ||
                LeftKey.IsPressed(mouseState, keyboardState) && LeftKey.IsReleased(_previousMouseState, _previousKeyboardState))
                manager.PushLeft();
            else if (keyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right) ||
                RightKey.IsPressed(mouseState, keyboardState) && RightKey.IsReleased(_previousMouseState, _previousKeyboardState))
                manager.PushRight();

            _previousKeyboardState = keyboardState;
            _previousMouseState = mouseState;
        }

        public void HandleIngameInputs(GameplayManager manager)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            if (manager.Player != null && !manager.Pause)
            {
                // moving
                if (LeftKey.IsPressed(mouseState, keyboardState))
                    manager.Player.MoveLeft();
                else if (RightKey.IsPressed(mouseState, keyboardState))
                    manager.Player.MoveRight();

                // rolling
                if (RollKey.IsPressed(mouseState, keyboardState) && RollKey.IsReleased(_previousMouseState, _previousKeyboardState))
                    manager.Player.Roll();

                // action
                if (ActionKey.IsPressed(mouseState, keyboardState) && ActionKey.IsReleased(_previousMouseState, _previousKeyboardState))
                    manager.Player.Action();

                // shouting
                if (ShoutKey.IsPressed(mouseState, keyboardState))
                    manager.Player.Shout();

                // taunting
                if (TauntKey.IsPressed(mouseState, keyboardState))
                    manager.Player.Taunt();

                // pause
                if (keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
                    manager.RequestPause();
            }
            else if (manager.Player != null && manager.Pause)
            {
                if (ActionKey.IsPressed(mouseState, keyboardState) && ActionKey.IsReleased(_previousMouseState, _previousKeyboardState))
                    manager.PauseSelect();
                else if (keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape))
                    manager.PauseCancel();

                if (keyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up) ||
                    UpKey.IsPressed(mouseState, keyboardState) && UpKey.IsReleased(_previousMouseState, _previousKeyboardState))
                    manager.PauseUp();
                else if (keyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down) ||
                    DownKey.IsPressed(mouseState, keyboardState) && DownKey.IsReleased(_previousMouseState, _previousKeyboardState))
                    manager.PauseDown();

                if (keyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left) ||
                    LeftKey.IsPressed(mouseState, keyboardState) && LeftKey.IsReleased(_previousMouseState, _previousKeyboardState))
                    manager.PauseLeft();
                else if (keyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right) ||
                    RightKey.IsPressed(mouseState, keyboardState) && RightKey.IsReleased(_previousMouseState, _previousKeyboardState))
                    manager.PauseRight();
            }
            else
            {
                // go
                if (ActionKey.IsPressed(mouseState, keyboardState) && ActionKey.IsReleased(_previousMouseState, _previousKeyboardState) ||
                    keyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                    manager.DisappearTransition();
            }

            _previousKeyboardState = keyboardState;
            _previousMouseState = mouseState;
        }

        public bool DetectKeyPress(out Keys key, out MouseButton mouseButton, out MappingType type)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            bool keyDetected = false;

            // mouse
            mouseButton = MouseButton.LeftButton;
            type = MappingType.Keyboard;

            if (mouseState.LeftButton == ButtonState.Pressed && _previousRebindMouseState.LeftButton == ButtonState.Released)
            {
                mouseButton = MouseButton.LeftButton;
                type = MappingType.Mouse;
                keyDetected = true;
            }
            else if (mouseState.RightButton == ButtonState.Pressed && _previousRebindMouseState.LeftButton == ButtonState.Released)
            {
                mouseButton = MouseButton.RightButton;
                type = MappingType.Mouse;
                keyDetected = true;
            }
            else if (mouseState.LeftButton == ButtonState.Pressed && _previousRebindMouseState.LeftButton == ButtonState.Released)
            {
                mouseButton = MouseButton.LeftButton;
                type = MappingType.Mouse;
                keyDetected = true;
            }
            else if (mouseState.XButton1 == ButtonState.Pressed && _previousRebindMouseState.XButton1 == ButtonState.Released)
            {
                mouseButton = MouseButton.XButton1;
                type = MappingType.Mouse;
                keyDetected = true;
            }
            else if (mouseState.XButton2 == ButtonState.Pressed && _previousRebindMouseState.XButton2 == ButtonState.Released)
            {
                mouseButton = MouseButton.XButton2;
                type = MappingType.Mouse;
                keyDetected = true;
            }

            // keyboard
            key = Keys.A;
            Keys[] keys = keyboardState.GetPressedKeys();
            Keys[] previousKeys = _previousRebindKeyboardState.GetPressedKeys();

            for (int i = 0; i < keys.Length; i++)
            {
                // check if the key wasn't pressed before
                bool wasNotPressed = true;
                for (int j = 0; j < previousKeys.Length; j++)
                {
                    if (keys[i] == previousKeys[j])
                        wasNotPressed = false;
                }
                if (wasNotPressed)
                {
                    key = keys[i];
                    keyDetected = true;
                    break;
                }
            }

            _previousRebindKeyboardState = keyboardState;
            _previousRebindMouseState = mouseState;

            return keyDetected;
        }

        public void ClearKeyPress()
        {
            _previousRebindKeyboardState = Keyboard.GetState();
            _previousRebindMouseState = Mouse.GetState();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = TextureManager.Gameplay;

            spriteBatch.Draw(texture,
                new Rectangle(
                    (int)((_previousMouseState.Position.X - GreedyKidGame.Viewport.X) * (GreedyKidGame.Width / (float)GreedyKidGame.Viewport.Width)),
                    (int)((_previousMouseState.Position.Y - GreedyKidGame.Viewport.Y) * (GreedyKidGame.Height / (float)GreedyKidGame.Viewport.Height)),
                    _mouseRectangle.Width,
                    _mouseRectangle.Height),
                _mouseRectangle,
                Color.White,
                0.0f,
                _mouseCenter,
                SpriteEffects.None,
                0.0f);
        }
    }
}

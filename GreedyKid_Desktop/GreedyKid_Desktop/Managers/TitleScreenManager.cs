using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public enum TitleScreenState
    {
        Title,
        Main,
        Play,
        LevelSelection,
        SteamWorkshop,
        Settings,
    }

    public sealed class TitleScreenManager
    {
        public bool StartGame = false;

        private Rectangle _viewport;
        private Rectangle[] _backgroundRectangles;
        private Rectangle _titleRectangle;
        private Rectangle _selectionRectangle;

        private TitleScreenState _state = TitleScreenState.Title;

        private int _selectionOption = 0;
        private Color _selectionColor = new Color(217, 87, 99);

        public TitleScreenManager()
        {
            _viewport = new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height);

            _backgroundRectangles = new Rectangle[2];
            for (int i = 0; i < _backgroundRectangles.Length; i++)
            {
                _backgroundRectangles[i] = new Rectangle(898 + GreedyKidGame.Width * i, TextureManager.GameplayHeight - GreedyKidGame.Height, GreedyKidGame.Width, GreedyKidGame.Height);
            }

            _titleRectangle = new Rectangle(TextureManager.GameplayWidth - 95, TextureManager.GameplayHeight - 81, 95, 81);
            _selectionRectangle = new Rectangle(142, TextureManager.GameplayHeight - 140, 4, 7);
        }

        public void Update(float gameTime)
        {
            if (_state == TitleScreenState.Title && InputManager.CheckEngagement())
            {
                _state = TitleScreenState.Main;
            }
            else if (InputManager.PlayerDevice != null && _state != TitleScreenState.Title)
            {
                InputManager.PlayerDevice.HandleTitleInputs(this);
            }            
        }

        public void PushStart()
        {
            StartGame = true;
        }

        public void PushSelect()
        {
            switch (_state)
            {
                case TitleScreenState.Title: break;
                case TitleScreenState.Main:
                    if (_selectionOption == 0)
                        _state = TitleScreenState.Play;
                    else if (_selectionOption == 1)
                        _state = TitleScreenState.Settings;
                    else if (_selectionOption == 2)
                        GreedyKidGame.ShouldExit = true;
                    break;
                case TitleScreenState.Settings: break;
                case TitleScreenState.Play:
                    if (_selectionOption == 0)
                        _state = TitleScreenState.LevelSelection;
                    //else if (_selectionOption == 1)
                    //    _state = TitleScreenState.SteamWorkshop;
                    else if (_selectionOption == 2)
                        PushBack();
                    break;
                case TitleScreenState.LevelSelection:
                    StartGame = true;
                    break;
                case TitleScreenState.SteamWorkshop:
                    StartGame = true;
                    break;
            }
        }

        public void PushBack()
        {
            _selectionOption = 0;

            switch (_state)
            {
                case TitleScreenState.Title: break;
                case TitleScreenState.Main: _state = TitleScreenState.Title; break;
                case TitleScreenState.Settings: _selectionOption = 1; _state = TitleScreenState.Main; break;
                case TitleScreenState.Play: _state = TitleScreenState.Main; break;
                case TitleScreenState.LevelSelection: _state = TitleScreenState.Play; break;
                case TitleScreenState.SteamWorkshop: _selectionOption = 1; _state = TitleScreenState.Play; break;
            }
        }

        public void PushUp()
        {
            _selectionOption--;

            if (_selectionOption < 0)
                switch (_state)
                {
                    case TitleScreenState.Title: _selectionOption = 0; break;
                    case TitleScreenState.Main: _selectionOption = (Program.RunningOnConsole ? 1 : 2); break;
                    case TitleScreenState.Settings: SettingsManager.Instance.PushUp(); break;
                    case TitleScreenState.Play: break;
                    case TitleScreenState.LevelSelection: break;
                    case TitleScreenState.SteamWorkshop: break;
                }
        }

        public void PushDown()
        {
            _selectionOption++;

            switch (_state)
            {
                case TitleScreenState.Title: _selectionOption = 0; break;
                case TitleScreenState.Main: _selectionOption %= (Program.RunningOnConsole ? 2 : 3); break;
                case TitleScreenState.Settings: SettingsManager.Instance.PushDown(); break;
                case TitleScreenState.Play: break;
                case TitleScreenState.LevelSelection: break;
                case TitleScreenState.SteamWorkshop: break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Gameplay;

            // background
            spriteBatch.Draw(texture,
                _viewport,
                _backgroundRectangles[(_state == TitleScreenState.Title ? 0 : 1)],
                Color.White);

            int yStart = 105;

            if (_state == TitleScreenState.Main || _state == TitleScreenState.Play)
            {
                yStart = 30;
            }
            else if (_state == TitleScreenState.Settings || _state == TitleScreenState.LevelSelection)
            {
                yStart = -1;
            }

            // title
            if (yStart >= 0)
                spriteBatch.Draw(texture,
                    new Rectangle(
                        GreedyKidGame.Width / 2 - _titleRectangle.Width / 2 - 1,
                        yStart,
                        _titleRectangle.Width,
                        _titleRectangle.Height),
                    _titleRectangle,
                    Color.White);

            // text
            if (_state == TitleScreenState.Main)
            {
                yStart = 115;

                DrawCenteredText(spriteBatch, TextManager.Instance.Play, yStart, 0);
                DrawCenteredText(spriteBatch, TextManager.Instance.Settings, yStart + 15, 1);
                if (!Program.RunningOnConsole)
                    DrawCenteredText(spriteBatch, TextManager.Instance.Quit, yStart + 30, 2);
            }
            else if (_state == TitleScreenState.Play)
            {
                yStart = 115;

                DrawCenteredText(spriteBatch, TextManager.Instance.Campaign, yStart, 0);
                DrawCenteredText(spriteBatch, TextManager.Instance.Workshop, yStart + 15, 1);
                DrawCenteredText(spriteBatch, TextManager.Instance.Back, yStart + 30, 2);
            }            
            else if (_state == TitleScreenState.Settings)
            {
                SettingsManager.Instance.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, int yPos, int option)
        {
            SpriteFont font = TextManager.Instance.Font;

            int textWidth = (int)font.MeasureString(text).X;
            spriteBatch.DrawString(font, text, new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, yPos), Color.White);

            if (_selectionOption == option)
            {
                Texture2D texture = TextureManager.Gameplay;
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - _selectionRectangle.Width - 2, yPos + 4, _selectionRectangle.Width, _selectionRectangle.Height),
                    _selectionRectangle,
                    _selectionColor);
            }
        }
    }
}

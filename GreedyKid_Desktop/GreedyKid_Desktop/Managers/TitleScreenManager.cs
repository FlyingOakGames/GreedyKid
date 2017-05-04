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
        private Rectangle[] _bordersRectangle;

        private TitleScreenState _state = TitleScreenState.Title;

        private int _selectionOption = 0;
        private Color _selectionColor = new Color(217, 87, 99);
        private Color _notSelectedColor = new Color(132, 126, 135);

        public TitleScreenManager()
        {
            _viewport = new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height);

            _backgroundRectangles = new Rectangle[2];
            // title
            _backgroundRectangles[0] = new Rectangle(898, TextureManager.GameplayHeight - GreedyKidGame.Height, 298, 164);
            // main
            _backgroundRectangles[1] = new Rectangle(898 + _backgroundRectangles[0].Width + 1, TextureManager.GameplayHeight - GreedyKidGame.Height, 308 , 165);

            _titleRectangle = new Rectangle(TextureManager.GameplayWidth - 95, TextureManager.GameplayHeight - 81, 95, 81);
            _selectionRectangle = new Rectangle(142, TextureManager.GameplayHeight - 140, 4, 7);

            _bordersRectangle = new Rectangle[8];
            _bordersRectangle[0] = new Rectangle(0, TextureManager.GameplayHeight - 24, 12, 13); // upper left
            _bordersRectangle[1] = new Rectangle(11, TextureManager.GameplayHeight - 24, 12, 13); // upper right
            _bordersRectangle[2] = new Rectangle(0, TextureManager.GameplayHeight - 12, 12, 12); // lower left
            _bordersRectangle[3] = new Rectangle(11, TextureManager.GameplayHeight - 12, 12, 12); // lower right
            _bordersRectangle[4] = new Rectangle(11, TextureManager.GameplayHeight - 24, 1, 9); // up
            _bordersRectangle[5] = new Rectangle(11, TextureManager.GameplayHeight - 8, 1, 8); // down
            _bordersRectangle[6] = new Rectangle(0, TextureManager.GameplayHeight - 12, 8, 1); // left
            _bordersRectangle[7] = new Rectangle(15, TextureManager.GameplayHeight - 12, 8, 1); // right
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
                case TitleScreenState.Settings: _selectionOption = 1; _state = TitleScreenState.Main; SettingsManager.Instance.Save(); break;
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
                    case TitleScreenState.Play: _selectionOption = 2; break;
                    case TitleScreenState.LevelSelection: _selectionOption = 0; break;
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
                case TitleScreenState.Play: _selectionOption %= 3; break;
                case TitleScreenState.LevelSelection: _selectionOption %= 1; break;
                case TitleScreenState.SteamWorkshop: break;
            }
        }

        public void PushLeft()
        {
            if (_state == TitleScreenState.Settings)
                SettingsManager.Instance.PushLeft();
        }

        public void PushRight()
        {
            if (_state == TitleScreenState.Settings)
                SettingsManager.Instance.PushRight();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Gameplay;

            // ****** BORDERS ******
            // up
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, GreedyKidGame.Width, _bordersRectangle[4].Height),
                _bordersRectangle[4],
                Color.White);

            // down
            spriteBatch.Draw(texture,
                new Rectangle(0, GreedyKidGame.Height - _bordersRectangle[5].Height, GreedyKidGame.Width, _bordersRectangle[5].Height),
                _bordersRectangle[5],
                Color.White);

            // left
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, _bordersRectangle[6].Width, GreedyKidGame.Height),
                _bordersRectangle[6],
                Color.White);

            // right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[7].Width, 0, _bordersRectangle[6].Width, GreedyKidGame.Height),
                _bordersRectangle[7],
                Color.White);

            // upper left
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, _bordersRectangle[0].Width, _bordersRectangle[0].Height),
                _bordersRectangle[0],
                Color.White);

            // upper right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[1].Width, 0, _bordersRectangle[1].Width, _bordersRectangle[1].Height),
                _bordersRectangle[1],
                Color.White);

            // lower left
            spriteBatch.Draw(texture,
                new Rectangle(0, GreedyKidGame.Height - _bordersRectangle[2].Height, _bordersRectangle[2].Width, _bordersRectangle[2].Height),
                _bordersRectangle[2],
                Color.White);

            // lower right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[3].Width, GreedyKidGame.Height - _bordersRectangle[3].Height, _bordersRectangle[3].Width, _bordersRectangle[3].Height),
                _bordersRectangle[3],
                Color.White);

            // background
            if (_state == TitleScreenState.Title)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(14, 4, _backgroundRectangles[0].Width, _backgroundRectangles[0].Height),
                    _backgroundRectangles[0],
                    Color.White);
            }
            else if (_state == TitleScreenState.Main || _state == TitleScreenState.Play)
            {
                 spriteBatch.Draw(texture,
                    new Rectangle(4, 3, _backgroundRectangles[1].Width, _backgroundRectangles[1].Height),
                    _backgroundRectangles[1],
                    Color.White);
            }
            

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
            else if (_state == TitleScreenState.LevelSelection)
            {
                yStart = 115;

                DrawCenteredText(spriteBatch, "NOTHING HERE YET, JUST PRESS A", yStart, 0);
            }

            spriteBatch.End();
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, int yPos, int option)
        {
            SpriteFont font = TextManager.Instance.Font;

            int textWidth = (int)font.MeasureString(text).X;
            spriteBatch.DrawString(font,
                text,
                new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, yPos),
                (_selectionOption == option ? Color.White : _notSelectedColor));

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

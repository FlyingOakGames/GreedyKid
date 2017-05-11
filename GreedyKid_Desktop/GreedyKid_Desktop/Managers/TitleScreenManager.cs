using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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

        private TitleScreenState _state = TitleScreenState.Title;

        private int _selectionOption = 0;

        public TitleScreenManager()
        {
            _viewport = new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height);

            _backgroundRectangles = new Rectangle[2];
            // title
            _backgroundRectangles[0] = new Rectangle(898, TextureManager.GameplayHeight - GreedyKidGame.Height, 298, 164);
            // main
            _backgroundRectangles[1] = new Rectangle(898 + _backgroundRectangles[0].Width + 1, TextureManager.GameplayHeight - GreedyKidGame.Height, 308 , 165);

            _titleRectangle = new Rectangle(TextureManager.GameplayWidth - 95, TextureManager.GameplayHeight - 81, 95, 81);
        }

        public void SetState(TitleScreenState state)
        {
            _state = state;
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
                if (_state == TitleScreenState.Settings)
                {
                    SettingsManager.Instance.Update(gameTime);
                }
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
                    {
                        _state = TitleScreenState.Settings;
                        SettingsManager.Instance.Reset();
                    }
                    else if (_selectionOption == 2)
                        GreedyKidGame.ShouldExit = true;
                    break;
                case TitleScreenState.Settings: SettingsManager.Instance.PushSelect(); break;
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
                case TitleScreenState.Main: _state = TitleScreenState.Title; InputManager.PlayerDevice = null; break;
                case TitleScreenState.Settings:
                    if (!SettingsManager.Instance.PushCancel())
                    {
                        _selectionOption = 1;
                        _state = TitleScreenState.Main;
                        SettingsManager.Instance.Save();
                    }
                    break;
                case TitleScreenState.Play: _state = TitleScreenState.Main; break;
                case TitleScreenState.LevelSelection: _state = TitleScreenState.Play; break;
                case TitleScreenState.SteamWorkshop: _selectionOption = 1; _state = TitleScreenState.Play; break;
            }
        }

        public void PushUp()
        {
            if (_state != TitleScreenState.Settings)
                _selectionOption--;   
            else
                SettingsManager.Instance.PushUp();

            if (_selectionOption < 0)
                switch (_state)
                {
                    case TitleScreenState.Title: _selectionOption = 0; break;
                    case TitleScreenState.Main: _selectionOption = (Program.RunningOnConsole ? 1 : 2); break;
                    case TitleScreenState.Settings: break;
                    case TitleScreenState.Play: _selectionOption = 2; break;
                    case TitleScreenState.LevelSelection: _selectionOption = 0; break;
                    case TitleScreenState.SteamWorkshop: break;
                }
        }

        public void PushDown()
        {
            if (_state != TitleScreenState.Settings)
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

            UIHelper.Instance.DrawBorders(spriteBatch);            

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

                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Play, yStart, 0, _selectionOption);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Settings, yStart + 15, 1, _selectionOption);
                if (!Program.RunningOnConsole)
                    UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Quit, yStart + 30, 2, _selectionOption);
            }
            else if (_state == TitleScreenState.Play)
            {
                yStart = 115;

                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Campaign, yStart, 0, _selectionOption);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Workshop, yStart + 15, 1, _selectionOption);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Back, yStart + 30, 2, _selectionOption);
            }            
            else if (_state == TitleScreenState.Settings)
            {
                SettingsManager.Instance.Draw(spriteBatch);
            }
            else if (_state == TitleScreenState.LevelSelection)
            {
                yStart = 115;

                UIHelper.Instance.DrawCenteredText(spriteBatch, "NOTHING HERE YET, JUST PRESS A", yStart, 0, _selectionOption);
            }

            if (_state != TitleScreenState.Title)
            {
                UIHelper.Instance.DrawMicrophoneVolume(spriteBatch);
            }

            // commands
            if (_state == TitleScreenState.Title)
            {
                UIHelper.Instance.DrawCommand(spriteBatch, GreedyKidGame.Version, CommandType.None, true);
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Press, CommandType.Select);
            }
            else
            {
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Select, CommandType.Select);
            }

            if (_state != TitleScreenState.Title && _state != TitleScreenState.Main)
            {
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Back, CommandType.Back, true);
            }            

            spriteBatch.End();
        }
    }
}

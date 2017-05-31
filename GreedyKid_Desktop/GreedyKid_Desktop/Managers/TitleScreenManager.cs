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
        public bool ShouldLoadBuilding = false;
        public string RequiredBuildingIdentifier = "Default";

        private Rectangle _viewport;
        private Rectangle[] _backgroundRectangles;
        private Rectangle _titleRectangle;
        private Rectangle[] _selectionRectangle;
        private Rectangle[] _numberRectangle;

        private TitleScreenState _state = TitleScreenState.Title;

        private int _selectionOption = 0;

        private int _selectedLevel = 0;
        private int[] _targetTime;
        private int[] _targetMoney;
        private int _levelCount = 0;

        public TitleScreenManager()
        {
            _viewport = new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height);

            _backgroundRectangles = new Rectangle[4];
            // title
            _backgroundRectangles[0] = new Rectangle(898, TextureManager.GameplayHeight - GreedyKidGame.Height, 298, 164);
            // main
            _backgroundRectangles[1] = new Rectangle(898 + _backgroundRectangles[0].Width + 1, TextureManager.GameplayHeight - GreedyKidGame.Height, 308 , 165);
            // level selection
            _backgroundRectangles[2] = new Rectangle(1921, TextureManager.GameplayHeight - 250, 123, 123);
            _backgroundRectangles[3] = new Rectangle(1921, TextureManager.GameplayHeight - 127, 126, 126);

            _titleRectangle = new Rectangle(0, TextureManager.GameplayHeight - 225, 95, 81);

            // level selection
            _selectionRectangle = new Rectangle[11];
            // small stars
            _selectionRectangle[0] = new Rectangle(523, TextureManager.GameplayHeight - 239, 23, 7); // no star
            _selectionRectangle[1] = new Rectangle(523, TextureManager.GameplayHeight - 231, 23, 7); // 1 star
            _selectionRectangle[2] = new Rectangle(523, TextureManager.GameplayHeight - 223, 23, 7); // 2 stars
            _selectionRectangle[3] = new Rectangle(523, TextureManager.GameplayHeight - 215, 23, 7); // 3 stars
            // level block
            _selectionRectangle[4] = new Rectangle(413, TextureManager.GameplayHeight - 229, 35, 44); // selected
            _selectionRectangle[5] = new Rectangle(377, TextureManager.GameplayHeight - 229, 35, 44); // unlocked
            _selectionRectangle[6] = new Rectangle(341, TextureManager.GameplayHeight - 229, 35, 44); // locked
            // arrows
            _selectionRectangle[7] = new Rectangle(483, TextureManager.GameplayHeight - 230, 9, 13); // left off
            _selectionRectangle[8] = new Rectangle(493, TextureManager.GameplayHeight - 230, 9, 13); // left on
            _selectionRectangle[9] = new Rectangle(503, TextureManager.GameplayHeight - 230, 9, 13); // right off
            _selectionRectangle[10] = new Rectangle(513, TextureManager.GameplayHeight - 230, 9, 13); // right on

            // number font
            _numberRectangle = new Rectangle[10];
            for (int i = 0; i < 10; i++)
            {
                _numberRectangle[i] = new Rectangle(230 + i * 11, TextureManager.GameplayHeight - 195, 11, 10);
            }
        }

        public void SetState(TitleScreenState state)
        {
            _state = state;
        }

        public void SetBuilding(Building building)
        {
            _levelCount = building.LevelCount;
            _targetMoney = new int[_levelCount];
            _targetTime = new int[_levelCount];

            _selectedLevel = -1;

            for (int i = 0; i < _levelCount; i++)
            {
                _targetTime[i] = building.TargetTime[i];
                _targetMoney[i] = building.TargetMoney[i];
                if (_selectedLevel == -1 && !SaveManager.Instance.IsLevelDone(i))
                    _selectedLevel = i;
            }

            if (_selectedLevel == -1)
                _selectedLevel = 0;
        }

        public void Update(float gameTime)
        {
            if (_state == TitleScreenState.Title && InputManager.CheckEngagement())
            {
                _state = TitleScreenState.Main;
            }
            else if (InputManager.PlayerDevice != null && _state != TitleScreenState.Title)
            {
                InputManager.PlayerDevice.Update(gameTime);
                InputManager.PlayerDevice.HandleTitleInputs(this);
                
                if (_state == TitleScreenState.Settings)
                {
                    SettingsManager.Instance.Update(gameTime);
                }
            }            
        }

        public void UpdateMouseSelection(int x, int y)
        {
            int previous = _selectionOption;

            switch (_state)
            {
                case TitleScreenState.Title: break;
                case TitleScreenState.Main:
                    if (y >= 115 && y < 130)
                    {
                        _selectionOption = 0;
                    }
                    else if (y >= 130 && y < 145)
                    {
                        _selectionOption = 1;
                    }
                    else if (!Program.RunningOnConsole && y >= 145 && y < 160)
                    {
                        _selectionOption = 2;
                    }
                    break;
                case TitleScreenState.Settings:
                    SettingsManager.Instance.UpdateMouseSelection(x, y);
                    break;
                case TitleScreenState.Play:
                    if (y >= 115 && y < 130)
                    {
                        _selectionOption = 0;
                    }
                    else if (y >= 130 && y < 145)
                    {
                        _selectionOption = 1;
                    }
                    else if (y >= 145 && y < 160)
                    {
                        _selectionOption = 2;
                    }
                    break;
                case TitleScreenState.LevelSelection:
                    // todo
                    break;
                case TitleScreenState.SteamWorkshop:
                    // todo
                    break;
            }

            if (previous != _selectionOption)
                SfxManager.Instance.Play(Sfx.MenuBlip);
        }

        public void PushSelect(bool fromMouse = false, int mouseX = 0)
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
                case TitleScreenState.Settings:
                    SettingsManager.Instance.PushSelect(fromMouse, mouseX);
                    break;
                case TitleScreenState.Play:
                    if (_selectionOption == 0)
                    {
                        _state = TitleScreenState.LevelSelection;
                        // load building
                        ShouldLoadBuilding = true;
                        RequiredBuildingIdentifier = "Default";
                    }
                    //else if (_selectionOption == 1)
                    //    _state = TitleScreenState.SteamWorkshop;
                    else if (_selectionOption == 2)
                        PushBack(fromMouse);
                    break;
                case TitleScreenState.LevelSelection:
                    StartGame = true;
                    break;
                case TitleScreenState.SteamWorkshop:
                    StartGame = true;
                    break;
            }

            if (fromMouse)
                MouseKeyboardInputsHandler.ShouldUpdateMouse = true;
        }

        public void PushBack(bool fromMouse = false)
        {
            _selectionOption = 0;

            switch (_state)
            {
                case TitleScreenState.Title: break;
                case TitleScreenState.Main:
                    _state = TitleScreenState.Title;
                    InputManager.PlayerDevice = null;
                    break;
                case TitleScreenState.Settings:
                    if (!SettingsManager.Instance.PushCancel(fromMouse))
                    {
                        _selectionOption = 1;
                        _state = TitleScreenState.Main;
                        SettingsManager.Instance.Save();
                    }
                    break;
                case TitleScreenState.Play: _state = TitleScreenState.Main; break;
                case TitleScreenState.LevelSelection:
                    _state = TitleScreenState.Play;
                    break;
                case TitleScreenState.SteamWorkshop:
                    _selectionOption = 1;
                    _state = TitleScreenState.Play;
                    break;
            }

            if (fromMouse)
                MouseKeyboardInputsHandler.ShouldUpdateMouse = true;
        }

        public void PushUp()
        {
            int previous = _selectionOption;

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

            if (previous != _selectionOption)
                SfxManager.Instance.Play(Sfx.MenuBlip);
        }

        public void PushDown()
        {
            int previous = _selectionOption;

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

            if (previous != _selectionOption)
                SfxManager.Instance.Play(Sfx.MenuBlip);
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

            if (_state != TitleScreenState.Title)
            {
                UIHelper.Instance.DrawMicrophoneVolume(spriteBatch);
            }

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
            else if (_state == TitleScreenState.LevelSelection || _state == TitleScreenState.SteamWorkshop)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(17, 17, _backgroundRectangles[2].Width, _backgroundRectangles[2].Height),
                    _backgroundRectangles[2],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(186, 42, _backgroundRectangles[3].Width, _backgroundRectangles[3].Height),
                    _backgroundRectangles[3],
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
                // previous levels
                for (int i = 0; i < 3; i++)
                {
                    if (_selectedLevel - 1 - i < 0)
                        continue;

                    // current level
                    spriteBatch.Draw(texture,
                        new Rectangle(106 - i * 41, 68, _selectionRectangle[5].Width, _selectionRectangle[5].Height),
                        _selectionRectangle[5],
                        Color.White);
                }

                // current level
                spriteBatch.Draw(texture,
                    new Rectangle(147, 68, _selectionRectangle[4].Width, _selectionRectangle[4].Height),
                    _selectionRectangle[4],
                    Color.White);

                // next levels
                for (int i = 0; i < 3; i++)
                {
                    if (_selectedLevel + 1 + i > _levelCount)
                        continue;

                    int locked = (SaveManager.Instance.IsLevelDone(_selectedLevel + 1 + i) ? 5 : 6);
                    if (locked == 6 && _selectedLevel + 2 + i < _levelCount && !SaveManager.Instance.IsLevelDone(_selectedLevel + 2 + i) &&
                        _selectedLevel + i >= 0 && SaveManager.Instance.IsLevelDone(_selectedLevel + i))
                        locked = 5;

                    // current level
                    spriteBatch.Draw(texture,
                        new Rectangle(188 + i * 41, 68, _selectionRectangle[locked].Width, _selectionRectangle[locked].Height),
                        _selectionRectangle[locked],
                        Color.White);
                }
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

            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}

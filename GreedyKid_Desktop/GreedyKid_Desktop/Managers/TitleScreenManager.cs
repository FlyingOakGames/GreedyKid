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

        public bool IsWorkshopBuilding = false;

        private Rectangle _viewport;
        private Rectangle[] _backgroundRectangles;
        private Rectangle _titleRectangle;
        private Rectangle[] _selectionRectangle;
        private Rectangle[] _numberRectangle;

        private Color _targetScoreColor = new Color(155, 173, 183);

        private TitleScreenState _state = TitleScreenState.Title;

        private int _selectionOption = 0;        

        private int _selectedLevel = 0;
        private int[] _targetTime;
        private int[] _targetMoney;
        private int _levelCount = 0;
        private int _completedLevels = 0;
        private string _completionString;
        private string _buildingName;

        private string _bestTimeString;
        private string _targetTimeString;
        private string _bestMoneyString;
        private string _targetMoneyString;

        private string[] _workshopIdentifiers = null;
        private string[] _workshopBuildingNames = null;
        private int _workshopOffset = 0;
        private int _workshopMaxItem = 8;

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

            // workshop scan
            _workshopIdentifiers = null;
            _workshopBuildingNames = null;
            if (System.IO.Directory.Exists("Content/Workshop/"))
            {
                _workshopIdentifiers = System.IO.Directory.GetDirectories("Content/Workshop/");
                _workshopBuildingNames = new string[_workshopIdentifiers.Length];
                for (int i = 0; i < _workshopIdentifiers.Length; i++)
                {
                    Building.GetName(_workshopIdentifiers[i], out _workshopIdentifiers[i], out _workshopBuildingNames[i]);
                }
            }
        }

        public int SelectedLevel
        {
            get { return _selectedLevel; }
        }

        public void SetState(TitleScreenState state)
        {
            _state = state;
        }

        public void SetBuilding(Building building, int selectedLevel = -1)
        {
            _buildingName = building.Name.ToUpperInvariant();
            _levelCount = building.LevelCount;
            _targetMoney = new int[_levelCount];
            _targetTime = new int[_levelCount];

            _selectedLevel = -1;

            _completedLevels = 0;

            for (int i = 0; i < _levelCount; i++)
            {                
                _targetTime[i] = building.TargetTime[i];
                _targetMoney[i] = building.TargetMoney[i];
                if (SaveManager.Instance.IsLevelDone(i))
                    _completedLevels++;
                if (_selectedLevel == -1 && !SaveManager.Instance.IsLevelDone(i))
                    _selectedLevel = i;
            }

            if (_selectedLevel == -1)
                _selectedLevel = 0;

            if (selectedLevel != -1)
                _selectedLevel = selectedLevel;

            _completionString = "( " + (_completedLevels < 10 ? "0" : String.Empty) + _completedLevels + "/" + (_levelCount < 10 ? "0" : String.Empty) + _levelCount + " )";

            UpdateScoreStrings();
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
                    _selectionOption = 0;
                    if (y > 60 && y < 130)
                    {
                        if (x < 140)
                            _selectionOption = 1;
                        else if (x > GreedyKidGame.Width - 140)
                            _selectionOption = 2;
                    }
                    break;
                case TitleScreenState.SteamWorkshop:
                    if (y >= 30 && y < _workshopMaxItem * 15 + 30)
                    {
                        _selectionOption = (y - 30) / 15;
                    }
                    else if (y < 30 && _workshopOffset > 0)
                    {
                        _selectionOption = _workshopMaxItem + 1;
                    }
                    else if (y >= _workshopMaxItem * 15 + 30 && _workshopBuildingNames != null && _workshopBuildingNames.Length > _workshopMaxItem && _workshopOffset + _workshopMaxItem < _workshopBuildingNames.Length)
                    {
                        _selectionOption = _workshopMaxItem;
                    }
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
                        IsWorkshopBuilding = false;
                        RequiredBuildingIdentifier = "Default";
                    }
                    else if (_selectionOption == 1)
                    {
                        _selectionOption = 0;
                        _state = TitleScreenState.SteamWorkshop;
                        // scan folder and load level names
                        _workshopOffset = 0;
                        _workshopIdentifiers = null;
                        _workshopBuildingNames = null;
                        if (System.IO.Directory.Exists("Content/Workshop/"))
                        {
                            _workshopIdentifiers = System.IO.Directory.GetDirectories("Content/Workshop/");
                            _workshopBuildingNames = new string[_workshopIdentifiers.Length];
                            for (int i = 0; i < _workshopIdentifiers.Length; i++)
                            {
                                Building.GetName(_workshopIdentifiers[i], out _workshopIdentifiers[i], out _workshopBuildingNames[i]);
                            }
                        }
                    }
                    else if (_selectionOption == 2)
                        PushBack(fromMouse);
                    break;
                case TitleScreenState.LevelSelection:
                    if (fromMouse)
                    {
                        if (_selectionOption == 0)
                            StartGame = true;
                        else if (_selectionOption == 1)
                            PushLeft();
                        else if (_selectionOption == 2)
                            PushRight();
                        else
                            PushBack();
                    }
                    else
                    {
                        StartGame = true;
                    }                                        
                    break;
                case TitleScreenState.SteamWorkshop:
                    if (_selectionOption == _workshopMaxItem) // down
                    {
                        if (_workshopBuildingNames != null && _workshopBuildingNames.Length > _workshopMaxItem && _workshopOffset + _workshopMaxItem < _workshopBuildingNames.Length)
                        {
                            _workshopOffset++;
                        }
                    }
                    else if (_selectionOption == _workshopMaxItem + 1) // up
                    {
                        if (_workshopOffset > 0)
                        {
                            _workshopOffset--;
                        }
                    }
                    else if (_workshopIdentifiers != null && _workshopIdentifiers.Length > 0)
                    {
                        _state = TitleScreenState.LevelSelection;
                        // load building
                        ShouldLoadBuilding = true;
                        IsWorkshopBuilding = true;
                        RequiredBuildingIdentifier = _workshopIdentifiers[_selectionOption + _workshopOffset];
                    }
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
                    _selectionOption = 0;
                    if (IsWorkshopBuilding)
                        _state = TitleScreenState.SteamWorkshop;
                    else
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
                    case TitleScreenState.SteamWorkshop:
                        if (_selectionOption == -1 && _workshopOffset > 0)
                        {
                            _workshopOffset--;
                        }
                        if (_selectionOption < 0)
                        {
                            _selectionOption = 0;
                        }
                        break;
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
                case TitleScreenState.SteamWorkshop:
                    if (_selectionOption == _workshopMaxItem && _workshopBuildingNames != null && _workshopBuildingNames.Length > _workshopMaxItem && _workshopOffset + _workshopMaxItem < _workshopBuildingNames.Length)
                    {
                        _workshopOffset++;
                    }
                    if (_selectionOption > _workshopMaxItem - 1)
                        _selectionOption = _workshopMaxItem - 1;
                    break;
            }

            if (previous != _selectionOption)
                SfxManager.Instance.Play(Sfx.MenuBlip);
        }

        public void PushLeft()
        {
            if (_state == TitleScreenState.Settings)
                SettingsManager.Instance.PushLeft();
            else if (_state == TitleScreenState.LevelSelection)
            {
                int previous = _selectedLevel;

                _selectedLevel--;
                if (_selectedLevel < 0)
                    _selectedLevel = 0;

                if (previous != _selectedLevel)
                {
                    SfxManager.Instance.Play(Sfx.MenuBlip);
                    UpdateScoreStrings();
                }
            }
        }

        public void PushRight()
        {
            if (_state == TitleScreenState.Settings)
                SettingsManager.Instance.PushRight();
            else if (_state == TitleScreenState.LevelSelection)
            {
                int previous = _selectedLevel;

                if (SaveManager.Instance.IsLevelDone(_selectedLevel))
                {
                    _selectedLevel++;
                    if (_selectedLevel >= _levelCount)
                        _selectedLevel = _levelCount - 1;
                }

                if (previous != _selectedLevel)
                {
                    SfxManager.Instance.Play(Sfx.MenuBlip);
                    UpdateScoreStrings();
                }
            }
        }

        private void UpdateScoreStrings()
        {
            int min = _targetTime[_selectedLevel] / 60;
            int sec = _targetTime[_selectedLevel] % 60;
            _targetTimeString = "  (" + (min < 10 ? "0" : String.Empty) + min + ":" + (sec < 10 ? "0" : String.Empty) + sec + ")";

            int money = _targetMoney[_selectedLevel];
            _targetMoneyString = "  ($" + (money < 100 ? (money < 10 ? "00" : "0") : String.Empty) + money + ")";

            if (SaveManager.Instance.IsLevelDone(_selectedLevel))
            {
                min = SaveManager.Instance.LevelTime(_selectedLevel) / 60;
                sec = SaveManager.Instance.LevelTime(_selectedLevel) % 60;
                _bestTimeString = (min < 10 ? "0" : String.Empty) + min + ":" + (sec < 10 ? "0" : String.Empty) + sec;

                money = SaveManager.Instance.LevelMoney(_selectedLevel);
                _bestMoneyString = "$" + (money < 100 ? (money < 10 ? "00" : "0") : String.Empty) + money;
            }
            else
            {
                _bestTimeString = "--:--";
                _bestMoneyString = "$---";
            }
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
            else if (_state == TitleScreenState.Settings || _state == TitleScreenState.LevelSelection || _state == TitleScreenState.SteamWorkshop)
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
            else if (_state == TitleScreenState.SteamWorkshop)
            {
                yStart = 30;

                // title
                UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Workshop);

                if (_workshopIdentifiers != null && _workshopIdentifiers.Length > 0)
                {
                    if (_workshopOffset > 0)
                    {
                        UIHelper.Instance.DrawCenteredText(spriteBatch, "...", yStart - 15, _workshopMaxItem + 1, _selectionOption);
                    }
                    for (int i = 0; i < _workshopMaxItem; i++)
                    {                        
                        int currentId = _workshopOffset + i;
                        if (currentId >= _workshopBuildingNames.Length)
                            break;
                        UIHelper.Instance.DrawCenteredText(spriteBatch, _workshopBuildingNames[currentId], yStart + i * 15, i, _selectionOption);
                    }
                    if (_workshopBuildingNames.Length > _workshopMaxItem && _workshopOffset + _workshopMaxItem < _workshopBuildingNames.Length)
                    {
                        UIHelper.Instance.DrawCenteredText(spriteBatch, "...", yStart + _workshopMaxItem * 15, _workshopMaxItem, _selectionOption);
                    }
                }
                else
                {
                    // no workshop level
                    UIHelper.Instance.DrawCenteredText(spriteBatch, "THERE'S NO WORKSHOP ITEM", 45, -1, 0);

                    UIHelper.Instance.DrawCenteredText(spriteBatch, "GO TO THE STEAM WORKSHOP", 80, -1, 0);
                    UIHelper.Instance.DrawCenteredText(spriteBatch, "AND SUBSCRIBE TO ITEMS TO SEE THEM HERE", 90, -1, 0);
                }
            }
            else if (_state == TitleScreenState.LevelSelection)
            {
                Rectangle[] scoreRectangle = UIHelper.Instance.ScoreRectangles;

                // title
                if (IsWorkshopBuilding)
                    UIHelper.Instance.DrawTitle(spriteBatch, _buildingName);
                else
                    UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Campaign);

                int stars = (int)(_completedLevels / (_levelCount / 3.0f));
                
                // big stars
                spriteBatch.Draw(texture,
                    new Rectangle(
                        GreedyKidGame.Width / 2 - scoreRectangle[0].Width / 2,
                        24,
                        scoreRectangle[0].Width,
                        scoreRectangle[0].Height),
                    scoreRectangle[stars],
                    Color.White);

                // level count
                UIHelper.Instance.DrawCenteredText(spriteBatch, _completionString, 38, -1, 0);

                // separator
                spriteBatch.Draw(texture,
                    new Rectangle(
                        GreedyKidGame.Width / 2 - scoreRectangle[6].Width / 2,
                        54,
                        scoreRectangle[6].Width,
                        scoreRectangle[6].Height),
                    scoreRectangle[6],
                    Color.White);

                int d, e, textWidth;

                // previous levels
                for (int i = 0; i < 3; i++)
                {
                    if (_selectedLevel - 1 - i < 0)
                        continue;

                    // box
                    spriteBatch.Draw(texture,
                        new Rectangle(106 - i * 41, 68, _selectionRectangle[5].Width, _selectionRectangle[5].Height),
                        _selectionRectangle[5],
                        Color.White);

                    // number
                    d = (_selectedLevel - 1 - i + 1) / 10;
                    e = (_selectedLevel - 1 - i + 1) % 10;

                    textWidth = _numberRectangle[e].Width;

                    if (d > 0)
                    {
                        textWidth += _numberRectangle[d].Width;

                        spriteBatch.Draw(texture,
                            new Rectangle(106 - i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2, 85, _numberRectangle[d].Width, _numberRectangle[d].Height),
                            _numberRectangle[d],
                            Color.White);

                        spriteBatch.Draw(texture,
                            new Rectangle(106 - i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2 + _numberRectangle[d].Width, 85, _numberRectangle[e].Width, _numberRectangle[e].Height),
                            _numberRectangle[e],
                            Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(texture,
                            new Rectangle(106 - i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2, 85, _numberRectangle[e].Width, _numberRectangle[e].Height),
                            _numberRectangle[e],
                            Color.White);
                    }

                    // small stars
                    stars = SaveManager.Instance.LevelStars(_selectedLevel - 1 - i);                    
                    spriteBatch.Draw(texture,
                        new Rectangle(106 - i * 41 + 6, 68 + 44, _selectionRectangle[stars].Width, _selectionRectangle[stars].Height),
                        _selectionRectangle[stars],
                        Color.White);
                }

                // current level
                spriteBatch.Draw(texture,
                    new Rectangle(147, 68, _selectionRectangle[4].Width, _selectionRectangle[4].Height),
                    _selectionRectangle[4],
                    Color.White);

                // number
                d = (_selectedLevel + 1) / 10;
                e = (_selectedLevel + 1) % 10;

                textWidth = _numberRectangle[e].Width;

                if (d > 0)
                {
                    textWidth += _numberRectangle[d].Width;

                    spriteBatch.Draw(texture,
                        new Rectangle(147 + _selectionRectangle[5].Width / 2 - textWidth / 2, 85, _numberRectangle[d].Width, _numberRectangle[d].Height),
                        _numberRectangle[d],
                        Color.White);

                    spriteBatch.Draw(texture,
                        new Rectangle(147 + _selectionRectangle[5].Width / 2 - textWidth / 2 + _numberRectangle[d].Width, 85, _numberRectangle[e].Width, _numberRectangle[e].Height),
                        _numberRectangle[e],
                        Color.White);
                }
                else
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(147 + _selectionRectangle[5].Width / 2 - textWidth / 2, 85, _numberRectangle[e].Width, _numberRectangle[e].Height),
                        _numberRectangle[e],
                        Color.White);
                }

                // small stars
                stars = SaveManager.Instance.LevelStars(_selectedLevel);
                spriteBatch.Draw(texture,
                    new Rectangle(147 + 6, 68 + 44, _selectionRectangle[stars].Width, _selectionRectangle[stars].Height),
                    _selectionRectangle[stars],
                    Color.White);

                // next levels
                for (int i = 0; i < 3; i++)
                {
                    if (_selectedLevel + 1 + i >= _levelCount)
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

                    if (locked != 6)
                    {
                        // number
                        d = (_selectedLevel + 1 + i + 1) / 10;
                        e = (_selectedLevel + 1 + i + 1) % 10;

                        textWidth = _numberRectangle[e].Width;

                        if (d > 0)
                        {
                            textWidth += _numberRectangle[d].Width;

                            spriteBatch.Draw(texture,
                                new Rectangle(188 + i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2, 85, _numberRectangle[d].Width, _numberRectangle[d].Height),
                                _numberRectangle[d],
                                Color.White);

                            spriteBatch.Draw(texture,
                                new Rectangle(188 + i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2 + _numberRectangle[d].Width, 85, _numberRectangle[e].Width, _numberRectangle[e].Height),
                                _numberRectangle[e],
                                Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(texture,
                                new Rectangle(188 + i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2, 85, _numberRectangle[e].Width, _numberRectangle[e].Height),
                                _numberRectangle[e],
                                Color.White);
                        }

                        // small stars
                        stars = SaveManager.Instance.LevelStars(_selectedLevel + 1 + i);
                        spriteBatch.Draw(texture,
                            new Rectangle(188 + i * 41 + 6, 68 + 44, _selectionRectangle[stars].Width, _selectionRectangle[stars].Height),
                            _selectionRectangle[stars],
                            Color.White);
                    }
                }

                // separator
                spriteBatch.Draw(texture,
                    new Rectangle(
                        GreedyKidGame.Width / 2 - scoreRectangle[6].Width / 2,
                        123,
                        scoreRectangle[6].Width,
                        scoreRectangle[6].Height),
                    scoreRectangle[6],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically,
                    0.0f
                    );

                SpriteFont genericFont = TextManager.Instance.GenericFont;
                SpriteFont font = TextManager.Instance.Font;

                // time
                int time = SaveManager.Instance.LevelTime(_selectedLevel);
                int icon = (time <= _targetTime[_selectedLevel] ? 4 : 5);
                if (!SaveManager.Instance.IsLevelDone(_selectedLevel))
                    icon = 5;
                int timeWidth = scoreRectangle[icon].Width + (int)font.MeasureString(TextManager.Instance.Time).X + (int)genericFont.MeasureString(_bestTimeString).X + (int)genericFont.MeasureString(_targetTimeString).X;
                int timeX = GreedyKidGame.Width / 2 - timeWidth / 2;
                spriteBatch.Draw(texture,
                    new Rectangle(
                        timeX,
                        139,
                        scoreRectangle[icon].Width,
                        scoreRectangle[icon].Height),
                    scoreRectangle[icon],
                    Color.White);
                spriteBatch.DrawString(font,
                    TextManager.Instance.Time,
                    new Vector2(timeX + scoreRectangle[icon].Width, 136),
                    Color.White);
                spriteBatch.DrawString(font,
                    _bestTimeString,
                    new Vector2(timeX + scoreRectangle[icon].Width + (int)genericFont.MeasureString(TextManager.Instance.Time).X, 136),
                    Color.White);
                spriteBatch.DrawString(font,
                    _targetTimeString,
                    new Vector2(timeX + timeWidth - (int)genericFont.MeasureString(_targetTimeString).X, 136),
                    _targetScoreColor);

                // money
                int money = SaveManager.Instance.LevelMoney(_selectedLevel);
                icon = (money >= _targetMoney[_selectedLevel] ? 4 : 5);
                if (!SaveManager.Instance.IsLevelDone(_selectedLevel))
                    icon = 5;
                int moneyWidth = scoreRectangle[icon].Width + (int)font.MeasureString(TextManager.Instance.Money).X + (int)genericFont.MeasureString(_bestMoneyString).X + (int)genericFont.MeasureString(_targetMoneyString).X;
                int moneyX = GreedyKidGame.Width / 2 - moneyWidth / 2;
                spriteBatch.Draw(texture,
                    new Rectangle(
                        moneyX,
                        154,
                        scoreRectangle[icon].Width,
                        scoreRectangle[icon].Height),
                    scoreRectangle[icon],
                    Color.White);
                spriteBatch.DrawString(font,
                    TextManager.Instance.Money,
                    new Vector2(moneyX + scoreRectangle[icon].Width, 151),
                    Color.White);
                spriteBatch.DrawString(font,
                    _bestMoneyString,
                    new Vector2(moneyX + scoreRectangle[icon].Width + (int)genericFont.MeasureString(TextManager.Instance.Money).X, 151),
                    Color.White);
                spriteBatch.DrawString(font,
                    _targetMoneyString,
                    new Vector2(moneyX + moneyWidth - (int)genericFont.MeasureString(_targetMoneyString).X, 151),
                    _targetScoreColor);

                // arrows
                int arrow = (_selectedLevel > 0 ? 8 : 7);
                spriteBatch.Draw(texture,
                    new Rectangle(13, 83, _selectionRectangle[arrow].Width, _selectionRectangle[arrow].Height),
                    _selectionRectangle[arrow],
                    Color.White);

                arrow = (SaveManager.Instance.IsLevelDone(_selectedLevel) && _selectedLevel < _levelCount - 1 ? 10 : 9);
                spriteBatch.Draw(texture,
                    new Rectangle(305, 83, _selectionRectangle[arrow].Width, _selectionRectangle[arrow].Height),
                    _selectionRectangle[arrow],
                    Color.White);
            }            

            // commands
            if (_state == TitleScreenState.Title)
            {
                UIHelper.Instance.DrawCommand(spriteBatch, GreedyKidGame.Version, CommandType.None, true);
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Press, CommandType.Select);
            }
            else if (_state != TitleScreenState.SteamWorkshop || (_state == TitleScreenState.SteamWorkshop && _workshopIdentifiers != null && _workshopIdentifiers.Length > 0))
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

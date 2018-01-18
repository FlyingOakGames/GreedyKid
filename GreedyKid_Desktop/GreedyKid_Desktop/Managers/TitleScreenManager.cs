using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public enum TitleScreenState
    {
        Title,
        Main,
        Play,
        LevelSelection,
        Workshop,
        Settings,
    }

    public enum RequestedTransition
    {
        None,
        ToGameplay,
        ToSettings,
        ToMainMenu,
        ToLevelSelection,
        ToPlayMenuFromLevelSelection,
        ToPlayMenuFromWorkshop,
        ToWorkshopMenu,
    }

    public sealed class TitleScreenManager
    {
        private bool _waitForTransition = false;
        private RequestedTransition _requestedTransition = RequestedTransition.None;
        public bool StartGame = false;
        public bool ShouldLoadBuilding = false;
        public string RequiredBuildingIdentifier = Building.MainCampaignIdentifier;
        public bool IsSteamWorkshopBuilding = false;

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
        private bool[] _workshopSteamFolder = null;
        private bool[] _workshopIsDownloading = null;
        private int _workshopOffset = 0;
        private int _workshopMaxItem = 8;

        // title animation
        private int _currentAnimationFrame = 0;
        private float _currentAnimationFrameTime = 0.0f;
        private const float _animationFrameTime = 0.1f;
        private Rectangle[] _animationFrames;
        private bool _reverseToTitle = false;

        public TitleScreenManager()
        {
            _viewport = new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height);

            // title animation
            _animationFrames = new Rectangle[13];
            _animationFrames[0] = new Rectangle(898, 1864, 296, 152); // background
            _animationFrames[1] = new Rectangle(1195, 1864, 158, 122); // retiree
            _animationFrames[2] = new Rectangle(1354, 1864, 65, 83); // kid
            _animationFrames[3] = new Rectangle(1420, 1864, 52, 74); // hat
            _animationFrames[4] = new Rectangle(898, 2017, 276, 2); // mask 1
            _animationFrames[5] = new Rectangle(898, 2020, 276, 2); // mask 2
            _animationFrames[6] = new Rectangle(898, 2023, 276, 2); // mask 3
            _animationFrames[7] = new Rectangle(801, 1772, 193, 33 ); // logo 1
            _animationFrames[8] = new Rectangle(995, 1728, 121, 77); // logo 2
            _animationFrames[9] = new Rectangle(1117, 1716, 109, 89 ); // logo 3
            _animationFrames[10] = new Rectangle(1227, 1722, 113, 83); // logo 4
            _animationFrames[11] = new Rectangle(1341, 1688, 95, 117); // logo 5
            _animationFrames[12] = new Rectangle(1437, 1722, 117, 83); // logo 6

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
            _numberRectangle = new Rectangle[11];
            for (int i = 0; i < 10; i++)
            {
                _numberRectangle[i] = new Rectangle(230 + i * 11, TextureManager.GameplayHeight - 195, 11, 10);
            }
            _numberRectangle[10] = new Rectangle(212, TextureManager.GameplayHeight - 198, 17, 13);

            ScanWorkshop();
        }

        private void ScanWorkshop(bool resetSelection = false)
        {
            if (resetSelection)
                _selectionOption = 0;

            // workshop scan
            _workshopOffset = 0;
            _workshopIdentifiers = null;
            _workshopBuildingNames = null;
            _workshopSteamFolder = null;
            _workshopIsDownloading = null;

            // local files
            string[] localWorkshopIdentifiers = new string[0];
            string[] localWorkshopBuildingNames = new string[0];
            if (System.IO.Directory.Exists(Building.LocalWorkshopPath))
            {
                localWorkshopIdentifiers = System.IO.Directory.GetDirectories(Building.LocalWorkshopPath);
                localWorkshopBuildingNames = new string[localWorkshopIdentifiers.Length];
                for (int i = 0; i < localWorkshopIdentifiers.Length; i++)
                {
                    try
                    {
                        Building.GetName(localWorkshopIdentifiers[i], out localWorkshopIdentifiers[i], out localWorkshopBuildingNames[i]);
                    }
                    catch (Exception)
                    {
                        localWorkshopBuildingNames[i] = "DOWNLOADING...";
                    }
                }
            }

            // steam files
            string[] steamWorkshopIdentifiers = new string[0];
            string[] steamWorkshopBuildingNames = new string[0];
            string workshopPath = Helper.SteamworksHelper.Instance.WorkshopPath;
            if (System.IO.Directory.Exists(workshopPath)) // dir ?
            {
                steamWorkshopIdentifiers = System.IO.Directory.GetDirectories(workshopPath);
                steamWorkshopBuildingNames = new string[steamWorkshopIdentifiers.Length];
                for (int i = 0; i < steamWorkshopIdentifiers.Length; i++)
                {                    
                    try
                    {
                        Building.GetName(steamWorkshopIdentifiers[i], out steamWorkshopIdentifiers[i], out steamWorkshopBuildingNames[i]);
                    }
                    catch (Exception)
                    {
                        steamWorkshopBuildingNames[i] = "DOWNLOADING...";
                    }
                }
            }

            // mergning
            if (localWorkshopIdentifiers.Length + steamWorkshopIdentifiers.Length > 0)
            {
                _workshopIdentifiers = new string[localWorkshopIdentifiers.Length + steamWorkshopIdentifiers.Length];
                _workshopBuildingNames = new string[localWorkshopBuildingNames.Length + steamWorkshopBuildingNames.Length];
                _workshopSteamFolder = new bool[localWorkshopBuildingNames.Length + steamWorkshopBuildingNames.Length];
                _workshopIsDownloading = new bool[localWorkshopBuildingNames.Length + steamWorkshopBuildingNames.Length];

                for (int i = 0; i < localWorkshopIdentifiers.Length; i++)
                {
                    _workshopIdentifiers[i] = localWorkshopIdentifiers[i];
                    _workshopBuildingNames[i] = localWorkshopBuildingNames[i];
                    _workshopSteamFolder[i] = false;
                    if (_workshopBuildingNames[i] == "DOWNLOADING...")
                        _workshopIsDownloading[i] = true;
                    else
                        _workshopIsDownloading[i] = false;
                }

                for (int i = 0; i < steamWorkshopIdentifiers.Length; i++)
                {
                    _workshopIdentifiers[localWorkshopIdentifiers.Length + i] = steamWorkshopIdentifiers[i];
                    _workshopBuildingNames[localWorkshopIdentifiers.Length + i] = steamWorkshopBuildingNames[i];
                    _workshopSteamFolder[localWorkshopIdentifiers.Length + i] = true;
                    if (_workshopBuildingNames[i] == "DOWNLOADING...")
                        _workshopIsDownloading[i] = true;
                    else
                        _workshopIsDownloading[i] = false;
                }
            }
        }

        private bool _localWorkshopWatched = false;
        private bool _steamWorkshopWatched = false;

        private void WatchWorkshopFolders()
        {
            if (_localWorkshopWatched == false)
            {
                try
                {
                    CreateFileWatcher(Building.LocalWorkshopPath);
                    _localWorkshopWatched = true;
                }
                catch (Exception) { }
            }

            if (_steamWorkshopWatched == false)
            { 
                try
                {
                    CreateFileWatcher(Helper.SteamworksHelper.Instance.WorkshopPath);
                    _steamWorkshopWatched = true;
                }
                catch (Exception) { }                
            }    
        }

        private void CreateFileWatcher(string path)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
            // Only watch text files.
            watcher.Filter = "*.*";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnWorkshopChanged);
            watcher.Created += new FileSystemEventHandler(OnWorkshopChanged);
            watcher.Deleted += new FileSystemEventHandler(OnWorkshopChanged);
            watcher.Renamed += new RenamedEventHandler(OnWorkshopRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private void OnWorkshopChanged(object source, FileSystemEventArgs e)
        {
            if (_state == TitleScreenState.Workshop)
                ScanWorkshop(true);
        }

        private void OnWorkshopRenamed(object source, RenamedEventArgs e)
        {
            if (_state == TitleScreenState.Workshop)
                ScanWorkshop(true);
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

            if (IsWorkshopBuilding && !SaveManager.Instance.IsLevelDone(-1))
                SaveManager.Instance.SetIntro();

            if (!SaveManager.Instance.IsLevelDone(-1))
                _selectedLevel = -1;

            _completionString = "( " + (_completedLevels < 10 ? "0" : String.Empty) + _completedLevels + "/" + (_levelCount < 10 ? "0" : String.Empty) + _levelCount + " )";

            if (_selectedLevel >= 0 && _selectedLevel < _levelCount)
                UpdateScoreStrings();
        }

        public void Update(float gameTime)
        {            
            _currentAnimationFrameTime += gameTime;
            if (_currentAnimationFrameTime >= _animationFrameTime)
            {
                _currentAnimationFrameTime -= _animationFrameTime;
                if (_reverseToTitle)
                    _currentAnimationFrame--;
                else
                    _currentAnimationFrame++;

                if (_state == TitleScreenState.Title && _currentAnimationFrame == 15)
                    _currentAnimationFrame = 9;
                else if (_state != TitleScreenState.Title && _currentAnimationFrame == 4)
                    _currentAnimationFrame = 3;
                else if (_state != TitleScreenState.Title && _currentAnimationFrame < 0)
                {
                    _currentAnimationFrame = 9;
                    _state = TitleScreenState.Title;
                    _reverseToTitle = false;
                }
            }
            

            if (_state == TitleScreenState.Title && InputManager.CheckEngagement() && _currentAnimationFrame >= 9)
            {
                _currentAnimationFrame = 0;
                _currentAnimationFrameTime = 0.0f;
                _state = TitleScreenState.Main;
                _reverseToTitle = false;
            }
            else if (InputManager.PlayerDevice != null && _state != TitleScreenState.Title && _currentAnimationFrame >= 3)
            {
                InputManager.PlayerDevice.Update(gameTime);
                InputManager.PlayerDevice.HandleTitleInputs(this);

                if (_state == TitleScreenState.Settings)
                {
                    SettingsManager.Instance.Update(gameTime);
                }
            }

            if (_waitForTransition && TransitionManager.Instance.IsDone)
            {
                _waitForTransition = false;
                switch (_requestedTransition)
                {
                    case RequestedTransition.ToGameplay:
                        StartGame = true;
                        break;
                    case RequestedTransition.ToSettings:
                        _state = TitleScreenState.Settings;
                        SettingsManager.Instance.Reset();
                        TransitionManager.Instance.AppearTransition();
                        break;
                    case RequestedTransition.ToMainMenu:
                        _selectionOption = 1;
                        _state = TitleScreenState.Main;
                        SettingsManager.Instance.Save();
                        TransitionManager.Instance.AppearTransition();
                        break;
                    case RequestedTransition.ToLevelSelection:
                        _state = TitleScreenState.LevelSelection;
                        // load building
                        ShouldLoadBuilding = true;
                        IsWorkshopBuilding = false;
                        RequiredBuildingIdentifier = Building.MainCampaignIdentifier;
                        TransitionManager.Instance.AppearTransition();
                        break;
                    case RequestedTransition.ToWorkshopMenu:                        
                        _state = TitleScreenState.Workshop;
                        // scan folder and load level names                        
                        ScanWorkshop(true);
                        WatchWorkshopFolders();
                        TransitionManager.Instance.AppearTransition();
                        break;
                    case RequestedTransition.ToPlayMenuFromWorkshop:
                        _selectionOption = 1;
                        _state = TitleScreenState.Play;
                        TransitionManager.Instance.AppearTransition();
                        break;
                    case RequestedTransition.ToPlayMenuFromLevelSelection:
                        _state = TitleScreenState.Play;
                        TransitionManager.Instance.AppearTransition();
                        break;
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
                    if (y >= 122 && y < 137)
                    {
                        _selectionOption = 0;
                    }
                    else if (y >= 137 && y < 152)
                    {
                        _selectionOption = 1;
                    }
                    else if (!Program.RunningOnConsole && y >= 152 && y < 167)
                    {
                        _selectionOption = 2;
                    }
                    break;
                case TitleScreenState.Settings:
                    SettingsManager.Instance.UpdateMouseSelection(x, y);
                    break;
                case TitleScreenState.Play:
                    if (y >= 122 && y < 137)
                    {
                        _selectionOption = 0;
                    }
                    else if (y >= 137 && y < 152)
                    {
                        _selectionOption = 1;
                    }
                    else if (y >= 152 && y < 167)
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
                case TitleScreenState.Workshop:
                    if (_workshopBuildingNames == null || _workshopBuildingNames.Length == 0)
                    {
                        _selectionOption = 0;
                    }
                    else if (y >= 30 && y < _workshopMaxItem * 15 + 30)
                    {
                        _selectionOption = (y - 30) / 15;
                        if (_workshopBuildingNames != null && _workshopBuildingNames.Length < _workshopMaxItem && _selectionOption >= _workshopBuildingNames.Length)
                            _selectionOption = _workshopBuildingNames.Length - 1;
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
                    else if (_selectionOption == 1 && !_waitForTransition)
                    {
                        _requestedTransition = RequestedTransition.ToSettings;
                        _waitForTransition = true;
                        TransitionManager.Instance.DisappearTransition();                        
                    }
                    else if (_selectionOption == 2)
                        GreedyKidGame.ShouldExit = true;
                    break;
                case TitleScreenState.Settings:
                    SettingsManager.Instance.PushSelect(fromMouse, mouseX);
                    break;
                case TitleScreenState.Play:
                    if (_selectionOption == 0 && !_waitForTransition)
                    {
                        _requestedTransition = RequestedTransition.ToLevelSelection;
                        _waitForTransition = true;
                        TransitionManager.Instance.DisappearTransition();
                    }
                    else if (_selectionOption == 1 && !_waitForTransition)
                    {
                        _requestedTransition = RequestedTransition.ToWorkshopMenu;
                        _waitForTransition = true;
                        TransitionManager.Instance.DisappearTransition();
                    }
                    else if (_selectionOption == 2)
                        PushBack(fromMouse);
                    break;
                case TitleScreenState.LevelSelection:
                    if (fromMouse)
                    {
                        if (_selectionOption == 0 && !_waitForTransition)
                        {
                            _requestedTransition = RequestedTransition.ToGameplay;
                            _waitForTransition = true;
                            TransitionManager.Instance.DisappearTransition();
                        }
                        else if (_selectionOption == 1)
                            PushLeft();
                        else if (_selectionOption == 2)
                            PushRight();
                        else
                            PushBack();
                    }
                    else if (!_waitForTransition)
                    {
                        _requestedTransition = RequestedTransition.ToGameplay;
                        _waitForTransition = true;
                        TransitionManager.Instance.DisappearTransition();
                    }                                        
                    break;
                case TitleScreenState.Workshop:
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
                        if (_workshopIsDownloading[_selectionOption + _workshopOffset] == false)
                        {
                            _state = TitleScreenState.LevelSelection;
                            // load building
                            ShouldLoadBuilding = true;
                            IsWorkshopBuilding = true;
                            RequiredBuildingIdentifier = _workshopIdentifiers[_selectionOption + _workshopOffset];
                            IsSteamWorkshopBuilding = _workshopSteamFolder[_selectionOption + _workshopOffset];
                        }
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
                    _reverseToTitle = true;
                    _currentAnimationFrameTime = 0.0f;
                    InputManager.PlayerDevice = null;
                    break;
                case TitleScreenState.Settings:
                    if (!SettingsManager.Instance.PushCancel(fromMouse) && !_waitForTransition)
                    {
                        _requestedTransition = RequestedTransition.ToMainMenu;
                        _waitForTransition = true;
                        TransitionManager.Instance.DisappearTransition();
                    }
                    break;
                case TitleScreenState.Play:
                    _state = TitleScreenState.Main;
                    break;
                case TitleScreenState.LevelSelection:
                    _selectionOption = 0;
                    if (IsWorkshopBuilding)
                    {
                        _state = TitleScreenState.Workshop;
                        ScanWorkshop(true);
                        WatchWorkshopFolders();
                    }
                    else if (!_waitForTransition)
                    {
                        _requestedTransition = RequestedTransition.ToPlayMenuFromLevelSelection;
                        _waitForTransition = true;
                        TransitionManager.Instance.DisappearTransition();
                    }
                    break;
                case TitleScreenState.Workshop:
                    if (!_waitForTransition)
                    {
                        _requestedTransition = RequestedTransition.ToPlayMenuFromWorkshop;
                        _waitForTransition = true;
                        TransitionManager.Instance.DisappearTransition();
                    }
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
                    case TitleScreenState.Workshop:
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
                case TitleScreenState.Workshop:
                    if (_selectionOption == _workshopMaxItem && _workshopBuildingNames != null && _workshopBuildingNames.Length > _workshopMaxItem && _workshopOffset + _workshopMaxItem < _workshopBuildingNames.Length)
                    {
                        _workshopOffset++;
                    }
                    if (_selectionOption > _workshopMaxItem - 1)
                        _selectionOption = _workshopMaxItem - 1;
                    if (_workshopBuildingNames != null && _workshopBuildingNames.Length < _workshopMaxItem && _selectionOption >= _workshopBuildingNames.Length)
                        _selectionOption = _workshopBuildingNames.Length - 1;
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
                if (!IsWorkshopBuilding && _selectedLevel < -1)
                    _selectedLevel = -1;
                else if (IsWorkshopBuilding && _selectedLevel < 0)
                    _selectedLevel = 0;

                if (previous != _selectedLevel)
                {
                    SfxManager.Instance.Play(Sfx.MenuBlip);
                    if (_selectedLevel >= 0 && _selectedLevel < _levelCount)
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

                if (!IsWorkshopBuilding)
                {
                    if ((_selectedLevel < _levelCount - 1 && SaveManager.Instance.IsLevelDone(_selectedLevel))
                        || (_selectedLevel == _levelCount - 1 && SaveManager.Instance.IsLevelDone(_selectedLevel + 1))
                        || (_selectedLevel == _levelCount && SaveManager.Instance.IsLevelDone(_selectedLevel + 1)))
                    {
                        _selectedLevel++;
                        if (_selectedLevel >= _levelCount + 2)
                            _selectedLevel = _levelCount + 1;
                    }
                }
                else
                {
                    if (_selectedLevel < _levelCount - 1 && SaveManager.Instance.IsLevelDone(_selectedLevel))
                    {
                        _selectedLevel++;
                        if (_selectedLevel >= _levelCount)
                            _selectedLevel = _levelCount - 1;
                    }
                }

                if (previous != _selectedLevel)
                {
                    SfxManager.Instance.Play(Sfx.MenuBlip);
                    if (_selectedLevel >= 0 && _selectedLevel < _levelCount)
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

            UIHelper.Instance.DrawBorders(spriteBatch, Vector2.Zero);

            if (_state != TitleScreenState.Title)
            {
                UIHelper.Instance.DrawMicrophoneVolume(spriteBatch, Vector2.Zero);
            }

            // background
            if (_state == TitleScreenState.Title || _state == TitleScreenState.Main || _state == TitleScreenState.Play)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(16, 16, _animationFrames[0].Width, _animationFrames[0].Height),
                    _animationFrames[0],
                    Color.White);
            }
            else if (_state == TitleScreenState.LevelSelection || _state == TitleScreenState.Workshop)
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

            

            // title animation
            if (_state == TitleScreenState.Title)
            {
                // flash
                if (_currentAnimationFrame < 4)
                    spriteBatch.Draw(TextureManager.Gameplay,
                        new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                        UIHelper.Instance.PixelRectangle,
                        UIHelper.Instance.BackgroundColor);
                if (_currentAnimationFrame == 4)
                    spriteBatch.Draw(TextureManager.Gameplay,
                        new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                        UIHelper.Instance.PixelRectangle,
                        Color.White);
                if (_currentAnimationFrame == 5)
                    spriteBatch.Draw(TextureManager.Gameplay,
                        new Rectangle((GreedyKidGame.Width - _animationFrames[4].Width) / 2, GreedyKidGame.Height - 8, _animationFrames[4].Width, _animationFrames[4].Height),
                        _animationFrames[4],
                        Color.White);
                if (_currentAnimationFrame == 1)
                    spriteBatch.Draw(TextureManager.Gameplay,
                        new Rectangle(155, 91, 18, 4),
                        UIHelper.Instance.PixelRectangle,
                        Color.White);
                else if (_currentAnimationFrame == 2)
                    UIHelper.Instance.DrawWhiteBorders(spriteBatch, GreedyKidGame.Height - 160, Color.White);
                else if (_currentAnimationFrame == 3)
                    UIHelper.Instance.DrawWhiteBorders(spriteBatch, GreedyKidGame.Height, Color.White);
                else if (_currentAnimationFrame == 4)
                    UIHelper.Instance.DrawWhiteBorders(spriteBatch, GreedyKidGame.Height, UIHelper.Instance.LightBordersColor);

                // elements
                int retireeX = -1;
                int kidX = -1;
                int hatY = -1;
                if (_currentAnimationFrame == 5)
                {
                    retireeX = -98;
                    kidX = 299;
                    hatY = -25;
                }
                if (_currentAnimationFrame == 6)
                {
                    retireeX = -12;
                    kidX = 225;
                    hatY = 5;
                }
                if (_currentAnimationFrame == 7)
                {
                    retireeX = -10;
                    kidX = 223;
                    hatY = 7;
                }
                else if (_currentAnimationFrame >= 8)
                {
                    retireeX = -12;
                    kidX = 225;
                    hatY = 5;
                }

                if (retireeX != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(retireeX, 6, _animationFrames[1].Width, _animationFrames[1].Height),
                        _animationFrames[1],
                        Color.White);
                if (kidX != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(kidX, 66, _animationFrames[2].Width, _animationFrames[2].Height),
                        _animationFrames[2],
                        Color.White);
                if (hatY != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(186, hatY, _animationFrames[3].Width, _animationFrames[3].Height),
                        _animationFrames[3],
                        Color.White);

                // logo
                int logoX = -1;
                int logoY = -1;
                int logoFrame = -1;

                if (_currentAnimationFrame == 6)
                {
                    logoX = 70;
                    logoY = 107;
                    logoFrame = 7;
                }
                else if (_currentAnimationFrame == 7)
                {
                    logoX = 106;
                    logoY = 85;
                    logoFrame = 8;
                }
                else if (_currentAnimationFrame == 8)
                {
                    logoX = 112;
                    logoY = 78;
                    logoFrame = 9;
                }
                else if (_currentAnimationFrame >= 9)
                {
                    logoX = 110;
                    logoY = 82;
                    logoFrame = 10;
                }

                if (logoFrame != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(logoX, logoY, _animationFrames[logoFrame].Width, _animationFrames[logoFrame].Height),
                        _animationFrames[logoFrame],
                        Color.White);
            }
            else if (_state == TitleScreenState.Main || _state == TitleScreenState.Play)
            {
                // background mask
                if (_currentAnimationFrame == 0)
                {
                    spriteBatch.Draw(TextureManager.Gameplay,
                        new Rectangle(121, 10, 87, GreedyKidGame.Height - 20),
                        UIHelper.Instance.PixelRectangle,
                        UIHelper.Instance.BackgroundColor);
                    spriteBatch.Draw(TextureManager.Gameplay,
                            new Rectangle((GreedyKidGame.Width - _animationFrames[5].Width) / 2, GreedyKidGame.Height - 8, _animationFrames[5].Width, _animationFrames[5].Height),
                            _animationFrames[5],
                            Color.White);
                }
                else
                {
                    if (_currentAnimationFrame == 1)
                        spriteBatch.Draw(TextureManager.Gameplay,
                            new Rectangle((GreedyKidGame.Width - _animationFrames[6].Width) / 2, GreedyKidGame.Height - 8, _animationFrames[6].Width, _animationFrames[6].Height),
                            _animationFrames[6],
                            Color.White);

                    spriteBatch.Draw(TextureManager.Gameplay,
                        new Rectangle(122, 10, 85, GreedyKidGame.Height - 20),
                        UIHelper.Instance.PixelRectangle,
                        UIHelper.Instance.BackgroundColor);
                }

                // elements
                int retireeX = -1;
                int kidX = -1;
                int hatX = -1;
                if (_currentAnimationFrame == 0)
                {
                    retireeX = -21;
                    kidX = 240;
                    hatX = 201;
                }
                if (_currentAnimationFrame == 1)
                {
                    retireeX = -22;
                    kidX = 241;
                    hatX = 202;
                }
                if (_currentAnimationFrame >= 2)
                {
                    retireeX = -21;
                    kidX = 240;
                    hatX = 201;
                }

                if (retireeX != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(retireeX, 6, _animationFrames[1].Width, _animationFrames[1].Height),
                        _animationFrames[1],
                        Color.White);
                if (kidX != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(kidX, 66, _animationFrames[2].Width, _animationFrames[2].Height),
                        _animationFrames[2],
                        Color.White);
                if (hatX != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(hatX, 5, _animationFrames[3].Width, _animationFrames[3].Height),
                        _animationFrames[3],
                        Color.White);

                // logo
                int logoX = -1;
                int logoY = -1;
                int logoFrame = -1;

                if (_currentAnimationFrame == 0)
                {
                    logoX = 119;
                    logoY = 33;
                    logoFrame = 11;
                }
                else if (_currentAnimationFrame == 1)
                {
                    logoX = 108;
                    logoY = 31;
                    logoFrame = 12;
                }
                else if (_currentAnimationFrame >= 2)
                {
                    logoX = 110;
                    logoY = 32;
                    logoFrame = 10;
                }

                if (logoFrame != -1)
                    spriteBatch.Draw(texture,
                        new Rectangle(logoX, logoY, _animationFrames[logoFrame].Width, _animationFrames[logoFrame].Height),
                        _animationFrames[logoFrame],
                        Color.White);
            }
            

            int yStart = 105;

            if (_state == TitleScreenState.Main || _state == TitleScreenState.Play)
            {
                yStart = 30;
            }
            else if (_state == TitleScreenState.Settings || _state == TitleScreenState.LevelSelection || _state == TitleScreenState.Workshop)
            {
                yStart = -1;
            }

            // title
            /*
            if (yStart >= 0)
                spriteBatch.Draw(texture,
                    new Rectangle(
                        GreedyKidGame.Width / 2 - _titleRectangle.Width / 2 - 1,
                        yStart,
                        _titleRectangle.Width,
                        _titleRectangle.Height),
                    _titleRectangle,
                    Color.White);*/

            // text
            if (_state == TitleScreenState.Main && _currentAnimationFrame > 0)
            {
                yStart = 122;

                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Play, yStart, 0, _selectionOption);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Settings, yStart + 15, 1, _selectionOption);
                if (!Program.RunningOnConsole)
                    UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Quit, yStart + 30, 2, _selectionOption);
            }
            else if (_state == TitleScreenState.Play && _currentAnimationFrame > 0)
            {
                yStart = 122;

                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Campaign, yStart, 0, _selectionOption);
                UIHelper.Instance.DrawCenteredText(spriteBatch, (Helper.SteamworksHelper.Instance.IsReady ? TextManager.Instance.Workshop : TextManager.Instance.Custom), yStart + 15, 1, _selectionOption);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Back, yStart + 30, 2, _selectionOption);
            }            
            else if (_state == TitleScreenState.Settings)
            {
                SettingsManager.Instance.Draw(spriteBatch);
            }
            else if (_state == TitleScreenState.Workshop)
            {
                yStart = 30;

                // title
                UIHelper.Instance.DrawTitle(spriteBatch, (Helper.SteamworksHelper.Instance.IsReady ? TextManager.Instance.Workshop : TextManager.Instance.Custom));

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
                    if (Helper.SteamworksHelper.Instance.IsReady)
                    {
                        UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.WorkshopNotice1, 45, -1, 0);

                        UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.WorkshopNotice2, 80, -1, 0);
                        UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.WorkshopNotice3, 90, -1, 0);
                    }
                    else
                    {
                        UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.WorkshopNotice4, 45, -1, 0);

                        UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.WorkshopNotice5, 80, -1, 0);
                        UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.WorkshopNotice6, 90, -1, 0);
                    }
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
                    if (!IsWorkshopBuilding && _selectedLevel - 1 - i < -1)
                        continue;
                    else if (IsWorkshopBuilding && _selectedLevel - 1 - i < 0)
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

                    if (_selectedLevel - 1 - i >= _levelCount || (d == 0 && e == 0))
                    {
                        spriteBatch.Draw(texture,
                            new Rectangle(103 - i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2, 83, _numberRectangle[10].Width, _numberRectangle[10].Height),
                            _numberRectangle[10],
                            Color.White);
                    }
                    else if (d > 0)
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

                    if ((d > 0 || e > 0) && _selectedLevel - 1 - i < _levelCount)
                    {
                        // small stars
                        stars = SaveManager.Instance.LevelStars(_selectedLevel - 1 - i);
                        spriteBatch.Draw(texture,
                            new Rectangle(106 - i * 41 + 6, 68 + 44, _selectionRectangle[stars].Width, _selectionRectangle[stars].Height),
                            _selectionRectangle[stars],
                            Color.White);
                    }
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

                if ((d == 0 && e == 0) || _selectedLevel >= _levelCount)
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(144 + _selectionRectangle[5].Width / 2 - textWidth / 2, 83, _numberRectangle[10].Width, _numberRectangle[10].Height),
                        _numberRectangle[10],
                        Color.White);
                }
                else if (d > 0)
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

                if (_selectedLevel >= 0 && _selectedLevel < _levelCount)
                {
                    // small stars
                    stars = SaveManager.Instance.LevelStars(_selectedLevel);
                    spriteBatch.Draw(texture,
                        new Rectangle(147 + 6, 68 + 44, _selectionRectangle[stars].Width, _selectionRectangle[stars].Height),
                        _selectionRectangle[stars],
                        Color.White);
                }

                // next levels
                for (int i = 0; i < 3; i++)
                {
                    if (!IsWorkshopBuilding && _selectedLevel + 1 + i >= _levelCount + 2)
                        continue;
                    else if (IsWorkshopBuilding && _selectedLevel + 1 + i >= _levelCount)
                        continue;

                    int locked = (SaveManager.Instance.IsLevelDone(_selectedLevel + 1 + i) ? 5 : 6);
                    if (locked == 6)
                    {
                        if (_selectedLevel + 1 + i < _levelCount && SaveManager.Instance.IsLevelDone(_selectedLevel + i))
                            locked = 5;
                        else if (_selectedLevel + 1 + i >= _levelCount && SaveManager.Instance.IsLevelDone(_selectedLevel + 1 + i))
                            locked = 5;
                    }

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

                        if (_selectedLevel + 1 + i >= _levelCount)
                        {
                            spriteBatch.Draw(texture,
                                new Rectangle(185 + i * 41 + _selectionRectangle[5].Width / 2 - textWidth / 2, 83, _numberRectangle[10].Width, _numberRectangle[10].Height),
                                _numberRectangle[10],
                                Color.White);
                        }
                        else if (d > 0)
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

                        if (_selectedLevel + 1 + i < _levelCount)
                        {
                            // small stars
                            stars = SaveManager.Instance.LevelStars(_selectedLevel + 1 + i);
                            spriteBatch.Draw(texture,
                                new Rectangle(188 + i * 41 + 6, 68 + 44, _selectionRectangle[stars].Width, _selectionRectangle[stars].Height),
                                _selectionRectangle[stars],
                                Color.White);
                        }
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

                if (_selectedLevel >= 0 && _selectedLevel < _levelCount)
                {

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
                }
                else if (_selectedLevel == -1)
                {
                    SpriteFont font = TextManager.Instance.Font;

                    int width = (int)font.MeasureString(TextManager.Instance.Intro).X;
                    int x = GreedyKidGame.Width / 2 - width / 2;

                    spriteBatch.DrawString(font,
                        TextManager.Instance.Intro,
                        new Vector2(x, 136),
                        Color.White);
                }
                else if (_selectedLevel == _levelCount)
                {
                    SpriteFont font = TextManager.Instance.Font;

                    int width = (int)font.MeasureString(TextManager.Instance.Ending1).X;
                    int x = GreedyKidGame.Width / 2 - width / 2;

                    spriteBatch.DrawString(font,
                        TextManager.Instance.Ending1,
                        new Vector2(x, 136),
                        Color.White);
                }
                else if (_selectedLevel == _levelCount + 1)
                {
                    SpriteFont font = TextManager.Instance.Font;

                    int width = (int)font.MeasureString(TextManager.Instance.Ending2).X;
                    int x = GreedyKidGame.Width / 2 - width / 2;

                    spriteBatch.DrawString(font,
                        TextManager.Instance.Ending2,
                        new Vector2(x, 136),
                        Color.White);
                }

                // arrows
                int arrow = (_selectedLevel > -1 ? 8 : 7);
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
                if (_currentAnimationFrame >= 5)                
                    UIHelper.Instance.DrawCommand(spriteBatch, GreedyKidGame.Version, CommandType.None);                                    
                if (_currentAnimationFrame >= 6)
                    UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Press, CommandType.Select, CommandPosition.Center, (_currentAnimationFrame > 11 ? true : false));
            }
            else if ((_state != TitleScreenState.Workshop && _currentAnimationFrame > 1) ||
                (_state == TitleScreenState.Workshop && _workshopIdentifiers != null && _workshopIdentifiers.Length > 0))
            {
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Select, CommandType.Select, CommandPosition.Right, (_currentAnimationFrame == 2));
            }

            if (_state != TitleScreenState.Title && _currentAnimationFrame > 1)
            {
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Back, CommandType.Back, CommandPosition.Left, (_currentAnimationFrame == 2));
            }
            if (_state == TitleScreenState.Main || _state == TitleScreenState.Play)
                UIHelper.Instance.DrawCommand(spriteBatch, GreedyKidGame.Version, CommandType.None);

            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GreedyKid
{
    public enum ObjectType
    {
        HealthPack,
        CashBig,
        CashMedium,
        CashSmall,
        Arrow,

        Count
    }

    public enum ElevatorState
    {
        Closed,
        Opening,
        Open,
        Closing,
    }

    public enum TransitionState
    {
        None,
        Appearing,
        Disappearing,
        Hidden,
    }

    public enum TimerType
    {
        Cop,
        Swat,
        Robocop,
        None
    }

    public sealed class GameplayManager
    {
        private Building _building;

        private Rectangle[][][] _roomRectangle;
        private Rectangle[][][] _detailRectangle;
        private Rectangle[][][] _floorDoorRectangle;
        private Rectangle[][] _roomDoorRectangle;
        private Rectangle[][] _elevatorRectangle;
        private Rectangle[][][] _furnitureRectangle;
        private Rectangle[][] _objectsRectangle;

        private Rectangle[] _iconRectangle;
        private Rectangle[] _maskRectangle;
        private Rectangle[] _numberRectangle;

        private Rectangle _robocopBeamRectangle;

        private Rectangle[] _copTimerRectangle;

        private Rectangle[] _gameoverRectangle;

        private Rectangle[] _scoreRectangle;

        public int SelectedLevel = 0;

        public Player Player;

        public int Score = 0;
        private int[] _encodedScore = new int[] { 0, 0, 0 };
        public int Time = 0;
        private int[] _encodedTime = new int[] { 0, 0, 10, 0, 0 };

        private float _currentSeconds = 0.0f;

        // arrow animation
        private int _currentArrowFrame = 0;
        private float _currentArrowFrameTime = 0.0f;
        private const float _arrowFrameTime = 0.1f;

        // shouting animation
        private int _currentShoutFrame = 0;
        private float _currentShoutFrameTime = 0.0f;
        private const float _shoutFrameTime = 0.05f;

        private int _shoutDistance = 32;

        // elevators
        private ElevatorState _entranceState = ElevatorState.Closed;
        private ElevatorState _exitState = ElevatorState.Closed;

        private int _currentEntranceFrame = 0;
        private float _currentEntranceFrameTime = 0.0f;
        private int _currentExitFrame = 0;
        private float _currentExitFrameTime = 0.0f;
        private const float _elevatorFrameTime = 0.1f;

        private bool _canEscape = false;

        // transition
        private Rectangle[] _transitionRectangle;
        private TransitionState _transitionState = TransitionState.Hidden;
        private int _currentTransitionFrame = 0;
        private float _currentTransitionFrameTime = 0.0f;
        private const float _transitionFrameTime = 0.1f;

        // inter level
        private Rectangle[] _interLevelRectangle;
        private int _elevatorX = 0;
        private int _elevatorY = 0;
        private const int _elevatorFrameCount = 8;
        private int _currentElevatorFrame = 0;
        private const int _cableFrameCount = 8;
        private int _currentCableFrame = 0;
        private float _currentInterLevelFrameTime = 0.0f;
        private const float _interLevelFrameTime = 0.05f;
        private int _currentElevatorBackgroundY = 0;
        private const int _elevatorKidFrameCount = 4;
        private int _currentElevatorKidFrame = 0;
        private float _currentElevatorKidFrameTime = 0.0f;
        private const float _elevatorKidFrameTime = 0.1f;

        private string _timeString;
        private string _moneyString;
        private int _targetTime = 0;
        private int _targetMoney = 0;

        private int _copTimer = 0;
        private int _totalCopTimer = 0;

        // spawning cop
        private int _initialNormalCopCount = 0;
        private TimerType _timerType = TimerType.Cop;
        private bool _spawnEntrance = false;
        private Cop _spawningCop = null;   // to reset
        private float _currentCopArrivingTime = -1.0f;   // to reset
        private const float _copArrivingTime = 2.0f;
        private float _nextSwatSpawn = 0.0f;
        private float _nextRobocopSpawn = 0.0f;

        // camera
        private float _cameraPositionY = 0.0f;        
        private float _initialCameraPositionY = 0.0f;
        private float _differenceCameraPositionY = 0.0f;
        private const float _totalCameraTime = 1.0f;
        private float _currentCameraTime = _totalCameraTime;

        // microphone
        private int _microphoneSensitivity = 5;

        // bullets
        private Bullet[] _bullets;
        private const int _maxBullets = 10;
        private int _bulletCount = 0;
        private Rectangle[][] _bulletRectangle;

        // pause
        private bool _pause = false;
        private int _pauseOption = 0;
        private bool _inSettings = false;
        private Rectangle[] _pauseBackgroundRectangles;

        public bool ReturnToLevelSelection = false;

        // gameover
        private float _currentGameOverFrameTime = 0.0f;
        private int _currentGameOverFrame = 0;
        private int _currentFrameBeforeGameOver = 0;
        private const int _frameBeforeGameOver = 10;
        private const float _gameOverFrameTime = 0.1f;

        public GameplayManager()
        {
            _roomRectangle = new Rectangle[Room.PaintCount][][]; // colors
            _detailRectangle = new Rectangle[Room.PaintCount][][];
            _floorDoorRectangle = new Rectangle[Room.PaintCount][][];
            _roomDoorRectangle = new Rectangle[Room.PaintCount][];
            _furnitureRectangle = new Rectangle[Room.PaintCount][][];

            int nbDoorLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);

            for (int p = 0; p < Room.PaintCount; p++)
            {
                _roomRectangle[p] = new Rectangle[Room.DecorationCount][]; // decoration

                for (int d = 0; d < Room.DecorationCount; d++)
                {
                    _roomRectangle[p][d] = new Rectangle[]
                    {
                        new Rectangle(56 * d, 48 * p, 24, 48), // left
                        new Rectangle(24 + 56 * d, 48 * p, 8, 48), // fill
                        new Rectangle(24 + 8 + 56 * d, 48 * p, 24, 48), // right
                    };
                }

                _detailRectangle[p] = new Rectangle[Detail.NormalDetailCount + Detail.AnimatedDetailCount][];
                for (int d = 0; d < Detail.NormalDetailCount + Detail.AnimatedDetailCount; d++)
                {
                    if (d < Detail.NormalDetailCount)
                    {
                        _detailRectangle[p][d] = new Rectangle[1];
                        _detailRectangle[p][d][0] = new Rectangle(56 * Room.DecorationCount + d * 32, 48 * p, 32, 48);
                    }
                    else
                    {
                        _detailRectangle[p][d] = new Rectangle[Detail.AnimatedDetailFrames];
                        for (int f = 0; f < Detail.AnimatedDetailFrames; f++)
                        {
                            _detailRectangle[p][d][f] = new Rectangle(56 * Room.DecorationCount + Detail.NormalDetailCount * 32 + (d - Detail.NormalDetailCount) * 32 * Detail.AnimatedDetailFrames + f * 32, 48 * p, 32, 48);
                        }
                    }
                }

                _floorDoorRectangle[p] = new Rectangle[FloorDoor.DoorCount][];
                for (int d = 0; d < FloorDoor.DoorCount; d++)
                {
                    int row = d / FloorDoor.DoorPerLine;
                    int col = d % FloorDoor.DoorPerLine;
                    int nbLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);

                    _floorDoorRectangle[p][d] = new Rectangle[FloorDoor.DoorFrames];

                    for (int f = 0; f < FloorDoor.DoorFrames; f++)
                    {
                        _floorDoorRectangle[p][d][f] = new Rectangle(col * 40 * FloorDoor.DoorFrames + f * 40, Room.PaintCount * 48 + p * 48 * nbLine + row * 48, 40, 48);
                    }
                }

                _roomDoorRectangle[p] = new Rectangle[RoomDoor.DoorFrames];
                for (int f = 0; f < RoomDoor.DoorFrames; f++)
                {
                    int row = f / RoomDoor.FramePerLine;
                    int col = f % RoomDoor.FramePerLine;
                    int nbLine = (int)Math.Ceiling(RoomDoor.DoorFrames / (float)RoomDoor.FramePerLine);

                    _roomDoorRectangle[p][f] = new Rectangle(FloorDoor.DoorPerLine * FloorDoor.DoorFrames * 40 + col * 32, Room.PaintCount * 48 + p * 48 * nbLine + row * 48, 32, 48);
                }

                _furnitureRectangle[p] = new Rectangle[Furniture.FurnitureCount][];
                for (int f = 0; f < Furniture.FurnitureCount; f++)
                {
                    int row = f / Furniture.FurniturePerLine;
                    int col = f % Furniture.FurniturePerLine;
                    int nbLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

                    _furnitureRectangle[p][f] = new Rectangle[Furniture.FurnitureFrames];

                    for (int ff = 0; ff < Furniture.FurnitureFrames; ff++)
                    {
                        _furnitureRectangle[p][f][ff] = new Rectangle(col * 32 * Furniture.FurnitureFrames + ff * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + row * 48 + p * 48 * nbLine, 32, 48);
                    }
                }
            }

            _elevatorRectangle = new Rectangle[3][];
            _elevatorRectangle[0] = new Rectangle[Room.ElevatorFrames];
            _elevatorRectangle[1] = new Rectangle[Room.ElevatorFrames];
            _elevatorRectangle[2] = new Rectangle[Room.PaintCount];

            for (int f = 0; f < Room.ElevatorFrames; f++)
            {
                _elevatorRectangle[0][f] = new Rectangle(f * 40, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);
                _elevatorRectangle[1][f] = new Rectangle(f * 40 + 40 * Room.ElevatorFrames, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);
            }
            for (int p = 0; p < Room.PaintCount; p++)
            {
                _elevatorRectangle[2][p] = new Rectangle(2 * 40 * Room.ElevatorFrames + p * 40, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);
            }

            _objectsRectangle = new Rectangle[(int)ObjectType.Count][];
            _objectsRectangle[(int)ObjectType.HealthPack] = new Rectangle[6];
            _objectsRectangle[(int)ObjectType.CashBig] = new Rectangle[5];
            _objectsRectangle[(int)ObjectType.CashMedium] = new Rectangle[5];
            _objectsRectangle[(int)ObjectType.CashSmall] = new Rectangle[5];
            _objectsRectangle[(int)ObjectType.Arrow] = new Rectangle[4];

            int objectFrameCount = 0;
            for (int o = 0; o < (int)ObjectType.Count; o++)
            {
                for (int f = _objectsRectangle[o].Length - 1; f >= 0; f--)
                {
                    objectFrameCount++;
                    _objectsRectangle[o][f] = new Rectangle(TextureManager.GameplayWidth - objectFrameCount * 16, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 16, 16);
                }
            }

            _bullets = new Bullet[_maxBullets];
            for (int i = 0; i < _maxBullets; i++)
                _bullets[i] = new Bullet();
            _bulletRectangle = new Rectangle[(int)BulletType.Count][];
            for (int t = 0; t < (int)BulletType.Count; t++)
            {
                _bulletRectangle[t] = new Rectangle[4];
                for (int f = 0; f < 4; f++)
                {
                    _bulletRectangle[t][f] = new Rectangle(TextureManager.GameplayWidth - objectFrameCount * 16 - 64 + 16 * f - t * 16 * 4, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 16, 16);
                }
            }

            // UI
            _iconRectangle = new Rectangle[3];
            _iconRectangle[0] = new Rectangle(23, TextureManager.GameplayHeight - 24, 13, 13);
            _iconRectangle[1] = new Rectangle(36, TextureManager.GameplayHeight - 24, 13, 13);
            _iconRectangle[2] = new Rectangle(49, TextureManager.GameplayHeight - 24, 13, 13);

            _maskRectangle = new Rectangle[5];
            _maskRectangle[0] = new Rectangle(23, TextureManager.GameplayHeight - 10, 49, 10);
            _maskRectangle[1] = new Rectangle(72, TextureManager.GameplayHeight - 10, 57, 10);
            _maskRectangle[2] = new Rectangle(129, TextureManager.GameplayHeight - 10, 51, 10);
            _maskRectangle[3] = new Rectangle(152, TextureManager.GameplayHeight - 130, 1, 1); // 1x1
            _maskRectangle[4] = new Rectangle(201, TextureManager.GameplayHeight - 184, 328, 2);

            _numberRectangle = new Rectangle[12];
            for (int i = 0; i < _numberRectangle.Length; i++)
            {
                _numberRectangle[i] = new Rectangle(74 + 11 * i, TextureManager.GameplayHeight - 24, 11, 13);
            }
            _numberRectangle[10].Width = 5;
            _numberRectangle[11].X = _numberRectangle[10].X + _numberRectangle[10].Width;

            // transition
            _transitionRectangle = new Rectangle[7];
            _transitionRectangle[0] = new Rectangle(152, TextureManager.GameplayHeight - 130, 1, 1); // 1x1
            _transitionRectangle[2] = new Rectangle(150, TextureManager.GameplayHeight - 133, 50, 50); // circle half full
            _transitionRectangle[1] = new Rectangle(150, TextureManager.GameplayHeight - 184, 50, 50); // circle empty            
            _transitionRectangle[3] = new Rectangle(201, TextureManager.GameplayHeight - 184, 328, 184); // half full

            _transitionRectangle[4] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 13); // gameover 1
            _transitionRectangle[5] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 28); // gameover 2
            _transitionRectangle[6] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 82); // gameover 3

            // inter level
            _interLevelRectangle = new Rectangle[_elevatorFrameCount + _cableFrameCount + 3 + _elevatorKidFrameCount];
            // elevator
            for (int i = 0; i < _elevatorFrameCount; i++)
            {
                _interLevelRectangle[i] = new Rectangle(613 + 32 * i, TextureManager.GameplayHeight - 45, 32, 45);
            }
            // cable
            for (int i = 0; i < _cableFrameCount; i++)
            {
                _interLevelRectangle[_elevatorFrameCount + i] = new Rectangle(530 + 2 * i, TextureManager.GameplayHeight - 160, 1, 160);
            }
            // background
            _interLevelRectangle[_elevatorFrameCount + _cableFrameCount] = new Rectangle(548, TextureManager.GameplayHeight - 240, 64, 240);
            _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 1] = new Rectangle(483, TextureManager.GameplayHeight - 204, 64, 10);
            _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 2] = new Rectangle(483, TextureManager.GameplayHeight - 193, 64, 8);
            // kid
            for (int i = 0; i < _elevatorKidFrameCount; i++)
            {
                _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 3 + i] = new Rectangle(613 + 32 * i, TextureManager.GameplayHeight - 90, 32, 45);
            }

            // pause
            _pauseBackgroundRectangles = new Rectangle[2];
            _pauseBackgroundRectangles[0] = new Rectangle(1619, TextureManager.GameplayHeight - 184, 111, 153);
            _pauseBackgroundRectangles[1] = new Rectangle(1731, TextureManager.GameplayHeight - 184, 137, 153);

            // robocop
            _robocopBeamRectangle = new Rectangle(117, TextureManager.GameplayHeight - 113, 32, 16);

            _copTimerRectangle = new Rectangle[13];
            for (int i = 0; i < 9; i++)
            {
                _copTimerRectangle[i] = new Rectangle(70 + 7 * i, TextureManager.GameplayHeight - 66, 6, 12);
            }
            for (int i = 9; i < 12; i++)
            {
                _copTimerRectangle[i] = new Rectangle(133 + 15 * (i - 9), TextureManager.GameplayHeight - 66, 14, 12);
            }
            _copTimerRectangle[12] = new Rectangle(178, TextureManager.GameplayHeight - 66, 21, 12);

            // gameover
            _gameoverRectangle = new Rectangle[9];
            _gameoverRectangle[0] = new Rectangle(0, TextureManager.GameplayHeight - 38, 11, 6); // upper left
            _gameoverRectangle[1] = new Rectangle(11, TextureManager.GameplayHeight - 38, 1, 6); // up
            _gameoverRectangle[2] = new Rectangle(12, TextureManager.GameplayHeight - 38, 11, 6); // upper right
            _gameoverRectangle[3] = new Rectangle(12, TextureManager.GameplayHeight - 32, 11, 1); // right
            _gameoverRectangle[4] = new Rectangle(12, TextureManager.GameplayHeight - 31, 11, 6); // lower right
            _gameoverRectangle[5] = new Rectangle(11, TextureManager.GameplayHeight - 31, 1, 6); // down
            _gameoverRectangle[6] = new Rectangle(0, TextureManager.GameplayHeight - 31, 11, 6); // lower left
            _gameoverRectangle[7] = new Rectangle(0, TextureManager.GameplayHeight - 32, 11, 1); // left
            _gameoverRectangle[8] = new Rectangle(11, TextureManager.GameplayHeight - 32, 1, 1); // background

            // score
            _scoreRectangle = new Rectangle[7];
            _scoreRectangle[0] = new Rectangle(449, TextureManager.GameplayHeight - 232, 33, 11); // no star
            _scoreRectangle[1] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 12, 33, 11); // 1 star
            _scoreRectangle[2] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 24, 33, 11); // 2 stars
            _scoreRectangle[3] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 36, 33, 11); // 3 stars
            _scoreRectangle[4] = new Rectangle(494, TextureManager.GameplayHeight - 240, 14, 9); // check
            _scoreRectangle[5] = new Rectangle(509, TextureManager.GameplayHeight - 240, 12, 9); // no check
            _scoreRectangle[6] = new Rectangle(323, TextureManager.GameplayHeight - 240, 125, 10); // separation
        }        

        public string BuildingIdentifier
        {
            get {
                if (_building == null)
                    return String.Empty;
                else
                    return _building.Identifier;
            }
        }

        public void LoadBuilding(string identifier)
        {            
            _building = new Building();
            _building.Load(identifier);

            SaveManager.Instance.Load(_building);            
        }

        public void LoadLevel(int level)
        {
            SelectedLevel = level;

            _building.LoadLevel(SelectedLevel);            

            // init player
            Player = new Player();
            Player.Room = null;

            // look for player start
            for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
            {
                for (int r = 0; r < _building.CurrentLevel.Floors[f].Rooms.Length; r++)
                {
                    if (_building.CurrentLevel.Floors[f].Rooms[r].HasStart)
                    {
                        Player.Room = _building.CurrentLevel.Floors[f].Rooms[r];
                        Player.X = Player.Room.StartX + 4;
                        break;
                    }
                }
                if (Player.Room != null)
                    break;
            }            

            // transition init
            _entranceState = ElevatorState.Closed;
            _exitState = ElevatorState.Closed;
            _transitionState = TransitionState.Hidden;
            AppearTransition();

            // cop init            
            Cop dummy = new Cop(); // dummy load a cop to init static fields and avoid a freeze upon cop spawning
            dummy = null;
            _spawningCop = null;   // to reset
            _currentCopArrivingTime = -1.0f;   // to reset

            _totalCopTimer = _building.CurrentLevel.TimeBeforeCop + _building.CurrentLevel.TimeBeforeSwat + _building.CurrentLevel.TimeBeforeRobocop;

            _currentSeconds = 0.0f;
            _timerType = TimerType.None;
            if (_building.CurrentLevel.TimeBeforeCop > 0)
            {
                _initialNormalCopCount = _building.CurrentLevel.Cop1Count + _building.CurrentLevel.Cop2Count;
                _copTimer = _building.CurrentLevel.TimeBeforeCop;
                _timerType = TimerType.Cop;                
            }
            else if (_building.CurrentLevel.TimeBeforeSwat > 0)
            {
                _copTimer = _building.CurrentLevel.TimeBeforeSwat;
                _timerType = TimerType.Swat;
            }
            else if (_building.CurrentLevel.TimeBeforeRobocop > 0)
            {
                _copTimer = _building.CurrentLevel.TimeBeforeRobocop;
                _timerType = TimerType.Robocop;
            }

            Score = 0;
            Time = 0;

            _targetTime = _building.CurrentLevel.TargetTime;
            _targetMoney = _building.CurrentLevel.TargetMoney;

            // camera init
            _cameraPositionY = 0.0f;
            _currentCameraTime = _totalCameraTime;

            if (_building.CurrentLevel.Floors.Length > 4 && Player.Room.Y >= 3)
            {
                if (Player.Room.Y == _building.CurrentLevel.Floors.Length - 1)
                    _cameraPositionY = (_building.CurrentLevel.Floors.Length - 4) * 40.0f; // last floor
                else
                    _cameraPositionY = (Player.Room.Y - 2) * 40.0f;
            }

            // bullets
            _bulletCount = 0;

            // gameover
            _currentGameOverFrameTime = 0.0f;
            _currentGameOverFrame = 0;
            _currentFrameBeforeGameOver = 0;

            ReturnToLevelSelection = false;
            _pauseOption = 0;

            // clean memory
            GC.Collect();
        }

        public void UpdateMouseSelection(int x, int y)
        {
            if (_inSettings)
            {
                SettingsManager.Instance.UpdateMouseSelection(x, y);
            }
            else if (Gameover)
            {
                int previous = _pauseOption;

                if (y < GreedyKidGame.Height / 2)
                    _pauseOption = 0;
                else
                    _pauseOption = 1;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
            else if (!IsIngame)
            {
                int previous = _pauseOption;

                if (y < 126)
                    _pauseOption = 0;
                else if (y > 142)
                    _pauseOption = 2;
                else
                    _pauseOption = 1;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
            else
            {
                int previous = _pauseOption;

                if (y >= 70 && y < 85)
                {
                    _pauseOption = 0;
                }
                else if (y >= 85 && y < 100)
                {
                    _pauseOption = 1;
                }
                else if (y >= 100 && y < 115)
                {
                    _pauseOption = 2;
                }
                else if (y >= 115 && y < 130)
                {
                    _pauseOption = 3;
                }

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
        }

        public void ResetLevel()
        {
            LoadLevel(SelectedLevel);
        }

        public void NextLevel()
        {
            if (_building.CurrentLevel != null)
            {
                SelectedLevel++;
                SelectedLevel %= _building.LevelCount;
            }
            LoadLevel(SelectedLevel);
        }

        public void PreviousLevel()
        {
            if (_building.CurrentLevel != null)
            {
                SelectedLevel--;
                if (SelectedLevel < 0)
                    SelectedLevel = _building.LevelCount - 1;
            }
            LoadLevel(SelectedLevel);
        }

        public void RequestPause()
        {
            if (Player != null && Player.Life > 0)
            {
                _pause = true;
                _pauseOption = 0;
                _inSettings = false;
            }
        }

        public void PauseUp()
        {
            if (_inSettings)
                SettingsManager.Instance.PushUp();
            else if (Gameover)
            {
                int previous = _pauseOption;

                _pauseOption--;
                if (_pauseOption < 0)
                    _pauseOption = 1;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
            else if (!IsIngame)
            {
                int previous = _pauseOption;

                _pauseOption--;
                if (_pauseOption < 0)
                    _pauseOption = 2;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
            else
            {
                int previous = _pauseOption;

                _pauseOption--;
                if (_pauseOption < 0)
                    _pauseOption = 3;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
        }

        public void PauseDown()
        {
            if (_inSettings)
                SettingsManager.Instance.PushDown();
            else if (Gameover)
            {
                int previous = _pauseOption;

                _pauseOption++;
                _pauseOption %= 2;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
            else if (!IsIngame)
            {
                int previous = _pauseOption;

                _pauseOption++;
                _pauseOption %= 3;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
            else
            {
                int previous = _pauseOption;

                _pauseOption++;
                _pauseOption %= 4;

                if (previous != _pauseOption)
                    SfxManager.Instance.Play(Sfx.MenuBlip);
            }
        }

        public void PauseLeft()
        {
            if (_inSettings)
                SettingsManager.Instance.PushLeft();            
        }

        public void PauseRight()
        {
            if (_inSettings)
                SettingsManager.Instance.PushRight();            
        }

        public void PauseSelect(bool fromMouse = false, int mouseX = 0)
        {
            if (_inSettings)
            {
                SettingsManager.Instance.PushSelect(fromMouse, mouseX);
            }
            else if (Gameover)
            {
                switch (_pauseOption)
                {
                    case 0: ResetLevel(); _pause = false; break;
                    case 1: ReturnToLevelSelection = true; break;
                }

                if (fromMouse)
                    MouseKeyboardInputsHandler.ShouldUpdateMouse = true;
            }
            else if (!IsIngame)
            {
                switch (_pauseOption)
                {
                    case 0:                            
                        // end level, load next
                        SelectedLevel++;
                        SelectedLevel %= _building.LevelCount;
                        DisappearTransition(); break;
                    case 1:
                        DisappearTransition();
                        break;
                    case 2:
                        ReturnToLevelSelection = true;
                        break;
                }

                if (fromMouse)
                    MouseKeyboardInputsHandler.ShouldUpdateMouse = true;
            }
            else
            {
                switch (_pauseOption)
                {
                    case 0: _pause = false; _pauseOption = 0; break;
                    case 1: _inSettings = true; SettingsManager.Instance.Reset(); break;
                    case 2: ResetLevel(); _pause = false; break;
                    case 3: ReturnToLevelSelection = true; break;
                }

                if (fromMouse)
                    MouseKeyboardInputsHandler.ShouldUpdateMouse = true;
            }
        }

        public void PauseCancel(bool fromMouse = false)
        {
            if (_inSettings)
            {
                if (!SettingsManager.Instance.PushCancel(fromMouse))
                {
                    _inSettings = false;
                    SettingsManager.Instance.Save();

                    if (fromMouse)
                        MouseKeyboardInputsHandler.ShouldUpdateMouse = true;
                }
            }
            else
            {
                _pause = false;
            }
        }

        public bool Pause
        {
            get { return _pause; }
        }

        public bool Gameover
        {
            get { return !_pause && Player != null && _currentGameOverFrame > 0; }
        }

        public bool IsIngame
        {
            get { return _building.CurrentLevel != null && SelectedLevel >= 0 && SelectedLevel < _building.LevelCount; }
        }

        public void Update(float gameTime)
        {
            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.HandleIngameInputs(this);

            if (_pause)
            {
                InputManager.PlayerDevice.Update(gameTime);
                if (_inSettings)
                    SettingsManager.Instance.Update(gameTime);
                return;
            }

            // transition
            UpdateTransition(gameTime);
            
            if (_building.CurrentLevel != null && Player != null && SelectedLevel >= 0 && SelectedLevel < _building.LevelCount)
            {
                int prevPlayerY = Player.Room.Y;
                Player.Update(gameTime);
                int playerY = Player.Room.Y;

                // camera handling
                if (prevPlayerY != playerY && _building.CurrentLevel.Floors.Length > 4)
                {               
                    if (playerY > prevPlayerY && _cameraPositionY < (playerY - 2) * 40.0f)
                        MoveCamera((playerY - 2) * 40.0f);
                    else if (playerY < prevPlayerY && _cameraPositionY > (playerY - 1) * 40.0f)
                        MoveCamera((playerY - 1) * 40.0f);                    
                }

                // gameover
                if (Player.Life <= 0)
                {
                    _currentGameOverFrameTime += gameTime;
                    if (_currentGameOverFrameTime >= _gameOverFrameTime)
                    {
                        _currentGameOverFrameTime -= _gameOverFrameTime;
                        if (_currentFrameBeforeGameOver < _frameBeforeGameOver)
                            _currentFrameBeforeGameOver++;
                        else
                        {
                            _currentGameOverFrame++;
                            if (_currentGameOverFrame > 7)
                                _currentGameOverFrame = 4;
                        }
                    }
                }

                // cop spawning
                _currentSeconds += gameTime;
                if (_currentSeconds >= 1.0f)
                {
                    _currentSeconds -= 1.0f;

                    Time++;

                    if (_copTimer > 0)
                    {
                        _copTimer--;

                        if (_timerType == TimerType.Cop)
                        {
                            _building.CurrentLevel.TimeBeforeCop--;
                        }
                        else if (_timerType == TimerType.Swat)
                        {
                            _building.CurrentLevel.TimeBeforeSwat--;
                        }
                        else if (_timerType == TimerType.Robocop)
                        {
                            _building.CurrentLevel.TimeBeforeRobocop--;
                        }

                        if (_copTimer == (int)_copArrivingTime && _timerType == TimerType.Cop)
                        {
                            if (_building.CurrentLevel.Cop1Count > 0)
                            {
                                _building.CurrentLevel.Cop1Count--;
                                SpawnCop();
                                _spawningCop.Type = 0;
                            }
                            else if (_building.CurrentLevel.Cop2Count > 0)
                            {
                                _building.CurrentLevel.Cop2Count--;
                                SpawnCop();
                                _spawningCop.Type = 1;
                            }
                        }
                        else if (_copTimer == 0 && _timerType < TimerType.Robocop)
                        {
                            if (_timerType == TimerType.Cop && _building.CurrentLevel.TimeBeforeSwat > 0)
                            {
                                // start swat timer
                                _timerType = TimerType.Swat;
                                _copTimer = _building.CurrentLevel.TimeBeforeSwat;
                            }
                            else if (_timerType == TimerType.Cop && _building.CurrentLevel.TimeBeforeRobocop > 0)
                            {
                                // start robocop timer
                                _timerType = TimerType.Robocop;
                                _copTimer = _building.CurrentLevel.TimeBeforeRobocop;
                            }
                            else if (_timerType == TimerType.Cop)
                            {
                                _timerType = TimerType.None;
                            }
                            else if (_timerType == TimerType.Swat)
                            {
                                // spawn swat
                                SpawnSwat();

                                if (_building.CurrentLevel.TimeBeforeRobocop > 0)
                                {
                                    // start robocop timer
                                    _timerType = TimerType.Robocop;
                                    _copTimer = _building.CurrentLevel.TimeBeforeRobocop;
                                }
                                else
                                {
                                    _timerType = TimerType.None;
                                }
                            }
                        }
                        else if (_copTimer == 0 && _timerType == TimerType.Robocop)
                        {
                            _timerType = TimerType.None;
                            SpawnRobocop();
                        }
                    }
                }

                // swat spawing
                if (_nextSwatSpawn > 0.0f)
                {
                    _nextSwatSpawn -= gameTime;
                    
                    if (_nextSwatSpawn <= 0.0f)
                    {
                        SpawnSwat();
                    }
                }

                // robocop spawn
                if (_nextRobocopSpawn > 0.0f)
                {
                    _nextRobocopSpawn -= gameTime;

                    if (_nextRobocopSpawn <= 0.0f)
                    {
                        SpawnRobocop();
                    }
                }

                if (MicrophoneManager.Instance.LeveledVolume >= _microphoneSensitivity)
                    Player.Shout();

                bool isShouting = Player.IsShouting;
                bool isTaunting = Player.IsTaunting;
                int playerMiddle = (int)Player.X + 16;

                // elevators
                UpdateElevators(gameTime);

                // open entrance / door for cop arriving
                if (_currentCopArrivingTime >= 0.0f)
                {
                    _currentCopArrivingTime -= gameTime;
                    if (_currentCopArrivingTime < 0.0f)
                    {
                        if (_spawnEntrance)
                            OpenEntranceElevator();
                        else
                            OpenExitElevator();
                    }
                }

                _canEscape = true;
                Player.CanEnterElevator = false;

                for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
                {
                    Floor floor = _building.CurrentLevel.Floors[f];

                    // rooms
                    for (int r = 0; r < floor.Rooms.Length; r++)
                    {
                        Room room = floor.Rooms[r];

                        // room doors
                        for (int d = 0; d < room.RoomDoors.Length; d++)
                        {
                            room.RoomDoors[d].Update(gameTime);
                        }

                        // floor doors
                        for (int d = 0; d < room.FloorDoors.Length; d++)
                        {
                            room.FloorDoors[d].Update(gameTime);
                        }

                        // furnitures
                        for (int ff = 0; ff < room.Furnitures.Length; ff++)
                        {
                            room.Furnitures[ff].Update(gameTime);
                        }

                        // exit                        
                        if (Player.Room == room && room.HasExit && _exitState == ElevatorState.Open)
                        {
                            if (Player.X + 16 > room.ExitX + 11 && Player.X + 16 < room.ExitX + 27)
                                Player.CanEnterElevator = true;
                        }

                        // cops
                        for (int c = 0; c < room.Cops.Count; c++)
                        {
                            Cop cop = room.Cops[c];
                            if (cop != null)
                            {
                                // boo
                                bool boo = false;

                                if (isShouting && cop.Room == Player.Room)
                                {
                                    int retiredMiddle = (int)cop.X + 16;
                                    if (Math.Abs(retiredMiddle - playerMiddle) <= _shoutDistance && cop.NotFacing(playerMiddle))
                                        boo = true;
                                }

                                // player pos
                                if (cop.Room == Player.Room && Player.IsVisible)
                                {
                                    cop.LastKnownPlayerPosition = playerMiddle;
                                }
                                else
                                {
                                    cop.LastKnownPlayerPosition = -1;
                                }

                                cop.Update(gameTime, boo, isTaunting);
                                if (cop.HasFired)
                                {
                                    cop.HasFired = false;
                                    if (cop.Type < Cop.NormalCopCount + Cop.SwatCopCount)
                                    {
                                        BulletType type = BulletType.Taser + cop.Type - 1;
                                        FireBullet(type, cop.X, cop.Orientation, cop.Room);
                                    }
                                    else
                                    {
                                        // robocop fire
                                        if (Player.CanBeHit)
                                        {
                                            Player.HitRobocop((cop.Orientation == SpriteEffects.None ? SpriteEffects.FlipHorizontally : SpriteEffects.None));
                                        }
                                        //FireBullet(BulletType.Taser, cop.X, cop.Orientation, cop.Room);
                                    }
                                }
                            }
                        }

                        // retireds                        
                        for (int rr = 0; rr < room.Retireds.Count; rr++)
                        {
                            Retired retired = room.Retireds[rr];
                            if (retired != null)
                            {
                                // boo
                                bool boo = false;

                                if (isShouting && retired.Room == Player.Room)
                                {
                                    int retiredMiddle = (int)retired.X + 16;
                                    if (Math.Abs(retiredMiddle - playerMiddle) <= _shoutDistance && retired.NotFacing(playerMiddle))
                                    boo = true;
                                }

                                // player pos
                                if (retired.Room == Player.Room)
                                {
                                    retired.LastKnownPlayerPosition = playerMiddle;
                                }
                                else
                                {
                                    retired.LastKnownPlayerPosition = -1;
                                }

                                retired.Update(gameTime, boo, isTaunting);

                                if (retired.Life > 0)
                                    _canEscape = false;
                            }
                        }

                        // nurses
                        for (int n = 0; n < room.Nurses.Count; n++)
                        {
                            Nurse nurse = room.Nurses[n];
                            if (nurse != null)
                            {
                                // boo
                                bool boo = false;
                         
                                if (isShouting && nurse.Room == Player.Room)
                                {
                                    int retiredMiddle = (int)nurse.X + 16;
                                    if (Math.Abs(retiredMiddle - playerMiddle) <= _shoutDistance && nurse.NotFacing(playerMiddle))
                                        boo = true;
                                }

                                // player pos
                                if (nurse.Room == Player.Room)
                                {
                                    nurse.LastKnownPlayerPosition = playerMiddle;
                                }
                                else
                                {
                                    nurse.LastKnownPlayerPosition = -1;
                                }

                                // ko retired pos
                                if (!nurse.IsRessurecting)
                                {
                                    nurse.LastKnownKORetired = null;
                                    if (nurse.Life > 0)
                                    {
                                        for (int rr = 0; rr < room.Retireds.Count; rr++)
                                        {
                                            Retired retired = room.Retireds[rr];
                                            if (retired != null && retired.Life <= 0)
                                            {
                                                bool canSeeRetired = true;

                                                for (int d = 0; d < room.RoomDoors.Length; d++)
                                                {
                                                    RoomDoor roomDoor = room.RoomDoors[d];

                                                    int retiredPos = (int)retired.X + 16;

                                                    // check if KO retired in sight
                                                    if (retiredPos > nurse.X + 16
                                                        && nurse.Orientation == SpriteEffects.None
                                                        && roomDoor.IsClosed
                                                        && roomDoor.X + 16 > nurse.X + 16 && retiredPos > roomDoor.X + 16)
                                                        canSeeRetired = false;
                                                    else if (retiredPos < nurse.X + 16
                                                        && nurse.Orientation == SpriteEffects.FlipHorizontally
                                                        && roomDoor.IsClosed
                                                        && roomDoor.X + 16 < nurse.X + 16 && retiredPos < roomDoor.X + 16)
                                                        canSeeRetired = false;
                                                    else if (retiredPos < nurse.X + 16
                                                        && nurse.Orientation == SpriteEffects.None)
                                                        canSeeRetired = false;
                                                    else if (retiredPos > nurse.X + 16
                                                        && nurse.Orientation == SpriteEffects.FlipHorizontally)
                                                        canSeeRetired = false;
                                                }

                                                if (
                                                    (retired.X + 16.0f < nurse.X + 16.0f && nurse.Orientation == SpriteEffects.None) ||
                                                    (retired.X + 16.0f > nurse.X + 16.0f && nurse.Orientation != SpriteEffects.None)
                                                    )
                                                    canSeeRetired = false;

                                                if (canSeeRetired && nurse.LastKnownKORetired == null)
                                                    nurse.LastKnownKORetired = retired;
                                                else if (canSeeRetired && Math.Abs(nurse.X - retired.X) < Math.Abs(nurse.X - nurse.LastKnownKORetired.X))
                                                    nurse.LastKnownKORetired = retired;
                                            }
                                        }
                                    }
                                }

                                nurse.Update(gameTime, boo, isTaunting);
                            }
                        }

                        // drops
                        for (int d = room.Drops.Count - 1; d >= 0; d--)
                        {
                            Droppable drop = room.Drops[d];
                            drop.Update(gameTime);

                            if (drop.Room == Player.Room && Math.Abs((drop.X + 8.0f) - (Player.X + 16.0f)) < 8.0f)
                            {
                                if (drop.Type == ObjectType.HealthPack)
                                {
                                    Player.Life += 2;
                                    Player.Life = Math.Min(Player.Life, 6);
                                }
                                else
                                {
                                    Score += 4 - (int)drop.Type;
                                }
                                room.Drops.RemoveAt(d);
                            }
                        }
                    }
                }

                // bullets
                for (int i = _bulletCount - 1; i >= 0; i--)
                {
                    if (_bullets[i].Update(gameTime, _bulletRectangle, Player))
                    {
                        _bulletCount--;
                        Bullet b = _bullets[_bulletCount];
                        _bullets[_bulletCount] = _bullets[i];
                        _bullets[i] = b;
                    }
                }

                // elevator
                if (_canEscape && _exitState == ElevatorState.Closed && _currentCopArrivingTime < 0.0f)
                    OpenExitElevator();
                else if (Player.HasEnteredElevator && _exitState == ElevatorState.Open)
                    CloseExitElevator();
            }
            // inter level
            else
            {
                _currentInterLevelFrameTime += gameTime;
                if (_currentInterLevelFrameTime >= _interLevelFrameTime)
                {
                    _currentElevatorFrame++;
                    _currentElevatorFrame %= _elevatorFrameCount;
                    _currentCableFrame++;
                    _currentCableFrame %= _cableFrameCount;
                    _currentElevatorBackgroundY++;
                    _currentElevatorBackgroundY %= _interLevelRectangle[_elevatorFrameCount + _cableFrameCount].Height;

                    _currentInterLevelFrameTime -= _interLevelFrameTime;
                }

                _currentElevatorKidFrameTime += gameTime;
                if (_currentElevatorKidFrameTime >= _elevatorKidFrameTime)
                {
                    _currentElevatorKidFrameTime -= _elevatorKidFrameTime;
                    _currentElevatorKidFrame++;
                    _currentElevatorKidFrame %= _elevatorKidFrameCount;
                }
            }

            // arrow animation
            _currentArrowFrameTime += gameTime;
            if (_currentArrowFrameTime > _arrowFrameTime)
            {
                _currentArrowFrameTime -= _arrowFrameTime;
                _currentArrowFrame++;
                _currentArrowFrame %= _objectsRectangle[(int)ObjectType.Arrow].Length;
            }

            // shout animation
            _currentShoutFrameTime += gameTime;
            if (_currentShoutFrameTime > _shoutFrameTime)
            {
                _currentShoutFrameTime -= _shoutFrameTime;
                _currentShoutFrame++;
                _currentShoutFrame %= Detail.AnimatedDetailFrames;
            }

            // camera
            if (_currentCameraTime < _totalCameraTime)
            {
                _currentCameraTime += gameTime;
                _cameraPositionY = EasingHelper.EaseOutExpo(_currentCameraTime, _initialCameraPositionY, _differenceCameraPositionY, _totalCameraTime);

                if (_currentCameraTime >= _totalCameraTime)
                    _cameraPositionY = _initialCameraPositionY + _differenceCameraPositionY;
            }
        }

        private void UpdateTransition(float gameTime)
        {
            if (_transitionState == TransitionState.Appearing)
            {
                _currentTransitionFrameTime += gameTime;

                if (_currentTransitionFrameTime >=_transitionFrameTime)
                {
                    _currentTransitionFrameTime -= _transitionFrameTime;

                    _currentTransitionFrame--;                    
                    if (_currentTransitionFrame < 0)
                    {
                        _transitionState = TransitionState.None;
                        OpenEntranceElevator();
                    }
                }
            }
            else if (_transitionState == TransitionState.Disappearing)
            {
                _currentTransitionFrameTime += gameTime;

                if (_currentTransitionFrameTime >= _transitionFrameTime)
                {
                    _currentTransitionFrameTime -= _transitionFrameTime;

                    _currentTransitionFrame++;
                    if (_currentTransitionFrame >= 3)
                    {
                        _currentTransitionFrame = 2;
                        _transitionState = TransitionState.Hidden;

                        if (Player != null && Player.HasEnteredElevator)
                        {                            
                            // init elevator position
                            //_elevatorX = (int)Player.X;
                            _elevatorX = 210;
                            _elevatorY = 69;// 128 - 40 * Player.Room.Y + 4;

                            // target score
                            _moneyString = "$" + (_targetMoney < 100 ? (_targetMoney < 10 ? "00" : "0") : String.Empty) + _targetMoney;
                            int min = _targetTime / 60;
                            int sec = _targetTime % 60;
                            _timeString = (min < 10 ? "0" : String.Empty) + min + ":" + (sec < 10 ? "0" : String.Empty) + sec;

                            // save score
                            SaveManager.Instance.SetScore(SelectedLevel, Score, Time);

                            // go to inter level
                            _building.CurrentLevel = null;
                            Player = null;

                            _pauseOption = 0;

                            // load transition
                            AppearTransition();
                        }
                        else
                        {
                            LoadLevel(SelectedLevel);
                        }
                    }
                }
            }
        }

        private void UpdateElevators(float gameTime)
        {
            if (_entranceState == ElevatorState.Closed)
            {
                _currentEntranceFrame = 0;
                _currentEntranceFrameTime = 0.0f;
            }
            else if (_entranceState == ElevatorState.Open)
            {
                _currentEntranceFrame = 4;
                _currentEntranceFrameTime = 0.0f;
            }
            else
            {
                _currentEntranceFrameTime += gameTime;

                if (_currentEntranceFrameTime >= _elevatorFrameTime)
                {
                    _currentEntranceFrameTime -= _elevatorFrameTime;

                    if (_entranceState == ElevatorState.Opening)
                    {
                        _currentEntranceFrame++;
                        if (_currentEntranceFrame > 4)
                        {
                            CloseEntranceElevator();
                        }
                        else if (_currentEntranceFrame == 2)
                        {
                            // pop cop
                            if (_spawningCop != null && _spawnEntrance)
                            {
                                _spawningCop.Exit();
                                //_spawningCop = null;
                            }
                            // pop player
                            else
                                Player.Exit();
                        }
                    }
                    else
                    {
                        _currentEntranceFrame--;
                        if (_currentEntranceFrame <= 0)
                        {
                            _currentEntranceFrame = 0;
                            _entranceState = ElevatorState.Closed;

                            if (_spawningCop != null)
                            {
                                _spawningCop = null;
                                if (_building.CurrentLevel.Cop1Count > 0)
                                {
                                    _building.CurrentLevel.Cop1Count--;
                                    SpawnCop(0.0f);
                                    _spawningCop.Type = 0;
                                }
                                else if (_building.CurrentLevel.Cop2Count > 0)
                                {
                                    _building.CurrentLevel.Cop2Count--;
                                    SpawnCop(0.0f);
                                    _spawningCop.Type = 1;
                                }
                            }
                        }
                    }
                }
            }

            if (_exitState == ElevatorState.Closed)
            {
                _currentExitFrame = 0;
                _currentExitFrameTime = 0.0f;
            }
            else if (_exitState == ElevatorState.Open)
            {
                _currentExitFrame = 4;
                _currentExitFrameTime = 0.0f;
            }
            else
            {
                _currentExitFrameTime += gameTime;

                if (_currentExitFrameTime >= _elevatorFrameTime)
                {
                    _currentExitFrameTime -= _elevatorFrameTime;

                    if (_exitState == ElevatorState.Opening)
                    {
                        _currentExitFrame++;
                        if (_currentExitFrame > 4)
                        {
                            _currentExitFrame = 4;
                            _exitState = ElevatorState.Open;

                            if (_spawningCop != null && !_spawnEntrance)
                            {                                
                                //_spawningCop = null;
                                CloseExitElevator();
                            }
                        }
                        else if (_currentExitFrame == 2)
                        {
                            // pop cop
                            if (_spawningCop != null && !_spawnEntrance)
                            {
                                _spawningCop.Exit();
                            }
                        }
                    }
                    else
                    {
                        _currentExitFrame--;
                        if (_currentExitFrame <= 0)
                        {
                            _currentExitFrame = 0;
                            _exitState = ElevatorState.Closed;

                            // start next level transition
                            if (Player.HasEnteredElevator)
                            {
                                DisappearTransition();
                            }

                            else if (_spawningCop != null)
                            {
                                _spawningCop = null;
                                if (_building.CurrentLevel.Cop1Count > 0)
                                {
                                    _building.CurrentLevel.Cop1Count--;
                                    SpawnCop(0.0f);
                                    _spawningCop.Type = 0;
                                }
                                else if (_building.CurrentLevel.Cop2Count > 0)
                                {
                                    _building.CurrentLevel.Cop2Count--;
                                    SpawnCop(0.0f);
                                    _spawningCop.Type = 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DisappearTransition()
        {
            if (_transitionState == TransitionState.None)
            {
                _transitionState = TransitionState.Disappearing;
                _currentTransitionFrame = 0;
                _currentTransitionFrameTime = 0.0f;
            }
        }

        public void AppearTransition()
        {
            if (_transitionState == TransitionState.Hidden)
            {
                _transitionState = TransitionState.Appearing;
                _currentTransitionFrame = 2;
                _currentTransitionFrameTime = 0.0f;
            }
        }

        private void OpenEntranceElevator()
        {
            _currentEntranceFrame = 0;
            _currentEntranceFrameTime = 0.0f;
            _entranceState = ElevatorState.Opening;
        }

        private void OpenExitElevator()
        {
            _currentExitFrame = 0;
            _currentExitFrameTime = 0.0f;
            _exitState = ElevatorState.Opening;
        }

        private void CloseEntranceElevator()
        {
            _currentEntranceFrame = 4;
            _currentEntranceFrameTime = 0.0f;
            _entranceState = ElevatorState.Closing;
        }

        private void CloseExitElevator()
        {
            _currentExitFrame = 4;
            _currentExitFrameTime = 0.0f;
            _exitState = ElevatorState.Closing;
        }

        private void SpawnRobocop()
        {
            Cop robocop = null;

            if (_building.CurrentLevel.RobocopCount > 0)
            {
                _building.CurrentLevel.RobocopCount--;
                robocop = new Cop();
                robocop.Type = Cop.NormalCopCount + Cop.SwatCopCount;
            }

            if (robocop == null)
                return;

            _nextRobocopSpawn = 0.1f + RandomHelper.Next() * 0.2f;

            // look for space to spawn
            int rFloor = RandomHelper.Next(_building.CurrentLevel.Floors.Length);
            while (_building.CurrentLevel.Floors[rFloor].Rooms.Length == 0) // avoid empty floor
            {
                rFloor++;
                rFloor %= _building.CurrentLevel.Floors.Length;
            }
            int rRoom = RandomHelper.Next(_building.CurrentLevel.Floors[rFloor].Rooms.Length);
            Room room = _building.CurrentLevel.Floors[rFloor].Rooms[rRoom];
            // find correct X
            int roomSize = (304 - room.RightMargin * 8 + 8 - 64 - (room.LeftMargin * 8 + 16 + 32));
            int x = room.LeftMargin * 8 + 16 + 32 + RandomHelper.Next(roomSize);
            for (int r = 0; r < room.RoomDoors.Length; r++)
            {
                RoomDoor door = room.RoomDoors[r];
                if ((x >= door.X && x <= door.X + 32) ||
                    (x + 32 >= door.X && x + 32 <= door.X + 32))
                {
                    door.OpenLeft();
                    door.IsRobocopBlocked = true;
                }
            }

            robocop.Room = room;
            room.Cops.Add(robocop);

            robocop.SpawnLand(x, (RandomHelper.Next() > 0.5f ? SpriteEffects.None : SpriteEffects.FlipHorizontally));
        }
        
        private void SpawnSwat()
        {
            Cop swat = null;

            if (_building.CurrentLevel.Swat1Count > 0)
            {
                _building.CurrentLevel.Swat1Count--;
                swat = new Cop();
                swat.Type = Cop.NormalCopCount;
            }

            if (swat == null)
                return;

            _nextSwatSpawn = 0.1f + RandomHelper.Next() * 0.2f;

            // look for a window
            int rFloor = RandomHelper.Next(_building.CurrentLevel.Floors.Length);
            while (_building.CurrentLevel.Floors[rFloor].Rooms.Length == 0) // avoid empty floor
            {
                rFloor++;
                rFloor %= _building.CurrentLevel.Floors.Length;
            }
            int rRoom = RandomHelper.Next(_building.CurrentLevel.Floors[rFloor].Rooms.Length);
            Room room = _building.CurrentLevel.Floors[rFloor].Rooms[rRoom];

            swat.Room = room;
            room.Cops.Add(swat);

            if (RandomHelper.Next() > 0.5f)
            {
                // spawn on left wall
                swat.SpawnWindow(room.LeftMargin * 8 + 16, SpriteEffects.None);                
            }
            else
            {
                // spawn on right wall                
                swat.SpawnWindow(304 - room.RightMargin * 8 + 8 - 32, SpriteEffects.FlipHorizontally);
            }
        }

        public void SpawnCop(float timeBeforeSpawn = _copArrivingTime)
        {
            if (_currentCopArrivingTime >= 0.0f)
                return; // cop already in queue

            _spawningCop = new Cop();

            // look for entrance or exit
            _spawnEntrance = false;
            if (_exitState != ElevatorState.Closed || RandomHelper.Next() < 0.5f)
            {
                _spawnEntrance = true;
            }

            // look for start
            if (_spawnEntrance)
            {
                for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
                {
                    for (int r = 0; r < _building.CurrentLevel.Floors[f].Rooms.Length; r++)
                    {
                        if (_building.CurrentLevel.Floors[f].Rooms[r].HasStart)
                        {
                            _spawningCop.Room = _building.CurrentLevel.Floors[f].Rooms[r];
                            _spawningCop.Room.Cops.Add(_spawningCop);
                            _spawningCop.SpawnElevator(_spawningCop.Room.StartX + 4);
                            break;
                        }
                    }
                    if (_spawningCop.Room != null)
                        break;
                }
            }
            // look for exit
            else
            {
                for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
                {
                    for (int r = 0; r < _building.CurrentLevel.Floors[f].Rooms.Length; r++)
                    {
                        if (_building.CurrentLevel.Floors[f].Rooms[r].HasExit)
                        {
                            _spawningCop.Room = _building.CurrentLevel.Floors[f].Rooms[r];
                            _spawningCop.Room.Cops.Add(_spawningCop);
                            _spawningCop.SpawnElevator(_spawningCop.Room.ExitX + 4);
                            break;
                        }
                    }
                    if (_spawningCop.Room != null)
                        break;
                }
            }

            // open entrance / door
            _currentCopArrivingTime = timeBeforeSpawn;
        }

        private void MoveCamera(float targetPosition)
        {
            // limit
            targetPosition = Math.Max(0.0f, targetPosition);
            targetPosition = Math.Min(targetPosition, (_building.CurrentLevel.Floors.Length - 4) * 40.0f);

            _initialCameraPositionY = _cameraPositionY;
            _differenceCameraPositionY = targetPosition - _cameraPositionY;
            _currentCameraTime = 0.0f;
        }

        private void FireBullet(BulletType type, float x, SpriteEffects orientation, Room room)
        {
            if (_bulletCount < _maxBullets)
            {
                if (orientation == SpriteEffects.None)
                    x += 16.0f;
                _bullets[_bulletCount].Fire(type, x, orientation, room);
                _bulletCount++;
            }
        }

        private void DrawGameoverBox(SpriteBatch spriteBatch, int width, int frame)
        {
            Texture2D texture = TextureManager.Gameplay;

            if (frame == 0)
            {
                Rectangle pixel = UIHelper.Instance.PixelRectangle;

                spriteBatch.Draw(texture,
                    new Rectangle(
                        GreedyKidGame.Width / 2 - 15 / 2,
                        GreedyKidGame.Height / 2 - 9 / 2,
                        15,
                        9),
                    pixel,
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(
                        GreedyKidGame.Width / 2 - 13 / 2,
                        GreedyKidGame.Height / 2 - 11 / 2,
                        13,
                        11),
                    pixel,
                    Color.White);

                return;
            }

            int height = 46;

            if (frame == 1)
            {
                width = width + 4;
                height = 15;
            }
            else if (frame == 2)
            {
                width = width - 4;
                height = height + 4;
            }

            int x = GreedyKidGame.Width / 2 - width / 2;
            int y = GreedyKidGame.Height / 2 - height / 2;


            // background
            spriteBatch.Draw(texture,
                new Rectangle(
                    x + _gameoverRectangle[0].Width - 1,
                    y + _gameoverRectangle[0].Height - 1,
                    width - _gameoverRectangle[0].Width - _gameoverRectangle[2].Width + 2,
                    height - _gameoverRectangle[0].Height - _gameoverRectangle[6].Height + 2),
                _gameoverRectangle[8],
                Color.White);

            // upper left
            spriteBatch.Draw(texture,
                new Rectangle(
                    x,
                    y,
                    _gameoverRectangle[0].Width,
                    _gameoverRectangle[0].Height),
                _gameoverRectangle[0],
                Color.White);

            // up
            spriteBatch.Draw(texture,
                new Rectangle(
                    x + _gameoverRectangle[0].Width - 1,
                    y,
                    width - _gameoverRectangle[0].Width - _gameoverRectangle[2].Width + 2,
                    _gameoverRectangle[1].Height),
                _gameoverRectangle[1],
                Color.White);

            // upper right
            spriteBatch.Draw(texture,
                new Rectangle(
                    x + width - _gameoverRectangle[2].Width,
                    y,
                    _gameoverRectangle[2].Width,
                    _gameoverRectangle[2].Height),
                _gameoverRectangle[2],
                Color.White);

            // right
            spriteBatch.Draw(texture,
                new Rectangle(
                    x + width - _gameoverRectangle[3].Width,
                    y + _gameoverRectangle[2].Height - 1,
                    _gameoverRectangle[3].Width,
                    height - _gameoverRectangle[2].Height - _gameoverRectangle[4].Height + 2),
                _gameoverRectangle[3],
                Color.White);

            // lower right
            spriteBatch.Draw(texture,
                new Rectangle(
                    x + width - _gameoverRectangle[4].Width,
                    y + height - _gameoverRectangle[4].Height,
                    _gameoverRectangle[4].Width,
                    _gameoverRectangle[4].Height),
                _gameoverRectangle[4],
                Color.White);

            // down
            spriteBatch.Draw(texture,
                new Rectangle(
                    x + _gameoverRectangle[0].Width - 1,
                    y + height - _gameoverRectangle[5].Height,
                    width - _gameoverRectangle[0].Width - _gameoverRectangle[2].Width + 2,
                    _gameoverRectangle[5].Height),
                _gameoverRectangle[5],
                Color.White);

            // lower left
            spriteBatch.Draw(texture,
                new Rectangle(
                    x,
                    y + height - _gameoverRectangle[6].Height,
                    _gameoverRectangle[6].Width,
                    _gameoverRectangle[6].Height),
                _gameoverRectangle[6],
                Color.White);

            // left
            spriteBatch.Draw(texture,
                new Rectangle(
                    x,
                    y + _gameoverRectangle[2].Height - 1,
                    _gameoverRectangle[3].Width,
                    height - _gameoverRectangle[2].Height - _gameoverRectangle[4].Height + 2),
                _gameoverRectangle[7],
                Color.White);

            if (frame >= 2)
            {
                // text
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Restart, y + 8, 0, _pauseOption);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Quit, y + 8 + 15, 1, _pauseOption);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Gameplay;

            int cameraPosY = (int)Math.Round(_cameraPositionY);

            if (!_pause && IsIngame)
            {                
                int shoutingFrame = Furniture.FurnitureFrames - 4 + _currentShoutFrame;
                bool isShouting = Player.IsShouting;
                int playerMiddle = (int)Player.X + 16;

                for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
                {
                    Floor floor = _building.CurrentLevel.Floors[f];

                    // rooms
                    for (int r = 0; r < floor.Rooms.Length; r++)
                    {
                        Room room = floor.Rooms[r];
                        // background
                        int startX = 16 + room.LeftMargin * 8;
                        int nbSlice = 37 - room.LeftMargin - room.RightMargin;

                        Rectangle source = _roomRectangle[room.BackgroundColor][0][1];

                        for (int s = 0; s < nbSlice; s++)
                        {
                            spriteBatch.Draw(texture,
                                new Rectangle(startX + 8 * s, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // left wall
                        source = _roomRectangle[room.BackgroundColor][room.LeftDecoration][0];

                        spriteBatch.Draw(texture,
                            new Rectangle(room.LeftMargin * 8, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                            source,
                            Color.White);

                        // right wall
                        source = _roomRectangle[room.BackgroundColor][room.RightDecoration][2];

                        spriteBatch.Draw(texture,
                            new Rectangle(304 - room.RightMargin * 8, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                            source,
                            Color.White);
                    }

                    // rooms details
                    for (int r = 0; r < floor.Rooms.Length; r++)
                    {
                        Room room = floor.Rooms[r];

                        for (int d = 0; d < room.Details.Length; d++)
                        {
                            Detail detail = room.Details[d];

                            int frame = 0;
                            if (isShouting && room == Player.Room && detail.Type >= Detail.NormalDetailCount && Math.Abs(detail.X + 16 - playerMiddle) <= _shoutDistance)
                                frame = _currentShoutFrame;

                            Rectangle source = _detailRectangle[room.BackgroundColor][detail.Type][frame];

                            spriteBatch.Draw(texture,
                                new Rectangle(detail.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }
                    }


                    for (int r = 0; r < floor.Rooms.Length; r++)
                    {
                        Room room = floor.Rooms[r];

                        // floor doors
                        for (int d = 0; d < room.FloorDoors.Length; d++)
                        {
                            FloorDoor floorDoor = room.FloorDoors[d];
                            Rectangle source = _floorDoorRectangle[room.BackgroundColor][floorDoor.Color][floorDoor.Frame];

                            spriteBatch.Draw(texture,
                                new Rectangle(floorDoor.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            if (floorDoor.CanOpen)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(floorDoor.X + 8, 128 - 40 * f + 8 + cameraPosY, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                        }

                        // furniture
                        for (int ff = 0; ff < room.Furnitures.Length; ff++)
                        {
                            Furniture furniture = room.Furnitures[ff];

                            int frame = furniture.Frame;
                            if (isShouting && room == Player.Room && Math.Abs(furniture.X + 16 - playerMiddle) <= _shoutDistance)
                                frame = shoutingFrame;
                            
                            Rectangle source = _furnitureRectangle[room.BackgroundColor][furniture.Type][frame];

                            spriteBatch.Draw(texture,
                                new Rectangle(furniture.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            if (furniture.CanHide)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(furniture.X + 8, 128 - 40 * f + 8 + cameraPosY, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                        }

                        // room doors
                        for (int d = 0; d < room.RoomDoors.Length; d++)
                        {
                            RoomDoor roomDoor = room.RoomDoors[d];
                            Rectangle source = _roomDoorRectangle[room.BackgroundColor][roomDoor.Frame];

                            spriteBatch.Draw(texture,
                                new Rectangle(roomDoor.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            if (roomDoor.CanClose && roomDoor.IsOpenLeft)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(roomDoor.X, 128 - 40 * f + 8 + cameraPosY, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                            else if (roomDoor.CanClose && roomDoor.IsOpenRight)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(roomDoor.X + 16, 128 - 40 * f + 8 + cameraPosY, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                        }

                        // elevator
                        if (room.HasStart)
                        {
                            // background
                            Rectangle source = _elevatorRectangle[0][_currentEntranceFrame];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            // player
                            if (_entranceState == ElevatorState.Opening && _spawningCop == null)
                                Player.Draw(spriteBatch, cameraPosY);

                            // door
                            source = _elevatorRectangle[1][_currentEntranceFrame];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            // shadow
                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        if (room.HasExit)
                        {
                            // background
                            Rectangle source = _elevatorRectangle[0][_currentExitFrame];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            // player
                            if (Player.HasEnteredElevator)
                                Player.Draw(spriteBatch, cameraPosY);

                            // door
                            source = _elevatorRectangle[1][_currentExitFrame];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            // shadow
                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            // arrow
                            if (Player.CanEnterElevator)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(room.ExitX + 12, 128 - 40 * f + 8 + cameraPosY, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                        }
                    }
                }

                if (!_pause)
                    if ((Player != null && _spawnEntrance && _spawningCop != null && Player.Life <= 0) || (_entranceState != ElevatorState.Opening && Player != null && !Player.HasEnteredElevator && Player.Life <= 0))
                        Player.Draw(spriteBatch, cameraPosY);

                // retireds & nurses
                for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
                {
                    Floor floor = _building.CurrentLevel.Floors[f];

                    // rooms
                    for (int r = 0; r < floor.Rooms.Length; r++)
                    {
                        Room room = floor.Rooms[r];

                        // retired
                        for (int rr = 0; rr < room.Retireds.Count; rr++)
                        {
                            Retired retired = room.Retireds[rr];
                            if (retired != null)
                                retired.Draw(spriteBatch, cameraPosY);
                        }

                        // nurse
                        for (int n = 0; n < room.Nurses.Count; n++)
                        {
                            Nurse nurse = room.Nurses[n];
                            if (nurse != null)
                                nurse.Draw(spriteBatch, cameraPosY);
                        }
                    }
                }

                // cops & drops
                for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
                {
                    Floor floor = _building.CurrentLevel.Floors[f];

                    // rooms
                    for (int r = 0; r < floor.Rooms.Length; r++)
                    {
                        Room room = floor.Rooms[r];

                         // cops
                        for (int c = 0; c < room.Cops.Count; c++)
                        {
                            Cop cop = room.Cops[c];
                            if (cop != null)
                                cop.Draw(spriteBatch, cameraPosY);
                        }

                        // drops
                        for (int d = 0; d < room.Drops.Count; d++)
                        {
                            Droppable drop = room.Drops[d];

                            drop.Draw(spriteBatch, _objectsRectangle, cameraPosY);
                        }
                    }
                }

                // bullets
                for (int i = 0; i < _bulletCount; i++)
                    _bullets[i].Draw(spriteBatch, _bulletRectangle, cameraPosY);
            }
            // pause
            else if (_pause)
            {
                if (!_inSettings)
                {
                    // background
                    spriteBatch.Draw(texture,
                        new Rectangle(20, 17, _pauseBackgroundRectangles[0].Width, _pauseBackgroundRectangles[0].Height),
                        _pauseBackgroundRectangles[0],
                        Color.White);
                    spriteBatch.Draw(texture,
                        new Rectangle(180, 17, _pauseBackgroundRectangles[1].Width, _pauseBackgroundRectangles[1].Height),
                        _pauseBackgroundRectangles[1],
                        Color.White);

                    // text
                    int yStart = 70;

                    UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Resume, yStart, 0, _pauseOption);
                    UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Settings, yStart + 15, 1, _pauseOption);
                    UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Restart, yStart + 30, 2, _pauseOption);
                    UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Quit, yStart + 45, 3, _pauseOption);
                }
                else
                {
                    SettingsManager.Instance.Draw(spriteBatch);
                }
            }
            // inter level
            else
            {
                // background
                Rectangle backgroundSource1 = _interLevelRectangle[_elevatorFrameCount + _cableFrameCount];
                Rectangle backgroundSource2 = backgroundSource1;                
                backgroundSource1.Y = backgroundSource1.Y + (backgroundSource1.Height - _currentElevatorBackgroundY);
                backgroundSource1.Height = _currentElevatorBackgroundY;
                backgroundSource2.Height = backgroundSource2.Height - _currentElevatorBackgroundY;

                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX - 16, -28, backgroundSource1.Width, backgroundSource1.Height),
                    backgroundSource1,
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX - 16, -28 + _currentElevatorBackgroundY, backgroundSource2.Width, backgroundSource2.Height),
                    backgroundSource2,
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX - 16, 9, _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 1].Width, _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 1].Height),
                    _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 1],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX - 16, 168, _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 2].Width, _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 2].Height),
                    _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 2],
                    Color.White);


                // cable
                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX + 8, 13, _interLevelRectangle[_elevatorFrameCount].Width, _interLevelRectangle[_elevatorFrameCount].Height),
                    _interLevelRectangle[_elevatorFrameCount + _currentCableFrame],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX + 23, 13, _interLevelRectangle[_elevatorFrameCount].Width, _interLevelRectangle[_elevatorFrameCount].Height),
                    _interLevelRectangle[_elevatorFrameCount + (_cableFrameCount - 1 - _currentCableFrame)],
                    Color.White);

                // elevator
                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX, _elevatorY, _interLevelRectangle[0].Width, _interLevelRectangle[0].Height),
                    _interLevelRectangle[_currentElevatorFrame],
                    Color.White);

                // kid
                spriteBatch.Draw(texture,
                    new Rectangle(_elevatorX, _elevatorY, _interLevelRectangle[0].Width, _interLevelRectangle[0].Height),
                    _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 3 + _currentElevatorKidFrame],
                    Color.White);


                // score recap
                int recapX = 36;
                int recapY = 30;
                int recapWidth = _scoreRectangle[6].Width;

                // stars
                int stars = 1;
                if (Time <= _targetTime)
                    stars++;
                if (Score >= _targetMoney)
                    stars++;

                spriteBatch.Draw(texture,
                    new Rectangle(
                        recapX + recapWidth / 2 - _scoreRectangle[0].Width / 2,
                        recapY,
                        _scoreRectangle[0].Width,
                        _scoreRectangle[0].Height),
                    _scoreRectangle[stars],
                    Color.White);

                // stage clear
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.StageClear, recapY + 13, -1, _pauseOption, recapWidth, recapX);

                // sperator
                spriteBatch.Draw(texture,
                    new Rectangle(
                        recapX + recapWidth / 2 - _scoreRectangle[6].Width / 2,
                        recapY + 29,
                        _scoreRectangle[6].Width,
                        _scoreRectangle[6].Height),
                    _scoreRectangle[6],
                    Color.White);

                SpriteFont genericFont = TextManager.Instance.GenericFont;
                SpriteFont font = TextManager.Instance.Font;

                // time
                int timeWidth = _scoreRectangle[(Time <= _targetTime ? 4 : 5)].Width + (int)font.MeasureString(TextManager.Instance.Time).X + (int)genericFont.MeasureString(_timeString).X;
                int timeX = recapX + recapWidth / 2 - timeWidth / 2;
                spriteBatch.Draw(texture,
                    new Rectangle(
                        timeX,
                        recapY + 42,
                        _scoreRectangle[(Time <= _targetTime ? 4 : 5)].Width,
                        _scoreRectangle[(Time <= _targetTime ? 4 : 5)].Height),
                    _scoreRectangle[(Time <= _targetTime ? 4 : 5)],
                    Color.White);
                spriteBatch.DrawString(font,
                    TextManager.Instance.Time,
                    new Vector2(timeX + _scoreRectangle[(Time <= _targetTime ? 4 : 5)].Width, recapY + 39),
                    Color.White);
                spriteBatch.DrawString(font,
                    _timeString,
                    new Vector2(timeX + timeWidth - (int)genericFont.MeasureString(_timeString).X, recapY + 39),
                    Color.White);

                // money
                int moneyWidth = _scoreRectangle[(Score >= _targetMoney ? 4 : 5)].Width + (int)font.MeasureString(TextManager.Instance.Money).X + (int)genericFont.MeasureString(_moneyString).X;
                int moneyX = recapX + recapWidth / 2 - moneyWidth / 2;
                spriteBatch.Draw(texture,
                    new Rectangle(
                        moneyX,
                        recapY + 57,
                        _scoreRectangle[(Score >= _targetMoney ? 4 : 5)].Width,
                        _scoreRectangle[(Score >= _targetMoney ? 4 : 5)].Height),
                    _scoreRectangle[(Score >= _targetMoney ? 4 : 5)],
                    Color.White);
                spriteBatch.DrawString(font,
                    TextManager.Instance.Money,
                    new Vector2(moneyX + _scoreRectangle[(Score >= _targetMoney ? 4 : 5)].Width, recapY + 54),
                    Color.White);
                spriteBatch.DrawString(font,
                    _moneyString,
                    new Vector2(moneyX + moneyWidth - (int)genericFont.MeasureString(_moneyString).X, recapY + 54),
                    Color.White);

                // separator
                spriteBatch.Draw(texture,
                    new Rectangle(
                        recapX + recapWidth / 2 - _scoreRectangle[6].Width / 2,
                        recapY + 69,
                        _scoreRectangle[6].Width,
                        _scoreRectangle[6].Height),
                    _scoreRectangle[6],
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    SpriteEffects.FlipVertically,
                    0.0f
                    );

                // selection
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.NextStage, recapY + 82, 0, _pauseOption, recapWidth, recapX);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Restart, recapY + 82 + 15, 1, _pauseOption, recapWidth, recapX);
                UIHelper.Instance.DrawCenteredText(spriteBatch, TextManager.Instance.Quit, recapY + 82 + 30, 2, _pauseOption, recapWidth, recapX);
            }


            if (!_pause)
                if ((Player != null && _spawnEntrance && _spawningCop != null && Player.Life > 0) || (_entranceState != ElevatorState.Opening && Player != null && !Player.HasEnteredElevator && Player.Life > 0))
                    Player.Draw(spriteBatch, cameraPosY);

            // gameover
            if (Gameover)
            {
                // background
                if (_currentGameOverFrame == 1)
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(0, GreedyKidGame.Height / 2 - _transitionRectangle[4].Height / 2, GreedyKidGame.Width, _transitionRectangle[4].Height),
                        _transitionRectangle[4],
                        Color.White);
                }
                else if (_currentGameOverFrame == 2)
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(0, GreedyKidGame.Height / 2 - _transitionRectangle[5].Height / 2, GreedyKidGame.Width, _transitionRectangle[5].Height),
                        _transitionRectangle[5],
                        Color.White);
                }
                else if (_currentGameOverFrame == 3)
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(0, GreedyKidGame.Height / 2 - _transitionRectangle[6].Height / 2, GreedyKidGame.Width, _transitionRectangle[6].Height),
                        _transitionRectangle[6],
                        Color.White);
                }
                else
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                        _transitionRectangle[3],
                        Color.White);
                }

                // box
                if (_currentGameOverFrame > 0)
                    DrawGameoverBox(spriteBatch, 117, _currentGameOverFrame - 1);
            }

            // transition
            if (_transitionState != TransitionState.None && !_pause)
            {
                if (_currentTransitionFrame == 2) // full
                {
                    spriteBatch.Draw(texture,
                        new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                        _transitionRectangle[0],
                        Color.White);
                }
                else
                {
                    int focusX = _elevatorX - 8;
                    int focusY = _elevatorY - 2;
                    if (Player != null)
                    {
                        focusX = (int)Player.X - 9;
                        focusY = 128 - 40 * Player.Room.Y + 5;
                    }

                    spriteBatch.Draw(texture,
                        new Rectangle(focusX, focusY, _transitionRectangle[_currentTransitionFrame + 1].Width, _transitionRectangle[_currentTransitionFrame + 1].Height),
                        _transitionRectangle[_currentTransitionFrame + 1],
                        Color.White);

                    // background
                    int frame = 3;
                    if (_currentTransitionFrame == 1)
                        frame = 0;

                    // right
                    spriteBatch.Draw(texture,
                    new Rectangle(focusX + _transitionRectangle[_currentTransitionFrame + 1].Width,
                        focusY,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);
                    // left
                    spriteBatch.Draw(texture,
                    new Rectangle(focusX - _transitionRectangle[3].Width,
                        focusY +_transitionRectangle[_currentTransitionFrame + 1].Height - _transitionRectangle[3].Height,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);
                    // up
                    spriteBatch.Draw(texture,
                    new Rectangle(focusX,
                        focusY - _transitionRectangle[3].Height,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);
                    // down
                    spriteBatch.Draw(texture,
                    new Rectangle(focusX + _transitionRectangle[_currentTransitionFrame + 1].Width - _transitionRectangle[3].Width,
                        focusY + _transitionRectangle[_currentTransitionFrame + 1].Height,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);
                    
                }
            }

            // ****** UI ******
            if (IsIngame)
            {
                // floor mask
                spriteBatch.Draw(texture,
                    new Rectangle(0, 0, GreedyKidGame.Width, 14),
                    _maskRectangle[3],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(0, 14, GreedyKidGame.Width, 2),
                    _maskRectangle[4],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(0, GreedyKidGame.Height - 12, GreedyKidGame.Width, 12),
                    _maskRectangle[3],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(0, GreedyKidGame.Height - 14, GreedyKidGame.Width, 2),
                    _maskRectangle[4],
                    Color.White);
            }

            UIHelper.Instance.DrawBorders(spriteBatch);

            // masks
            spriteBatch.Draw(texture,
                new Rectangle(19, 0, _maskRectangle[0].Width, _maskRectangle[0].Height),
                _maskRectangle[0],
                Color.White);
            if (!_pause && !Gameover)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(136, 0, _maskRectangle[1].Width, _maskRectangle[1].Height),
                    _maskRectangle[1],
                    Color.White);
            }
            spriteBatch.Draw(texture,
                new Rectangle(257, 0, _maskRectangle[2].Width, _maskRectangle[2].Height),
                _maskRectangle[2],
                Color.White);

            UIHelper.Instance.DrawMicrophoneVolume(spriteBatch);

            // cop timer
            if (IsIngame && !_pause && _totalCopTimer > 0 && !Gameover)
            {
                int maxX = GreedyKidGame.Width - _copTimerRectangle[12].Width - 15;
                int minX = 15;
                // warning
                spriteBatch.Draw(texture,
                    new Rectangle(maxX,
                        GreedyKidGame.Height - _copTimerRectangle[12].Height - 1,
                        _copTimerRectangle[12].Width,
                        _copTimerRectangle[12].Height),
                    _copTimerRectangle[12],
                    Color.White);

                int distance = maxX - _copTimerRectangle[9].Width - _copTimerRectangle[0].Width + 2 - minX;
                int time = 0;

                if (_building.CurrentLevel.TimeBeforeRobocop > 0)
                {
                    time = _building.CurrentLevel.TimeBeforeRobocop + _building.CurrentLevel.TimeBeforeSwat + _building.CurrentLevel.TimeBeforeCop;

                    // icon
                    spriteBatch.Draw(texture,
                       new Rectangle(minX - 1 + _copTimerRectangle[0].Width + (int)(distance - distance * ((time - _currentSeconds) / _totalCopTimer)),
                           GreedyKidGame.Height - _copTimerRectangle[11].Height - 1,
                           _copTimerRectangle[11].Width,
                           _copTimerRectangle[11].Height),
                       _copTimerRectangle[11],
                       Color.White);

                    // count
                    spriteBatch.Draw(texture,
                      new Rectangle(minX + (int)(distance - distance * ((time - _currentSeconds) / _totalCopTimer)),
                          GreedyKidGame.Height - _copTimerRectangle[9].Height - 1,
                          _copTimerRectangle[0].Width,
                          _copTimerRectangle[0].Height),
                      _copTimerRectangle[_building.CurrentLevel.RobocopCount - 1],
                      Color.White);                    
                }

                if (_building.CurrentLevel.TimeBeforeSwat > 0)
                {
                    time = _building.CurrentLevel.TimeBeforeSwat + _building.CurrentLevel.TimeBeforeCop;

                    // icon
                    spriteBatch.Draw(texture,
                       new Rectangle(minX - 1 + _copTimerRectangle[0].Width + (int)(distance - distance * ((time - _currentSeconds) / _totalCopTimer)),
                           GreedyKidGame.Height - _copTimerRectangle[10].Height - 1,
                           _copTimerRectangle[10].Width,
                           _copTimerRectangle[10].Height),
                       _copTimerRectangle[10],
                       Color.White);

                    // count
                    spriteBatch.Draw(texture,
                      new Rectangle(minX + (int)(distance - distance * ((time - _currentSeconds) / _totalCopTimer)),
                          GreedyKidGame.Height - _copTimerRectangle[9].Height - 1,
                          _copTimerRectangle[0].Width,
                          _copTimerRectangle[0].Height),
                      _copTimerRectangle[_building.CurrentLevel.Swat1Count - 1],
                      Color.White);
                }

                if (_building.CurrentLevel.TimeBeforeCop > 0)
                {
                    time = _building.CurrentLevel.TimeBeforeCop;

                    // icon
                    spriteBatch.Draw(texture,
                       new Rectangle(minX - 1 + _copTimerRectangle[0].Width + (int)(distance - distance * ((time - _currentSeconds) / _totalCopTimer)),
                           GreedyKidGame.Height - _copTimerRectangle[9].Height - 1,
                           _copTimerRectangle[9].Width,
                           _copTimerRectangle[9].Height),
                       _copTimerRectangle[9],
                       Color.White);

                    // count
                    spriteBatch.Draw(texture,
                      new Rectangle(minX + (int)(distance - distance * ((time - _currentSeconds) / _totalCopTimer)),
                          GreedyKidGame.Height - _copTimerRectangle[9].Height - 1,
                          _copTimerRectangle[0].Width,
                          _copTimerRectangle[0].Height),
                      _copTimerRectangle[_initialNormalCopCount - 1],
                      Color.White);
                }
            }

            // player life
            for (int h = 0; h < 3; h++)
            {
                if (Player != null && Player.Life / ((h + 1) * 2) >= 1)
                {
                    // full
                    spriteBatch.Draw(texture,
                        new Rectangle(24 + h * _iconRectangle[0].Width, 0, _iconRectangle[0].Width, _iconRectangle[0].Height),
                        _iconRectangle[0],
                        Color.White);
                }
                else if (Player != null && (Player.Life - h * 2) % ((h + 1) * 2) >= 1)
                {
                    // half
                    spriteBatch.Draw(texture,
                        new Rectangle(24 + h * _iconRectangle[1].Width, 0, _iconRectangle[1].Width, _iconRectangle[1].Height),
                        _iconRectangle[1],
                        Color.White);
                }
                else
                {
                    // empty
                    spriteBatch.Draw(texture,
                        new Rectangle(24 + h * _iconRectangle[2].Width, 0, _iconRectangle[2].Width, _iconRectangle[2].Height),
                        _iconRectangle[2],
                        Color.White);
                }
            }

            int textX = 0;

            if (!_pause && !Gameover)
            {
                // time
                _encodedTime[0] = Time / 600;
                _encodedTime[1] = (Time - _encodedTime[0] * 600) / 60;
                int seconds = Time % 60;
                _encodedTime[3] = seconds / 10;
                _encodedTime[4] = seconds % 10;

                
                for (int t = 0; t < _encodedTime.Length; t++)
                {
                    Rectangle source = _numberRectangle[_encodedTime[t]];
                    spriteBatch.Draw(texture,
                        new Rectangle(140 + textX, 0, source.Width, source.Height),
                        source,
                        Color.White);

                    textX += source.Width;
                }
            }
            else if (Gameover)
            {
                if (_currentGameOverFrame < 2)
                    UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Gameover, 0);
                else if (_currentGameOverFrame < 4)
                    UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Gameover, 1);
                else if (_currentGameOverFrame < 6)
                    UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Gameover, 2);
                else
                    UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Gameover, 1);
            }
            else if (!_inSettings)
            {
                UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Pause);
            }
            else
            {
                UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Settings);
            }

            if (_pause)
            {
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Select, CommandType.Select);
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Back, CommandType.Back, true);

                if (InputManager.PlayerDevice != null)
                    InputManager.PlayerDevice.Draw(spriteBatch);
            }
            else if (Gameover)
            {
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Select, CommandType.Select);

                if (InputManager.PlayerDevice != null)
                    InputManager.PlayerDevice.Draw(spriteBatch);
            }
            else if (!IsIngame)
            {
                UIHelper.Instance.DrawCommand(spriteBatch, TextManager.Instance.Select, CommandType.Select);

                if (InputManager.PlayerDevice != null)
                    InputManager.PlayerDevice.Draw(spriteBatch);
            }

            // score
            _encodedScore[0] = Score / 100;
            _encodedScore[1] = (Score - _encodedScore[0] * 100) / 10;
            _encodedScore[2] = Score % 10;

            textX = 0;
            for (int s = 0; s < _encodedScore.Length; s++)
            {
                Rectangle source = _numberRectangle[_encodedScore[s]];
                spriteBatch.Draw(texture,
                    new Rectangle(261 + textX, 0, source.Width, source.Height),
                    source,
                    Color.White);

                textX += source.Width;
            }
            // $
            spriteBatch.Draw(texture,
                new Rectangle(261 + textX, 0, _numberRectangle[11].Width, _numberRectangle[11].Height),
                _numberRectangle[11],
                Color.White);

            // robocop beam
            if (!_pause && IsIngame)
            {
                // cops & drops
                for (int f = 0; f < _building.CurrentLevel.Floors.Length; f++)
                {
                    Floor floor = _building.CurrentLevel.Floors[f];

                    // rooms
                    for (int r = 0; r < floor.Rooms.Length; r++)
                    {
                        Room room = floor.Rooms[r];

                        // cops
                        for (int c = 0; c < room.Cops.Count; c++)
                        {
                            Cop cop = room.Cops[c];
                            if (cop != null)
                            {
                                int x = cop.RobocopBeamPosition;
                                if (x >= 0)
                                {
                                    spriteBatch.Draw(texture,
                                        new Rectangle(x, 0, _robocopBeamRectangle.Width, _robocopBeamRectangle.Height),
                                        _robocopBeamRectangle,
                                        Color.White);
                                }
                                if (cop.RobocopFlash)
                                {
                                    spriteBatch.Draw(texture,
                                        new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                                        UIHelper.Instance.PixelRectangle,
                                        Color.White);
                                }
                            }
                        }
                    }
                }
            }

            spriteBatch.End();
        }        
    }
}

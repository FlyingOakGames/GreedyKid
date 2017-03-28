﻿using Microsoft.Xna.Framework;
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

    public sealed class BuildingManager
    {
        private Building _building;

        private Rectangle[][][] _roomRectangle;
        private Rectangle[][][] _detailRectangle;
        private Rectangle[][][] _floorDoorRectangle;
        private Rectangle[][] _roomDoorRectangle;
        private Rectangle[][] _elevatorRectangle;
        private Rectangle[][][] _furnitureRectangle;
        private Rectangle[][] _objectsRectangle;

        private Rectangle[] _uiRectangle;
        private Rectangle[] _iconRectangle;
        private Rectangle[] _maskRectangle;
        private Rectangle[] _numberRectangle;

        public int SelectedLevel = 0;

        public Player Player;

        public int Score = 0;
        private int[] _encodedScore = new int[] { 0, 0, 0 };
        public int Time = 0;
        private int[] _encodedTime = new int[] { 0, 0, 10, 0, 0 };

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

        public BuildingManager()
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
                    _objectsRectangle[o][f] = new Rectangle(2048 - objectFrameCount * 16, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 16, 16);
                }
            }

            // UI
            _uiRectangle = new Rectangle[8];
            _uiRectangle[0] = new Rectangle(0, 2024, 12, 13); // upper left
            _uiRectangle[1] = new Rectangle(11, 2024, 12, 13); // upper right
            _uiRectangle[2] = new Rectangle(0, 2036, 12, 12); // lower left
            _uiRectangle[3] = new Rectangle(11, 2036, 12, 12); // lower right
            _uiRectangle[4] = new Rectangle(11, 2024, 1, 9); // up
            _uiRectangle[5] = new Rectangle(11, 2040, 1, 8); // down
            _uiRectangle[6] = new Rectangle(0, 2036, 8, 1); // left
            _uiRectangle[7] = new Rectangle(15, 2036, 8, 1); // right

            _iconRectangle = new Rectangle[3];
            _iconRectangle[0] = new Rectangle(23, 2024, 13, 13);
            _iconRectangle[1] = new Rectangle(36, 2024, 13, 13);
            _iconRectangle[2] = new Rectangle(49, 2024, 13, 13);

            _maskRectangle = new Rectangle[3];
            _maskRectangle[0] = new Rectangle(23, 2038, 49, 10);
            _maskRectangle[1] = new Rectangle(72, 2038, 57, 10);
            _maskRectangle[2] = new Rectangle(129, 2038, 51, 10);

            _numberRectangle = new Rectangle[12];
            for (int i = 0; i < _numberRectangle.Length; i++)
            {
                _numberRectangle[i] = new Rectangle(74 + 11 * i, 2024, 11, 13);
            }
            _numberRectangle[10].Width = 5;
            _numberRectangle[11].X = _numberRectangle[10].X + _numberRectangle[10].Width;

            // transition
            _transitionRectangle = new Rectangle[4];
            _transitionRectangle[0] = new Rectangle(152, 1918, 1, 1); // 1x1
            _transitionRectangle[2] = new Rectangle(150, 1915, 50, 50); // circle half full
            _transitionRectangle[1] = new Rectangle(150, 1864, 50, 50); // circle empty            
            _transitionRectangle[3] = new Rectangle(201, 1864, 328, 184); // half full

            // inter level
            _interLevelRectangle = new Rectangle[_elevatorFrameCount + _cableFrameCount + 3];
            // elevator
            for (int i = 0; i < _elevatorFrameCount; i++)
            {
                _interLevelRectangle[i] = new Rectangle(613 + 32 * i, 2003, 32, 45);
            }
            // cable
            for (int i = 0; i < _cableFrameCount; i++)
            {
                _interLevelRectangle[_elevatorFrameCount + i] = new Rectangle(530 + 2 * i, 1888, 1, 160);
            }
            // background
            _interLevelRectangle[_elevatorFrameCount + _cableFrameCount] = new Rectangle(548, 1808, 64, 240);
            _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 1] = new Rectangle(483, 1844, 64, 10);
            _interLevelRectangle[_elevatorFrameCount + _cableFrameCount + 2] = new Rectangle(483, 1855, 64, 8);
        }

        public void LoadBuilding()
        {
            _building = new Building();
            _building.Load();

            LoadLevel(0);
        }

        public void LoadLevel(int level)
        {
            SelectedLevel = level;

            _building.LoadLevel(SelectedLevel);

            // init player
            Player = new Player();
            Player.Room = null;

            // look for start
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

            _entranceState = ElevatorState.Closed;
            _exitState = ElevatorState.Closed;

            _transitionState = TransitionState.Hidden;

            AppearTransition();

            // clean memory
            GC.Collect();
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

        public void Update(float gameTime)
        {
            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.HandleIngameInputs(this);

            // transition
            UpdateTransition(gameTime);
            
            if (_building.CurrentLevel != null && Player != null && SelectedLevel >= 0 && SelectedLevel < _building.LevelCount)
            {

                Player.Update(gameTime);

                bool isShouting = Player.IsShouting;
                bool isTaunting = Player.IsTaunting;
                int playerMiddle = (int)Player.X + 16;

                // elevators
                UpdateElevators(gameTime);                                

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
                        if (room.HasExit && _exitState == ElevatorState.Open)
                        {
                            if (Player.X + 16 > room.ExitX + 11 && Player.X + 16 < room.ExitX + 27)
                                Player.CanEnterElevator = true;
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
                                                        && roomDoor.X + 16 > nurse.X && retiredPos > roomDoor.X + 16)
                                                        canSeeRetired = false;
                                                    else if (retiredPos < nurse.X + 16
                                                        && nurse.Orientation == SpriteEffects.FlipHorizontally
                                                        && roomDoor.IsClosed
                                                        && roomDoor.X + 16 < nurse.X && retiredPos < roomDoor.X + 16)
                                                        canSeeRetired = false;
                                                    else if (retiredPos < nurse.X + 16
                                                        && nurse.Orientation == SpriteEffects.None)
                                                        canSeeRetired = false;
                                                    else if (retiredPos > nurse.X + 16
                                                        && nurse.Orientation == SpriteEffects.FlipHorizontally)
                                                        canSeeRetired = false;
                                                }

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
                    }
                }

                // elevator
                if (_canEscape && _exitState == ElevatorState.Closed)
                    _exitState = ElevatorState.Opening;
                else if (Player.HasEnteredElevator && _exitState == ElevatorState.Open)
                    _exitState = ElevatorState.Closing;     
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
                        OpenElevator();
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
                            // end level, load next
                            SelectedLevel++;
                            SelectedLevel %= _building.LevelCount;
                            // init elevator position
                            _elevatorX = (int)Player.X;
                            _elevatorY = 69;// 128 - 40 * Player.Room.Y + 4;

                            _building.CurrentLevel = null;
                            Player = null;
                            // save score
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
                            _currentEntranceFrame = 4;
                            _entranceState = ElevatorState.Closing;
                        }
                        else if (_currentEntranceFrame == 2)
                        {
                            // pop player
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

        private void OpenElevator()
        {
            _currentEntranceFrame = 0;
            _currentEntranceFrameTime = 0.0f;
            _entranceState = ElevatorState.Opening;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Building;

            if (_building.CurrentLevel != null && SelectedLevel >= 0 && SelectedLevel < _building.LevelCount)
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
                                new Rectangle(startX + 8 * s, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // left wall
                        source = _roomRectangle[room.BackgroundColor][room.LeftDecoration][0];

                        spriteBatch.Draw(texture,
                            new Rectangle(room.LeftMargin * 8, 128 - 40 * f, source.Width, source.Height),
                            source,
                            Color.White);

                        // right wall
                        source = _roomRectangle[room.BackgroundColor][room.RightDecoration][2];

                        spriteBatch.Draw(texture,
                            new Rectangle(304 - room.RightMargin * 8, 128 - 40 * f, source.Width, source.Height),
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
                            if (isShouting && floor.Y == Player.Room.Y && detail.Type >= Detail.NormalDetailCount && Math.Abs(detail.X + 16 - playerMiddle) <= _shoutDistance)
                                frame = _currentShoutFrame;

                            Rectangle source = _detailRectangle[room.BackgroundColor][detail.Type][frame];

                            spriteBatch.Draw(texture,
                                new Rectangle(detail.X, 128 - 40 * f, source.Width, source.Height),
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
                                new Rectangle(floorDoor.X, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            if (floorDoor.CanOpen)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(floorDoor.X + 8, 128 - 40 * f + 8, source.Width, source.Height),
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
                                new Rectangle(roomDoor.X, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            if (roomDoor.CanClose && roomDoor.IsOpenLeft)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(roomDoor.X, 128 - 40 * f + 8, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                            else if (roomDoor.CanClose && roomDoor.IsOpenRight)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(roomDoor.X + 16, 128 - 40 * f + 8, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                        }

                        // furniture
                        for (int ff = 0; ff < room.Furnitures.Length; ff++)
                        {
                            Furniture furniture = room.Furnitures[ff];

                            int frame = furniture.Frame;
                            if (isShouting && floor.Y == Player.Room.Y && Math.Abs(furniture.X + 16 - playerMiddle) <= _shoutDistance)
                                frame = shoutingFrame;
                            
                            Rectangle source = _furnitureRectangle[room.BackgroundColor][furniture.Type][frame];

                            spriteBatch.Draw(texture,
                                new Rectangle(furniture.X, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            if (furniture.CanHide)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(furniture.X + 8, 128 - 40 * f + 8, source.Width, source.Height),
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
                                new Rectangle(room.StartX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            // player
                            if (_entranceState == ElevatorState.Opening)
                                Player.Draw(spriteBatch);

                            // door
                            source = _elevatorRectangle[1][_currentEntranceFrame];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            // shadow
                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        if (room.HasExit)
                        {
                            // background
                            Rectangle source = _elevatorRectangle[0][_currentExitFrame];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            // player
                            if (Player.HasEnteredElevator)
                                Player.Draw(spriteBatch);

                            // door
                            source = _elevatorRectangle[1][_currentExitFrame];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            // shadow
                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            // arrow
                            if (Player.CanEnterElevator)
                            {
                                source = _objectsRectangle[(int)ObjectType.Arrow][_currentArrowFrame];

                                spriteBatch.Draw(texture,
                                    new Rectangle(room.ExitX + 12, 128 - 40 * f + 8, source.Width, source.Height),
                                    source,
                                    Color.White);
                            }
                        }

                        // retired
                        for (int rr = 0; rr < room.Retireds.Count; rr++)
                        {
                            Retired retired = room.Retireds[rr];
                            if (retired != null)
                                retired.Draw(spriteBatch);
                        }

                        // nurse
                        for (int n = 0; n < room.Nurses.Count; n++)
                        {
                            Nurse nurse = room.Nurses[n];
                            if (nurse != null)
                                nurse.Draw(spriteBatch);
                        }
                    }
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
            }
            

            if (_entranceState != ElevatorState.Opening && Player != null && !Player.HasEnteredElevator)
                Player.Draw(spriteBatch);

            // transition
            if (_transitionState != TransitionState.None)
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

            // up
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, GreedyKidGame.Width, _uiRectangle[4].Height),
                _uiRectangle[4],
                Color.White);

            // down
            spriteBatch.Draw(texture,
                new Rectangle(0, GreedyKidGame.Height - _uiRectangle[5].Height, GreedyKidGame.Width, _uiRectangle[5].Height),
                _uiRectangle[5],
                Color.White);

            // left
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, _uiRectangle[6].Width, GreedyKidGame.Height),
                _uiRectangle[6],
                Color.White);

            // right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _uiRectangle[7].Width, 0, _uiRectangle[6].Width, GreedyKidGame.Height),
                _uiRectangle[7],
                Color.White);

            // upper left
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, _uiRectangle[0].Width, _uiRectangle[0].Height),
                _uiRectangle[0],
                Color.White);

            // upper right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _uiRectangle[1].Width, 0, _uiRectangle[1].Width, _uiRectangle[1].Height),
                _uiRectangle[1],
                Color.White);

            // lower left
            spriteBatch.Draw(texture,
                new Rectangle(0, GreedyKidGame.Height - _uiRectangle[2].Height, _uiRectangle[2].Width, _uiRectangle[2].Height),
                _uiRectangle[2],
                Color.White);

            // lower right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _uiRectangle[3].Width, GreedyKidGame.Height - _uiRectangle[3].Height, _uiRectangle[3].Width, _uiRectangle[3].Height),
                _uiRectangle[3],
                Color.White);

            // masks
            spriteBatch.Draw(texture,
                new Rectangle(19, 0, _maskRectangle[0].Width, _maskRectangle[0].Height),
                _maskRectangle[0],
                Color.White);
            spriteBatch.Draw(texture,
                new Rectangle(136, 0, _maskRectangle[1].Width, _maskRectangle[1].Height),
                _maskRectangle[1],
                Color.White);
            spriteBatch.Draw(texture,
                new Rectangle(257, 0, _maskRectangle[2].Width, _maskRectangle[2].Height),
                _maskRectangle[2],
                Color.White);

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

            // time
            _encodedTime[0] = Time / 600;
            _encodedTime[1] = (Time - _encodedTime[0] * 600) / 60;
            int seconds = Time % 60;
            _encodedTime[3] = seconds / 10;
            _encodedTime[4] = seconds % 10;

            int textX = 0;
            for (int t = 0; t < _encodedTime.Length; t++)
            {
                Rectangle source = _numberRectangle[_encodedTime[t]];
                spriteBatch.Draw(texture,
                    new Rectangle(140 + textX, 0, source.Width, source.Height),
                    source,
                    Color.White);

                textX += source.Width;
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

            spriteBatch.End();
        }
    }
}

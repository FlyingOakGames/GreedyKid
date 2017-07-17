using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace GreedyKidEditor
{
    public enum SelectionMode
    {
        Room,
        Detail,        
        RoomDoor,
        FloorDoor,
        Furniture,
        Elevator,
        Retired,
        Nurse,
        Cop,

        Selection,

        Count
    }

    class PreviewRenderer : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        IntPtr _handle;
        MainWindow _w;

        System.Windows.Forms.Form _form;

        public static int Width = 328; // 312
        public static int Height = 184; // 176

        Texture2D _levelTexture;
        
        Rectangle[][][] _roomRectangle;
        Rectangle[][] _detailRectangle;
        Rectangle[][][] _floorDoorRectangle;
        Rectangle[][] _roomDoorRectangle;
        Rectangle[][] _elevatorRectangle;
        Rectangle[][][] _furnitureRectangle;
        Rectangle[][] _retiredRectangle;
        Rectangle[][] _nurseRectangle;
        Rectangle[][] _copRectangle;

        private Rectangle[] _uiRectangle;
        private Rectangle[] _iconRectangle;
        private Rectangle[] _maskRectangle;
        private Rectangle[] _numberRectangle;
        private Rectangle[] _smallNumberRectangle;
        private Rectangle[] _bubbleRectangle;

        private Color _lifeColor = new Color(217, 87, 99);
        private Color _moneyColor = new Color(153, 229, 80);

        private Rectangle[] _editorIcons;
        private Rectangle[] _editorHelp;

        public int Score = 0;
        private int[] _encodedScore = new int[] { 0, 0, 0 };
        public int Time = 0;
        private int[] _encodedTime = new int[] { 0, 0, 10, 0, 0 };
        public int PlayerLife = 3;

        Color _fillColor = new Color(34, 32, 52);

        private static Building _building;

        public static int SelectedLevel = -1;

        int[] _floorDoorSequence = new int[] { 
            3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 
            0, 1, 2, 3, 4, 5 
        };
        int[] _roomDoorSequence = new int[] {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 1, 2, 3, 4,
            4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
            4, 5, 6, 7, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            9, 10, 11, 12, 13,
            13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
            13, 14, 15, 16, 17
        };
        int[] _elevatorSequence = new int[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 1, 2, 3, 4,
            4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
            4, 3, 2, 1, 0,
        };
        int _currentFloorDoorFrame = 0;
        int _currentRoomDoorFrame = 0;
        int _currentElevatorFrame = 0;
        float _currentFrameTime = 0.0f;

        int[] _retiredSequence = new int[]
        {
            /*
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 0, 1, 2,*/
            0, 1, 2, 3, 4, 5, 6, 7,
        };
        int _currentRetiredFrame = 0;

        int[] _nurseSequence = new int[]
        {
            /*
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 0, 1, 2,*/
            0, 1, 2, 3, 4, 5, 6, 7,
        };
        int _currentNurseFrame = 0;

        int[] _copSequence = new int[]
        {
            /*
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 0, 1, 2,*/
            0, 1, 2, 3, 4, 5, 6, 7,
        };
        int _currentCopFrame = 0;

        public static bool PreviewAnimation = false;

        // mouse and keyboard states
        private MouseState _mouseState = new MouseState();
        private MouseState _prevMouseState = new MouseState();
        public Point MousePosition = new Point();
        public int MouseWheelDelta = 0;
        public SelectionMode SelectionMode = SelectionMode.Room;
        private bool _hasRightClick = false;
        private bool _hasLeftClick = false;
        private bool _hasWheelUp = false;
        private bool _hasWheelDown = false;

        public bool SpaceState = false;
        private bool _prevSpaceState = false;
        private bool _hasSpaceDown = false;

        public bool CState = false;
        private bool _prevCState = false;
        private bool _hasCDown = false;

        public bool VState = false;
        private bool _prevVState = false;
        private bool _hasVDown = false;

        public bool BState = false;
        private bool _prevBState = false;
        private bool _hasBDown = false;

        public bool NState = false;
        private bool _prevNState = false;
        private bool _hasNDown = false;

        // dragging
        private IMovable _lockedObject;
        private Room _lockedRoom;
        private bool _lockedLeft = false;        

        private Color _selectionColor = Color.White * 0.5f;
        private Color _selectionColorRed = Color.Red * 0.5f;

        private float EaseOutExpo(float t, float b, float c, float d)
        {
            return (t == d) ? b + c : c * (-(float)Math.Pow(2, -10 * t / d) + 1) + b;
        }

        private float _cameraPositionY = 0.0f;        

        // tween
        private float _initialCameraPositionY = 0.0f;
        private float _differenceCameraPositionY = 0.0f;        
        private const float _totalCameraTime = 1.0f;
        private float _currentCameraTime = _totalCameraTime;

        private int _currentCameraFloor = 0;

        public int RoomToRemove = -1;
        public int RoomToRemoveFloor = -1;

        private Rectangle _1x1 = new Rectangle(33, 1911, 1, 1);

        public void ResetCamera()
        {
            MoveCameraToFloor(0);
        }

        private void MoveCameraToFloor(int f)
        {
            if (f < 0)
                f = 0;
            _currentCameraFloor = f;
            MoveCamera(_currentCameraFloor * 40.0f);
        }

        private void MoveCamera(float targetPosition)
        {
            _initialCameraPositionY = _cameraPositionY;
            _differenceCameraPositionY = targetPosition - _cameraPositionY;
            _currentCameraTime = 0.0f;
        }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {
            args.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = _handle;
        }

        public PreviewRenderer(IntPtr handle, MainWindow w)
        {
            _w = w;
            _handle = handle;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += OnPreparingDeviceSettings;
            Content.RootDirectory = "Content";
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 33);

            graphics.PreferredBackBufferHeight = Width;
            graphics.PreferredBackBufferWidth = Height;
            IsMouseVisible = true;

            _selectionColor.A = 255;
            _selectionColorRed.A = 255;
        }

        public static Building Building
        {
            set { _building = value; }
        }

        protected override void Initialize()
        {
            // force hide the hosted window
            _form = System.Windows.Forms.Control.FromHandle(Window.Handle) as System.Windows.Forms.Form;
            _form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            _form.ShowInTaskbar = false;
            _form.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            _form.Location = new System.Drawing.Point(-4000, -4000);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

#if DEVMODE
            if (File.Exists("Content\\Textures\\level.png")) // devmode only
            {
                using (System.IO.FileStream file = System.IO.File.OpenRead("Content\\Textures\\level.png"))
                {
                    _levelTexture = Texture2D.FromStream(this.GraphicsDevice, file);
                }
            }
            else
            {
                _levelTexture = Content.Load<Texture2D>("Textures\\level");
            }
#else
            _levelTexture = Content.Load<Texture2D>("Textures\\level");
#endif


            _roomRectangle = new Rectangle[Room.PaintCount][][]; // colors
            _detailRectangle = new Rectangle[Room.PaintCount][];
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

                _detailRectangle[p] = new Rectangle[Detail.NormalDetailCount + Detail.AnimatedDetailCount];
                for (int d = 0; d < Detail.NormalDetailCount + Detail.AnimatedDetailCount; d++)
                {
                    if (d < Detail.NormalDetailCount)
                        _detailRectangle[p][d] = new Rectangle(56 * Room.DecorationCount + d * 32, 48 * p, 32, 48);
                    else
                        _detailRectangle[p][d] = new Rectangle(56 * Room.DecorationCount + Detail.NormalDetailCount * 32 + (d - Detail.NormalDetailCount) * 32 * Detail.AnimatedDetailFrames, 48 * p, 32, 48);
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

            _elevatorRectangle = new Rectangle[4][];
            _elevatorRectangle[0] = new Rectangle[Room.ElevatorFrames];
            _elevatorRectangle[1] = new Rectangle[Room.ElevatorFrames];
            _elevatorRectangle[2] = new Rectangle[Room.PaintCount];
            _elevatorRectangle[3] = new Rectangle[2];

            for (int f = 0; f < Room.ElevatorFrames; f++)
            {
                _elevatorRectangle[0][f] = new Rectangle(f * 40, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);
                _elevatorRectangle[1][f] = new Rectangle(f * 40 + 40 * Room.ElevatorFrames, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);
            }
            for (int p = 0; p < Room.PaintCount; p++)
            {
                _elevatorRectangle[2][p] = new Rectangle(2 * 40 * Room.ElevatorFrames + p * 40, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);
            }
            _elevatorRectangle[3][0] = new Rectangle(2 * 40 * Room.ElevatorFrames + Room.PaintCount * 40, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);
            _elevatorRectangle[3][1] = new Rectangle(2 * 40 * Room.ElevatorFrames + Room.PaintCount * 40 + 40, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine, 40, 48);

            int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

            _retiredRectangle = new Rectangle[Retired.RetiredCount][];
            for (int t = 0; t < Retired.RetiredCount; t++)
            {
                _retiredRectangle[t] = new Rectangle[8];
                for (int f = 0; f < 8; f++) // idle animation
                {
                    _retiredRectangle[t][f] = new Rectangle(38 * 32 + f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                }
            }

            _nurseRectangle = new Rectangle[Nurse.NurseCount][];
            for (int t = 0; t < Nurse.NurseCount; t++)
            {
                _nurseRectangle[t] = new Rectangle[8];
                for (int f = 0; f < 8; f++) // idle animation
                {
                    _nurseRectangle[t][f] = new Rectangle(38 * 32 + f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + 32 * Retired.RetiredCount + t * 32, 32, 32);
                }
            }

            _copRectangle = new Rectangle[Cop.CopCount][];
            for (int t = 0; t < Cop.CopCount; t++)
            {
                _copRectangle[t] = new Rectangle[8];
                for (int f = 0; f < 8; f++) // idle animation
                {
                    _copRectangle[t][f] = new Rectangle(35 * 32 + f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + 32 * Retired.RetiredCount + 32 * Nurse.NurseCount + t * 32, 32, 32);
                }
            }

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

            _maskRectangle = new Rectangle[5];
            _maskRectangle[0] = new Rectangle(23, 2038, 49, 10);
            _maskRectangle[1] = new Rectangle(72, 2038, 57, 10);
            _maskRectangle[2] = new Rectangle(129, 2038, 51, 10);
            _maskRectangle[3] = new Rectangle(152, 1918, 1, 1); // 1x1
            _maskRectangle[4] = new Rectangle(201, 1864, 328, 2);

            _numberRectangle = new Rectangle[12];
            for (int i = 0; i < _numberRectangle.Length; i++)
            {
                _numberRectangle[i] = new Rectangle(74 + 11 * i, 2024, 11, 13);
            }
            _numberRectangle[10].Width = 5;
            _numberRectangle[11].X = _numberRectangle[10].X + _numberRectangle[10].Width;

            _smallNumberRectangle = new Rectangle[10];
            for (int i = 0; i < _smallNumberRectangle.Length; i++)
            {
                _smallNumberRectangle[i] = new Rectangle(0 + 4 * i, 1935, 3, 5);
            }

            _bubbleRectangle = new Rectangle[3];
            for (int i = 0; i < _bubbleRectangle.Length; i++)
            {
                _bubbleRectangle[i] = new Rectangle(0, 1941 + i * 10, 29, 10);
            }

            _editorIcons = new Rectangle[(int)SelectionMode.Count + 6];
            _editorHelp = new Rectangle[(int)SelectionMode.Count];
            for (int i = 0; i < (int)SelectionMode.Count; i++)
            {
                _editorIcons[i] = new Rectangle(0 + i * 14, 1905, 14, 14);
                _editorHelp[i] = new Rectangle(613, 1808 + i * 13, 284, 12);
            }

            for (int i = 0; i < 6; i++)
            {
                _editorIcons[(int)SelectionMode.Count + i] = new Rectangle(0 + i * 14, 1920, 14, 14);
            }
        }

        protected override void UnloadContent()
        {            
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _form.Location = new System.Drawing.Point(-4000, -4000);
            if (Width != graphics.PreferredBackBufferWidth || Height != graphics.PreferredBackBufferHeight)
            {
                graphics.PreferredBackBufferHeight = Height;
                graphics.PreferredBackBufferWidth = Width;
                graphics.ApplyChanges();                
            }

            _mouseState = Mouse.GetState();
            _mouseState = new MouseState(
                MousePosition.X,
                MousePosition.Y,
                MouseWheelDelta,
                _mouseState.LeftButton,
                _mouseState.MiddleButton,
                _mouseState.RightButton,
                _mouseState.XButton1,
                _mouseState.XButton2);

            MouseWheelDelta = 0;

            // mouse update
            _hasRightClick = false;
            _hasLeftClick = false;
            _hasWheelUp = false;
            _hasWheelDown = false;
            _hasSpaceDown = false;
            _hasCDown = false;
            _hasVDown = false;
            _hasBDown = false;
            _hasNDown = false;

            if (_prevMouseState.ScrollWheelValue == 0 && _mouseState.ScrollWheelValue < _prevMouseState.ScrollWheelValue)
                _hasWheelDown = true;
            else if (_prevMouseState.ScrollWheelValue == 0 && _mouseState.ScrollWheelValue > _prevMouseState.ScrollWheelValue)
                _hasWheelUp = true;

            if (_prevMouseState.LeftButton == ButtonState.Released && _mouseState.LeftButton == ButtonState.Pressed)
                _hasLeftClick = true;

            if (_prevMouseState.RightButton == ButtonState.Released && _mouseState.RightButton == ButtonState.Pressed)
                _hasRightClick = true;

            if (!_prevSpaceState && SpaceState)
                _hasSpaceDown = true;
            else if (!SpaceState)
            {
                _lockedObject = null;               
                _lockedRoom = null;
            }

            if (!_prevCState && CState)
                _hasCDown = true;
            if (!_prevVState && VState)
                _hasVDown = true;
            if (!_prevBState && BState)
                _hasBDown = true;
            if (!_prevNState && NState)
                _hasNDown = true;

            _prevMouseState = _mouseState;
            _prevSpaceState = SpaceState;
            _prevCState = CState;
            _prevVState = VState;
            _prevBState = BState;
            _prevNState = NState;

            if (_lockedObject != null)
                _lockedObject.Move(_mouseState.Position.X - 16);

            if (_lockedRoom != null)
            {
                if (_lockedLeft)
                {
                    _lockedRoom.LeftMargin = (_mouseState.Position.X - 12) / 8;
                    _lockedRoom.LeftMargin = Math.Max(0, _lockedRoom.LeftMargin);
                }
                else
                {
                    _lockedRoom.RightMargin = 37 - (_mouseState.Position.X - 12) / 8;
                    _lockedRoom.RightMargin = Math.Max(0, _lockedRoom.RightMargin);
                }
            }

            float elaspedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _currentFrameTime += elaspedSeconds;
            if (_currentFrameTime > 0.1f)
            {
                _currentFrameTime -= 0.1f;
                _currentFloorDoorFrame++;
                if (_currentFloorDoorFrame >= _floorDoorSequence.Length)
                    _currentFloorDoorFrame = 0;
                _currentRoomDoorFrame++;
                if (_currentRoomDoorFrame >= _roomDoorSequence.Length)
                    _currentRoomDoorFrame = 0;
                _currentElevatorFrame++;
                if (_currentElevatorFrame >= _elevatorSequence.Length)
                    _currentElevatorFrame = 0;

                _currentRetiredFrame++;
                if (_currentRetiredFrame >= _retiredSequence.Length)
                {
                    _currentRetiredFrame = 0;
                }

                _currentNurseFrame++;
                if (_currentNurseFrame >= _nurseSequence.Length)
                {
                    _currentNurseFrame = 0;
                }

                _currentCopFrame++;
                if (_currentCopFrame >= _copSequence.Length)
                {
                    _currentCopFrame = 0;
                }
            }

            if (!PreviewAnimation)
            {
                _currentFloorDoorFrame = 0;
                _currentRoomDoorFrame = 0;
                _currentElevatorFrame = 0;
            }

            // camera
            if (_currentCameraTime < _totalCameraTime)
            {
                _currentCameraTime += elaspedSeconds;
                _cameraPositionY = EaseOutExpo(_currentCameraTime, _initialCameraPositionY, _differenceCameraPositionY, _totalCameraTime);

                if (_currentCameraTime >= _totalCameraTime)
                    _cameraPositionY = _initialCameraPositionY + _differenceCameraPositionY;
            }

            base.Update(gameTime);
        }

        private void RemoveStart()
        {
            for (int f = 0; f < _building.Levels[SelectedLevel].Floors.Count; f++)
            {
                Floor floor = _building.Levels[SelectedLevel].Floors[f];

                // rooms
                for (int r = 0; r < floor.Rooms.Count; r++)
                {
                    Room room = floor.Rooms[r];

                    room.HasStart = false;
                }
            }
        }

        private void RemoveExit()
        {
            for (int f = 0; f < _building.Levels[SelectedLevel].Floors.Count; f++)
            {
                Floor floor = _building.Levels[SelectedLevel].Floors[f];

                // rooms
                for (int r = 0; r < floor.Rooms.Count; r++)
                {
                    Room room = floor.Rooms[r];

                    room.HasExit = false;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_fillColor);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            int cameraPosY = (int)Math.Round(_cameraPositionY);
            Rectangle source;
            Rectangle destination;
            bool isHoverAnyRoom = false;

            Score = 0;

            if (_building != null && SelectedLevel >= 0 && SelectedLevel < _building.Levels.Count)
            {
                bool drawHelper = false;
                int helperX = 0;

                // target time
                Rectangle timeRectangle = new Rectangle(138, 0, 50, 12);

                if (IsHover(timeRectangle, true) && _hasWheelUp)
                    _building.Levels[SelectedLevel].TargetTime++;
                else if (IsHover(timeRectangle, true) && _hasLeftClick)
                    _building.Levels[SelectedLevel].TargetTime += 5;
                else if (IsHover(timeRectangle, true) && _hasWheelDown)
                {
                    _building.Levels[SelectedLevel].TargetTime--;
                }
                else if (IsHover(timeRectangle, true) && _hasRightClick)
                {
                    _building.Levels[SelectedLevel].TargetTime -= 5;                    
                }

                if (_building.Levels[SelectedLevel].TargetTime < 1)
                    _building.Levels[SelectedLevel].TargetTime = 1;
                if (_building.Levels[SelectedLevel].TargetTime > 99 * 60 + 59)
                    _building.Levels[SelectedLevel].TargetTime = 99 * 60 + 59;

                for (int f = 0; f < _building.Levels[SelectedLevel].Floors.Count; f++)
                {
                    Floor floor = _building.Levels[SelectedLevel].Floors[f];

                    // rooms                    
                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];
                        // background
                        int startX = 16 + room.LeftMargin * 8;
                        int nbSlice = 37 - room.LeftMargin - room.RightMargin;

                        source = _roomRectangle[room.BackgroundColor][0][1];

                        bool isAnySliceHovered = false;

                        for (int s = 0; s < nbSlice; s++)
                        {
                            destination = new Rectangle(startX + 8 * s, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            if (s > 0 && s < nbSlice - 1 && IsHover(destination))
                                isAnySliceHovered = true;                            
                        }

                        // update
                        if (isAnySliceHovered && SelectionMode == SelectionMode.Room && _hasWheelUp)
                        {
                            room.BackgroundColor++;
                            room.BackgroundColor = Math.Min(room.BackgroundColor, Room.PaintCount - 1);
                        }
                        else if (isAnySliceHovered && SelectionMode == SelectionMode.Room && _hasWheelDown)
                        {
                            room.BackgroundColor--;
                            room.BackgroundColor = Math.Max(room.BackgroundColor, 0);
                        }

                        for (int s = 0; s < nbSlice; s++)
                        {
                            destination = new Rectangle(startX + 8 * s, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                (isAnySliceHovered && s > 0 && s < nbSlice - 1 && SelectionMode == SelectionMode.Room ? _selectionColor : Color.White));
                        }

                        // left wall
                        source = _roomRectangle[room.BackgroundColor][room.LeftDecoration][0];
                        destination = new Rectangle(room.LeftMargin * 8, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                        spriteBatch.Draw(_levelTexture,
                            destination,
                            source,
                            (IsHover(destination) && SelectionMode == SelectionMode.Room ? _selectionColor : Color.White));

                        if (IsHover(destination) && SelectionMode == SelectionMode.Room && _hasSpaceDown)
                        {
                            _lockedRoom = room;
                            _lockedLeft = true;
                        }

                        // update
                        if (IsHover(destination) && SelectionMode == SelectionMode.Room && _hasWheelUp)
                        {
                            room.LeftDecoration++;
                            room.LeftDecoration = Math.Min(room.LeftDecoration, Room.DecorationCount - 1);
                        }
                        else if (IsHover(destination) && SelectionMode == SelectionMode.Room && _hasWheelDown)
                        {
                            room.LeftDecoration--;
                            room.LeftDecoration = Math.Max(room.LeftDecoration, 0);
                        }                        

                        // right wall
                        source = _roomRectangle[room.BackgroundColor][room.RightDecoration][2];
                        destination = new Rectangle(304 - room.RightMargin * 8, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                        spriteBatch.Draw(_levelTexture,
                            destination,
                            source,
                            (IsHover(destination) && SelectionMode == SelectionMode.Room ? _selectionColor : Color.White));

                        if (IsHover(destination) && SelectionMode == SelectionMode.Room && _hasSpaceDown)
                        {
                            _lockedRoom = room;
                            _lockedLeft = false;
                        }

                        // update
                        if (IsHover(destination) && SelectionMode == SelectionMode.Room && _hasWheelUp)
                        {
                            room.RightDecoration++;
                            room.RightDecoration = Math.Min(room.RightDecoration, Room.DecorationCount - 1);
                        }
                        else if (IsHover(destination) && SelectionMode == SelectionMode.Room && _hasWheelDown)
                        {
                            room.RightDecoration--;
                            room.RightDecoration = Math.Max(room.RightDecoration, 0);
                        }

                        // remove
                        if (IsHover(room, f, cameraPosY) && SelectionMode == SelectionMode.Room)
                        {
                            isHoverAnyRoom = true;
                            if (_hasRightClick)
                            {
                                RoomToRemoveFloor = f;
                                RoomToRemove = r;
                            }
                        }
                    }

                    // rooms details
                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        int remove = -1;
                        int swap1 = -1;
                        int swap2 = -1;

                        for (int d = 0; d < room.Details.Count; d++)
                        {
                            Detail detail = room.Details[d];
                            source = _detailRectangle[room.BackgroundColor][detail.Type];
                            destination = new Rectangle(detail.X, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            Rectangle selectionDestination = destination;
                            selectionDestination.X = selectionDestination.X + 8;
                            selectionDestination.Width = selectionDestination.Width - 16;

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                (IsHover(selectionDestination) && SelectionMode == SelectionMode.Detail ? _selectionColor : Color.White));

                            // update
                            if (IsHover(selectionDestination) && SelectionMode == SelectionMode.Detail && _hasWheelUp)
                            {
                                detail.Type++;
                                detail.Type = Math.Min(detail.Type, Detail.NormalDetailCount + Detail.AnimatedDetailCount - 1);
                            }
                            else if (IsHover(selectionDestination) && SelectionMode == SelectionMode.Detail && _hasWheelDown)
                            {
                                detail.Type--;
                                detail.Type = Math.Max(detail.Type, 0);
                            }

                            // remove
                            if (IsHover(selectionDestination) && SelectionMode == SelectionMode.Detail && _hasRightClick)
                            {
                                remove = d;
                            }

                            // selection
                            if (IsHover(selectionDestination) && SelectionMode == SelectionMode.Detail && _hasSpaceDown)
                            {
                                _lockedObject = detail;
                            }

                            // swap 
                            if (IsHover(selectionDestination) && SelectionMode == SelectionMode.Detail && _hasCDown)
                            {
                                if (d < room.Details.Count - 1)
                                {
                                    swap1 = d;
                                    swap2 = d + 1;
                                }
                            }
                            else if (IsHover(selectionDestination) && SelectionMode == SelectionMode.Detail && _hasVDown)
                            {
                                if (d > 0)
                                {
                                    swap1 = d;
                                    swap2 = d - 1;
                                }
                            }
                        }

                        // add
                        if (SelectionMode == SelectionMode.Detail && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            room.Details.Add(new Detail(_mouseState.Position.X - 16));
                        }
                        else if (remove >= 0)
                        {
                            room.Details.RemoveAt(remove);
                        }

                        // swap
                        if (swap1 >= 0 && swap2 >= 0)
                        {
                            Detail d = room.Details[swap1];
                            room.Details[swap1] = room.Details[swap2];
                            room.Details[swap2] = d;
                        }
                    }

                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        int remove = -1;
                        int swap1 = -1;
                        int swap2 = -1;

                        // floor doors
                        for (int d = 0; d < room.FloorDoors.Count; d++)
                        {
                            FloorDoor floorDoor = room.FloorDoors[d];
                            source = _floorDoorRectangle[room.BackgroundColor][floorDoor.Color][_floorDoorSequence[_currentFloorDoorFrame]];
                            destination = new Rectangle(floorDoor.X, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            Color color = (IsHover(destination) && SelectionMode == SelectionMode.FloorDoor ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, floorDoor))
                                color = (IsHover(destination) && SelectionMode == SelectionMode.FloorDoor ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            // update
                            if (IsHover(destination) && SelectionMode == SelectionMode.FloorDoor && _hasWheelUp)
                            {
                                floorDoor.Color++;
                                floorDoor.Color = Math.Min(floorDoor.Color, FloorDoor.DoorCount- 1);
                            }
                            else if (IsHover(destination) && SelectionMode == SelectionMode.FloorDoor && _hasWheelDown)
                            {
                                floorDoor.Color--;
                                floorDoor.Color = Math.Max(floorDoor.Color, 0);
                            }

                            if (IsHover(destination) && SelectionMode == SelectionMode.FloorDoor && floorDoor.Color == 0)
                            {
                                drawHelper = true;
                                helperX = destination.X + 6;
                            }

                            // remove
                            if (IsHover(destination) && SelectionMode == SelectionMode.FloorDoor && _hasRightClick)
                            {
                                remove = d;
                            }

                            // selection
                            if (IsHover(destination) && SelectionMode == SelectionMode.FloorDoor && _hasSpaceDown)
                            {
                                _lockedObject = floorDoor;
                            }                            
                        }

                        // add
                        if (SelectionMode == SelectionMode.FloorDoor && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            room.FloorDoors.Add(new FloorDoor(_mouseState.Position.X - 16));
                        }
                        else if (remove >= 0)
                        {
                            room.FloorDoors.RemoveAt(remove);
                        }

                        remove = -1;

                        // room doors
                        for (int d = 0; d < room.RoomDoors.Count; d++)
                        {
                            RoomDoor roomDoor = room.RoomDoors[d];
                            source = _roomDoorRectangle[room.BackgroundColor][_roomDoorSequence[_currentRoomDoorFrame]];
                            destination = new Rectangle(roomDoor.X, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            Color color = (IsHover(destination) && SelectionMode == SelectionMode.RoomDoor ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, roomDoor))
                                color = (IsHover(destination) && SelectionMode == SelectionMode.RoomDoor ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            // remove
                            if (IsHover(destination) && SelectionMode == SelectionMode.RoomDoor && _hasRightClick)
                            {
                                remove = d;
                            }

                            // selection
                            if (IsHover(destination) && SelectionMode == SelectionMode.RoomDoor && _hasSpaceDown)
                            {
                                _lockedObject = roomDoor;
                            }
                        }

                        // add
                        if (SelectionMode == SelectionMode.RoomDoor && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            room.RoomDoors.Add(new RoomDoor(_mouseState.Position.X - 16));
                        }
                        else if (remove >= 0)
                        {
                            room.RoomDoors.RemoveAt(remove);
                        }

                        remove = -1;

                        // furniture
                        for (int ff = 0; ff < room.Furnitures.Count; ff++)
                        {
                            Furniture furniture = room.Furnitures[ff];
                            source = _furnitureRectangle[room.BackgroundColor][furniture.Type][0];
                            destination = new Rectangle(furniture.X, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            Color color = (IsHover(destination) && SelectionMode == SelectionMode.Furniture ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, furniture))
                                color = (IsHover(destination) && SelectionMode == SelectionMode.Furniture ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            // update
                            if (IsHover(destination) && SelectionMode == SelectionMode.Furniture && _hasWheelUp)
                            {
                                furniture.Type++;
                                furniture.Type = Math.Min(furniture.Type, Furniture.FurnitureCount - 1);
                            }
                            else if (IsHover(destination) && SelectionMode == SelectionMode.Furniture && _hasWheelDown)
                            {
                                furniture.Type--;
                                furniture.Type = Math.Max(furniture.Type, 0);
                            }

                            // remove
                            if (IsHover(destination) && SelectionMode == SelectionMode.Furniture && _hasRightClick)
                            {
                                remove = ff;
                            }

                            // selection
                            if (IsHover(destination) && SelectionMode == SelectionMode.Furniture && _hasSpaceDown)
                            {
                                _lockedObject = furniture;
                            }

                            // swap 
                            if (IsHover(destination) && SelectionMode == SelectionMode.Furniture && _hasCDown)
                            {
                                if (ff < room.Furnitures.Count - 1)
                                {
                                    swap1 = ff;
                                    swap2 = ff + 1;
                                }
                            }
                            else if (IsHover(destination) && SelectionMode == SelectionMode.Furniture && _hasVDown)
                            {
                                if (ff > 0)
                                {
                                    swap1 = ff;
                                    swap2 = ff - 1;
                                }
                            }
                        }

                        // add
                        if (SelectionMode == SelectionMode.Furniture && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            room.Furnitures.Add(new Furniture(_mouseState.Position.X - 16));
                        }
                        else if (remove >= 0)
                        {
                            room.Furnitures.RemoveAt(remove);
                        }

                        // swap
                        if (swap1 >= 0 && swap2 >= 0)
                        {
                            Furniture d = room.Furnitures[swap1];
                            room.Furnitures[swap1] = room.Furnitures[swap2];
                            room.Furnitures[swap2] = d;
                        }

                        // elevator
                        if (room.HasStart)
                        {
                            source = _elevatorRectangle[0][_elevatorSequence[_currentElevatorFrame]];
                            destination = new Rectangle(room.StartX, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            Color color = (IsHover(destination) && SelectionMode == SelectionMode.Elevator ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, room.StartX))
                                color = (IsHover(destination) && SelectionMode == SelectionMode.Elevator ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            source = _elevatorRectangle[1][_elevatorSequence[_currentElevatorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            source = _elevatorRectangle[3][0];

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);
                        }

                        // add
                        if (SelectionMode == SelectionMode.Elevator && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            RemoveStart();
                            room.HasStart = true;
                            room.StartX = _mouseState.Position.X - 16;
                        }

                        if (room.HasExit)
                        {
                            source = _elevatorRectangle[0][_elevatorSequence[_currentElevatorFrame]];
                            destination = new Rectangle(room.ExitX, 128 - 40 * f + cameraPosY, source.Width, source.Height);

                            Color color = (IsHover(destination) && SelectionMode == SelectionMode.Elevator ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, room.ExitX))
                                color = (IsHover(destination) && SelectionMode == SelectionMode.Elevator ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            source = _elevatorRectangle[1][_elevatorSequence[_currentElevatorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            source = _elevatorRectangle[3][1];

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);
                        }

                        // add
                        if (SelectionMode == SelectionMode.Elevator && _hasRightClick && IsHover(room, f, cameraPosY))
                        {
                            RemoveExit();
                            room.HasExit = true;
                            room.ExitX = _mouseState.Position.X - 16;
                        }

                        remove = -1;                        

                        // retired
                        for (int rr = 0; rr < room.Retireds.Count; rr++)
                        {
                            Retired retired = room.Retireds[rr];

                            Score += retired.Money;

                            source = _retiredRectangle[retired.Type][_retiredSequence[_currentRetiredFrame]];
                            destination = new Rectangle((int)retired.X, 128 - 40 * f + 9 + cameraPosY, 32, 32);

                            Color color = (IsHover(destination, true) && SelectionMode == SelectionMode.Retired ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, retired))
                                color = (IsHover(destination, true) && SelectionMode == SelectionMode.Retired ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            // life
                            int bubble = (retired.Money > 9 ? 2 : 1);
                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)retired.X - 8, 128 - 40 * f + 9 + cameraPosY, _bubbleRectangle[0].Width, _bubbleRectangle[0].Height),
                                _bubbleRectangle[bubble],
                                Color.White);
                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)retired.X + 10, 128 - 40 * f + 10 + cameraPosY, _smallNumberRectangle[0].Width, _smallNumberRectangle[0].Height),
                                _smallNumberRectangle[retired.Life],
                                _lifeColor);
                            int digit = retired.Money % 10;
                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)retired.X - 2, 128 - 40 * f + 10 + cameraPosY, _smallNumberRectangle[0].Width, _smallNumberRectangle[0].Height),
                                _smallNumberRectangle[digit],
                                _moneyColor);
                            digit = retired.Money / 10;
                            if (digit > 0)
                            {
                                spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)retired.X - 6, 128 - 40 * f + 10 + cameraPosY, _smallNumberRectangle[0].Width, _smallNumberRectangle[0].Height),
                                _smallNumberRectangle[digit],
                                _moneyColor);
                            }

                            // update
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasWheelUp)
                            {
                                retired.Type++;
                                retired.Type = Math.Min(retired.Type, Retired.RetiredCount - 1);
                            }
                            else if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasWheelDown)
                            {
                                retired.Type--;
                                retired.Type = Math.Max(retired.Type, 0);
                            }
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasCDown)
                            {
                                retired.Life++;
                                retired.Life = Math.Min(retired.Life, 3);
                            }
                            else if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasVDown)
                            {
                                retired.Life--;
                                retired.Life = Math.Max(retired.Life, 1);
                            }
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasBDown)
                            {
                                retired.Money++;
                                retired.Money = Math.Min(retired.Money, Retired.MaxMoney);
                            }
                            else if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasNDown)
                            {
                                retired.Money--;
                                retired.Money = Math.Max(retired.Money, 0);
                            }

                            // remove
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasRightClick)
                            {
                                remove = rr;
                            }

                            // selection
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Retired && _hasSpaceDown)
                            {
                                _lockedObject = retired;
                            }
                        }

                        // add
                        if (SelectionMode == SelectionMode.Retired && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            if ((_building.Levels[SelectedLevel].GetRetiredCount() + 1) * Retired.MaxMoney <= 999)
                                room.Retireds.Add(new Retired(_mouseState.Position.X - 16));
                        }
                        else if (remove >= 0)
                        {
                            room.Retireds.RemoveAt(remove);
                        }

                        remove = -1;

                        // nurse
                        for (int n = 0; n < room.Nurses.Count; n++)
                        {
                            Nurse nurse = room.Nurses[n];

                            source = _nurseRectangle[nurse.Type][_nurseSequence[_currentNurseFrame]];
                            destination = new Rectangle((int)nurse.X, 128 - 40 * f + 9 + cameraPosY, 32, 32);

                            Color color = (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, nurse))
                                color = (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            // life
                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)nurse.X - 8, 128 - 40 * f + 9 + cameraPosY, _bubbleRectangle[0].Width, _bubbleRectangle[0].Height),
                                _bubbleRectangle[0],
                                Color.White);
                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)nurse.X + 10, 128 - 40 * f + 10 + cameraPosY, _smallNumberRectangle[0].Width, _smallNumberRectangle[0].Height),
                                _smallNumberRectangle[nurse.Life],
                                _lifeColor);

                            // update
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse && _hasWheelUp)
                            {
                                nurse.Type++;
                                nurse.Type = Math.Min(nurse.Type, Nurse.NurseCount - 1);
                            }
                            else if (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse && _hasWheelDown)
                            {
                                nurse.Type--;
                                nurse.Type = Math.Max(nurse.Type, 0);
                            }
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse && _hasCDown)
                            {
                                nurse.Life++;
                                nurse.Life = Math.Min(nurse.Life, 3);
                            }
                            else if (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse && _hasVDown)
                            {
                                nurse.Life--;
                                nurse.Life = Math.Max(nurse.Life, 1);
                            }

                            // remove
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse && _hasRightClick)
                            {
                                remove = n;
                            }

                            // selection
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Nurse && _hasSpaceDown)
                            {
                                _lockedObject = nurse;
                            }
                        }

                        // add
                        if (SelectionMode == SelectionMode.Nurse && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            room.Nurses.Add(new Nurse(_mouseState.Position.X - 16));
                        }
                        else if (remove >= 0)
                        {
                            room.Nurses.RemoveAt(remove);
                        }

                        remove = -1;

                        // cop
                        for (int c = 0; c < room.Cops.Count; c++)
                        {
                            Cop cop = room.Cops[c];

                            source = _copRectangle[cop.Type][_copSequence[_currentCopFrame]];
                            destination = new Rectangle((int)cop.X, 128 - 40 * f + 9 + cameraPosY, 32, 32);

                            Color color = (IsHover(destination, true) && SelectionMode == SelectionMode.Cop ? _selectionColor : Color.White);
                            if (IsOutsideRoom(room, cop))
                                color = (IsHover(destination, true) && SelectionMode == SelectionMode.Cop ? _selectionColorRed : Color.Red);

                            spriteBatch.Draw(_levelTexture,
                                destination,
                                source,
                                color);

                            // update
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Cop && _hasWheelUp)
                            {
                                cop.Type++;
                                cop.Type = Math.Min(cop.Type, Cop.CopCount - 1);
                            }
                            else if (IsHover(destination, true) && SelectionMode == SelectionMode.Cop && _hasWheelDown)
                            {
                                cop.Type--;
                                cop.Type = Math.Max(cop.Type, 0);
                            }

                            // remove
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Cop && _hasRightClick)
                            {
                                remove = c;
                            }

                            // selection
                            if (IsHover(destination, true) && SelectionMode == SelectionMode.Cop && _hasSpaceDown)
                            {
                                _lockedObject = cop;
                            }
                        }

                        // add
                        if (SelectionMode == SelectionMode.Cop && _hasLeftClick && IsHover(room, f, cameraPosY))
                        {
                            room.Cops.Add(new Cop(_mouseState.Position.X - 16));
                        }
                        else if (remove >= 0)
                        {
                            room.Cops.RemoveAt(remove);
                        }
                    }                  
                }

                if (drawHelper)
                {
                    spriteBatch.Draw(_levelTexture,
                        new Rectangle(helperX, 0, 1, Height),
                        _1x1,
                        Color.White * 0.5f);

                    spriteBatch.Draw(_levelTexture,
                        new Rectangle(helperX + 19, 0, 1, Height),
                        _1x1,
                        Color.White * 0.5f);
                }

                // room creation
                if (!isHoverAnyRoom && SelectionMode == SelectionMode.Room && _hasLeftClick)
                {
                    if (_mouseState.Position.X > 14 &&
                        _mouseState.Position.Y > 14 &&
                        _mouseState.Position.X < Width &&
                        _mouseState.Position.Y < Height)
                    {

                        destination = new Rectangle(0, 27, _editorIcons[(int)SelectionMode.Count].Width, _editorIcons[(int)SelectionMode.Count].Height);

                        if (!IsHover(destination))
                        {
                            destination = new Rectangle(0, 146, _editorIcons[(int)SelectionMode.Count].Width, _editorIcons[(int)SelectionMode.Count].Height);

                            if (!IsHover(destination))
                            {

                                destination = new Rectangle(314, 25, _editorIcons[0].Width, (int)SelectionMode.Count * _editorIcons[0].Height);

                                if (!IsHover(destination))
                                {
                                    // which floor?
                                    int floor = (160 - (_mouseState.Position.Y - 8)) / 40 + cameraPosY / 40;

                                    // adding missing floors
                                    for (int f = _building.Levels[SelectedLevel].Floors.Count - 1; f < floor + 1; f++)
                                        _building.Levels[SelectedLevel].Floors.Add(new Floor());

                                    Room room = new Room();
                                    room.LeftMargin = (_mouseState.Position.X - 24) / 8;
                                    room.LeftMargin = Math.Max(room.LeftMargin, 1);
                                    room.LeftMargin = Math.Min(room.LeftMargin, 33);
                                    room.RightMargin = 34 - room.LeftMargin;
                                    _building.Levels[SelectedLevel].Floors[floor].Rooms.Add(room);
                                }
                            }
                        }
                    }
                }
            }

            // ****** UI ******

            // floor mask
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, Width, 14),
                _maskRectangle[3],
                Color.White);
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 14, Width, 2),
                _maskRectangle[4],
                Color.White);
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, Height - 12, Width, 12),
                _maskRectangle[3],
                Color.White);
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, Height - 14, Width, 2),
                _maskRectangle[4],
                Color.White);

            // up
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, Width, _uiRectangle[4].Height),
                _uiRectangle[4],
                Color.White);

            // down
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, Height - _uiRectangle[5].Height, Width, _uiRectangle[5].Height),
                _uiRectangle[5],
                Color.White);

            // left
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, _uiRectangle[6].Width, Height),
                _uiRectangle[6],
                Color.White);

            // right
            spriteBatch.Draw(_levelTexture,
                new Rectangle(Width - _uiRectangle[7].Width, 0, _uiRectangle[6].Width, Height),
                _uiRectangle[7],
                Color.White);

            // upper left
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, _uiRectangle[0].Width, _uiRectangle[0].Height),
                _uiRectangle[0],
                Color.White);

            // upper right
            spriteBatch.Draw(_levelTexture,
                new Rectangle(Width - _uiRectangle[1].Width, 0, _uiRectangle[1].Width, _uiRectangle[1].Height),
                _uiRectangle[1],
                Color.White);

            // lower left
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, Height - _uiRectangle[2].Height, _uiRectangle[2].Width, _uiRectangle[2].Height),
                _uiRectangle[2],
                Color.White);

            // lower right
            spriteBatch.Draw(_levelTexture,
                new Rectangle(Width - _uiRectangle[3].Width, Height - _uiRectangle[3].Height, _uiRectangle[3].Width, _uiRectangle[3].Height),
                _uiRectangle[3],
                Color.White);

            // masks
            spriteBatch.Draw(_levelTexture,
                new Rectangle(19, 0, _maskRectangle[0].Width, _maskRectangle[0].Height),
                _maskRectangle[0],
                Color.White);
            spriteBatch.Draw(_levelTexture,
                new Rectangle(136, 0, _maskRectangle[1].Width, _maskRectangle[1].Height),
                _maskRectangle[1],
                Color.White);
            spriteBatch.Draw(_levelTexture,
                new Rectangle(257, 0, _maskRectangle[2].Width, _maskRectangle[2].Height),
                _maskRectangle[2],
                Color.White);

            // player life
            for (int h = 0; h < 3; h++)
            {
                if (PlayerLife / ((h + 1) * 2) >= 1)
                {
                    // full
                    spriteBatch.Draw(_levelTexture,
                        new Rectangle(24 + h * _iconRectangle[0].Width, 0, _iconRectangle[0].Width, _iconRectangle[0].Height),
                        _iconRectangle[0],
                        Color.White);
                }
                else if ((PlayerLife - h * 2) % ((h + 1) * 2) >= 1)
                {
                    // half
                    spriteBatch.Draw(_levelTexture,
                        new Rectangle(24 + h * _iconRectangle[1].Width, 0, _iconRectangle[1].Width, _iconRectangle[1].Height),
                        _iconRectangle[1],
                        Color.White);
                }
                else
                {
                    // empty
                    spriteBatch.Draw(_levelTexture,
                        new Rectangle(24 + h * _iconRectangle[2].Width, 0, _iconRectangle[2].Width, _iconRectangle[2].Height),
                        _iconRectangle[2],
                        Color.White);
                }
            }

            // time
            /*
            if (_building != null && SelectedLevel >= 0 && SelectedLevel < _building.Levels.Count)
            {
                Time = _building.Levels[SelectedLevel].TimeBeforeCop;
            }*/

            Time = _building.Levels[SelectedLevel].TargetTime;

            _encodedTime[0] = Time / 600;
            _encodedTime[1] = (Time - _encodedTime[0] * 600) / 60;
            int seconds = Time % 60;
            _encodedTime[3] = seconds / 10;
            _encodedTime[4] = seconds % 10;

            int textX = 0;
            for (int t = 0; t < _encodedTime.Length; t++)
            {
                source = _numberRectangle[_encodedTime[t]];
                spriteBatch.Draw(_levelTexture,
                    new Rectangle(140 + textX, 0, source.Width, source.Height),
                    source,
                    Color.White);

                textX += source.Width;
            }

            // score         
            if (Score > 999)
                Score = 999;   

            _encodedScore[0] = Score / 100;
            _encodedScore[1] = (Score - _encodedScore[0] * 100) / 10;
            _encodedScore[2] = Score % 10;

            textX = 0;

            // $
            spriteBatch.Draw(_levelTexture,
                new Rectangle(261 + textX, 0, _numberRectangle[11].Width, _numberRectangle[11].Height),
                _numberRectangle[11],
                Color.White);

            textX += _numberRectangle[11].Width;

            for (int s = 0; s < _encodedScore.Length; s++)
            {
                source = _numberRectangle[_encodedScore[s]];
                spriteBatch.Draw(_levelTexture,
                    new Rectangle(261 + textX, 0, source.Width, source.Height),
                    source,
                    Color.White);

                textX += source.Width;
            }
            
            // editor UI
            for (int i = 0; i < (int)SelectionMode.Count - 1; i++)
            {
                destination = new Rectangle(314, 25 + i * _editorIcons[i].Height, _editorIcons[i].Width, _editorIcons[i].Height);

                spriteBatch.Draw(_levelTexture,
                    destination,
                    _editorIcons[i],
                    Color.White);

                if (IsHover(destination, true) && (_hasLeftClick || _hasRightClick))
                    SelectionMode = (SelectionMode)i;

                if (SelectionMode == (SelectionMode)i)
                {
                    spriteBatch.Draw(_levelTexture,
                        destination,
                        _editorIcons[(int)SelectionMode.Selection],
                        Color.White);
                }

               
            }

            // help
            destination = new Rectangle(22, 171, _editorHelp[(int)SelectionMode].Width, _editorHelp[(int)SelectionMode].Height);

            spriteBatch.Draw(_levelTexture,
                destination,
                _editorHelp[(int)SelectionMode],
                Color.White);

            // camera control
            destination = new Rectangle(0, 27, _editorIcons[(int)SelectionMode.Count].Width, _editorIcons[(int)SelectionMode.Count].Height);

            spriteBatch.Draw(_levelTexture,
                destination,
                _editorIcons[(int)SelectionMode.Count + 3],
                Color.White);

            if (IsHover(destination, true) && _hasLeftClick)
            {
                _currentCameraFloor++;
                MoveCameraToFloor(_currentCameraFloor);
            }

            destination = new Rectangle(0, 146, _editorIcons[(int)SelectionMode.Count].Width, _editorIcons[(int)SelectionMode.Count].Height);

            spriteBatch.Draw(_levelTexture,
                destination,
                _editorIcons[(int)SelectionMode.Count + (_currentCameraFloor == 0 ? 0 : 2)],
                Color.White);

            if (IsHover(destination, true) && _hasLeftClick)
            {
                _currentCameraFloor--;
                MoveCameraToFloor(_currentCameraFloor);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public bool IsHover(Rectangle r, bool isCharacter = false)
        {
            if (!isCharacter)
            {
                r.Y = r.Y + 8;
                r.Height = r.Height - 16;
            }
            Rectangle rr = new Rectangle(_mouseState.Position, new Point(1));
            return r.Intersects(rr);
        }

        public bool IsHover(Room room, int f, int cameraPosY)
        {
            int startX = 16 + room.LeftMargin * 8;
            int nbSlice = 37 - room.LeftMargin - room.RightMargin;

            Rectangle source = _roomRectangle[room.BackgroundColor][0][1];
            Rectangle destination = new Rectangle(startX + 8, 128 - 40 * f + cameraPosY, source.Width * nbSlice, source.Height);

            Rectangle rr = new Rectangle(_mouseState.Position, new Point(1));
            return destination.Intersects(rr);
        }

        public bool IsOutsideRoom(Room room, IMovable actor)
        {
            // handle wall collisions
            if (actor.GetX() >= room.LeftMargin * 8 + 8 && actor.GetX() <= 304 - room.RightMargin * 8 - 16)
            {
                return false;
            }

            return true;
        }

        public bool IsOutsideRoom(Room room, int actorX)
        {
            // handle wall collisions
            if (actorX >= room.LeftMargin * 8 + 8 && actorX <= 304 - room.RightMargin * 8 - 16)
            {
                return false;
            }

            return true;
        }
    }
}

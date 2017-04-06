using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GreedyKidEditor
{
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

        public int Score = 0;
        private int[] _encodedScore = new int[] { 0, 0, 0 };
        public int Time = 0;
        private int[] _encodedTime = new int[] { 0, 0, 10, 0, 0 };
        public int PlayerLife = 3;

        Color _fillColor = new Color(34, 32, 52);

        Building _building;

        public int SelectedLevel = -1;
        public int SelectedFloor = -1;
        public int SelectedRoom = -1;

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
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 0, 1, 2,
        };
        int _currentRetiredFrame = 0;

        int[] _nurseSequence = new int[]
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 0, 1, 2,
        };
        int _currentNurseFrame = 0;

        int[] _copSequence = new int[]
        {
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            3, 0, 1, 2,
        };
        int _currentCopFrame = 0;

        public static bool PreviewAnimation = false;

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

        public void MoveCamera(float targetPosition)
        {
            _initialCameraPositionY = _cameraPositionY;
            _differenceCameraPositionY = targetPosition - _cameraPositionY;
            _currentCameraTime = 0.0f;
        }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {
            args.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = _handle;
        }

        public PreviewRenderer(IntPtr handle, MainWindow w, Building b)
        {
            _building = b;
            _w = w;
            _handle = handle;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += OnPreparingDeviceSettings;
            Content.RootDirectory = "Content";
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 33);

            graphics.PreferredBackBufferHeight = Width;
            graphics.PreferredBackBufferWidth = Height;
            IsMouseVisible = true;
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

            using (System.IO.FileStream file = System.IO.File.OpenRead("Content\\Textures\\level.png"))
            {
                _levelTexture = Texture2D.FromStream(this.GraphicsDevice, file);
            }



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

            int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

            _retiredRectangle = new Rectangle[Retired.RetiredCount][];
            for (int t = 0; t < Retired.RetiredCount; t++)
            {
                _retiredRectangle[t] = new Rectangle[4];
                for (int f = 0; f < 4; f++) // idle animation
                {
                    _retiredRectangle[t][f] = new Rectangle(4 * 32 + f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                }
            }

            _nurseRectangle = new Rectangle[Nurse.NurseCount][];
            for (int t = 0; t < Nurse.NurseCount; t++)
            {
                _nurseRectangle[t] = new Rectangle[4];
                for (int f = 0; f < 4; f++) // idle animation
                {
                    _nurseRectangle[t][f] = new Rectangle(4 * 32 + f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + 32 * Retired.RetiredCount, 32, 32);
                }
            }

            _copRectangle = new Rectangle[Cop.CopCount][];
            for (int t = 0; t < Cop.CopCount; t++)
            {
                _copRectangle[t] = new Rectangle[4];
                for (int f = 0; f < 4; f++) // idle animation
                {
                    _copRectangle[t][f] = new Rectangle(4 * 32 + f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + 32 * Retired.RetiredCount + 32 * Nurse.NurseCount, 32, 32);
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

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_fillColor);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            int cameraPosY = (int)Math.Round(_cameraPositionY);
            
            if (SelectedLevel >= 0 && SelectedLevel < _building.Levels.Count)
            {
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

                        Rectangle source = _roomRectangle[room.BackgroundColor][0][1];

                        for (int s = 0; s < nbSlice; s++)
                        {
                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(startX + 8 * s, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // left wall
                        source = _roomRectangle[room.BackgroundColor][room.LeftDecoration][0];

                        spriteBatch.Draw(_levelTexture,
                            new Rectangle(room.LeftMargin * 8, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                            source,
                            Color.White);

                        // right wall
                        source = _roomRectangle[room.BackgroundColor][room.RightDecoration][2];

                        spriteBatch.Draw(_levelTexture,
                            new Rectangle(304 - room.RightMargin * 8, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                            source,
                            Color.White);
                    }

                    // rooms details
                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];                        

                        for (int d = 0; d < room.Details.Count; d++)
                        {
                            Detail detail = room.Details[d];
                            Rectangle source = _detailRectangle[room.BackgroundColor][detail.Type];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(detail.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }
                    }

                    
                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        // floor doors
                        for (int d = 0; d < room.FloorDoors.Count; d++)
                        {
                            FloorDoor floorDoor = room.FloorDoors[d];
                            Rectangle source = _floorDoorRectangle[room.BackgroundColor][floorDoor.Color][_floorDoorSequence[_currentFloorDoorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(floorDoor.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // room doors
                        for (int d = 0; d < room.RoomDoors.Count; d++)
                        {
                            RoomDoor roomDoor = room.RoomDoors[d];
                            Rectangle source = _roomDoorRectangle[room.BackgroundColor][_roomDoorSequence[_currentRoomDoorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(roomDoor.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // furniture
                        for (int ff = 0; ff < room.Furnitures.Count; ff++)
                        {
                            Furniture furniture = room.Furnitures[ff];
                            Rectangle source = _furnitureRectangle[room.BackgroundColor][furniture.Type][0];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(furniture.X, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // elevator
                        if (room.HasStart)
                        {
                            Rectangle source = _elevatorRectangle[0][_elevatorSequence[_currentElevatorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(room.StartX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[1][_elevatorSequence[_currentElevatorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(room.StartX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(room.StartX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }                    

                        if (room.HasExit)
                        {
                            Rectangle source = _elevatorRectangle[0][_elevatorSequence[_currentElevatorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(room.ExitX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[1][_elevatorSequence[_currentElevatorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(room.ExitX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(room.ExitX, 128 - 40 * f + cameraPosY, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // retired

                        for (int rr = 0; rr < room.Retireds.Count; rr++)
                        {
                            Retired retired = room.Retireds[rr];

                            Rectangle source = _retiredRectangle[retired.Type][_retiredSequence[_currentRetiredFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)retired.X, 128 - 40 * f + 9 + cameraPosY, 32, 32),
                                source,
                                Color.White);
                        }

                        // nurse
                        for (int n = 0; n < room.Nurses.Count; n++)
                        {
                            Nurse nurse = room.Nurses[n];

                            Rectangle source = _nurseRectangle[nurse.Type][_nurseSequence[_currentNurseFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)nurse.X, 128 - 40 * f + 9 + cameraPosY, 32, 32),
                                source,
                                Color.White);
                        }

                        // cop
                        for (int c = 0; c < room.Cops.Count; c++)
                        {
                            Cop cop = room.Cops[c];

                            Rectangle source = _copRectangle[cop.Type][_copSequence[_currentCopFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle((int)cop.X, 128 - 40 * f + 9 + cameraPosY, 32, 32),
                                source,
                                Color.White);
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
            _encodedTime[0] = Time / 600;
            _encodedTime[1] = (Time - _encodedTime[0] * 600) / 60;
            int seconds = Time % 60;
            _encodedTime[3] = seconds / 10;
            _encodedTime[4] = seconds % 10;

            int textX = 0;
            for (int t = 0; t < _encodedTime.Length; t++)
            {
                Rectangle source = _numberRectangle[_encodedTime[t]];
                spriteBatch.Draw(_levelTexture,
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
                spriteBatch.Draw(_levelTexture,
                    new Rectangle(261 + textX, 0, source.Width, source.Height),
                    source,
                    Color.White);

                textX += source.Width;
            }
            // $
            spriteBatch.Draw(_levelTexture,
                new Rectangle(261 + textX, 0, _numberRectangle[11].Width, _numberRectangle[11].Height),
                _numberRectangle[11],
                Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

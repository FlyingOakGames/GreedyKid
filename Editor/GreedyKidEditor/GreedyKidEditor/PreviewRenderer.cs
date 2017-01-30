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
        int _currentFloorDoorFrame = 0;
        int _currentRoomDoorFrame = 0;
        float _currentFrameTime = 0.0f;

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

                _detailRectangle[p] = new Rectangle[Detail.DetailCount];
                for (int d = 0; d < Detail.DetailCount; d++)
                {
                    _detailRectangle[p][d] = new Rectangle(56 * Room.DecorationCount + d * 32, 48 * p, 32, 48);
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
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_fillColor);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            
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
                                new Rectangle(startX + 8 * s, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        // left wall
                        source = _roomRectangle[room.BackgroundColor][room.LeftDecoration][0];

                        spriteBatch.Draw(_levelTexture,
                            new Rectangle(room.LeftMargin * 8, 128 - 40 * f, source.Width, source.Height),
                            source,
                            Color.White);

                        // right wall
                        source = _roomRectangle[room.BackgroundColor][room.RightDecoration][2];

                        spriteBatch.Draw(_levelTexture,
                            new Rectangle(304 - room.RightMargin * 8, 128 - 40 * f, source.Width, source.Height),
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
                                new Rectangle(detail.X, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
                        }
                    }

                    // floor doors
                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        for (int d = 0; d < room.FloorDoors.Count; d++)
                        {
                            FloorDoor floorDoor = room.FloorDoors[d];
                            Rectangle source = _floorDoorRectangle[room.BackgroundColor][floorDoor.Color][_floorDoorSequence[_currentFloorDoorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(floorDoor.X, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
                        }
                    }

                    // room doors
                    for (int r = 0; r < floor.Rooms.Count; r++)
                    {
                        Room room = floor.Rooms[r];

                        for (int d = 0; d < room.RoomDoors.Count; d++)
                        {
                            RoomDoor roomDoor = room.RoomDoors[d];
                            Rectangle source = _roomDoorRectangle[room.BackgroundColor][_roomDoorSequence[_currentRoomDoorFrame]];

                            spriteBatch.Draw(_levelTexture,
                                new Rectangle(roomDoor.X, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
                        }
                    }
                }
            }

            // overlay
            spriteBatch.Draw(_levelTexture,
                new Rectangle(0, 0, 328, 184),
                new Rectangle(0, 840, 328, 184),
                Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

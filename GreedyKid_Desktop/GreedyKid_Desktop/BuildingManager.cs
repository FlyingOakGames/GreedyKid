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

        public int SelectedLevel = 0;

        public Player Player;

        // arrow animation
        private int _currentArrowFrame = 0;
        private float _currentArrowFrameTime = 0.0f;
        private const float _arrowFrameTime = 0.1f;

        // shouting animation
        private int _currentShoutFrame = 0;
        private float _currentShoutFrameTime = 0.0f;
        private const float _shoutFrameTime = 0.05f;

        private int _shoutDistance = 32;

        public BuildingManager()
        {
            Player = new Player();

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
        }

        public void LoadBuilding(string building = null)
        {
            _building = new Building();
            _building.Load(building);

            ResetLevel(0);
        }

        public void ResetLevel(int level)
        {
            SelectedLevel = level;

            Player.Room = null;

            // look for start
            for (int f = 0; f < _building.Levels[SelectedLevel].Floors.Length; f++)
            {
                for (int r = 0; r < _building.Levels[SelectedLevel].Floors[f].Rooms.Length; r++)
                {
                    if (_building.Levels[SelectedLevel].Floors[f].Rooms[r].HasStart)
                    {
                        Player.Room = _building.Levels[SelectedLevel].Floors[f].Rooms[r];
                        Player.X = Player.Room.StartX + 4;
                        break;
                    }
                }
                if (Player.Room != null)
                    break;
            }
        }

        public void Update(float gameTime)
        {
            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.HandleIngameInputs(Player);

            Player.Update(gameTime);

            bool isShouting = Player.IsShouting;
            bool isTaunting = Player.IsTaunting;
            int playerMiddle = (int)Player.X + 16;

            if (SelectedLevel >= 0 && SelectedLevel < _building.Levels.Length)
            {
                for (int f = 0; f < _building.Levels[SelectedLevel].Floors.Length; f++)
                {
                    Floor floor = _building.Levels[SelectedLevel].Floors[f];

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

                        // retireds
                        for (int rr = 0; rr < room.Retireds.Count; rr++)
                        {
                            Retired retired = room.Retireds[rr];
                            if (retired != null)
                            {
                                bool boo = false;

                                if (isShouting && retired.Room == Player.Room)
                                {
                                    int retiredMiddle = (int)retired.X + 16;
                                    if (Math.Abs(retiredMiddle - playerMiddle) <= _shoutDistance && retired.NotFacing(playerMiddle))
                                    boo = true;
                                }
                                if (retired.Room == Player.Room)
                                {
                                    retired.LastKnownPlayerPosition = playerMiddle;
                                }
                                else
                                {
                                    retired.LastKnownPlayerPosition = -1;
                                }

                                retired.Update(gameTime, boo, isTaunting);
                            }
                        }

                        // nurses
                        for (int n = 0; n < room.Nurses.Count; n++)
                        {
                            Nurse nurse = room.Nurses[n];
                            if (nurse != null)
                            {
                                bool boo = false;

                                if (isShouting && nurse.Room == Player.Room)
                                {
                                    int retiredMiddle = (int)nurse.X + 16;
                                    if (Math.Abs(retiredMiddle - playerMiddle) <= _shoutDistance && nurse.NotFacing(playerMiddle))
                                        boo = true;
                                }
                                if (nurse.Room == Player.Room)
                                {
                                    nurse.LastKnownPlayerPosition = playerMiddle;
                                }
                                else
                                {
                                    nurse.LastKnownPlayerPosition = -1;
                                }

                                nurse.Update(gameTime, boo, isTaunting);
                            }
                        }
                    }
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

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Building;

            if (SelectedLevel >= 0 && SelectedLevel < _building.Levels.Length)
            {
                int shoutingFrame = Furniture.FurnitureFrames - 4 + _currentShoutFrame;
                bool isShouting = Player.IsShouting;
                int playerMiddle = (int)Player.X + 16;

                for (int f = 0; f < _building.Levels[SelectedLevel].Floors.Length; f++)
                {
                    Floor floor = _building.Levels[SelectedLevel].Floors[f];

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
                            Rectangle source = _elevatorRectangle[0][0];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[1][0];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.StartX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
                        }

                        if (room.HasExit)
                        {
                            Rectangle source = _elevatorRectangle[0][0];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[1][0];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);

                            source = _elevatorRectangle[2][room.BackgroundColor];

                            spriteBatch.Draw(texture,
                                new Rectangle(room.ExitX, 128 - 40 * f, source.Width, source.Height),
                                source,
                                Color.White);
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

            Player.Draw(spriteBatch);

            spriteBatch.End();
        }
    }
}

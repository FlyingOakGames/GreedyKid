using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Retired : IEntity
    {
        public const int RetiredCount = 1;

        private const float _walkSpeed = 16.0f;
        private const float _runSpeed = 48.0f;

        public int Type = 0;
        public float X = 0;
        public Room Room = null;

        public int Life = 1;

        public EntityState State = EntityState.Idle;
        public SpriteEffects Orientation = SpriteEffects.None;

        // shared statics
        private Rectangle[][][] _frames;
        private float[] _frameDuration;

        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;

        // entering / exiting
        private bool _isVisible = true;
        private FloorDoor _targetDoor = null;
        private bool _wantsToOpenDoor = false;

        // walking
        private float _actionTime = 0.0f;
        private bool _hasJustTurned = false;

        public Retired()
        {
            // init once
            if (_frames == null)
            {
                _frames = new Rectangle[(int)EntityState.Count][][];
                _frameDuration = new float[(int)EntityState.Count];

                int nbDoorLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);
                int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

                // type
                for (int t = 0; t < RetiredCount; t++)
                {
                    // idle
                    _frames[(int)EntityState.Idle] = new Rectangle[RetiredCount][];
                    _frames[(int)EntityState.Idle][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Idle][t].Length; f++)
                    {
                        _frames[(int)EntityState.Idle][t][f] = new Rectangle(f * 32 + 4 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Idle] = 0.1f;

                    // turning
                    _frames[(int)EntityState.Turning] = new Rectangle[RetiredCount][];
                    _frames[(int)EntityState.Turning][t] = new Rectangle[5];
                    for (int f = 0; f < _frames[(int)EntityState.Turning][t].Length; f++)
                    {
                        _frames[(int)EntityState.Turning][t][f] = new Rectangle(f * 32 + 12 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Turning] = 0.1f;

                    // walking
                    _frames[(int)EntityState.Walking] = new Rectangle[RetiredCount][];
                    _frames[(int)EntityState.Walking][t] = new Rectangle[4];
                    for (int f = 0; f < _frames[(int)EntityState.Walking][t].Length; f++)
                    {
                        _frames[(int)EntityState.Walking][t][f] = new Rectangle(f * 32 + 8 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Walking] = 0.1f;

                    // entering
                    _frames[(int)EntityState.Entering] = new Rectangle[RetiredCount][];
                    _frames[(int)EntityState.Entering][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Entering][t].Length; f++)
                    {
                        _frames[(int)EntityState.Entering][t][f] = new Rectangle(f * 32 + 32 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Entering] = 0.1f;

                    // exiting
                    _frames[(int)EntityState.Exiting] = new Rectangle[RetiredCount][];
                    _frames[(int)EntityState.Exiting][t] = new Rectangle[3];
                    for (int f = 0; f < _frames[(int)EntityState.Exiting][t].Length; f++)
                    {
                        _frames[(int)EntityState.Exiting][t][f] = new Rectangle(f * 32 + 35 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine + 32 + t * 32, 32, 32);
                    }
                    _frameDuration[(int)EntityState.Exiting] = 0.1f;
                }
            }
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
            Life = reader.ReadInt32();
        }

        public void Update(float gameTime)
        {
            // update state
            _currentFrameTime += gameTime;

            if (_currentFrameTime > _frameDuration[(int)State])
            {
                _currentFrameTime -= _frameDuration[(int)State];
                _currentFrame++;

                if (_currentFrame == _frames[(int)State][Type].Length)
                {
                    _currentFrame = 0;
                   
                    // AI states

                    if (State == EntityState.Idle && _isVisible)
                    {
                        // should walk or turn
                        if (!_hasJustTurned && RandomHelper.Next() < 0.5f)
                            Turn();
                        else
                            Walk();
                    }
                    else if (State == EntityState.Turning)
                    {
                        if (Orientation == SpriteEffects.None)
                            Orientation = SpriteEffects.FlipHorizontally;
                        else
                            Orientation = SpriteEffects.None;


                        State = EntityState.Idle;
                    }


                    // entering doors
                    else if (State == EntityState.Entering)
                    {
                        _isVisible = false;
                        State = EntityState.Idle;
                        if (_targetDoor != null)
                        {
                            Room.Retireds.Remove(this);
                            Room = _targetDoor.Room;
                            Room.Retireds.Add(this);
                            X = _targetDoor.X;
                            _targetDoor = null;
                        }
                    }
                    // exiting doors
                    else if (State == EntityState.Exiting)
                    {
                        State = EntityState.Idle;
                    }                    
                }
            }

            if (_actionTime > 0.0f)
            {
                _actionTime -= gameTime;

                if (_actionTime <= 0.0f)
                {
                    State = EntityState.Idle;
                    _currentFrame = 0;
                    _wantsToOpenDoor = false;
                }
            }

            if (State == EntityState.Walking)
            {
                if (Orientation == SpriteEffects.None)
                    X = X + _walkSpeed * gameTime;
                else
                    X = X - _walkSpeed * gameTime;

                // handle wall collisions
                if (X < Room.LeftMargin * 8 + 8)
                {
                    Turn();
                    X = Room.LeftMargin * 8 + 8;
                }
                if (X > 304 - Room.RightMargin * 8 - 16)
                {
                    Turn();
                    X = 304 - Room.RightMargin * 8 - 16;
                }

                // handle room door collisions
                for (int d = 0; d < Room.RoomDoors.Length; d++)
                {
                    RoomDoor roomDoor = Room.RoomDoors[d];

                    if (!roomDoor.IsOpenLeft && !roomDoor.IsOpenRight)
                    {
                        if (Orientation == SpriteEffects.FlipHorizontally && X - 12 < roomDoor.X && X - 12 > roomDoor.X - 4)
                        {
                            Turn();
                        }
                        else if (Orientation == SpriteEffects.None && X + 8 > roomDoor.X && X + 8 < roomDoor.X + 4)
                        {
                            Turn();
                        }
                    }
                }

                // entering floor doors
                if (_wantsToOpenDoor)
                {
                    for (int d = 0; d < Room.FloorDoors.Length; d++)
                    {
                        FloorDoor floorDoor = Room.FloorDoors[d];

                        if (X + 16 > floorDoor.X + 11 && X + 16 < floorDoor.X + 27)
                        {
                            if (_wantsToOpenDoor && floorDoor.CanAIOpen)
                            {
                                floorDoor.EnterOpen();
                                Enter(floorDoor);
                                _wantsToOpenDoor = false;
                            }
                        }
                    }
                }
            }
        }

        private void Turn()
        {
            State = EntityState.Turning;
            _hasJustTurned = true;
            _actionTime = 0.0f;
        }

        private void Walk()
        {
            State = EntityState.Walking;
            _actionTime = RandomHelper.Next() * 2.0f + 1.0f;
            _hasJustTurned = false;

            _wantsToOpenDoor = false;
            if (RandomHelper.Next() <= 0.25f)
            {
                _wantsToOpenDoor = true;
            }
        }

        private void Enter(FloorDoor floorDoor)
        {
            X = floorDoor.X;
            _targetDoor = floorDoor.SisterDoor;
            floorDoor.SisterDoor.ArrivingEntity = this;

            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Entering;

            _actionTime = 0.0f;
        }

        public void Exit()
        {
            _isVisible = true;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            State = EntityState.Exiting;

            _actionTime = 0.0f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible)
                return;

            Texture2D texture = TextureManager.Building;

            spriteBatch.Draw(texture,
                new Rectangle((int)X, 128 - 40 * Room.Y + 16, 32, 32),
                _frames[(int)State][Type][_currentFrame],
                Color.White,
                0.0f,
                Vector2.Zero,
                Orientation,
                0.0f);
        }
    }
}

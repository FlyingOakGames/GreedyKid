using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Retired
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

                    if (State == EntityState.Idle)
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
                            Room = _targetDoor.Room;
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
                    _actionTime = 0.0f;
                    Turn();
                    X = Room.LeftMargin * 8 + 8;
                }
                if (X > 304 - Room.RightMargin * 8 - 16)
                {
                    _actionTime = 0.0f;
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
                            _actionTime = 0.0f;
                            Turn();
                        }
                        else if (Orientation == SpriteEffects.None && X + 8 > roomDoor.X && X + 8 < roomDoor.X + 4)
                        {
                            _actionTime = 0.0f;
                            Turn();
                        }
                    }

                }
            }
        }

        private void Turn()
        {
            State = EntityState.Turning;
            _hasJustTurned = true;
        }

        private void Walk()
        {
            State = EntityState.Walking;
            _actionTime = RandomHelper.Next() * 2.0f + 1.0f;
            _hasJustTurned = false;
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

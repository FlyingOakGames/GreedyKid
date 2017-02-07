using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GreedyKid
{
    public sealed class Player
    {
        private const float _walkSpeed = 64.0f;
        private const float _rollSpeed = 96.0f;

        public float X = 0;
        public Room Room = null;

        public EntityState State = EntityState.Idle;
        public SpriteEffects Orientation = SpriteEffects.None;

        private Rectangle[][] _frames;
        private float[] _frameDuration;

        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;

        // moving
        private int _moveDirection = 0;

        // action
        private bool _doingAction = false;

        public Player()
        {
            _frames = new Rectangle[(int)EntityState.Count][];
            _frameDuration = new float[(int)EntityState.Count];

            int nbDoorLine = (int)Math.Ceiling(FloorDoor.DoorCount / (float)FloorDoor.DoorPerLine);
            int nbFurnitureLine = (int)Math.Ceiling(Furniture.FurnitureCount / (float)Furniture.FurniturePerLine);

            // idle
            _frames[(int)EntityState.Idle] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Idle].Length; f++)
            {
                _frames[(int)EntityState.Idle][f] = new Rectangle(f * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Idle] = 0.1f;

            // running
            _frames[(int)EntityState.Running] = new Rectangle[4];
            for (int f = 0; f < _frames[(int)EntityState.Running].Length; f++)
            {
                _frames[(int)EntityState.Running][f] = new Rectangle(f * 32 + 8 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Running] = 0.1f;

            // rolling
            _frames[(int)EntityState.Rolling] = new Rectangle[7];
            for (int f = 0; f < _frames[(int)EntityState.Rolling].Length; f++)
            {
                _frames[(int)EntityState.Rolling][f] = new Rectangle(f * 32 + 12 * 32, Room.PaintCount * 48 + Room.PaintCount * 48 * nbDoorLine + 48 + Room.PaintCount * 48 * nbFurnitureLine, 32, 32);
            }
            _frameDuration[(int)EntityState.Rolling] = 0.1f;
        }

        public void Update(float gameTime)
        {
            // update state
            _currentFrameTime += gameTime;

            if (_currentFrameTime > _frameDuration[(int)State])
            {
                _currentFrameTime -= _frameDuration[(int)State];
                _currentFrame++;

                if (_currentFrame == _frames[(int)State].Length)
                {
                    _currentFrame = 0;

                    // stop rolling
                    if (State == EntityState.Rolling)
                        State = EntityState.Idle;
                }
            }

            // start / stop running
            if (_moveDirection != 0 && State == EntityState.Idle)
            {
                State = EntityState.Running;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
            }
            else if (_moveDirection == 0 && State == EntityState.Running)
            {
                State = EntityState.Idle;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
            }

            // can interact with entities?
            for (int d = 0; d < Room.RoomDoors.Length; d++)
            {
                RoomDoor roomDoor = Room.RoomDoors[d];

                if (X + 16 > roomDoor.X && X + 16 < roomDoor.X + 8)
                    roomDoor.CheckCanCloseFromLeft();
                else if (X + 16 > roomDoor.X + 24 && X + 16 < roomDoor.X + 32)
                    roomDoor.CheckCanCloseFromRight();
                else
                    roomDoor.CanClose = false;

                if (_doingAction && roomDoor.CanClose)
                {
                    roomDoor.Close();
                }
            }

            for (int d = 0; d < Room.FloorDoors.Length; d++)
            {
                FloorDoor floorDoor = Room.FloorDoors[d];

                if (X + 16 > floorDoor.X + 11 && X + 16 < floorDoor.X + 27)
                    floorDoor.CheckCanOpen();
                else
                {
                    floorDoor.CanOpen = false;
                    floorDoor.SisterDoor.CanOpen = false;
                }

                if (_doingAction && floorDoor.CanOpen)
                    floorDoor.EnterOpen();
            }

            // moving
            if (_moveDirection != 0)
            {
                if (State == EntityState.Running)
                    X += _moveDirection * _walkSpeed * gameTime;
                else if (State == EntityState.Rolling)
                    X += _moveDirection * _rollSpeed * gameTime;

                // handle wall collisions
                if (X < Room.LeftMargin * 8 + 4)
                    X = Room.LeftMargin * 8 + 4;
                if (X > 304 - Room.RightMargin * 8 - 12)
                    X = 304 - Room.RightMargin * 8 - 12;

                // moving toward a closed door
                for (int d = 0; d < Room.RoomDoors.Length; d++)
                {
                    RoomDoor roomDoor = Room.RoomDoors[d];
                    
                    // auto open
                    if (_moveDirection < 0 && X - 12 < roomDoor.X && X - 12 > roomDoor.X - 4)
                        roomDoor.OpenLeft();
                    else if (_moveDirection > 0 && X + 6 > roomDoor.X && X + 6 < roomDoor.X + 4)
                        roomDoor.OpenRight();

                    
                }

                if (State == EntityState.Running)
                    _moveDirection = 0;
            }

            _doingAction = false;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = TextureManager.Building;

            spriteBatch.Draw(texture,
                new Rectangle((int)X, 128 - 40 * Room.Y + 16, 32, 32),
                _frames[(int)State][_currentFrame],
                Color.White,
                0.0f,
                Vector2.Zero,
                Orientation,
                0.0f);
        }

        public void MoveLeft()
        {
            if (State == EntityState.Idle || State == EntityState.Running)
            {
                _moveDirection = -1;
                Orientation = SpriteEffects.FlipHorizontally;
            }
        }

        public void MoveRight()
        {
            if (State == EntityState.Idle || State == EntityState.Running)
            {
                _moveDirection = 1;
                Orientation = SpriteEffects.None;
            }
        }

        public void Roll()
        {
            if (State == EntityState.Idle || State == EntityState.Running)
            {                
                if (Orientation == SpriteEffects.None)
                    _moveDirection = 1;
                else
                    _moveDirection = -1;

                State = EntityState.Rolling;
                _currentFrame = 0;
                _currentFrameTime = 0.0f;
            }
        }

        public void Action()
        {
            if (State == EntityState.Idle || State == EntityState.Running)
            {
                _doingAction = true;
            }
        }
    }
}

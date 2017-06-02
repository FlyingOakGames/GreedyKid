using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public sealed class Droppable
    {
        public ObjectType Type = ObjectType.HealthPack;

        public Room Room = null;

        public float X = 0.0f;

        // movement
        private float _initialX = 0.0f;
        private float _differenceX = 0.0f;
        private float _currentTime = 0.0f;
        private float _totalTime = 0.0f;

        // animation
        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;
        private const float _frameTime = 0.1f;

        private float _lockedTime = 0.0f;

        public Droppable(ObjectType type)
        {
            Type = type;
            _currentFrame = RandomHelper.Next(5);
        }

        public bool CanBeLooted
        {
            get { return _lockedTime <= 0.0f; }
        }

        public void Drop(float x, float lockedTime)
        {
            _initialX = x;
            X = x;
            _currentTime = 0.0f;

            _differenceX = 16.0f + RandomHelper.Next() * 16.0f;
            if (RandomHelper.Next() < 0.5f)
                _differenceX *= -1.0f;
            _totalTime = 1.0f + RandomHelper.Next() * 1.0f;

            _lockedTime = lockedTime;
        }

        public void Update(float gameTime)
        {
            // animation
            _currentFrameTime += gameTime;
            if (_currentFrameTime >= _frameTime)
            {
                _currentFrameTime -= _frameTime;

                _currentFrame++;

                if (Type == ObjectType.HealthPack)
                    _currentFrame %= 6;
                else
                    _currentFrame %= 5;
            }

            // locked
            if (_lockedTime > 0.0f)
                _lockedTime -= gameTime;

            if (_currentTime < _totalTime)
            {
                float previousX = X;
                _currentTime += gameTime;
                X = EasingHelper.EaseOutExpo(_currentTime, _initialX, _differenceX, _totalTime);
                
                // handle wall collisions
                if (X < Room.LeftMargin * 8 + 12)
                {
                    X = Room.LeftMargin * 8 + 12;
                    _currentTime = _totalTime;
                }
                if (X > 304 - Room.RightMargin * 8 - 4)
                {
                    X = 304 - Room.RightMargin * 8 - 4;
                    _currentTime = _totalTime;
                }

                // handle room door collisions
                for (int d = 0; d < Room.RoomDoors.Length; d++)
                {
                    RoomDoor roomDoor = Room.RoomDoors[d];

                    if (roomDoor.IsClosed)
                    {                        
                        if (X + 8 > roomDoor.X + 10 && X + 8 < roomDoor.X + 22)
                        {
                            X = previousX;
                            _currentTime = _totalTime;
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle[][] source, int cameraPosY)
        {
            Texture2D texture = TextureManager.Gameplay;

            spriteBatch.Draw(texture,
                new Rectangle(
                    (int)X,
                    128 - 40 * Room.Y + 25 + cameraPosY,
                    source[(int)Type][_currentFrame].Width,
                    source[(int)Type][_currentFrame].Height),
                    source[(int)Type][_currentFrame],
                Color.White);
        }
    }
}

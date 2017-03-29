using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GreedyKid
{
    public sealed class Droppable
    {
        public ObjectType Type = ObjectType.HealthPack;

        public Room Room = null;

        public float X = 0.0f;

        private float _speed = 0.0f;
        private float _direction = 1.0f;
        private float _acceleration = -256.0f;

        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;
        private const float _frameTime = 0.1f;

        public Droppable(ObjectType type)
        {
            Type = type;
            _currentFrame = RandomHelper.Next(5);
        }

        public void Drop()
        {
            _speed = 64.0f + RandomHelper.Next() * 64.0f;
            _acceleration = -192.0f - RandomHelper.Next() * 128.0f;
            if (RandomHelper.Next() < 0.5f)
                _direction = -1.0f;
        }

        public void Update(float gameTime)
        {

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

            if (_speed != 0.0f)
            {

                X += _direction * _speed * gameTime;
                float prevSpeed = _speed;
                _speed += _acceleration * gameTime;

                if (prevSpeed > 0 && _speed < 0 || prevSpeed < 0 && _speed > 0)
                    _speed = 0.0f;

                // handle wall collisions
                if (X < Room.LeftMargin * 8 + 12)
                {
                    X = Room.LeftMargin * 8 + 12;
                }
                if (X > 304 - Room.RightMargin * 8 - 4)
                {
                    X = 304 - Room.RightMargin * 8 - 4;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle[][] source)
        {
            Texture2D texture = TextureManager.Building;

            spriteBatch.Draw(texture,
                new Rectangle(
                    (int)X,
                    128 - 40 * Room.Y + 25,
                    source[(int)Type][_currentFrame].Width,
                    source[(int)Type][_currentFrame].Height),
                    source[(int)Type][_currentFrame],
                Color.White);
        }
    }
}

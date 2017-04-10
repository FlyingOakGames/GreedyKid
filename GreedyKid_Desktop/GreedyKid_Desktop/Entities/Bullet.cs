using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public enum BulletType
    {
        Taser,

        Count
    }

    public sealed class Bullet
    {
        public Room Room = null;

        public BulletType Type = BulletType.Taser;
        public float X = 0.0f;
        public SpriteEffects _orientation = SpriteEffects.None;

        private static float[] _bulletSpeed = new float[] { 128.0f };

        // animation
        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;
        private const float _frameTime = 0.1f;

        public void Fire(BulletType type, float x, SpriteEffects orientation, Room room)
        {
            Type = type;
            X = x;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            _orientation = orientation;
            Room = room;
        }

        public bool Update(float gameTime, Rectangle[][] source)
        {
            // animation
            _currentFrameTime += gameTime;
            if (_currentFrameTime >= _frameTime)
            {
                _currentFrameTime -= _frameTime;

                _currentFrame++;                
                _currentFrame %= source[(int)Type].Length;                
            }

            // move
            if (_orientation == SpriteEffects.None)
                X += _bulletSpeed[(int)Type] * gameTime;
            else
                X -= _bulletSpeed[(int)Type] * gameTime;

            // handle wall collisions
            if (X < Room.LeftMargin * 8 + 12)
            {
                X = Room.LeftMargin * 8 + 12;
                return true;
            }
            if (X > 304 - Room.RightMargin * 8 - 4)
            {
                X = 304 - Room.RightMargin * 8 - 4;
                return true;
            }

            // handle room door collisions
            for (int d = 0; d < Room.RoomDoors.Length; d++)
            {
                RoomDoor roomDoor = Room.RoomDoors[d];

                if (roomDoor.IsClosed)
                {
                    if (X + 8 > roomDoor.X + 10 && X + 8 < roomDoor.X + 22)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle[][] source, int cameraPosY)
        {
            Texture2D texture = TextureManager.Building;

            spriteBatch.Draw(texture,
                new Rectangle(
                    (int)X,
                    128 - 40 * Room.Y + 25 + cameraPosY,
                    source[(int)Type][_currentFrame].Width,
                    source[(int)Type][_currentFrame].Height),
                    source[(int)Type][_currentFrame],
                Color.White,
                0.0f,
                Vector2.Zero,
                _orientation,
                0.0f);
        }
    }
}

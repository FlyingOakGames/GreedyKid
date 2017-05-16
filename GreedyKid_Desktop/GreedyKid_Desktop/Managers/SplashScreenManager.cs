using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public sealed class SplashScreenManager
    {
        public bool SkipToTitle = false;

        private Rectangle[] _backgroundRectangles;
        private Rectangle[] _oakFrameRectangles;
        private Rectangle[] _scoreRectangles;
        private Rectangle _titleRectangle;

        private Rectangle _backgroundDestination;
        private Rectangle _scoreDestination;
        private Rectangle _titleDestination;

        private int _backgroundFrame = 0;
        private int _oakFrame = 0;
        private float _currentFrameTime = 0.0f;
        private const float _frameTime = 0.067f;
        private int _scoreFrame = -1;

        public SplashScreenManager()
        {
            TextureManager.LoadSplash();

            _backgroundRectangles = new Rectangle[4];
            for (int i = 0; i < _backgroundRectangles.Length; i++)
            {
                _backgroundRectangles[i] = new Rectangle(0, 42 * i, 147, 42);
            }

            _oakFrameRectangles = new Rectangle[13];
            for (int i = 0; i < _oakFrameRectangles.Length; i++)
            {
                int col = 0;
                int row = i + 4;

                if (i > 7)
                {
                    col = 2;
                    row = i - 8;
                }
                else if (i > 1)
                {
                    col = 1;
                    row = i - 2;
                }

                _oakFrameRectangles[i] = new Rectangle(147 * col, 42 * row, 147, 42);
            }

            _scoreRectangles = new Rectangle[6];
            for (int i = 0; i < _scoreRectangles.Length; i++)
            {
                _scoreRectangles[i] = new Rectangle(440, 16 * i, 23, 16);
            }

            _titleRectangle = new Rectangle(294, 217, 135, 24);

            _titleDestination = new Rectangle(
                (GreedyKidGame.Width - _titleRectangle.Width) / 2,
                (GreedyKidGame.Height - _backgroundRectangles[0].Height * 2) / 2 + _backgroundRectangles[0].Height * 2 + (GreedyKidGame.Height - _backgroundRectangles[0].Height * 2) / 4 - _titleRectangle.Height / 2,
                _titleRectangle.Width,
                _titleRectangle.Height
                );

            _scoreDestination = new Rectangle(
                (GreedyKidGame.Width - _scoreRectangles[0].Width * 2) / 2,
                16,
                _scoreRectangles[0].Width * 2,
                _scoreRectangles[0].Height * 2
                );

            _backgroundDestination = new Rectangle(
                (GreedyKidGame.Width - _backgroundRectangles[0].Width * 2) / 2,
                (GreedyKidGame.Height - _backgroundRectangles[0].Height * 2) / 2,
                _backgroundRectangles[0].Width * 2,
                _backgroundRectangles[0].Height * 2
                );
        }

        public void Update(float gameTime)
        {
            if (InputManager.CheckKeypress())
                SkipToTitle = true;

            _currentFrameTime += gameTime;
            if (_currentFrameTime > _frameTime)
            {
                _currentFrameTime -= _frameTime;

                _backgroundFrame++;
                _backgroundFrame %= _backgroundRectangles.Length;

                _oakFrame++;
                _oakFrame %= _oakFrameRectangles.Length;

                if (_scoreFrame >= 0)
                {
                    _scoreFrame++;
                    if (_scoreFrame == _scoreRectangles.Length)
                        _scoreFrame = -1;
                }

                if (_oakFrame == 9)
                {
                    _scoreFrame = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Splash;

            spriteBatch.Draw(texture,
                _backgroundDestination,
                _backgroundRectangles[_backgroundFrame],
                Color.White);

            spriteBatch.Draw(texture,
                _backgroundDestination,
                _oakFrameRectangles[_oakFrame],
                Color.White);

            if (_scoreFrame >= 0)
                spriteBatch.Draw(texture,
                    _scoreDestination,
                    _scoreRectangles[_scoreFrame],
                    Color.White);

            spriteBatch.Draw(texture,
                _titleDestination,
                _titleRectangle,
                Color.White);

            spriteBatch.End();
        }
    }
}

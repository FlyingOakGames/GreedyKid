// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public sealed class DisclaimerManager
    {
        public bool SkipToSplash = false;

        private Rectangle[] _rectangles;
        
        private Rectangle _1x1;

        private const float _timeBeforeRendering = 1.0f;
        private float _currentTimeBeforeRendering = 0.0f;
        private float _fadeInTime = 0.0f;
        private const float _fadeTime = 0.35f;
        private const float _maxDisclaimerTime = 3.5f;
        private float _currentDisclaimerTime = 0.0f;
        private float _fadeOutTime = -1.0f;

        public DisclaimerManager()
        {          
            _rectangles = new Rectangle[(int)Language.Count];
            for (int i = 0; i < _rectangles.Length; i++)
            {
                _rectangles[i] = new Rectangle(240 * i, 1644, 240, 101);
            }            

            _1x1 = new Rectangle(0, 1644, 1, 1);
        }

        public void Update(float gameTime)
        {
            if (_currentTimeBeforeRendering < _timeBeforeRendering)
            {
                _currentTimeBeforeRendering += gameTime;
                return;
            }

            _currentDisclaimerTime += gameTime;

            // fade in/out
            if (_fadeInTime < _fadeTime)
            {
                _fadeInTime += gameTime;
            }
            if (_fadeOutTime >= 0.0f)
            {
                _fadeOutTime -= gameTime;

                if (_fadeOutTime < 0.0f)
                {
                    _fadeOutTime = 0.0f; // force one full frame                    
                }
            }

            // skip
            if ((InputManager.CheckKeypress() || _currentDisclaimerTime >= _maxDisclaimerTime) && _fadeOutTime < 0.0f && SkipToSplash == false)
            {
                _fadeOutTime = _fadeTime;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_currentTimeBeforeRendering < _timeBeforeRendering)
            {
                return;
            }

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Ending1;

            // render
            spriteBatch.Draw(texture,
                new Rectangle(
                    (GreedyKidGame.Width - 240) / 2,
                    (GreedyKidGame.Height - 101) / 2,
                    240, 101),
                _rectangles[(int)TextManager.Instance.Language],
                Color.White);

            // fade in
            if (_fadeInTime < _fadeTime)
            {
                float amount = 1.0f - _fadeInTime / _fadeTime;

                spriteBatch.Draw(texture,
                    new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                    _1x1,
                    Color.White * amount);
            }
            // fade out
            if (_fadeOutTime >= 0.0f)
            {
                float amount = 1.0f - _fadeOutTime / _fadeTime;

                spriteBatch.Draw(texture,
                    new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                    _1x1,
                    Color.White * amount);

                if (_fadeOutTime == 0.0f)
                    SkipToSplash = true;
            }

            spriteBatch.End();
        }
    }
}

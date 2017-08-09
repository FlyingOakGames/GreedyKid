using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public sealed class IntroScreenManager
    {
        public bool StartGame = false;

        // animation
        private Rectangle[] _animationFrames;
        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;
        private const float _frameTime = 0.1f;

        private bool _waitForTransition = false;

        public IntroScreenManager()
        {
            _animationFrames = new Rectangle[219];

            for (int i = 0; i < _animationFrames.Length; i++)
            {
                int row = i / (TextureManager.IntroWidth / 204);
                int col = i % (TextureManager.IntroWidth / 204);

                _animationFrames[i] = new Rectangle(col * 204, row * 48, 204, 48);
            }
        }

        public void Reset()
        {
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            StartGame = false;
            _waitForTransition = false;
            TransitionManager.Instance.AppearTransition(129, 70);
        }

        public void SkipIntro()
        {
            _waitForTransition = true;
            TransitionManager.Instance.DisappearTransition(191, 70);
        }

        public void Update(float gameTime)
        {
            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.HandleIntroInputs(this);

            if (!SaveManager.Instance.IsLevelDone(-1))
                StartGame = false;

            if (_waitForTransition && TransitionManager.Instance.IsDone)
            {
                _waitForTransition = false;
                StartGame = true;
            }

            _currentFrameTime += gameTime;
            if (_currentFrameTime >= _frameTime)
            {
                _currentFrameTime -= _frameTime;
                _currentFrame++;

                if (_currentFrame == 96)
                {
                    SfxManager.Instance.Play(Sfx.Shout1 + RandomHelper.Next(5));
                    SfxManager.Instance.Play(Sfx.RKOH);
                    SfxManager.Instance.Play(Sfx.ElevatorOpen);
                }

                if (_currentFrame == 115 || _currentFrame == 118 || _currentFrame == 133 || _currentFrame == 140)
                    SfxManager.Instance.Play(Sfx.MoneyGrab);

                if (_currentFrame == 143 || _currentFrame == 147)
                    SfxManager.Instance.Play(Sfx.Taunt1);

                if (_currentFrame == 145 || _currentFrame == 149)
                    SfxManager.Instance.Play(Sfx.Taunt2);

                if (_currentFrame == _animationFrames.Length - 3)
                    SkipIntro();

                if (_currentFrame >= _animationFrames.Length)
                    _currentFrame = _animationFrames.Length - 1;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);            

            Texture2D textureIntro = TextureManager.Intro;

            spriteBatch.Draw(textureIntro,                
                new Rectangle(
                    (GreedyKidGame.Width - _animationFrames[_currentFrame].Width) / 2,
                    (GreedyKidGame.Height - _animationFrames[_currentFrame].Height) / 2,
                    _animationFrames[_currentFrame].Width,
                    _animationFrames[_currentFrame].Height),
                _animationFrames[_currentFrame],
                Color.White);

            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            UIHelper.Instance.DrawBorders(spriteBatch);
            spriteBatch.End();
        }
    }
}

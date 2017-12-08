using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public enum EndingType
    {
        Normal,
        Secret,
    }

    public sealed class EndingScreenManager
    {
        public bool ReturnToLevelSelection = false;

        // animation
        private Rectangle[] _backgrounds;
        private Rectangle[] _animationFrames;
        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;
        private const float _frameTime = 0.1f;

        private bool _waitForTransition = false;

        private EndingType _type = EndingType.Normal;

        private bool _waitForInnerTransition1 = false;
        private bool _waitForInnerTransition2 = false;

        public EndingType Ending
        {
            get { return _type; }
        }

        public EndingScreenManager()
        {
            _animationFrames = new Rectangle[250];

            // common part 0-118
            for (int i = 0; i < 119; i++)
            {
                int row = i / (TextureManager.EndingWidth / 280);
                int col = i % (TextureManager.EndingWidth / 280);

                _animationFrames[i] = new Rectangle(col * 280, row * 73, 280, 73);
            }

            // ending 1 119-141
            for (int i = 0; i < 23; i++)
            {
                int row = i / (TextureManager.EndingWidth / 280);
                int col = i % (TextureManager.EndingWidth / 280);

                _animationFrames[119 + i] = new Rectangle(col * 280, row * 73, 280, 73);
            }

            // ending 2
            // common part 142-211
            for (int i = 0; i < 70; i++)
            {
                int row = i / (TextureManager.EndingWidth / 280);
                int col = i % (TextureManager.EndingWidth / 280);

                _animationFrames[119 + 23 + i] = new Rectangle(col * 280, row * 73 + 4 * 73, 280, 73);
            }
            // common part 212-249
            for (int i = 0; i < 38; i++)
            {
                int row = i / (TextureManager.EndingWidth / 258);
                int col = i % (TextureManager.EndingWidth / 280);

                _animationFrames[119 + 23 + 70 + i] = new Rectangle(col * 258, row * 68 + 4 * 73 + 10 * 73, 258, 68);
            }

            _backgrounds = new Rectangle[2];
            _backgrounds[0] = new Rectangle(9, 1329, 310, 153);
            _backgrounds[1] = new Rectangle(350, 1377, 276, 66);
        }

        public void Reset(EndingType type)
        {
            _type = type;
            _currentFrame = 0;
            _currentFrameTime = 0.0f;
            ReturnToLevelSelection = false;
            _waitForTransition = false;
            _waitForInnerTransition1 = false;
            _waitForInnerTransition2 = false;
            TransitionManager.Instance.AppearTransition(129, 70);
        }

        public void Skip()
        {
            if (!_waitForTransition && !_waitForInnerTransition1 && !_waitForInnerTransition2)
            {
                _waitForTransition = true;
                TransitionManager.Instance.DisappearTransition(191, 70);
            }
        }

        public void Update(float gameTime)
        {
            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.HandleEndingInputs(this);

            // anti skip if first time
            if (!SaveManager.Instance.IsLevelDone(-1))
                ReturnToLevelSelection = false;

            // outro transition
            if (_waitForTransition && TransitionManager.Instance.IsDone)
            {
                _waitForTransition = false;
                ReturnToLevelSelection = true;
            }
            // inner transition
            if (_waitForInnerTransition1 && TransitionManager.Instance.IsDone)
            {
                _waitForInnerTransition1 = false;
                _waitForInnerTransition2 = false;
                TransitionManager.Instance.DisappearTransition(191, 70);
            }
            if (_waitForInnerTransition2 && TransitionManager.Instance.IsDone)
            {
                _waitForInnerTransition2 = false;
            }

            // frames
            _currentFrameTime += gameTime;
            if (_currentFrameTime >= _frameTime)
            {
                _currentFrameTime -= _frameTime;
                _currentFrame++;

                if (_currentFrame == 116 && _type == EndingType.Secret)
                {
                    _waitForInnerTransition1 = true;
                    TransitionManager.Instance.AppearTransition(129, 70);
                }
                if (_currentFrame == 119 && _type == EndingType.Secret)
                    _currentFrame = 142;

                /*
                if (_currentFrame == 96)
                {
                    SfxManager.Instance.Play(Sfx.Shout1 + RandomHelper.Next(5));
                    SfxManager.Instance.Play(Sfx.RKOH);
                    SfxManager.Instance.Play(Sfx.ElevatorOpen);
                }

                if (_currentFrame == 5 || _currentFrame == 21 || _currentFrame == 37 || _currentFrame == 53 || _currentFrame == 69 || _currentFrame == 85)
                    SfxManager.Instance.Play(Sfx.TV);

                if (_currentFrame == 115 || _currentFrame == 118 || _currentFrame == 133 || _currentFrame == 140)
                    SfxManager.Instance.Play(Sfx.MoneyGrab);

                if (_currentFrame == 143 || _currentFrame == 147)
                    SfxManager.Instance.Play(Sfx.Taunt1);

                if (_currentFrame == 145 || _currentFrame == 149)
                    SfxManager.Instance.Play(Sfx.Taunt2);

                if (_currentFrame == 173)
                    SfxManager.Instance.Play(Sfx.HealthPack);

                if (_currentFrame == 179)
                    SfxManager.Instance.Play(Sfx.CopTaser);

                if (_currentFrame == 196)
                    SfxManager.Instance.Play(Sfx.HeavyHit);
                */

                // end animation
                if (_type == EndingType.Normal && _currentFrame == 141 - 3)
                    Skip();
                else if (_type == EndingType.Secret && _currentFrame == _animationFrames.Length - 3)
                    Skip();

                if (_type == EndingType.Normal && _currentFrame >= 142)
                    _currentFrame = 141;
                else if (_type == EndingType.Secret && _currentFrame >= _animationFrames.Length)
                    _currentFrame = _animationFrames.Length - 1;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = (_currentFrame < 119 ? TextureManager.Ending1 : TextureManager.Ending2);

            spriteBatch.Draw(texture,
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

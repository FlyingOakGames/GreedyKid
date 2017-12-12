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

        private const int _commonPartCount = 136;
        private const int _endPart1Count = 47;
        private const int _endPart2ACount = 78;
        private const int _endPart2BCount = 63;

        private const int _part1Width = 280;
        private const int _part1Height = 73;
        private const int _part2BWidth = 258;
        private const int _part2BHeight = 68;

        public EndingType Ending
        {
            get { return _type; }
        }

        public EndingScreenManager()
        {
            _animationFrames = new Rectangle[_commonPartCount + _endPart1Count + _endPart2ACount + _endPart2BCount];

            // common part
            for (int i = 0; i < _commonPartCount; i++)
            {
                int row = i / (TextureManager.EndingWidth / _part1Width);
                int col = i % (TextureManager.EndingWidth / _part1Width);

                _animationFrames[i] = new Rectangle(col * _part1Width, row * _part1Height, _part1Width, _part1Height);
            }

            // ending 1
            for (int i = 0; i < _endPart1Count; i++)
            {
                int row = i / (TextureManager.EndingWidth / _part1Width);
                int col = i % (TextureManager.EndingWidth / _part1Width);

                _animationFrames[_commonPartCount + i] = new Rectangle(col * _part1Width, row * _part1Height, _part1Width, _part1Height);
            }

            // ending 2
            // 142-211
            int rowMargin = (int)System.Math.Ceiling(_endPart1Count / (float)(TextureManager.EndingWidth / _part1Width));
            for (int i = 0; i < _endPart2ACount; i++)
            {
                int row = i / (TextureManager.EndingWidth / _part1Width);
                int col = i % (TextureManager.EndingWidth / _part1Width);

                _animationFrames[_commonPartCount + _endPart1Count + i] = new Rectangle(col * _part1Width, row * _part1Height + rowMargin * _part1Height, _part1Width, _part1Height);
            }
            // 212-249
            rowMargin += (int)System.Math.Ceiling(_endPart2ACount / (float)(TextureManager.EndingWidth / _part1Width));
            for (int i = 0; i < _endPart2BCount; i++)
            {
                int row = i / (TextureManager.EndingWidth / _part2BWidth);
                int col = i % (TextureManager.EndingWidth / _part2BWidth);

                _animationFrames[_commonPartCount + _endPart1Count + _endPart2ACount + i] = new Rectangle(col * _part2BWidth, row * _part2BHeight + rowMargin * _part1Height, _part2BWidth, _part2BHeight);
            }

            _backgrounds = new Rectangle[2];
            _backgrounds[0] = new Rectangle(9, 1476  , 310, 153);
            _backgrounds[1] = new Rectangle(350, 1524, 279, 66);
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
            TransitionManager.Instance.AppearTransition(220, 86);
        }

        public void Skip()
        {
            if (!_waitForTransition && !_waitForInnerTransition1 && !_waitForInnerTransition2)
            {
                _waitForTransition = true;
                if (_type == EndingType.Secret)
                    TransitionManager.Instance.DisappearTransition(217, 80, TextManager.Instance.TheEnd);
                else
                    TransitionManager.Instance.DisappearTransition(248, 85, TextManager.Instance.TheEnd);
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
                _waitForInnerTransition2 = true;
                _currentFrame = _commonPartCount + _endPart1Count + _endPart2ACount;
                TransitionManager.Instance.AppearTransition(217, 80);
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

                if (_currentFrame == _commonPartCount + _endPart1Count + _endPart2ACount - 4 && _type == EndingType.Secret)
                {
                    _waitForInnerTransition1 = true;
                    TransitionManager.Instance.DisappearTransition(238, 86, TextManager.Instance.Later);
                }
                // jump to ending 2
                if (_currentFrame == _commonPartCount && _type == EndingType.Secret)
                    _currentFrame = _commonPartCount + _endPart1Count;

                // end animation
                if (_type == EndingType.Normal && _currentFrame == _commonPartCount + _endPart1Count - 4)
                    Skip();
                else if (_type == EndingType.Secret && _currentFrame == _animationFrames.Length - 3)
                    Skip();

                if (_type == EndingType.Normal && _currentFrame >= _commonPartCount + _endPart1Count)
                    _currentFrame = _commonPartCount + _endPart1Count - 1; // end of ending 1
                else if (_type == EndingType.Secret && _currentFrame >= _animationFrames.Length)
                    _currentFrame = _animationFrames.Length - 1; // end of ending 2
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = (_currentFrame < _commonPartCount ? TextureManager.Ending1 : TextureManager.Ending2);
            Texture2D bgTexture = TextureManager.Ending1;

            if (_currentFrame < _commonPartCount + _endPart1Count + _endPart2ACount)
            {
                spriteBatch.Draw(bgTexture,
                    new Rectangle(9, 16, _backgrounds[0].Width, _backgrounds[0].Height),
                    _backgrounds[0],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(24, 55, _animationFrames[_currentFrame].Width, _animationFrames[_currentFrame].Height),
                    _animationFrames[_currentFrame],
                    Color.White);
            }
            else
            {
                spriteBatch.Draw(bgTexture,
                    new Rectangle(22, 64, _backgrounds[1].Width, _backgrounds[1].Height),
                    _backgrounds[1],
                    Color.White);
                spriteBatch.Draw(texture,
                    new Rectangle(31, 63, _animationFrames[_currentFrame].Width, _animationFrames[_currentFrame].Height),
                    _animationFrames[_currentFrame],
                    Color.White);
            }

            spriteBatch.End();

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            UIHelper.Instance.DrawBorders(spriteBatch);
            spriteBatch.End();
        }
    }
}

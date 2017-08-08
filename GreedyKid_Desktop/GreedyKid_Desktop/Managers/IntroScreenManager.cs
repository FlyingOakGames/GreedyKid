using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public sealed class IntroScreenManager
    {
        public bool StartGame = false;

        // transition       
        private TransitionState _transitionState = TransitionState.Hidden;
        private int _currentTransitionFrame = 0;
        private float _currentTransitionFrameTime = 0.0f;
        private const float _transitionFrameTime = 0.1f;
        private int _transitionFocusX = 100;
        private int _transitionFocusY = 100;

        // animation
        private Rectangle[] _animationFrames;
        private int _currentFrame = 0;
        private float _currentFrameTime = 0.0f;
        private const float _frameTime = 0.1f;

        public IntroScreenManager()
        {
            _animationFrames = new Rectangle[165];

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
            _transitionState = TransitionState.Hidden;
            AppearTransition();
        }

        private void UpdateTransition(float gameTime)
        {
            if (_transitionState == TransitionState.Appearing)
            {
                _currentTransitionFrameTime += gameTime;

                if (_currentTransitionFrameTime >= _transitionFrameTime)
                {
                    _currentTransitionFrameTime -= _transitionFrameTime;

                    _currentTransitionFrame--;
                    if (_currentTransitionFrame < 0)
                    {
                        _transitionState = TransitionState.None;
                    }
                }
            }
            else if (_transitionState == TransitionState.Disappearing)
            {
                _currentTransitionFrameTime += gameTime;

                if (_currentTransitionFrameTime >= _transitionFrameTime)
                {
                    _currentTransitionFrameTime -= _transitionFrameTime;

                    _currentTransitionFrame++;
                    if (_currentTransitionFrame >= 3)
                    {
                        _currentTransitionFrame = 2;
                        _transitionState = TransitionState.Hidden;

                        StartGame = true;
                    }
                }
            }
        }

        public void DisappearTransition()
        {
            if (_transitionState == TransitionState.None && !StartGame)
            {
                _transitionFocusX = 191;
                _transitionFocusY = 70;
                _transitionState = TransitionState.Disappearing;
                _currentTransitionFrame = 0;
                _currentTransitionFrameTime = 0.0f;
            }
        }

        private void AppearTransition()
        {
            if (_transitionState == TransitionState.Hidden)
            {
                _transitionFocusX = 129;
                _transitionFocusY = 70;
                _transitionState = TransitionState.Appearing;
                _currentTransitionFrame = 2;
                _currentTransitionFrameTime = 0.0f;
            }
        }

        public void Update(float gameTime)
        {
            if (InputManager.PlayerDevice != null)
                InputManager.PlayerDevice.HandleIntroInputs(this);

            if (!SaveManager.Instance.IsLevelDone(-1))
                StartGame = false;

            // transition
            UpdateTransition(gameTime);

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
                    DisappearTransition();

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

            Texture2D textureTransition = TextureManager.Gameplay;
            // transition
            if (_transitionState != TransitionState.None)
            {
                Rectangle[] transitionRectangle = UIHelper.Instance.TransitionRectangles;

                if (_currentTransitionFrame == 2) // full
                {
                    spriteBatch.Draw(textureTransition,
                        new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                        transitionRectangle[0],
                        Color.White);
                }
                else
                {
                    spriteBatch.Draw(textureTransition,
                        new Rectangle(_transitionFocusX, _transitionFocusY, transitionRectangle[_currentTransitionFrame + 1].Width, transitionRectangle[_currentTransitionFrame + 1].Height),
                        transitionRectangle[_currentTransitionFrame + 1],
                        Color.White);

                    // background
                    int frame = 3;
                    if (_currentTransitionFrame == 1)
                        frame = 0;

                    // right
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX + transitionRectangle[_currentTransitionFrame + 1].Width,
                        _transitionFocusY,
                        transitionRectangle[3].Width,
                        transitionRectangle[3].Height),
                    transitionRectangle[frame],
                    Color.White);
                    // left
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX - transitionRectangle[3].Width,
                        _transitionFocusY + transitionRectangle[_currentTransitionFrame + 1].Height - transitionRectangle[3].Height,
                        transitionRectangle[3].Width,
                        transitionRectangle[3].Height),
                    transitionRectangle[frame],
                    Color.White);
                    // up
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX,
                        _transitionFocusY - transitionRectangle[3].Height,
                        transitionRectangle[3].Width,
                        transitionRectangle[3].Height),
                    transitionRectangle[frame],
                    Color.White);
                    // down
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX + transitionRectangle[_currentTransitionFrame + 1].Width - transitionRectangle[3].Width,
                        _transitionFocusY + transitionRectangle[_currentTransitionFrame + 1].Height,
                        transitionRectangle[3].Width,
                        transitionRectangle[3].Height),
                    transitionRectangle[frame],
                    Color.White);

                }
            }

            UIHelper.Instance.DrawBorders(spriteBatch);

            spriteBatch.End();
        }
    }
}

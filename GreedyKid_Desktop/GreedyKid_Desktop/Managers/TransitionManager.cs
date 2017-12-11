using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public enum TransitionState
    {
        None,
        Appearing,
        Disappearing,
        Hidden,
    }

    public sealed class TransitionManager
    {
        private static TransitionManager _instance;

        // transition       
        private TransitionState _transitionState = TransitionState.Hidden;
        private int _currentTransitionFrame = 0;
        private float _currentTransitionFrameTime = 0.0f;
        private const float _transitionFrameTime = 0.1f;
        private int _transitionFocusX = 100;
        private int _transitionFocusY = 100;
        private Rectangle[] _transitionRectangle;
        private bool _isDone = false;
        private string _text = null;
        private const int _textDuration = 30;

        public static TransitionManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TransitionManager();
                return _instance;
            }
        }

        private TransitionManager()
        {
            // transition
            _transitionRectangle = new Rectangle[7];
            _transitionRectangle[0] = new Rectangle(152, TextureManager.GameplayHeight - 130, 1, 1); // 1x1
            _transitionRectangle[2] = new Rectangle(150, TextureManager.GameplayHeight - 133, 50, 50); // circle half full
            _transitionRectangle[1] = new Rectangle(150, TextureManager.GameplayHeight - 184, 50, 50); // circle empty            
            _transitionRectangle[3] = new Rectangle(201, TextureManager.GameplayHeight - 184, 328, 184); // half full

            _transitionRectangle[4] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 13); // gameover 1
            _transitionRectangle[5] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 28); // gameover 2
            _transitionRectangle[6] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 82); // gameover 3
        }

        public bool IsDone
        {
            get
            {
                bool val = _isDone;
                if (_isDone)
                    _isDone = false;
                return val;
            }
        }

        public void Update(float gameTime)
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
                        _isDone = true;
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
                    if ((_currentTransitionFrame >= 3 && _text == null)
                        || (_currentTransitionFrame >= 3 + _textDuration && _text != null))
                    {
                        _currentTransitionFrame = 2;
                        _transitionState = TransitionState.None; // should be hidden
                        _isDone = true;
                    }
                }
            }
        }

        public void DisappearTransition(int focusX = 0, int focusY = 0, string text = null)
        {
            if (focusX == 0 && focusY == 0)
            {
                focusX = (GreedyKidGame.Width - _transitionRectangle[1].Width) / 2;
                focusY = (GreedyKidGame.Height - _transitionRectangle[1].Height) / 2;
            }

            _isDone = false;
            _transitionFocusX = focusX;
            _transitionFocusY = focusY;
            _transitionState = TransitionState.Disappearing;
            _currentTransitionFrame = 0;
            _currentTransitionFrameTime = 0.0f;

            _text = text;
        }

        public void AppearTransition(int focusX = 0, int focusY = 0)
        {
            if (focusX == 0 && focusY == 0)
            {
                focusX = (GreedyKidGame.Width - _transitionRectangle[1].Width) / 2;
                focusY = (GreedyKidGame.Height - _transitionRectangle[1].Height) / 2;
            }

            _isDone = false;
            _transitionFocusX = focusX;
            _transitionFocusY = focusY;
            _transitionState = TransitionState.Appearing;
            _currentTransitionFrame = 2;
            _currentTransitionFrameTime = 0.0f;

            _text = null;
        }

        public void ForceAppear()
        {
            _isDone = true;
            _transitionState = TransitionState.None;
            _currentTransitionFrame = 2;
        }

        public void Draw(SpriteBatch spriteBatch)
        {            
            // transition
            if (_transitionState != TransitionState.None)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointWrap);

                Texture2D textureTransition = TextureManager.Gameplay;

                if (_currentTransitionFrame >= 2) // full
                {
                    spriteBatch.Draw(textureTransition,
                        new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                        _transitionRectangle[0],
                        Color.White);

                    if (_text != null)
                    {
                        SpriteFont font = TextManager.Instance.Font;
                        Vector2 textSize = font.MeasureString(_text);
                        int textWidth = (int)textSize.X;
                        int textHeight = (int)textSize.Y;

                        spriteBatch.DrawString(font, _text,
                            new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, GreedyKidGame.Height / 2 - textHeight / 2),
                            Color.White);
                    }
                }
                else
                {
                    spriteBatch.Draw(textureTransition,
                        new Rectangle(_transitionFocusX, _transitionFocusY, _transitionRectangle[_currentTransitionFrame + 1].Width, _transitionRectangle[_currentTransitionFrame + 1].Height),
                        _transitionRectangle[_currentTransitionFrame + 1],
                        Color.White);

                    // background
                    int frame = 3;
                    if (_currentTransitionFrame == 1)
                        frame = 0;

                    // right
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX + _transitionRectangle[_currentTransitionFrame + 1].Width,
                        _transitionFocusY,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);
                    // left
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX - _transitionRectangle[3].Width,
                        _transitionFocusY + _transitionRectangle[_currentTransitionFrame + 1].Height - _transitionRectangle[3].Height,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);
                    // up
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX,
                        _transitionFocusY - _transitionRectangle[3].Height,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);
                    // down
                    spriteBatch.Draw(textureTransition,
                    new Rectangle(_transitionFocusX + _transitionRectangle[_currentTransitionFrame + 1].Width - _transitionRectangle[3].Width,
                        _transitionFocusY + _transitionRectangle[_currentTransitionFrame + 1].Height,
                        _transitionRectangle[3].Width,
                        _transitionRectangle[3].Height),
                    _transitionRectangle[frame],
                    Color.White);

                }

                spriteBatch.End();
            }            
        }
    }
}

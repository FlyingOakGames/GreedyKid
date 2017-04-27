using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public enum TitleScreenState
    {
        Title,
        Main,
        Play,
        LevelSelection,
        SteamWorkshop,
        Options,
    }

    public sealed class TitleScreenManager
    {
        public bool StartGame = false;

        private Rectangle _viewport;
        private Rectangle[] _titleRectangles;

        private TitleScreenState _state = TitleScreenState.Title;

        public TitleScreenManager()
        {
            _viewport = new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height);

            _titleRectangles = new Rectangle[2];
            for (int i = 0; i < _titleRectangles.Length; i++)
            {
                _titleRectangles[i] = new Rectangle(898 + GreedyKidGame.Width * i, TextureManager.GameplayHeight - GreedyKidGame.Height, GreedyKidGame.Width, GreedyKidGame.Height);
            }
        }

        public void Update(float gameTime)
        {
            if (_state == TitleScreenState.Title && InputManager.CheckEngagement())
            {
                _state = TitleScreenState.Main;
            }
            else if (InputManager.PlayerDevice != null && _state != TitleScreenState.Title)
            {
                InputManager.PlayerDevice.HandleTitleInputs(this);
            }            
        }

        public void PushStart()
        {
            StartGame = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Gameplay;

            spriteBatch.Draw(texture,
                _viewport,
                _titleRectangles[(_state == TitleScreenState.Title ? 0 : 1)],
                Color.White);

            spriteBatch.End();
        }
    }
}

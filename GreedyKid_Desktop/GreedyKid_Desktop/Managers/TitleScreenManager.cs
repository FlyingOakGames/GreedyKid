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

            // background
            spriteBatch.Draw(texture,
                _viewport,
                _titleRectangles[(_state == TitleScreenState.Title ? 0 : 1)],
                Color.White);

            

            // text
            if (_state == TitleScreenState.Main)
            {
                int yStart = 125;

                DrawCenteredText(spriteBatch, TextManager.Instance.Play, yStart);
                DrawCenteredText(spriteBatch, TextManager.Instance.Settings, yStart + 15);
                DrawCenteredText(spriteBatch, TextManager.Instance.Quit, yStart + 30);
            }
            else if (_state == TitleScreenState.Play)
            {
                int yStart = 125;

                DrawCenteredText(spriteBatch, TextManager.Instance.Campaign, yStart);
                DrawCenteredText(spriteBatch, TextManager.Instance.Workshop, yStart + 15);
                DrawCenteredText(spriteBatch, TextManager.Instance.Back, yStart + 30);
            }            

            spriteBatch.End();
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, int yPos)
        {
            SpriteFont font = TextManager.Instance.Font;

            int textWidth = (int)font.MeasureString(text).X;
            spriteBatch.DrawString(font, text, new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, yPos), Color.White);
        }
    }
}

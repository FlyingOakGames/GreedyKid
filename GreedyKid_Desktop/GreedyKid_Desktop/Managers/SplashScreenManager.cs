using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public sealed class SplashScreenManager
    {
        public bool SkipToTitle = false;

        public SplashScreenManager(ContentManager content)
        {
            TextureManager.LoadSplash(content);
        }

        public void Update(float gameTime)
        {
            if (InputManager.CheckKeypress())
                SkipToTitle = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            Texture2D texture = TextureManager.Splash;

            spriteBatch.Draw(texture,
                new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                new Rectangle(0, 0, GreedyKidGame.Width, GreedyKidGame.Height),
                Color.White);

            spriteBatch.End();
        }
    }
}

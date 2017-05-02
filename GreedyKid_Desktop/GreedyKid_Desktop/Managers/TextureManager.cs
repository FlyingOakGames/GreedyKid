using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public static class TextureManager
    {
        public const int GameplayWidth = 2048;
        public const int GameplayHeight = 2048;

        public static Texture2D Gameplay;
        public static Texture2D Splash;

        public static SpriteFont LatinFont;

        public static ContentManager Content
        {
            get;
            set;
        }

        public static void LoadGameplay()
        {
            if (Content != null)
                Gameplay = Content.Load<Texture2D>(@"Textures/level");
        }

        public static void LoadSplash()
        {
            if (Content != null)
                Splash = Content.Load<Texture2D>(@"Textures/splash");
        }

        public static void LoadLatinFont()
        {
            if (Content != null)
            {
                LatinFont = Content.Load<SpriteFont>(@"Fonts/latin");
                LatinFont.Spacing = 1.0f;
            }
        }

        public static void LoadFont()
        {
            LoadLatinFont();
        }
    }
}

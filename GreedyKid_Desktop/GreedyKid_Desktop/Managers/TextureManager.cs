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

        public static void LoadGameplay(ContentManager content)
        {
            Gameplay = content.Load<Texture2D>(@"Textures/level");
        }

        public static void LoadSplash(ContentManager content)
        {
            Splash = content.Load<Texture2D>(@"Textures/splash");
        }
    }
}

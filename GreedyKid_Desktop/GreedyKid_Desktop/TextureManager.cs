using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GreedyKid
{
    public static class TextureManager
    {
        public static Texture2D Building;
        public static Texture2D Splash;

        public static void LoadBuilding(ContentManager content)
        {
            Building = content.Load<Texture2D>(@"Textures/level");
        }

        public static void LoadSplash(ContentManager content)
        {
            Splash = content.Load<Texture2D>(@"Textures/splash");
        }
    }
}

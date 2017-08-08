using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace GreedyKid
{
    public static class TextureManager
    {
        public const int GameplayWidth = 2048;
        public const int GameplayHeight = 2048;

        public const int IntroWidth = 2048;
        public const int IntroHeight = 1024;

        public static Texture2D Gameplay;
        public static Texture2D Intro;
        public static Texture2D Splash;

        public static SpriteFont LatinFont;

        public static ContentManager Content
        {
            get;
            set;
        }

        public static void LoadGameplay(GraphicsDevice device)
        {
            if (Content != null)
            {
                try
                {
                    Gameplay = Content.Load<Texture2D>(@"Textures/level");
                }
                catch (Exception)
                {
                    // development fallback
                    string path = Content.RootDirectory + "/Textures/";
                    if (!File.Exists(path + "level.png")) // MacOS hack
                    {
                        path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "Content/Textures/");
                    }
                    Gameplay = Texture2D.FromStream(device, File.OpenRead(path + "level.png"));
                }
            }
        }

        public static void LoadIntro(GraphicsDevice device)
        {
            if (Content != null)
            {
                try
                {
                    Intro = Content.Load<Texture2D>(@"Textures/intro");
                }
                catch (Exception)
                {
                    // development fallback
                    string path = Content.RootDirectory + "/Textures/";
                    if (!File.Exists(path + "intro.png")) // MacOS hack
                    {
                        path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "Content/Textures/");
                    }
                    Intro = Texture2D.FromStream(device, File.OpenRead(path + "intro.png"));
                }
            }
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

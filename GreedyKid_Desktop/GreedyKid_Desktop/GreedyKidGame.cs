using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GreedyKid
{
    public enum GameState
    {
        None,
        Splash,
        Title,
        Ingame,
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GreedyKidGame : Game
    {
        public static bool ShouldExit = false;

        // virtual resolution
        public const int Width = 328; // 312
        public const int Height = 184; // 176

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private GameState _state = GameState.None;

        // managers
        private SplashScreenManager _splashScreenManager;
        private TitleScreenManager _titleScreenManager;
        private GameplayManager _gameplayManager;

        // viewport handling
        private Rectangle _destination = new Rectangle();
        private RenderTarget2D _renderTarget;
        private Color _fillColor = new Color(34, 32, 52);

#if DEBUG
        KeyboardState previousKeyboardState;
#endif

        public GreedyKidGame()
        {
            // settings init
            SettingsManager SettingsManager = SettingsManager.Instance;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.AllowUserResizing = true;

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);            

            _renderTarget = new RenderTarget2D(GraphicsDevice, Width, Height);

            TextureManager.Content = Content;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (_gameplayManager != null)
                _gameplayManager.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ShouldExit)
                Exit();
#if DEBUG
            // should remove once menus are in
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyboardState = Keyboard.GetState();

            if (_state == GameState.Ingame && _gameplayManager != null)
            {
                if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
                    _gameplayManager.ResetLevel();

                if (keyboardState.IsKeyDown(Keys.V) && previousKeyboardState.IsKeyUp(Keys.V))
                    _gameplayManager.NextLevel();
                if (keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C))
                    _gameplayManager.PreviousLevel();

                if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
                    _gameplayManager.SpawnCop();
            }

            previousKeyboardState = keyboardState;
#endif

            float gameTimeF = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (_state)
            {
                case GameState.None:

                    _splashScreenManager = new SplashScreenManager();

                    _state = GameState.Splash;

                    break;
                case GameState.Splash:

                    _splashScreenManager.Update(gameTimeF);

                    if (_splashScreenManager.SkipToTitle)
                    {
                        Content.Unload();

                        TextureManager.LoadGameplay();
                        TextureManager.LoadFont();

                        _titleScreenManager = new TitleScreenManager();

                        _state = GameState.Title;

                        _splashScreenManager = null;

                        GC.Collect();
                    }

                    break;
                case GameState.Title:

                    _titleScreenManager.Update(gameTimeF);

                    if (_titleScreenManager.StartGame)
                    {
                        _gameplayManager = new GameplayManager();
                        _gameplayManager.LoadBuilding();

                        _titleScreenManager = null;

                        _state = GameState.Ingame;

                        GC.Collect();
                    }

                    break;
                case GameState.Ingame:

                    _gameplayManager.Update(gameTimeF);

                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(_fillColor);
       
            // do all the rendering here            
            switch (_state)
            {
                case GameState.Splash:
                    _splashScreenManager.Draw(spriteBatch);
                    break;
                case GameState.Title:
                    _titleScreenManager.Draw(spriteBatch);
                    break;
                case GameState.Ingame:
                    _gameplayManager.Draw(spriteBatch);
                    break;
            }

            // final rendering
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(_fillColor);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            float targetRatio = Width / (float)Height;
            float gameRatio = GraphicsDevice.Viewport.Bounds.Width / (float)GraphicsDevice.Viewport.Bounds.Height;

            if (gameRatio > targetRatio)
            {
                // black border width
                _destination.Width = (int)(GraphicsDevice.Viewport.Bounds.Height * targetRatio);
                _destination.Height = GraphicsDevice.Viewport.Bounds.Height;
                _destination.X = (GraphicsDevice.Viewport.Bounds.Width - _destination.Width) / 2;
                _destination.Y = 0;                
            }
            else if (gameRatio < targetRatio)
            {
                // black border height
                _destination.Width = GraphicsDevice.Viewport.Bounds.Width;
                _destination.Height = (int)(GraphicsDevice.Viewport.Bounds.Width / targetRatio);
                _destination.X = 0;
                _destination.Y = (GraphicsDevice.Viewport.Bounds.Height - _destination.Height) / 2;
            }
            else
            {
                _destination = GraphicsDevice.Viewport.Bounds;
            }

            spriteBatch.Draw(_renderTarget, _destination, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

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
        public const string Version = "V.0";

        public static bool ShouldExit = false;
        public static bool ShouldApplyChanges = false;

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
            // text init (default language)
            TextManager textManager = TextManager.Instance;
            // settings init
            SettingsManager settingsManager = SettingsManager.Instance;
            // init UI helper
            UIHelper uiHelper = UIHelper.Instance;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if DESKTOP
            settingsManager.Load();

            Window.AllowUserResizing = true;

            if (settingsManager.ResolutionX == -1 || settingsManager.ResolutionY == -1)
            {
                settingsManager.SetResolution(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            }

            graphics.PreferredBackBufferWidth = settingsManager.ResolutionX;
            graphics.PreferredBackBufferHeight = settingsManager.ResolutionY;
            switch(settingsManager.FullScreenMode)
            {
                case FullScreenMode.No: graphics.IsFullScreen = false; graphics.HardwareModeSwitch = false; break;
                case FullScreenMode.Bordeless: graphics.HardwareModeSwitch = false; graphics.IsFullScreen = true; break;
                case FullScreenMode.Real: graphics.HardwareModeSwitch = true; graphics.IsFullScreen = true; break;
            }

            graphics.ApplyChanges();

            ShouldApplyChanges = false;
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // microphone init
            MicrophoneManager.Instance.SetMicrophone(SettingsManager.Instance.SelectedMicrophone);

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
            MicrophoneManager.Instance.Dispose();
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

            if (ShouldApplyChanges)
            {
                ShouldApplyChanges = false;
#if DESKTOP
                SettingsManager settingsManager = SettingsManager.Instance;
                if (settingsManager.ResolutionX != graphics.PreferredBackBufferWidth ||
                    settingsManager.ResolutionY != graphics.PreferredBackBufferHeight ||
                    graphics.IsFullScreen != (settingsManager.FullScreenMode != FullScreenMode.No) ||
                    graphics.HardwareModeSwitch != (settingsManager.FullScreenMode == FullScreenMode.Real))
                {
                    graphics.PreferredBackBufferWidth = settingsManager.ResolutionX;
                    graphics.PreferredBackBufferHeight = settingsManager.ResolutionY;
                    switch (settingsManager.FullScreenMode)
                    {
                        case FullScreenMode.No: graphics.IsFullScreen = false; graphics.HardwareModeSwitch = false; break;
                        case FullScreenMode.Bordeless: graphics.HardwareModeSwitch = false; graphics.IsFullScreen = true; break;
                        case FullScreenMode.Real: graphics.HardwareModeSwitch = true; graphics.IsFullScreen = true; break;
                    }
                    graphics.ApplyChanges();
                }
#endif
            }
#if DEBUG
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

            MicrophoneManager.Instance.Update(gameTimeF);

#if DESKTOP
            InputManager.CheckNewGamepad();
#endif

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

                    if (_gameplayManager.ReturnToLevelSelection)
                    {
                        _titleScreenManager = new TitleScreenManager();
                        _titleScreenManager.SetState(TitleScreenState.LevelSelection);

                        _gameplayManager = null;

                        _state = GameState.Title;

                        GC.Collect();
                    }

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

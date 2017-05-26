using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        public const string Version = "V.2";

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
        public static Rectangle Viewport = new Rectangle();
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
            // init sfx
            SfxManager sfxManager = SfxManager.Instance;

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
            SfxManager.Content = Content;
            ContentManager musicContent = new ContentManager(this.Services);
            musicContent.RootDirectory = Content.RootDirectory;
            MusicManager.Content = musicContent;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            MicrophoneManager.Instance.Dispose();
            Content.Unload();
            MusicManager.Instance.Unload();
            base.UnloadContent();
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
                if (keyboardState.IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyUp(Keys.F1))
                    _gameplayManager.ResetLevel();

                if (keyboardState.IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyUp(Keys.F2))
                    _gameplayManager.NextLevel();
                if (keyboardState.IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyUp(Keys.F3))
                    _gameplayManager.PreviousLevel();

                if (keyboardState.IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyUp(Keys.F4))
                    _gameplayManager.SpawnCop();
            }

            previousKeyboardState = keyboardState;
#endif

            float gameTimeF = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MicrophoneManager.Instance.Update(gameTimeF);
            MusicManager.Instance.Update(gameTimeF);

#if DESKTOP
            InputManager.CheckNewGamepad();
#endif

            switch (_state)
            {
                case GameState.None:

                    MusicManager.Instance.LoadSong(1);
                    MusicManager.Instance.Play();

                    _splashScreenManager = new SplashScreenManager();

                    _state = GameState.Splash;

                    break;
                case GameState.Splash:

                    _splashScreenManager.Update(gameTimeF);

                    if (_splashScreenManager.SkipToTitle)
                    {
                        SfxManager.Instance.Unload();

                        Content.Unload();

                        TextureManager.Splash = null;

                        TextureManager.LoadGameplay();
                        TextureManager.LoadFont();
                        SfxManager.Instance.LoadGameplaySfx();

                        _titleScreenManager = new TitleScreenManager();

                        _state = GameState.Title;

                        _splashScreenManager = null;

                        GC.Collect();
                    }

                    break;
                case GameState.Title:

                    _titleScreenManager.Update(gameTimeF);

                    if (_titleScreenManager.ShouldLoadBuilding)
                    {
                        if (_gameplayManager == null || _gameplayManager.BuildingIdentifier != _titleScreenManager.RequiredBuildingIdentifier)
                        {
                            _gameplayManager = new GameplayManager();
                            _gameplayManager.LoadBuilding(_titleScreenManager.RequiredBuildingIdentifier);
                        }

                        _titleScreenManager.ShouldLoadBuilding = false;

                        GC.Collect();
                    }
                    else if (_titleScreenManager.StartGame)
                    {                       
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
                Viewport.Width = (int)(GraphicsDevice.Viewport.Bounds.Height * targetRatio);
                Viewport.Height = GraphicsDevice.Viewport.Bounds.Height;
                Viewport.X = (GraphicsDevice.Viewport.Bounds.Width - Viewport.Width) / 2;
                Viewport.Y = 0;                
            }
            else if (gameRatio < targetRatio)
            {
                // black border height
                Viewport.Width = GraphicsDevice.Viewport.Bounds.Width;
                Viewport.Height = (int)(GraphicsDevice.Viewport.Bounds.Width / targetRatio);
                Viewport.X = 0;
                Viewport.Y = (GraphicsDevice.Viewport.Bounds.Height - Viewport.Height) / 2;
            }
            else
            {
                Viewport = GraphicsDevice.Viewport.Bounds;
            }

            spriteBatch.Draw(_renderTarget, Viewport, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

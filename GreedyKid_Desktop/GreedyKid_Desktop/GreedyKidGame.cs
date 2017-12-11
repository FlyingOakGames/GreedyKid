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
        Intro,
        Ending,
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GreedyKidGame : Game
    {
        public const string Version = "V.5";

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
        private IntroScreenManager _introScreenManager;
        private EndingScreenManager _endingScreenManager;
        private GameplayManager _gameplayManager;

        // viewport handling
        public static Rectangle Viewport = new Rectangle();
        private RenderTarget2D _renderTarget;
        private Color _fillColor = new Color(34, 32, 52);

#if SFX_DEBUG
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
#if SFX_DEBUG
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.F5) && previousKeyboardState.IsKeyUp(Keys.F5))
            {
                SfxManager.Instance.LoadGameplaySfx(true);
                GC.Collect();
            }

            previousKeyboardState = keyboardState;
#endif

            float gameTimeF = (float)gameTime.ElapsedGameTime.TotalSeconds;

            MicrophoneManager.Instance.Update(gameTimeF);
            MusicManager.Instance.Update(gameTimeF);

#if DESKTOP
            InputManager.CheckNewGamepad();
#endif

            TransitionManager.Instance.Update(gameTimeF);

            switch (_state)
            {
                case GameState.None:
                    TextureManager.LoadGameplay(GraphicsDevice);
                    TextureManager.LoadIntro(GraphicsDevice);
                    TextureManager.LoadEnding(GraphicsDevice);
                    TextureManager.LoadFont();
                    SfxManager.Instance.LoadGameplaySfx();                    

                    _splashScreenManager = new SplashScreenManager();

                    MusicManager.Instance.LoadSong(1);
                    MusicManager.Instance.Play();

                    _state = GameState.Splash;                        
                    break;
                case GameState.Splash:

                    _splashScreenManager.Update(gameTimeF);

                    if (_splashScreenManager.SkipToTitle)
                    {
                        //SfxManager.Instance.Unload();

                        //Content.Unload();

                        //TextureManager.Splash = null;
                       
                        _titleScreenManager = new TitleScreenManager();
                        _introScreenManager = new IntroScreenManager();
                        _endingScreenManager = new EndingScreenManager();

                        _state = GameState.Title;

                        _splashScreenManager = null;

                        GC.Collect();

                        TransitionManager.Instance.ForceAppear();
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
                            _titleScreenManager.SetBuilding(_gameplayManager.Building);
                        }

                        _titleScreenManager.ShouldLoadBuilding = false;

                        GC.Collect();
                    }
                    else if (_titleScreenManager.StartGame)
                    {
                        if (_titleScreenManager.SelectedLevel >= _gameplayManager.Building.LevelCount)
                        {
                            if (_titleScreenManager.SelectedLevel == _gameplayManager.Building.LevelCount)
                                _endingScreenManager.Reset(EndingType.Normal);
                            else
                                _endingScreenManager.Reset(EndingType.Secret);
                            _state = GameState.Ending;
                        }
                        else if (_titleScreenManager.SelectedLevel >= 0)
                        {
                            _gameplayManager.LoadLevel(_titleScreenManager.SelectedLevel);                            
                            _state = GameState.Ingame;
                        }
                        else if (_titleScreenManager.SelectedLevel == -1)
                        {
                            _introScreenManager.Reset();
                            _state = GameState.Intro;
                        }

                        _gameplayManager.IsWorkshopBuilding = _titleScreenManager.IsWorkshopBuilding;
                        _titleScreenManager = null;                                              

                        GC.Collect();
                    }

                    break;
                case GameState.Intro:

                    _introScreenManager.Update(gameTimeF);

                    if (_introScreenManager.StartGame)
                    {
                        SaveManager.Instance.SetIntro();
                        SaveManager.Instance.Save(_gameplayManager.Building);
                        _gameplayManager.LoadLevel(0);
                        _state = GameState.Ingame;

                        GC.Collect();
                    }

                    break;
                case GameState.Ending:

                    _endingScreenManager.Update(gameTimeF);

                    if (_endingScreenManager.ReturnToLevelSelection)
                    {
                        SaveManager.Instance.SetEnding1();
                        if (_endingScreenManager.Ending == EndingType.Secret)
                            SaveManager.Instance.SetEnding2();
                         
                        SaveManager.Instance.Save(_gameplayManager.Building);

                        _titleScreenManager = new TitleScreenManager();
                        _titleScreenManager.SetState(TitleScreenState.LevelSelection);
                        _titleScreenManager.SetBuilding(_gameplayManager.Building, (_endingScreenManager.Ending == EndingType.Normal ? _gameplayManager.Building.LevelCount : _gameplayManager.Building.LevelCount + 1));

                        _titleScreenManager.IsWorkshopBuilding = _gameplayManager.IsWorkshopBuilding;

                        _state = GameState.Title;

                        GC.Collect();

                        TransitionManager.Instance.AppearTransition();
                    }

                    break;
                case GameState.Ingame:

                    _gameplayManager.Update(gameTimeF);

                    if (_gameplayManager.ReturnToLevelSelection)
                    {
                        _gameplayManager.ReturnToLevelSelection = false;

                        _titleScreenManager = new TitleScreenManager();
                        _titleScreenManager.SetState(TitleScreenState.LevelSelection);
                        _titleScreenManager.SetBuilding(_gameplayManager.Building, _gameplayManager.SelectedLevel);

                        _titleScreenManager.IsWorkshopBuilding = _gameplayManager.IsWorkshopBuilding;

                        _state = GameState.Title;

                        GC.Collect();

                        TransitionManager.Instance.AppearTransition();
                    }
                    else if (_gameplayManager.GoToEnding)
                    {
                        _gameplayManager.GoToEnding = false;

                        if (SaveManager.Instance.HasAllStars())
                            _endingScreenManager.Reset(EndingType.Secret);
                        else
                            _endingScreenManager.Reset(EndingType.Normal);
                        _state = GameState.Ending;
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
                    // transition
                    TransitionManager.Instance.Draw(spriteBatch);
                    break;
                case GameState.Ingame:
                    _gameplayManager.Draw(spriteBatch);
                    // transition
                    TransitionManager.Instance.Draw(spriteBatch);
                    break;
                case GameState.Intro:                
                    _introScreenManager.Draw(spriteBatch);
                    // transition
                    TransitionManager.Instance.Draw(spriteBatch);
                    break;
                case GameState.Ending:
                    _endingScreenManager.Draw(spriteBatch);
                    // transition
                    TransitionManager.Instance.Draw(spriteBatch);
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

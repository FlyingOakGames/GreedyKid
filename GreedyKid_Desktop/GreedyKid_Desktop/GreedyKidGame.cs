using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        // virtual resolution
        public const int Width = 328; // 312
        public const int Height = 184; // 176

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        private GameState _state = GameState.None;
        
        // gameplay
        private BuildingManager _buildingManager;

        // viewport handling
        private Rectangle _destination = new Rectangle();
        private RenderTarget2D _renderTarget;
        private Color _fillColor = new Color(34, 32, 52);

#if DEBUG
        KeyboardState previousKeyboardState;
#endif

        public GreedyKidGame()
        {
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


            _buildingManager = new BuildingManager();
            _buildingManager.LoadBuilding();

            TextureManager.LoadBuilding(Content);
            TextureManager.LoadSplash(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (_buildingManager != null)
                _buildingManager.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // should remove once menus are in
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#if DEBUG
            KeyboardState keyboardState = Keyboard.GetState();

            if (_state == GameState.Ingame && _buildingManager != null)
            {
                if (keyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
                    _buildingManager.ResetLevel();

                if (keyboardState.IsKeyDown(Keys.V) && previousKeyboardState.IsKeyUp(Keys.V))
                    _buildingManager.NextLevel();
                if (keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C))
                    _buildingManager.PreviousLevel();

                if (keyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
                    _buildingManager.SpawnCop();
            }

            previousKeyboardState = keyboardState;
#endif

            float gameTimeF = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (InputManager.PlayerDevice == null)
                InputManager.CheckEngagement();

            if (InputManager.PlayerDevice != null)
                _buildingManager.Update(gameTimeF);

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

            _buildingManager.Draw(spriteBatch);





            // final rendering
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

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

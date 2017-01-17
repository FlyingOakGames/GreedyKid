﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GreedyKidEditor
{
    class PreviewRenderer : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        IntPtr _handle;
        MainWindow _w;

        System.Windows.Forms.Form _form;

        public static int Width = 312;
        public static int Height = 176;

        Texture2D _levelTexture;
        
        Rectangle[][][] _floorRectangle;

        Color _fillColor = new Color(34, 32, 52);

        Building _building;

        public int SelectedLevel = -1;

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {
            args.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = _handle;
        }

        public PreviewRenderer(IntPtr handle, MainWindow w, Building b)
        {
            _building = b;
            _w = w;
            _handle = handle;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += OnPreparingDeviceSettings;
            Content.RootDirectory = "Content";
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 33);

            graphics.PreferredBackBufferHeight = Width;
            graphics.PreferredBackBufferWidth = Height;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // force hide the hosted window
            _form = System.Windows.Forms.Control.FromHandle(Window.Handle) as System.Windows.Forms.Form;
            _form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            _form.ShowInTaskbar = false;
            _form.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            _form.Location = new System.Drawing.Point(-4000, -4000);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            using (System.IO.FileStream file = System.IO.File.OpenRead("Content\\Textures\\level.png"))
            {
                _levelTexture = Texture2D.FromStream(this.GraphicsDevice, file);
            }



            _floorRectangle = new Rectangle[Floor.PaintCount][][]; // colors

            for (int p = 0; p < Floor.PaintCount; p++)
            {
                _floorRectangle[p] = new Rectangle[Floor.DecorationCount][]; // decoration

                for (int d = 0; d < Floor.DecorationCount; d++)
                {
                    _floorRectangle[p][d] = new Rectangle[]
                    {
                        new Rectangle(56 * d, 48 * p, 24, 48), // left
                        new Rectangle(24 + 56 * d, 48 * p, 8, 48), // fill
                        new Rectangle(24 + 8 + 56 * d, 48 * p, 24, 48), // right
                    };
                }
            }
        }

        protected override void UnloadContent()
        {            
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _form.Location = new System.Drawing.Point(-4000, -4000);
            if (Width != graphics.PreferredBackBufferWidth || Height != graphics.PreferredBackBufferHeight)
            {
                graphics.PreferredBackBufferHeight = Height;
                graphics.PreferredBackBufferWidth = Width;
                graphics.ApplyChanges();                
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_fillColor);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            if (SelectedLevel >= 0 && SelectedLevel < _building.Levels.Count)
            {
                for (int f = 0; f < _building.Levels[SelectedLevel].Floors.Count; f++)
                {
                    Floor floor = _building.Levels[SelectedLevel].Floors[f];

                    // background
                    int startX = 16 + floor.LeftMargin * 8;
                    int nbSlice = 35 - floor.LeftMargin - floor.RightMargin;

                    Rectangle source = _floorRectangle[floor.BackgroundColor][0][1];

                    for (int s = 0; s < nbSlice; s++)
                    {
                        spriteBatch.Draw(_levelTexture,
                            new Rectangle(startX + 8 * s, 128 - 40 * f, source.Width, source.Height),
                            source,
                            Color.White);
                    }

                    // left decoration
                    source = _floorRectangle[floor.BackgroundColor][floor.LeftDecoration][0];

                    spriteBatch.Draw(_levelTexture,
                        new Rectangle(floor.LeftMargin * 8, 128 - 40 *f, source.Width, source.Height),
                        source,
                        Color.White);

                    // right decoration
                    source = _floorRectangle[floor.BackgroundColor][floor.RightDecoration][2];

                    spriteBatch.Draw(_levelTexture,
                        new Rectangle(288 - floor.RightMargin  * 8, 128 - 40 * f, source.Width, source.Height),
                        source,
                        Color.White);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

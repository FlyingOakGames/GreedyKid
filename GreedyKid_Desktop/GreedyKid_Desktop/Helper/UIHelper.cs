// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GreedyKid
{
    public enum CommandType
    {
        None,
        Select,
        Back,
    }

    public enum CommandPosition
    {
        Right,
        Left,
        Center,
    }

    public sealed class UIHelper
    {
        private static UIHelper _instance;

        private Rectangle _selectionRectangle;
        private Rectangle _1x1Rectangle;
        private Rectangle[] _bordersRectangle;
        private Rectangle[] _whiteBordersRectangle;
        private Rectangle[] _microphoneRectangle;
        private Rectangle[] _scoreRectangle;
        private Rectangle[] _gameoverRectangle;
        private Color _backgroundColor = new Color(34, 32, 52);
        private Color _selectedColor = new Color(217, 87, 99);
        private Color _notSelectedColor = new Color(132, 126, 135);
        private Color _commandColor = new Color(91, 110, 225);
        private Color _lightBordersColor = new Color(203, 219, 252);

        public Rectangle SelectionRectangle
        {
            get { return _selectionRectangle; }
        }

        public Rectangle PixelRectangle
        {
            get { return _1x1Rectangle; }
        }

        public Color SelectedColor
        {
            get { return _selectedColor; }
        }

        public Color NotSelectedColor
        {
            get { return _notSelectedColor; }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
        }

        public Color LightBordersColor
        {
            get { return _lightBordersColor; }
        }

        public Rectangle[] ScoreRectangles
        {
            get { return _scoreRectangle; }
        }

        public Rectangle[] GameoverRectangles
        {
            get { return _gameoverRectangle; }
        }



        private UIHelper()
        {
            _bordersRectangle = new Rectangle[8];
            _bordersRectangle[0] = new Rectangle(0, TextureManager.GameplayHeight - 24, 12, 13); // upper left
            _bordersRectangle[1] = new Rectangle(11, TextureManager.GameplayHeight - 24, 12, 13); // upper right
            _bordersRectangle[2] = new Rectangle(0, TextureManager.GameplayHeight - 12, 12, 12); // lower left
            _bordersRectangle[3] = new Rectangle(11, TextureManager.GameplayHeight - 12, 12, 12); // lower right
            _bordersRectangle[4] = new Rectangle(11, TextureManager.GameplayHeight - 24, 1, 9); // up
            _bordersRectangle[5] = new Rectangle(11, TextureManager.GameplayHeight - 8, 1, 8); // down
            _bordersRectangle[6] = new Rectangle(0, TextureManager.GameplayHeight - 12, 8, 1); // left
            _bordersRectangle[7] = new Rectangle(15, TextureManager.GameplayHeight - 12, 8, 1); // right

            _whiteBordersRectangle = new Rectangle[8];
            _whiteBordersRectangle[0] = new Rectangle(0, TextureManager.GameplayHeight - 24 - 39, 12, 13); // upper left
            _whiteBordersRectangle[1] = new Rectangle(11, TextureManager.GameplayHeight - 24 - 39, 12, 13); // upper right
            _whiteBordersRectangle[2] = new Rectangle(0, TextureManager.GameplayHeight - 12 - 39, 12, 12); // lower left
            _whiteBordersRectangle[3] = new Rectangle(11, TextureManager.GameplayHeight - 12 - 39, 12, 12); // lower right
            _whiteBordersRectangle[4] = new Rectangle(11, TextureManager.GameplayHeight - 24 - 39, 1, 9); // up
            _whiteBordersRectangle[5] = new Rectangle(11, TextureManager.GameplayHeight - 8 - 39, 1, 8); // down
            _whiteBordersRectangle[6] = new Rectangle(0, TextureManager.GameplayHeight - 12 - 39, 8, 1); // left
            _whiteBordersRectangle[7] = new Rectangle(15, TextureManager.GameplayHeight - 12 - 39, 8, 1); // right

            _microphoneRectangle = new Rectangle[10];
            for (int i = 0; i < 10; i++)
            {
                _microphoneRectangle[i] = new Rectangle(40 + 3 * i, TextureManager.GameplayHeight - 113, 2, 89);
            }

            _selectionRectangle = new Rectangle(142, TextureManager.GameplayHeight - 140, 4, 7);
            _1x1Rectangle = new Rectangle(1, 1963, 1, 1);

            // score
            _scoreRectangle = new Rectangle[58];
            _scoreRectangle[0] = new Rectangle(449, TextureManager.GameplayHeight - 232, 33, 11); // no star
            _scoreRectangle[1] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 12, 33, 11); // 1 star
            _scoreRectangle[2] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 24, 33, 11); // 2 stars
            _scoreRectangle[3] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 36, 33, 11); // 3 stars
            _scoreRectangle[4] = new Rectangle(492, TextureManager.GameplayHeight - 240, 14, 9); // check
            _scoreRectangle[5] = new Rectangle(507, TextureManager.GameplayHeight - 240, 14, 9); // no check
            _scoreRectangle[6] = new Rectangle(323, TextureManager.GameplayHeight - 240, 125, 10); // separation
            // stars animation 
            for (int i = 0; i < 17; i++)
            {
                int j = (i <= 2 ? 0 : i - 2);
                _scoreRectangle[7 + i] = new Rectangle(0 + j * 51, TextureManager.GameplayHeight - 375, 51, 28);
                _scoreRectangle[24 + i] = new Rectangle(0 + j * 51, TextureManager.GameplayHeight - 346, 51, 28);
                _scoreRectangle[41 + i] = new Rectangle(0 + j * 51, TextureManager.GameplayHeight - 317, 51, 28);
            }

            // gamevoer
            _gameoverRectangle = new Rectangle[4];           
            _gameoverRectangle[0] = new Rectangle(201, TextureManager.GameplayHeight - 184, 328, 184); // half full
            _gameoverRectangle[1] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 13); // gameover 1
            _gameoverRectangle[2] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 28); // gameover 2
            _gameoverRectangle[3] = new Rectangle(201, TextureManager.GameplayHeight - 183, 328, 82); // gameover 3
        }

        public static UIHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UIHelper();
                return _instance;
            }
        }

        public void DrawBorders(SpriteBatch spriteBatch, Vector2 offset)
        {
            Texture2D texture = TextureManager.Gameplay;

            // ****** BORDERS ******
            // up
            spriteBatch.Draw(texture,
                new Rectangle(0 + (int)offset.X, 0 + (int)offset.Y, GreedyKidGame.Width, _bordersRectangle[4].Height),
                _bordersRectangle[4],
                Color.White);

            // down
            spriteBatch.Draw(texture,
                new Rectangle(0 + (int)offset.X, GreedyKidGame.Height - _bordersRectangle[5].Height + (int)offset.Y, GreedyKidGame.Width, _bordersRectangle[5].Height),
                _bordersRectangle[5],
                Color.White);

            // left
            spriteBatch.Draw(texture,
                new Rectangle(0 + (int)offset.X, 0 + (int)offset.Y, _bordersRectangle[6].Width, GreedyKidGame.Height),
                _bordersRectangle[6],
                Color.White);

            // right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[7].Width + (int)offset.X, 0 + (int)offset.Y, _bordersRectangle[6].Width, GreedyKidGame.Height),
                _bordersRectangle[7],
                Color.White);

            // upper left
            spriteBatch.Draw(texture,
                new Rectangle(0 + (int)offset.X, 0 + (int)offset.Y, _bordersRectangle[0].Width, _bordersRectangle[0].Height),
                _bordersRectangle[0],
                Color.White);

            // upper right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[1].Width + (int)offset.X, 0 + (int)offset.Y, _bordersRectangle[1].Width, _bordersRectangle[1].Height),
                _bordersRectangle[1],
                Color.White);

            // lower left
            spriteBatch.Draw(texture,
                new Rectangle(0 + (int)offset.X, GreedyKidGame.Height - _bordersRectangle[2].Height + (int)offset.Y, _bordersRectangle[2].Width, _bordersRectangle[2].Height),
                _bordersRectangle[2],
                Color.White);

            // lower right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[3].Width + (int)offset.X, GreedyKidGame.Height - _bordersRectangle[3].Height + (int)offset.Y, _bordersRectangle[3].Width, _bordersRectangle[3].Height),
                _bordersRectangle[3],
                Color.White);
        }

        public void DrawWhiteBorders(SpriteBatch spriteBatch, int height, Color color)
        {
            Texture2D texture = TextureManager.Gameplay;

            int yMargin = (GreedyKidGame.Height - height) / 2;

            // ****** BORDERS ******
            // up
            spriteBatch.Draw(texture,
                new Rectangle(_whiteBordersRectangle[6].Width + 1, yMargin, GreedyKidGame.Width - _whiteBordersRectangle[6].Width - _whiteBordersRectangle[6].Width - 2, _whiteBordersRectangle[4].Height),
                _whiteBordersRectangle[4],
                color);

            // down
            spriteBatch.Draw(texture,
                new Rectangle(_whiteBordersRectangle[6].Width + 1, GreedyKidGame.Height - _whiteBordersRectangle[5].Height - yMargin, GreedyKidGame.Width - _whiteBordersRectangle[6].Width - _whiteBordersRectangle[6].Width - 2, _whiteBordersRectangle[5].Height),
                _whiteBordersRectangle[5],
                color);

            // left
            spriteBatch.Draw(texture,
                new Rectangle(0, yMargin + _whiteBordersRectangle[4].Height + 1, _whiteBordersRectangle[6].Width, GreedyKidGame.Height - yMargin - yMargin - _whiteBordersRectangle[4].Height - _whiteBordersRectangle[4].Height - 2),
                _whiteBordersRectangle[6],
                color);

            // right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _whiteBordersRectangle[7].Width, yMargin + _whiteBordersRectangle[4].Height + 1, _whiteBordersRectangle[6].Width, GreedyKidGame.Height - yMargin - yMargin - _whiteBordersRectangle[4].Height - _whiteBordersRectangle[4].Height - 2),
                _whiteBordersRectangle[7],
                color);

            // upper left
            spriteBatch.Draw(texture,
                new Rectangle(0, yMargin, _whiteBordersRectangle[0].Width, _whiteBordersRectangle[0].Height),
                _whiteBordersRectangle[0],
                color);

            // upper right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _whiteBordersRectangle[1].Width, yMargin, _whiteBordersRectangle[1].Width, _whiteBordersRectangle[1].Height),
                _whiteBordersRectangle[1],
                color);

            // lower left
            spriteBatch.Draw(texture,
                new Rectangle(0, GreedyKidGame.Height - _whiteBordersRectangle[2].Height - yMargin, _whiteBordersRectangle[2].Width, _whiteBordersRectangle[2].Height),
                _whiteBordersRectangle[2],
                color);

            // lower right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _whiteBordersRectangle[3].Width, GreedyKidGame.Height - _whiteBordersRectangle[3].Height - yMargin, _whiteBordersRectangle[3].Width, _whiteBordersRectangle[3].Height),
                _whiteBordersRectangle[3],
                color);
        }

        public void Fill(SpriteBatch spriteBatch, Rectangle destination, Color color)
        {
            
        }

        public void DrawMicrophoneVolume(SpriteBatch spriteBatch, Vector2 screenshake)
        {
            if (MicrophoneManager.Instance.Working)
            {
                Texture2D texture = TextureManager.Gameplay;

                // microphone
                int micLevel = Math.Min(9, MicrophoneManager.Instance.LeveledVolume);
                int posY = GreedyKidGame.Height / 2 - _microphoneRectangle[0].Height / 2;
                spriteBatch.Draw(texture,
                    new Rectangle(6 + (int)screenshake.X, posY + (int)screenshake.Y, _microphoneRectangle[0].Width, _microphoneRectangle[0].Height),
                     _microphoneRectangle[micLevel],
                     Color.White,
                     0.0f,
                     Vector2.Zero,
                     SpriteEffects.FlipHorizontally,
                     0.0f);
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width - 8 + (int)screenshake.X, posY + (int)screenshake.Y, _microphoneRectangle[0].Width, _microphoneRectangle[0].Height),
                     _microphoneRectangle[micLevel],
                     Color.White);
            }
        }

        public void DrawCenteredText(SpriteBatch spriteBatch, string text, int yPos, int option, int selectedOption, int targetWidth = -1, int targetX = 0)
        {
            SpriteFont font = TextManager.Instance.Font;

            int textWidth = (int)font.MeasureString(text).X;

            Texture2D texture = TextureManager.Gameplay;

            if (targetWidth == -1)
                targetWidth = GreedyKidGame.Width;            

            // mask
            spriteBatch.Draw(texture,
                new Rectangle(targetX + targetWidth / 2 - textWidth / 2 - 2, yPos + 3 - (selectedOption == option ? 1 : 0), textWidth + 4, 9),
                _1x1Rectangle,
                _backgroundColor);
            spriteBatch.Draw(texture,
                new Rectangle(targetX + targetWidth / 2 - textWidth / 2 - 1, yPos + 2 - (selectedOption == option ? 1 : 0), textWidth + 2, 11),
                _1x1Rectangle,
                _backgroundColor);

            Color notSelectedColor = _notSelectedColor;
            if (option == -1)
                notSelectedColor = Color.White;

            spriteBatch.DrawString(font,
                text,
                new Vector2(targetX + targetWidth / 2 - textWidth / 2, yPos - (selectedOption == option ? 1 : 0)),
                (selectedOption == option ? Color.White : notSelectedColor));

            if (selectedOption == option)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(targetX + targetWidth / 2 - textWidth / 2 - _selectionRectangle.Width - 2, yPos + 3, _selectionRectangle.Width, _selectionRectangle.Height),
                    _selectionRectangle,
                    _selectedColor);
            }
        }

        public void DrawTitle(SpriteBatch spriteBatch, string text, int frame = -1)
        {
            SpriteFont font = TextManager.Instance.Font;
            Texture2D texture = TextureManager.Gameplay;

            int textWidth = (int)font.MeasureString(text).X;

            Color highlight = _selectedColor;
            if (frame == 0)
                highlight = Color.White;

            // mask
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - 8 + 1, 7, textWidth + 15, 1),
                _1x1Rectangle,
                _backgroundColor);
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - 8, 8, textWidth + 15, 1),
                _1x1Rectangle,
                _backgroundColor);

            // highlight
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - 4, 4, textWidth + 8, 9),
                _1x1Rectangle,
                _selectedColor);
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - 3, 3, textWidth + 6, 11),
                _1x1Rectangle,
                _selectedColor);

            // text
            if (frame != 2)
                spriteBatch.DrawString(font,
                    text,
                    new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, 1),
                    Color.White);
        }


        public void DrawCommand(SpriteBatch spriteBatch, string text, CommandType type, CommandPosition position = CommandPosition.Right, bool blink = false)
        {
            const int margin = 24;

            SpriteFont font = TextManager.Instance.Font;
            SpriteFont genericFont = TextManager.Instance.GenericFont;
            Texture2D texture = TextureManager.Gameplay;

            // command text
            string desktopCommand = (type == CommandType.Select ? "d" : "e");
            string consoleCommand = desktopCommand;
            if (GamePadInputsHandler.PreferredButtonType == ButtonType.Xbox)
                consoleCommand = (type == CommandType.Select ? "`" : "a");
            else
                consoleCommand = (type == CommandType.Select ? "b" : "c");

            // sizing
            int textWidth = (int)font.MeasureString(text).X;

            // positionning
            int yOffset = 0;

            int fullWidth = textWidth;
            if (type != CommandType.None)
            {
                fullWidth = textWidth + 3;

                if (!Program.RunningOnConsole)
                {
                    if (InputManager.HasGamepad)
                    {
                        fullWidth += (int)genericFont.MeasureString(consoleCommand).X;
                        fullWidth += (int)genericFont.MeasureString(" | ").X;
                        fullWidth += (int)genericFont.MeasureString(desktopCommand).X;
                    }
                    else
                    {
                        fullWidth += (int)genericFont.MeasureString(desktopCommand).X;
                    }
                }
                else
                {
                    fullWidth += (int)genericFont.MeasureString(consoleCommand).X;
                }
            }
            else
            {
                yOffset = -169;
            }

            int startX = (position == CommandPosition.Left ? margin : GreedyKidGame.Width - margin - fullWidth);
            if (position == CommandPosition.Center)
            {
                startX = (GreedyKidGame.Width - fullWidth) / 2;
            }

            // background
            spriteBatch.Draw(texture,
                new Rectangle(startX - 4, GreedyKidGame.Height - 8 + yOffset, fullWidth + 7, 1),
                _1x1Rectangle,
                _backgroundColor);
            spriteBatch.Draw(texture,
                new Rectangle(startX - 3, GreedyKidGame.Height - 7 + yOffset, fullWidth + 7, 1),
                _1x1Rectangle,
                _backgroundColor);

            Color color = (blink ? Color.White : _commandColor);

            // text
            spriteBatch.DrawString(font, text, new Vector2(startX, GreedyKidGame.Height - 15 + yOffset), color);
            
            // command
            if (type != CommandType.None)
            {
                int currentX = startX + textWidth + 3;

                if (!Program.RunningOnConsole)
                {
                    if (InputManager.HasGamepad)
                    {
                        spriteBatch.DrawString(genericFont, consoleCommand, new Vector2(currentX, GreedyKidGame.Height - 15 + yOffset), color);
                        currentX += (int)genericFont.MeasureString(consoleCommand).X;
                        spriteBatch.DrawString(genericFont, " | ", new Vector2(currentX, GreedyKidGame.Height - 15 + yOffset), color);
                        currentX += (int)genericFont.MeasureString(" | ").X;
                        spriteBatch.DrawString(genericFont, desktopCommand, new Vector2(currentX, GreedyKidGame.Height - 15 + yOffset), color);
                    }
                    else
                    {
                        fullWidth += (int)genericFont.MeasureString(desktopCommand).X;
                        spriteBatch.DrawString(genericFont, desktopCommand, new Vector2(currentX, GreedyKidGame.Height - 15 + yOffset), color);
                    }
                }
                else
                {
                    spriteBatch.DrawString(genericFont, consoleCommand, new Vector2(currentX, GreedyKidGame.Height - 15 + yOffset), color);
                }
            }
        }
    }
}

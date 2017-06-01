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

    public sealed class UIHelper
    {
        private static UIHelper _instance;

        private Rectangle _selectionRectangle;
        private Rectangle _1x1Rectangle;
        private Rectangle[] _bordersRectangle;
        private Rectangle[] _microphoneRectangle;
        private Rectangle[] _scoreRectangle;
        private Color _backgroundColor = new Color(34, 32, 52);
        private Color _selectedColor = new Color(217, 87, 99);
        private Color _notSelectedColor = new Color(132, 126, 135);
        private Color _commandColor = new Color(91, 110, 225);

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

        public Rectangle[] ScoreRectangles
        {
            get { return _scoreRectangle; }
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

            _microphoneRectangle = new Rectangle[10];
            for (int i = 0; i < 10; i++)
            {
                _microphoneRectangle[i] = new Rectangle(40 + 3 * i, TextureManager.GameplayHeight - 113, 2, 89);
            }

            _selectionRectangle = new Rectangle(142, TextureManager.GameplayHeight - 140, 4, 7);
            _1x1Rectangle = new Rectangle(1, 1963, 1, 1);

            // score
            _scoreRectangle = new Rectangle[7];
            _scoreRectangle[0] = new Rectangle(449, TextureManager.GameplayHeight - 232, 33, 11); // no star
            _scoreRectangle[1] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 12, 33, 11); // 1 star
            _scoreRectangle[2] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 24, 33, 11); // 2 stars
            _scoreRectangle[3] = new Rectangle(449, TextureManager.GameplayHeight - 232 + 36, 33, 11); // 3 stars
            _scoreRectangle[4] = new Rectangle(492, TextureManager.GameplayHeight - 240, 14, 9); // check
            _scoreRectangle[5] = new Rectangle(507, TextureManager.GameplayHeight - 240, 14, 9); // no check
            _scoreRectangle[6] = new Rectangle(323, TextureManager.GameplayHeight - 240, 125, 10); // separation
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

        public void DrawBorders(SpriteBatch spriteBatch)
        {
            Texture2D texture = TextureManager.Gameplay;

            // ****** BORDERS ******
            // up
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, GreedyKidGame.Width, _bordersRectangle[4].Height),
                _bordersRectangle[4],
                Color.White);

            // down
            spriteBatch.Draw(texture,
                new Rectangle(0, GreedyKidGame.Height - _bordersRectangle[5].Height, GreedyKidGame.Width, _bordersRectangle[5].Height),
                _bordersRectangle[5],
                Color.White);

            // left
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, _bordersRectangle[6].Width, GreedyKidGame.Height),
                _bordersRectangle[6],
                Color.White);

            // right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[7].Width, 0, _bordersRectangle[6].Width, GreedyKidGame.Height),
                _bordersRectangle[7],
                Color.White);

            // upper left
            spriteBatch.Draw(texture,
                new Rectangle(0, 0, _bordersRectangle[0].Width, _bordersRectangle[0].Height),
                _bordersRectangle[0],
                Color.White);

            // upper right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[1].Width, 0, _bordersRectangle[1].Width, _bordersRectangle[1].Height),
                _bordersRectangle[1],
                Color.White);

            // lower left
            spriteBatch.Draw(texture,
                new Rectangle(0, GreedyKidGame.Height - _bordersRectangle[2].Height, _bordersRectangle[2].Width, _bordersRectangle[2].Height),
                _bordersRectangle[2],
                Color.White);

            // lower right
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width - _bordersRectangle[3].Width, GreedyKidGame.Height - _bordersRectangle[3].Height, _bordersRectangle[3].Width, _bordersRectangle[3].Height),
                _bordersRectangle[3],
                Color.White);
        }

        public void DrawMicrophoneVolume(SpriteBatch spriteBatch)
        {
            if (MicrophoneManager.Instance.Working)
            {
                Texture2D texture = TextureManager.Gameplay;

                // microphone
                int micLevel = Math.Min(9, MicrophoneManager.Instance.LeveledVolume);
                int posY = GreedyKidGame.Height / 2 - _microphoneRectangle[0].Height / 2;
                spriteBatch.Draw(texture,
                    new Rectangle(6, posY, _microphoneRectangle[0].Width, _microphoneRectangle[0].Height),
                     _microphoneRectangle[micLevel],
                     Color.White,
                     0.0f,
                     Vector2.Zero,
                     SpriteEffects.FlipHorizontally,
                     0.0f);
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width - 8, posY, _microphoneRectangle[0].Width, _microphoneRectangle[0].Height),
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


        public void DrawCommand(SpriteBatch spriteBatch, string text, CommandType type, bool isLeft = false)
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

            int startX = (isLeft ? margin : GreedyKidGame.Width - margin - fullWidth);

            // background
            spriteBatch.Draw(texture,
                new Rectangle(startX - 4, GreedyKidGame.Height - 8, fullWidth + 7, 1),
                _1x1Rectangle,
                _backgroundColor);
            spriteBatch.Draw(texture,
                new Rectangle(startX - 3, GreedyKidGame.Height - 7, fullWidth + 7, 1),
                _1x1Rectangle,
                _backgroundColor);

            // text
            spriteBatch.DrawString(font, text, new Vector2(startX, GreedyKidGame.Height - 15), _commandColor);
            
            // command
            if (type != CommandType.None)
            {
                int currentX = startX + textWidth + 3;

                if (!Program.RunningOnConsole)
                {
                    if (InputManager.HasGamepad)
                    {
                        spriteBatch.DrawString(genericFont, consoleCommand, new Vector2(currentX, GreedyKidGame.Height - 15), _commandColor);
                        currentX += (int)genericFont.MeasureString(consoleCommand).X;
                        spriteBatch.DrawString(genericFont, " | ", new Vector2(currentX, GreedyKidGame.Height - 15), _commandColor);
                        currentX += (int)genericFont.MeasureString(" | ").X;
                        spriteBatch.DrawString(genericFont, desktopCommand, new Vector2(currentX, GreedyKidGame.Height - 15), _commandColor);
                    }
                    else
                    {
                        fullWidth += (int)genericFont.MeasureString(desktopCommand).X;
                        spriteBatch.DrawString(genericFont, desktopCommand, new Vector2(currentX, GreedyKidGame.Height - 15), _commandColor);
                    }
                }
                else
                {
                    spriteBatch.DrawString(genericFont, consoleCommand, new Vector2(currentX, GreedyKidGame.Height - 15), _commandColor);
                }
            }
        }
    }
}

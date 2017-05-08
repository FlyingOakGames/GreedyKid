using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GreedyKid
{
    public sealed class UIHelper
    {
        private static UIHelper _instance;

        private Rectangle _selectionRectangle;
        private Rectangle _1x1Rectangle;
        private Rectangle[] _bordersRectangle;
        private Rectangle[] _microphoneRectangle;
        private Color _backgroundColor = new Color(34, 32, 52);
        private Color _selectedColor = new Color(217, 87, 99);
        private Color _notSelectedColor = new Color(132, 126, 135);

        public Rectangle SelectionRectangle
        {
            get { return _selectionRectangle; }
        }

        public Color SelectedColor
        {
            get { return _selectedColor; }
        }

        public Color NotSelectedColor
        {
            get { return _notSelectedColor; }
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
                _microphoneRectangle[i] = new Rectangle(0, TextureManager.GameplayHeight - 148 - 4 * i, 146, 4);
            }

            _selectionRectangle = new Rectangle(142, TextureManager.GameplayHeight - 140, 4, 7);
            _1x1Rectangle = new Rectangle(1, 1963, 1, 1);
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
                spriteBatch.Draw(texture,
                    new Rectangle(93, 175, _microphoneRectangle[0].Width, _microphoneRectangle[0].Height),
                     _microphoneRectangle[micLevel],
                     Color.White);
            }
        }

        public void DrawCenteredText(SpriteBatch spriteBatch, string text, int yPos, int option, int selectedOption)
        {
            SpriteFont font = TextManager.Instance.Font;

            int textWidth = (int)font.MeasureString(text).X;
            spriteBatch.DrawString(font,
                text,
                new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, yPos),
                (selectedOption == option ? Color.White : _notSelectedColor));

            if (selectedOption == option)
            {
                Texture2D texture = TextureManager.Gameplay;
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - _selectionRectangle.Width - 2, yPos + 4, _selectionRectangle.Width, _selectionRectangle.Height),
                    _selectionRectangle,
                    _selectedColor);
            }
        }

        public void DrawTitle(SpriteBatch spriteBatch, string text)
        {
            SpriteFont font = TextManager.Instance.Font;
            Texture2D texture = TextureManager.Gameplay;

            int textWidth = (int)font.MeasureString(text).X;

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
            spriteBatch.DrawString(font,
                text,
                new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, 1),
                Color.White);
        }
    }
}

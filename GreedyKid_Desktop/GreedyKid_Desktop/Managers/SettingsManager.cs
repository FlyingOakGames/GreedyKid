using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GreedyKid
{

    public enum FullScreenMode
    {
        No,
        Bordeless,
        Real
    }

    public sealed class SettingsManager
    {
        // settings
        public int ResolutionX = 1280;
        public int ResolutionY = 720;        
        public FullScreenMode FullScreenMode = FullScreenMode.No;
        public ButtonType PreviousPreferredButtonType = ButtonType.Xbox;
        public float MusicVolume = 0.5f;
        public float SfxVolume = 1.0f;

        private int _selectedResolution = 0;

        private Rectangle _selectionRectangle;
        private Rectangle _dashRectangle;
        private Color _selectionColor = new Color(217, 87, 99);
        private Color _notSelectedColor = new Color(132, 126, 135);

        private int _selectionOption = 0;

        private static SettingsManager _instance;

        private SettingsManager()
        {
            _selectionRectangle = new Rectangle(142, TextureManager.GameplayHeight - 140, 4, 7);
            _dashRectangle = new Rectangle(617, TextureManager.GameplayHeight - 121, 274, 1);

            BuildResolutionCatalogue();
        }

        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsManager();
                return _instance;
            }
        }

        public void PushUp()
        {
            _selectionOption--;
            if (_selectionOption < 0)
                _selectionOption = 7;
        }

        public void PushDown()
        {
            _selectionOption++;
            _selectionOption %= 8;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = TextureManager.Gameplay;

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Language, 23, 0);
            DrawRightAlignedText(spriteBatch, TextManager.Instance.LanguageValue, 23, 0);

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Resolution, 23 + 15, 1);
            DrawRightAlignedText(spriteBatch, _compatibleResolutionName[_selectedResolution], 23 + 15, 1, true);

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Fullscreen, 23 + 30, 2);
            string fullScreenMode = TextManager.Instance.No;
            if (FullScreenMode == FullScreenMode.Bordeless)
                fullScreenMode = TextManager.Instance.Borderless;
            else if (FullScreenMode == FullScreenMode.Real)
                fullScreenMode = TextManager.Instance.Real;
            DrawRightAlignedText(spriteBatch, fullScreenMode, 23 + 30, 2);

            spriteBatch.Draw(texture,
                new Rectangle(27, 73, _dashRectangle.Width, _dashRectangle.Height),
                _dashRectangle,
                Color.White);

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Buttons, 23 + 30 + 24, 3);
            string buttons = "XBOX";
            if (PreviousPreferredButtonType == ButtonType.PlayStation)
                buttons = "PLAYSTATION";
            DrawRightAlignedText(spriteBatch, buttons, 23 + 32 + 24, 3, true);

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Keyboard, 23 + 30 + 24 + 15, 4);
            DrawRightAlignedText(spriteBatch, TextManager.Instance.Remap, 23 + 30 + 24 + 15, 4, false, true);

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Microphone, 23 + 30 + 24 + 30, 5);

            spriteBatch.Draw(texture,
                new Rectangle(27, 126, _dashRectangle.Width, _dashRectangle.Height),
                _dashRectangle,
                Color.White);

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Music, 23 + 30 + 24 + 30 + 24, 6);

            DrawLeftAlignedText(spriteBatch, TextManager.Instance.Sfx, 23 + 30 + 24 + 30 + 24 + 15, 7);
        }

        private void DrawLeftAlignedText(SpriteBatch spriteBatch, string text, int yPos, int option, bool genericFont = false)
        {
            SpriteFont font = (genericFont ? TextManager.Instance.GenericFont : TextManager.Instance.Font);

            spriteBatch.DrawString(font, text, new Vector2(35, yPos), (_selectionOption == option ? Color.White : _notSelectedColor));

            if (_selectionOption == option)
            {
                Texture2D texture = TextureManager.Gameplay;
                spriteBatch.Draw(texture,
                    new Rectangle(35 - _selectionRectangle.Width - 2, yPos + 4, _selectionRectangle.Width, _selectionRectangle.Height),
                    _selectionRectangle,
                    _selectionColor);
            }
        }

        private void DrawRightAlignedText(SpriteBatch spriteBatch, string text, int yPos, int option, bool genericFont = false, bool invertedSelection = false)
        {
            SpriteFont font = (genericFont ? TextManager.Instance.GenericFont : TextManager.Instance.Font);

            int textWidth = (int)font.MeasureString(text).X;
            spriteBatch.DrawString(font, text, new Vector2(GreedyKidGame.Width - 35 - textWidth, yPos), (_selectionOption == option ? Color.White : _notSelectedColor));

            if (_selectionOption == option)
            {
                Texture2D texture = TextureManager.Gameplay;
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width - 35 - textWidth - _selectionRectangle.Width - 2, yPos + 4, _selectionRectangle.Width, _selectionRectangle.Height),
                    _selectionRectangle,
                    _selectionColor,
                    0.0f,
                    Vector2.Zero,
                    (invertedSelection ? SpriteEffects.None : SpriteEffects.FlipHorizontally),
                    0.0f);
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width - 35 + 2, yPos + 4, _selectionRectangle.Width, _selectionRectangle.Height),
                    _selectionRectangle,
                    _selectionColor,
                    0.0f,
                    Vector2.Zero,
                    (invertedSelection ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
                    0.0f);
            }
        }

        private List<int> _compatibleXResolution = new List<int>();
        private List<int> _compatibleYResolution = new List<int>();
        private List<string> _compatibleResolutionName = new List<string>();

        private void BuildResolutionCatalogue()
        {
            foreach (DisplayMode displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (!(_compatibleXResolution.Contains(displayMode.Width) && _compatibleYResolution.Contains(displayMode.Height)))
                {
                    _compatibleXResolution.Add(displayMode.Width);
                    _compatibleYResolution.Add(displayMode.Height);
                }
            }
            if (_compatibleXResolution.Count == 0) // no resolution found, let's add the default one
            {
                _compatibleXResolution.Add(ResolutionX);
                _compatibleYResolution.Add(ResolutionY);
            }

            // sort Y       
            int pos = 0;
            while (pos < _compatibleYResolution.Count)
            {
                if (pos == 0 || _compatibleYResolution[pos] >= _compatibleYResolution[pos - 1])
                    pos++;
                else
                {
                    int y = _compatibleYResolution[pos];
                    _compatibleYResolution[pos] = _compatibleYResolution[pos - 1];
                    _compatibleYResolution[pos - 1] = y;
                    int x = _compatibleXResolution[pos];
                    _compatibleXResolution[pos] = _compatibleXResolution[pos - 1];
                    _compatibleXResolution[pos - 1] = x;
                    pos--;
                }
            }
            // sort X
            pos = 0;
            while (pos < _compatibleXResolution.Count)
            {
                if (pos == 0 || _compatibleXResolution[pos] >= _compatibleXResolution[pos - 1])
                    pos++;
                else
                {
                    int y = _compatibleYResolution[pos];
                    _compatibleYResolution[pos] = _compatibleYResolution[pos - 1];
                    _compatibleYResolution[pos - 1] = y;
                    int x = _compatibleXResolution[pos];
                    _compatibleXResolution[pos] = _compatibleXResolution[pos - 1];
                    _compatibleXResolution[pos - 1] = x;
                    pos--;
                }
            }

            for (int i = 0; i < _compatibleXResolution.Count; i++)
            {
                if (ResolutionX == _compatibleXResolution[i] && ResolutionY == _compatibleYResolution[i])
                    _selectedResolution = i;
                _compatibleResolutionName.Add(_compatibleXResolution[i] + "*" + _compatibleYResolution[i]);
            }
        }
    }
}

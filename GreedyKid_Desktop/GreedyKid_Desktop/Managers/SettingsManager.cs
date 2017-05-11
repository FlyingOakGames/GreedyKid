using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

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
        public string SaveDirectory = "";

        private static string _settingsPath = "Settings";

        // settings
        public int ResolutionX = -1;
        public int ResolutionY = -1;
        public FullScreenMode FullScreenMode = FullScreenMode.Bordeless;

        public float MusicVolume = 0.5f;
        public float SfxVolume = 1.0f;

        private int _selectedResolution = 0;

        public int SelectedMicrophone = -1;

        private Rectangle _dashRectangle;
        private Rectangle _volumeRectangle;
        private Rectangle _backgroundRectangle;

        private Color _volumeColor = new Color(89, 86, 82);
        private Color _backgroundColor = new Color(132, 126, 135);


        private int _selectionOption = 0;

        private static SettingsManager _instance;

        private List<string> _microphoneName = new List<string>();

        private bool _isRemapping = false;
        private bool _waitingForInput = false;

        private SettingsManager()
        {
            _dashRectangle = new Rectangle(617, TextureManager.GameplayHeight - 121, 274, 1);
            _volumeRectangle = new Rectangle(30, TextureManager.GameplayHeight - 107, 4, 7);
            _backgroundRectangle = new Rectangle(1506, TextureManager.GameplayHeight - GreedyKidGame.Height, 112, 146);

#if DESKTOP
            string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (Environment.OSVersion.Platform == PlatformID.MacOSX || (Environment.OSVersion.Platform == PlatformID.Unix && SystemHelper.IsMac))
                SaveDirectory = Path.Combine(userHome, "Library/Application Support/Flying Oak Games/Boo! Greedy Kid");
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
                SaveDirectory = Path.Combine(userHome, ".greedykid");
            else
                SaveDirectory = Path.Combine(userHome, "AppData\\LocalLow\\Flying Oak Games\\Boo! Greedy Kid");

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            _settingsPath = Path.Combine(SaveDirectory, _settingsPath);

            BuildResolutionCatalogue();
#endif

            for (int i = 0; i < Microsoft.Xna.Framework.Audio.Microphone.All.Count; i++)
            {
                _microphoneName.Add("MIC " + (i + 1));
            }
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

        public void Reset()
        {
            _selectionOption = 0;
            _isRemapping = false;
            _waitingForInput = false;
        }

        private void DefaultMicrophone()
        {
            for (int i = 0; i < Microsoft.Xna.Framework.Audio.Microphone.All.Count; i++)
            {
                if (Microsoft.Xna.Framework.Audio.Microphone.Default == Microsoft.Xna.Framework.Audio.Microphone.All[i])
                    SelectedMicrophone = i;
            }
        }

        private void ParseLine(string line)
        {
            if (line.Length == 0)
                return;

            string[] elem = line.Split('=');

            if (elem.Length == 2)
            {
                switch (elem[0])
                {
                    case "ResolutionX":
                        if (!Int32.TryParse(elem[1], out ResolutionX))
                        {
                            ResolutionX = -1;
                        }
                        break;
                    case "ResolutionY":
                        if (!Int32.TryParse(elem[1], out ResolutionY))
                        {
                            ResolutionY = -1;
                        }
                        break;
                    case "FullScreenMode":
                        if (elem[1] == "Bordeless")
                        {
                            FullScreenMode = FullScreenMode.Bordeless;
                        }
                        else if (elem[1] == "Real")
                        {
                            FullScreenMode = FullScreenMode.Real;
                        }
                        else
                        {
                            FullScreenMode = FullScreenMode.No;
                        }
                        break;

                    case "Microphone":
                        for (int i = 0; i < Microsoft.Xna.Framework.Audio.Microphone.All.Count; i++)
                        {
                            if (elem[1] == Microsoft.Xna.Framework.Audio.Microphone.All[i].Name)
                                SelectedMicrophone = i;
                        }
                        break;
                    case "PreferredButtonType":
                        if (elem[1] == "PlayStation")
                            GamePadInputsHandler.PreferredButtonType = ButtonType.PlayStation;
                        else
                            GamePadInputsHandler.PreferredButtonType = ButtonType.Xbox;
#if PLAYSTATION4
                        GamePadInputsHandler.PreferredButtonType = ButtonType.PlayStation;
#elif XBOXONE
                        GamePadInputsHandler.PreferredButtonType = ButtonType.Xbox;
#endif
                        break;

                    case "MusicVolume":
                        if (!Single.TryParse(elem[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out MusicVolume)) MusicVolume = 0.5f;
                        break;
                    case "SfxVolume":
                        if (!Single.TryParse(elem[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out SfxVolume)) SfxVolume = 1.0f;
                        break;

                    case "Language":
                        if (elem[1] == "EN")
                        {
                            TextManager.Instance.Language = Language.EN;
                        }
                        else if (elem[1] == "FR")
                        {
                            TextManager.Instance.Language = Language.FR;
                        }
                        else if (elem[1] == "DE")
                        {
                            TextManager.Instance.Language = Language.DE;
                        }
                        else if (elem[1] == "RU")
                        {
                            TextManager.Instance.Language = Language.RU;
                        }
                        else if (elem[1] == "SP")
                        {
                            TextManager.Instance.Language = Language.SP;
                        }
                        else if (elem[1] == "BR")
                        {
                            TextManager.Instance.Language = Language.BR;
                        }
                        else if (elem[1] == "IT")
                        {
                            TextManager.Instance.Language = Language.IT;
                        }
                        break;
                }
            }
        }

        public void Load()
        {
            string path = _settingsPath;

#if PLAYSTATION4
            path = PlatformHelper.PlayStation4.BeginSave(_configPath, true);
            if (path == null)
                return;
#endif

#if DESKTOP || PLAYSTATION4
            if (File.Exists(path))
#elif XBOXONE
            byte[] buffer;
            if (PlatformHelper.XboxOne.LoadData(path, path, out buffer))
#endif
            {

#if DESKTOP || PLAYSTATION4
                try
                {
                    // load
                    DefaultMicrophone();
                    using (TextReader reader = File.OpenText(path))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            ParseLine(line);
                        }
                    }
                    SetResolution(ResolutionX, ResolutionY, false);
                }
                catch (Exception) { }
#elif XBOXONE
                AutoResolution = true;

                try
                {
                    using (MemoryStream stream = new MemoryStream(buffer))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            MusicVolume = reader.ReadSingle();
                            SfxVolume = reader.ReadSingle();
                            Language = (Language)reader.ReadInt32();
                        }
                    }                    
                }
                catch (Exception) { }                
#endif

#if PLAYSTATION4
                PlatformHelper.PlayStation4.EndSave();
#endif
            }
            else
            {
#if PLAYSTATION4
                PlatformHelper.PlayStation4.EndSave();
#endif
                Save();
            }
        }

        public void Delete()
        {
#if DESKTOP
            if (File.Exists(_settingsPath))
                File.Delete(_settingsPath);
#endif
        }

        public void SetResolution(int width, int height, bool save = true)
        {
            ResolutionX = width;
            ResolutionY = height;

            for (int i = 0; i < _compatibleXResolution.Count; i++)
            {
                if (_compatibleXResolution[i] == width && _compatibleYResolution[i] == height)
                {
                    _selectedResolution = i;
                }
            }

            if (save)
                Save();
        }

        public void Save()
        {
            GreedyKidGame.ShouldApplyChanges = true;

            string path = _settingsPath;            

#if PLAYSTATION4
            path = PlatformHelper.PlayStation4.BeginSave(_configPath, false);
            if (path == null)
                return;
#endif

#if DESKTOP || PLAYSTATION4
            using (TextWriter writer = File.CreateText(path))
            {
                writer.Write("ResolutionX=" + ResolutionX + writer.NewLine);
                writer.Write("ResolutionY=" + ResolutionY + writer.NewLine);
                writer.Write("FullScreenMode=" + FullScreenMode + writer.NewLine);

                writer.Write("PreferredButtonType=" + GamePadInputsHandler.PreferredButtonType + writer.NewLine);

                writer.Write("MusicVolume=" + MusicVolume.ToString(System.Globalization.CultureInfo.InvariantCulture) + writer.NewLine);
                writer.Write("SfxVolume=" + SfxVolume.ToString(System.Globalization.CultureInfo.InvariantCulture) + writer.NewLine);

                writer.Write("Language=" + TextManager.Instance.Language + writer.NewLine);

                if (SelectedMicrophone >= 0)
                    writer.Write("Microphone=" + Microsoft.Xna.Framework.Audio.Microphone.All[SelectedMicrophone].Name + writer.NewLine);
                else
                    writer.Write("Microphone=disabled" + writer.NewLine);

                writer.Flush();
            }
#elif XBOXONE
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(MusicVolume);
                    writer.Write(SfxVolume);
                    writer.Write((int)Language);

                    writer.Flush();
                    stream.Flush();

                    byte[] buffer = stream.GetBuffer();
                    PlatformHelper.XboxOne.SaveData(path, path, buffer, buffer.Length);
                }
            }            
#endif

#if PLAYSTATION4
            PlatformHelper.PlayStation4.EndSave();
#endif
        }

        public void PushSelect()
        {
            if (_waitingForInput)
                return;

            if (!_isRemapping && _selectionOption == 4)
            {
                _isRemapping = true;
                _selectionOption = 0;
            }
            else if (_isRemapping)
            {
                if (_selectionOption == 8)
                {
                    MouseKeyboardInputsHandler.RestoreDefault();
                }
                else
                {
                    _waitingForInput = true;
                }
            }
        }

        public bool PushCancel()
        {
            if (_waitingForInput)
                return false;

            if (_isRemapping)
            {
                _isRemapping = false;
                _selectionOption = 4;
                _waitingForInput = false;
                MouseKeyboardInputsHandler.SaveMapping();
                return true;
            }
            return false;
        }

        public void PushUp()
        {
            if (_waitingForInput)
                return;

            _selectionOption--;
            if (_selectionOption < 0)
                _selectionOption = (_isRemapping ? 8 : 7);
        }

        public void PushDown()
        {
            if (_waitingForInput)
                return;

            _selectionOption++;
            _selectionOption %= (_isRemapping ? 9 : 8);
        }

        public void PushRight()
        {
            switch (_selectionOption)
            {
                case 0:
                    TextManager.Instance.Language++; break;
                case 1:
                    _selectedResolution++;
                    _selectedResolution = Math.Min(_selectedResolution, _compatibleXResolution.Count - 1);
                    ResolutionX = _compatibleXResolution[_selectedResolution];
                    ResolutionY = _compatibleYResolution[_selectedResolution];
                    break;
                case 2:
                    if (FullScreenMode == FullScreenMode.No)
                        FullScreenMode = FullScreenMode.Bordeless;
                    else if (FullScreenMode == FullScreenMode.Bordeless)
                        FullScreenMode = FullScreenMode.Real;
                    break;
                case 3:
                    if (GamePadInputsHandler.PreferredButtonType == ButtonType.Xbox)
                        GamePadInputsHandler.PreferredButtonType = ButtonType.PlayStation;
                    break;
                case 5:
                    SelectedMicrophone++;
                    SelectedMicrophone = Math.Min(SelectedMicrophone, _microphoneName.Count - 1);
                    MicrophoneManager.Instance.SetMicrophone(SelectedMicrophone);
                    break;
                case 6: MusicVolumeDown(); break;
                case 7: SfxVolumeDown(); break;
            }
        }

        public void PushLeft()
        {
            switch (_selectionOption)
            {
                case 0:
                    TextManager.Instance.Language--; break;
                case 1:
                    _selectedResolution--;
                    _selectedResolution = Math.Max(_selectedResolution, 0);
                    ResolutionX = _compatibleXResolution[_selectedResolution];
                    ResolutionY = _compatibleYResolution[_selectedResolution];
                    break;
                case 2:
                    if (FullScreenMode == FullScreenMode.Bordeless)
                        FullScreenMode = FullScreenMode.No;
                    else if (FullScreenMode == FullScreenMode.Real)
                        FullScreenMode = FullScreenMode.Bordeless;
                    break;
                case 3:
                    if (GamePadInputsHandler.PreferredButtonType == ButtonType.PlayStation)
                        GamePadInputsHandler.PreferredButtonType = ButtonType.Xbox;
                    break;
                case 5:
                    SelectedMicrophone--;
                    SelectedMicrophone = Math.Max(SelectedMicrophone, -1);
                    MicrophoneManager.Instance.SetMicrophone(SelectedMicrophone);
                    break;
                case 6: MusicVolumeUp(); break;
                case 7: SfxVolumeUp(); break;
            }
        }

        private void SfxVolumeUp()
        {
            int sfxVolume = (int)Math.Round(SfxVolume * 10);
            sfxVolume++;
            if (sfxVolume > 10)
                sfxVolume = 10;
            SfxVolume = sfxVolume / 10.0f;
            //SoundManager.SetVolume(1.0f);
            // blips
            //SoundManager.PlayPersistant((int)PersistantSfx.ItemEquip);
        }

        private void SfxVolumeDown()
        {
            int sfxVolume = (int)Math.Round(SfxVolume * 10);
            sfxVolume--;
            if (sfxVolume < 0)
                sfxVolume = 0;
            SfxVolume = sfxVolume / 10.0f;
            //SoundManager.SetVolume(1.0f);
            // blips
            //SoundManager.PlayPersistant((int)PersistantSfx.ItemEquip);
        }

        private void MusicVolumeUp()
        {
            int musicVolume = (int)Math.Round(MusicVolume * 10);
            musicVolume++;
            if (musicVolume > 10)
                musicVolume = 10;
            MusicVolume = musicVolume / 10.0f;
            //MusicManager.SetVolume(1.0f);
        }

        private void MusicVolumeDown()
        {
            int musicVolume = (int)Math.Round(MusicVolume * 10);
            musicVolume--;
            if (musicVolume < 0)
                musicVolume = 0;
            MusicVolume = musicVolume / 10.0f;
            //MusicManager.SetVolume(1.0f);
        }

        private bool OptionMax()
        {
            switch (_selectionOption)
            {
                case 0:
                    return TextManager.Instance.Language == Language.Count - 1;                    
                case 1:
                    return _selectedResolution == _compatibleXResolution.Count - 1;
                case 2:
                    return FullScreenMode == FullScreenMode.Real;
                case 3:
                    return GamePadInputsHandler.PreferredButtonType == ButtonType.PlayStation;
                case 5:
                    return SelectedMicrophone == _microphoneName.Count - 1;
            }
            return false;
        }

        private bool OptionMin()
        {
            switch (_selectionOption)
            {
                case 0:
                    return TextManager.Instance.Language == Language.EN;
                case 1:
                    return _selectedResolution == 0;
                case 2:
                    return FullScreenMode == FullScreenMode.No;
                case 3:
                    return GamePadInputsHandler.PreferredButtonType == ButtonType.Xbox;
                case 5:
                    return SelectedMicrophone == -1;
            }
            return false;
        }

        private void Rebind(Microsoft.Xna.Framework.Input.Keys key, MouseKeyboardInputsHandler.MouseButton mouseButton, MouseKeyboardInputsHandler.MappingType type)
        {
            if (_waitingForInput)
            {
                if (type == MouseKeyboardInputsHandler.MappingType.Mouse)
                {
                    switch (_selectionOption)
                    {
                        case 0: MouseKeyboardInputsHandler.UpKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                        case 1: MouseKeyboardInputsHandler.DownKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                        case 2: MouseKeyboardInputsHandler.LeftKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                        case 3: MouseKeyboardInputsHandler.RightKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                        case 4: MouseKeyboardInputsHandler.ShoutKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                        case 5: MouseKeyboardInputsHandler.ActionKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                        case 6: MouseKeyboardInputsHandler.RollKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                        case 7: MouseKeyboardInputsHandler.TauntKey = new MouseKeyboardInputsHandler.Mapping(mouseButton); break;
                    }
                }
                else
                {
                    switch (_selectionOption)
                    {
                        case 0: MouseKeyboardInputsHandler.UpKey = new MouseKeyboardInputsHandler.Mapping(key); break;
                        case 1: MouseKeyboardInputsHandler.DownKey = new MouseKeyboardInputsHandler.Mapping(key); break;
                        case 2: MouseKeyboardInputsHandler.LeftKey = new MouseKeyboardInputsHandler.Mapping(key); break;
                        case 3: MouseKeyboardInputsHandler.RightKey = new MouseKeyboardInputsHandler.Mapping(key); break;
                        case 4: MouseKeyboardInputsHandler.ShoutKey = new MouseKeyboardInputsHandler.Mapping(key); break;
                        case 5: MouseKeyboardInputsHandler.ActionKey = new MouseKeyboardInputsHandler.Mapping(key); break;
                        case 6: MouseKeyboardInputsHandler.RollKey = new MouseKeyboardInputsHandler.Mapping(key); break;
                        case 7: MouseKeyboardInputsHandler.TauntKey = new MouseKeyboardInputsHandler.Mapping(key); break;                        
                    }
                }

                _waitingForInput = false;
            }
        }

        public void Update(float gameTime)
        {
            if (_waitingForInput)
            {
                Microsoft.Xna.Framework.Input.Keys key;
                MouseKeyboardInputsHandler.MouseButton mouseButton;
                MouseKeyboardInputsHandler.MappingType type;
                if (InputManager.PlayerDevice.DetectKeyPress(out key, out mouseButton, out type))
                {
                    if (type == MouseKeyboardInputsHandler.MappingType.Keyboard && key == Microsoft.Xna.Framework.Input.Keys.Escape)
                        _waitingForInput = false;
                    else
                        Rebind(key, mouseButton, type);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = TextureManager.Gameplay;

            // background
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width / 2 - _backgroundRectangle.Width / 2, 21, _backgroundRectangle.Width, _backgroundRectangle.Height),
                _backgroundRectangle,
                Color.White);

            if (_isRemapping)
            {
                UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Remap);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Up, 23, 0);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.UpKey.Name, 23, 0, true);
                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Down, 23 + 15, 1);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.DownKey.Name, 23 + 15, 1, true);
                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Left, 23 + 30, 2);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.LeftKey.Name, 23 + 30, 2, true);
                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Right, 23 + 45, 3);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.RightKey.Name, 23 + 45, 3, true);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Shout, 23 + 60, 4);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.ShoutKey.Name, 23 + 60, 4, true);
                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Interact, 23 + 75, 5);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.ActionKey.Name, 23 + 75, 5, true);
                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Roll, 23 + 90, 6);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.RollKey.Name, 23 + 90, 6, true);
                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Taunt, 23 + 105, 7);
                DrawRightAlignedText(spriteBatch, MouseKeyboardInputsHandler.TauntKey.Name, 23 + 105, 7, true);

                DrawCenteredText(spriteBatch, TextManager.Instance.Restore, 23 + 120, 8);
            }
            else
            {
                UIHelper.Instance.DrawTitle(spriteBatch, TextManager.Instance.Settings);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.LanguageTitle, 23, 0);
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
                    new Rectangle(27, 72, _dashRectangle.Width, _dashRectangle.Height),
                    _dashRectangle,
                    Color.White);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Buttons, 23 + 30 + 24, 3);
                string buttons = "XBOX";
                if (GamePadInputsHandler.PreferredButtonType == ButtonType.PlayStation)
                    buttons = "PLAYSTATION";
                DrawRightAlignedText(spriteBatch, buttons, 23 + 30 + 24, 3, true);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Keyboard, 23 + 30 + 24 + 15, 4);
                DrawRightAlignedText(spriteBatch, TextManager.Instance.Remap, 23 + 30 + 24 + 15, 4, false, true);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Microphone, 23 + 30 + 24 + 30, 5);
                if (SelectedMicrophone == -1)
                    DrawRightAlignedText(spriteBatch, TextManager.Instance.No, 23 + 30 + 24 + 30, 5);
                else
                    DrawRightAlignedText(spriteBatch, _microphoneName[SelectedMicrophone], 23 + 30 + 24 + 30, 5, true);

                spriteBatch.Draw(texture,
                    new Rectangle(27, 126, _dashRectangle.Width, _dashRectangle.Height),
                    _dashRectangle,
                    Color.White);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Music, 23 + 30 + 24 + 30 + 24, 6);
                int musicVolume = (int)Math.Round(MusicVolume * 10);
                DrawVolume(spriteBatch, musicVolume, 135, 6);

                DrawLeftAlignedText(spriteBatch, TextManager.Instance.Sfx, 23 + 30 + 24 + 30 + 24 + 15, 7);
                int sfxVolume = (int)Math.Round(SfxVolume * 10);
                DrawVolume(spriteBatch, sfxVolume, 150, 7);
            }
        }

        private void DrawVolume(SpriteBatch spriteBatch, int volume, int yPos, int option)
        {
            Texture2D texture = TextureManager.Gameplay;

            for (int i = 0; i < 10; i++)
            {
                Color color = _volumeColor;
                if (i < volume && _selectionOption == option)
                    color = Color.White;
                else if (i < volume)
                    color = UIHelper.Instance.NotSelectedColor;

                spriteBatch.Draw(texture,
                    new Rectangle(289 - (_volumeRectangle.Width + 2) * i, yPos, _volumeRectangle.Width, _volumeRectangle.Height),
                    _volumeRectangle,
                    color);
            }

            if (_selectionOption == option)
            {
                if (volume < 10)
                    spriteBatch.Draw(texture,
                        new Rectangle(GreedyKidGame.Width - 35 - (_volumeRectangle.Width + 2) * 10 - UIHelper.Instance.SelectionRectangle.Width, yPos, UIHelper.Instance.SelectionRectangle.Width, UIHelper.Instance.SelectionRectangle.Height),
                        UIHelper.Instance.SelectionRectangle,
                        UIHelper.Instance.SelectedColor,
                        0.0f,
                        Vector2.Zero,
                        SpriteEffects.FlipHorizontally,
                        0.0f);
                if (volume > 0)
                    spriteBatch.Draw(texture,
                        new Rectangle(GreedyKidGame.Width - 35 + 2, yPos, UIHelper.Instance.SelectionRectangle.Width, UIHelper.Instance.SelectionRectangle.Height),
                        UIHelper.Instance.SelectionRectangle,
                        UIHelper.Instance.SelectedColor,
                        0.0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0.0f);
            }
        }

        private void DrawLeftAlignedText(SpriteBatch spriteBatch, string text, int yPos, int option, bool genericFont = false)
        {
            SpriteFont font = (genericFont ? TextManager.Instance.GenericFont : TextManager.Instance.Font);

            spriteBatch.DrawString(font, text, new Vector2(35, yPos), (_selectionOption == option ? Color.White : UIHelper.Instance.NotSelectedColor));

            if (_selectionOption == option)
            {
                Texture2D texture = TextureManager.Gameplay;
                spriteBatch.Draw(texture,
                    new Rectangle(35 - UIHelper.Instance.SelectionRectangle.Width - 2, yPos + 4, UIHelper.Instance.SelectionRectangle.Width, UIHelper.Instance.SelectionRectangle.Height),
                    UIHelper.Instance.SelectionRectangle,
                    UIHelper.Instance.SelectedColor);
            }
        }

        private void DrawRightAlignedText(SpriteBatch spriteBatch, string text, int yPos, int option, bool genericFont = false, bool invertedSelection = false)
        {
            SpriteFont font = (genericFont ? TextManager.Instance.GenericFont : TextManager.Instance.Font);
            Texture2D texture = TextureManager.Gameplay;

            if (_isRemapping && _waitingForInput && _selectionOption == option)
                text = "???";

            int textWidth = (int)font.MeasureString(text).X;

            if (_isRemapping)
            {
                // mask
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width - 35 - textWidth - 2, yPos + 3, textWidth + 4, 9),
                    UIHelper.Instance.PixelRectangle,
                    (_selectionOption == option ? UIHelper.Instance.SelectedColor : _backgroundColor));
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width - 35 - textWidth - 1, yPos + 2, textWidth + 2, 11),
                    UIHelper.Instance.PixelRectangle,
                    (_selectionOption == option ? UIHelper.Instance.SelectedColor : _backgroundColor));                
            }

            Color notSelected = UIHelper.Instance.NotSelectedColor;
            if (_isRemapping)
                notSelected = Color.White;
            spriteBatch.DrawString(font, text, new Vector2(GreedyKidGame.Width - 35 - textWidth, yPos), (_selectionOption == option ? Color.White : notSelected));

            if (_selectionOption == option && !_isRemapping)
            {               
                if (!OptionMin())
                    spriteBatch.Draw(texture,
                        new Rectangle(GreedyKidGame.Width - 35 - textWidth - UIHelper.Instance.SelectionRectangle.Width - 2, yPos + 4, UIHelper.Instance.SelectionRectangle.Width, UIHelper.Instance.SelectionRectangle.Height),
                        UIHelper.Instance.SelectionRectangle,
                        UIHelper.Instance.SelectedColor,
                        0.0f,
                        Vector2.Zero,
                        (invertedSelection ? SpriteEffects.None : SpriteEffects.FlipHorizontally),
                        0.0f);
                if (!OptionMax())
                    spriteBatch.Draw(texture,
                        new Rectangle(GreedyKidGame.Width - 35 + 2, yPos + 4, UIHelper.Instance.SelectionRectangle.Width, UIHelper.Instance.SelectionRectangle.Height),
                        UIHelper.Instance.SelectionRectangle,
                        UIHelper.Instance.SelectedColor,
                        0.0f,
                        Vector2.Zero,
                        (invertedSelection ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
                        0.0f);                
            }
        }

        public void DrawCenteredText(SpriteBatch spriteBatch, string text, int yPos, int option)
        {
            SpriteFont font = TextManager.Instance.Font;

            int textWidth = (int)font.MeasureString(text).X;

            Texture2D texture = TextureManager.Gameplay;

            // mask
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - 2, yPos + 3, textWidth + 4, 9),
                UIHelper.Instance.PixelRectangle,
                UIHelper.Instance.BackgroundColor);
            spriteBatch.Draw(texture,
                new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - 1, yPos + 2, textWidth + 2, 11),
                UIHelper.Instance.PixelRectangle,
                UIHelper.Instance.BackgroundColor);


            spriteBatch.DrawString(font,
                text,
                new Vector2(GreedyKidGame.Width / 2 - textWidth / 2, yPos),
                (_selectionOption == option ? Color.White : UIHelper.Instance.NotSelectedColor));

            if (_selectionOption == option)
            {
                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 - UIHelper.Instance.SelectionRectangle.Width - 2, yPos + 4, UIHelper.Instance.SelectionRectangle.Width, UIHelper.Instance.SelectionRectangle.Height),
                    UIHelper.Instance.SelectionRectangle,
                    UIHelper.Instance.SelectedColor,
                    0.0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.0f);

                spriteBatch.Draw(texture,
                    new Rectangle(GreedyKidGame.Width / 2 - textWidth / 2 + textWidth + 2, yPos + 4, UIHelper.Instance.SelectionRectangle.Width, UIHelper.Instance.SelectionRectangle.Height),
                    UIHelper.Instance.SelectionRectangle,
                    UIHelper.Instance.SelectedColor,
                    0.0f,
                    Vector2.Zero,
                    SpriteEffects.FlipHorizontally,
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

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GreedyKid
{
    public enum AvailableLanguage
    {
        EN,
        FR,
        DE,
        RU,
        SP,
        BR,
        IT,

        Count,
    }

    public sealed class TextManager
    {
        private static TextManager _instance;
        private AvailableLanguage _selectedLanguage = AvailableLanguage.EN;

        private TextManager()
        {

        }

        public static TextManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TextManager();
                return _instance; 
            }
        }

        public SpriteFont Font
        {
            get
            {
                return TextureManager.LatinFont;
            }
        }

        public SpriteFont GenericFont
        {
            get
            {
                return TextureManager.LatinFont;
            }
        }

        // *********************** MAIN SCREEN ***********************

        // 100%

        private string[] _play = new string[] { "PLAY", "JOUER", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Play { get { return _play[(int)_selectedLanguage]; } }

        private string[] _settings = new string[] { "SETTINGS", "PARAMÈTRES", "OPTIONEN", "НАСТРОЙКИ", "AJUSTES", "AJUSTES", "IMPOSTAZIONI" };
        public string Settings { get { return _settings[(int)_selectedLanguage]; } }

        private string[] _quit = new string[] { "QUIT", "QUITTER", "ENDE", "ВЫХОД", "SALIR", "SAIR", "ESCI" };
        public string Quit { get { return _quit[(int)_selectedLanguage]; } }

        // *********************** PLAY SCREEN ***********************

        // 33% campaign, workshop

        private string[] _campaign = new string[] { "MAIN CAMPAIGN", "CAMPAGNE PRINCIPALE", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Campaign { get { return _campaign[(int)_selectedLanguage]; } }

        private string[] _workshop = new string[] { "STEAM WORKSHOP", "WORKSHOP STEAM", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Workshop { get { return _workshop[(int)_selectedLanguage]; } }

        private string[] _back = new string[] { "BACK", "RETOUR", "ZURÜCK", "НАЗАД", "VOLVER", "RETORNAR", "INDIETRO" };
        public string Back { get { return _back[(int)_selectedLanguage]; } }

        // *********************** SETTINGS SCREEN ***********************

        // 95% microphone

        private string[] _language = new string[] { "LANGUAGE", "LANGUE", "SPRACHE", "ЯЗЫК", "IDIOMA", "IDIOMA", "LINGUA" };
        public string Language { get { return _language[(int)_selectedLanguage]; } }
        private string[] _languageValue = new string[] { "ENGLISH", "FRANÇAIS", "DEUTSCH", "РУССКИЙ", "ESPAÑOL", "PORTUGUÊS BRASILEIRO", "ITALIANO" };
        public string LanguageValue { get { return _languageValue[(int)_selectedLanguage]; } }

        private string[] _resolution = new string[] { "RESOLUTION", "RÉSOLUTION", "AUFLÖSUNG", "РАЗРЕШЕНИЕ ЭКРАНА", "RESOLUCIÓN", "RESOLUÇÃO", "RISOLUZIONE" };
        public string Resolution { get { return _resolution[(int)_selectedLanguage]; } }
        private string[] _fullscreen = new string[] { "FULLSCREEN", "PLEIN ÉCRAN", "VOLLBILD", "ВО ВЕСЬ ЭКРАН", "PANTALLA COMPLETA", "TELA CHEIA", "SCHERMO INTERO" };
        public string Fullscreen { get { return _fullscreen[(int)_selectedLanguage]; } }

        private string[] _no = new string[] { "NO", "NON", "NEIN", "НЕТ", "NO", "NÃO", "NO" };
        public string No { get { return _no[(int)_selectedLanguage]; } }
        private string[] _borderless = new string[] { "YES (BORDERLESS)", "OUI (SANS BORD)", "JA (BORDERLESS)", "ДА (БЕЗ РАМОК)", "SÍ (SIN MARCOS)", "SIM (SEM MARCA", "SÌ (SENZA BORDI)" };
        public string Borderless { get { return _borderless[(int)_selectedLanguage]; } }
        private string[] _real = new string[] { "YES (REAL)", "OUI (COMPLET)", "JA (VOLLBILD)", "ДА (СТАНДАРТНЫЙ)", "SÍ (REAL)", "SIM (REAL)", "SÌ (REALE)" };
        public string Real { get { return _real[(int)_selectedLanguage]; } }

        private string[] _buttons = new string[] { "PREFERRED BUTTONS", "PRÉFÉRENCE DE BOUTONS", "BEVORZUGTE TASTENBELEGUNG", "ПРЕДПОЧТИТЕЛЬНЫЕ КНОПКИ", "PREFERENCIAS DE BOTONES", "PREFERÊNCIAS DE BOTÕES", "TASTI PREFERITI" };
        public string Buttons { get { return _buttons[(int)_selectedLanguage]; } }
        private string[] _keyboard = new string[] { "KEYBOARD", "CLAVIER", "KEYBOARD", "КЛАВИАТУРА", "TECLADO", "TECLADO", "TASTIERA" };
        public string Keyboard { get { return _keyboard[(int)_selectedLanguage]; } }
        private string[] _remap = new string[] { "CHANGE MAPPING", "PERSONALISER", "BELEGUNG ÄNDERN", "ИЗМЕНИТЬ НАЗНАЧЕНИЯ", "PERSONALIZAR", "PERSONALIZAR", "CAMBIA MAPPA" };
        public string Remap { get { return _remap[(int)_selectedLanguage]; } }
        private string[] _microphone = new string[] { "MICROPHONE", "MICROPHONE", "BELEGUNG ÄNDERN", "ИЗМЕНИТЬ НАЗНАЧЕНИЯ", "PERSONALIZAR", "PERSONALIZAR", "CAMBIA MAPPA" };
        public string Microphone { get { return _microphone[(int)_selectedLanguage]; } }

        private string[] _music = new string[] { "MUSIC VOLUME", "VOLUME DES MUSIQUES", "MUSIKLAUTSTÄRKE", "ГРОМКОСТЬ МУЗЫКИ", "VOLUMEN DE MÚSICA", "VOLUME DA MÚSICA", "VOLUME MUSICA" };
        public string Music { get { return _music[(int)_selectedLanguage]; } }
        private string[] _sfx = new string[] { "SFX VOLUME", "VOLUME DES EFFETS", "EFFEKTLAUTSTÄRKE", "ГРОМКОСТЬ ЗВУКОВЫХ ЭФФЕКТОВ", "VOLUMEN DE EFECTOS", "VOLUME DOS EFEITOS", "VOLUME EFFETTI SONORI" };
        public string Sfx { get { return _sfx[(int)_selectedLanguage]; } }

    }
}

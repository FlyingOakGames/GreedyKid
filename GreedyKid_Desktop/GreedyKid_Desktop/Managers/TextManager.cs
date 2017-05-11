﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GreedyKid
{
    public enum Language
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
        private Language _language = Language.EN;

        private TextManager()
        {
            DetectStartingLanguage();
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

        public void DetectStartingLanguage()
        {
#if DESKTOP || PLAYSTATION4
            switch (System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName)
            {
                case "en": _language = Language.EN; break;
                case "fr": _language = Language.FR; break;
                case "de": _language = Language.DE; break;
                case "ru": _language = Language.RU; break;
                case "es": _language = Language.SP; break;
                case "pt":
                    if (System.Globalization.CultureInfo.InstalledUICulture.LCID == 1046)
                    {
                        _language = Language.BR;
                    }
                    break;
                case "it": _language = Language.IT; break;
            }
#elif XBOXONE
            switch (PlatformHelper.XboxOne.GetDefaultLocale())
            {
                case 0: _language = Language.EN; break;
                case 1: _language = Language.FR; break;
                case 2: _language = Language.DE; break;
                case 3: _language = Language.RU; break;
                case 4: _language = Language.SP; break;
                case 5: _language = Language.BR; break;
                case 6: _language = Language.IT; break;
            }
#endif
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

        public Language Language
        {
            get { return _language; }
            set {
                _language = value;
                if (_language == Language.Count)
                    _language = Language.Count - 1;
                else if (_language < 0)
                    _language = Language.EN;
            }
        }

        // *********************** MAIN SCREEN ***********************

        // 85%

        private string[] _play = new string[] { "PLAY", "JOUER", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Play { get { return _play[(int)_language]; } }

        private string[] _settings = new string[] { "SETTINGS", "PARAMÈTRES", "OPTIONEN", "НАСТРОЙКИ", "AJUSTES", "AJUSTES", "IMPOSTAZIONI" };
        public string Settings { get { return _settings[(int)_language]; } }

        private string[] _quit = new string[] { "QUIT", "QUITTER", "ENDE", "ВЫХОД", "SALIR", "SAIR", "ESCI" };
        public string Quit { get { return _quit[(int)_language]; } }

        private string[] _select = new string[] { "SELECT", "VALIDER", "WÄHLEN", "ВЫБРАТЬ", "SELECCIONAR", "SELECIONAR", "SELEZIONA" };
        public string Select { get { return _select[(int)_language]; } }

        private string[] _back = new string[] { "BACK", "RETOUR", "ZURÜCK", "НАЗАД", "VOLVER", "RETORNAR", "INDIETRO" };
        public string Back { get { return _back[(int)_language]; } }

        private string[] _press = new string[] { "PRESS", "APPUYER", "ZURÜCK", "НАЗАД", "VOLVER", "RETORNAR", "INDIETRO" };
        public string Press { get { return _press[(int)_language]; } }

        // *********************** PLAY SCREEN ***********************

        // 0% campaign, workshop

        private string[] _campaign = new string[] { "MAIN CAMPAIGN", "CAMPAGNE PRINCIPALE", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Campaign { get { return _campaign[(int)_language]; } }

        private string[] _workshop = new string[] { "STEAM WORKSHOP", "WORKSHOP STEAM", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Workshop { get { return _workshop[(int)_language]; } }        

        // *********************** SETTINGS SCREEN ***********************

        // 95% microphone

        private string[] _languageTitle = new string[] { "LANGUAGE", "LANGUE", "SPRACHE", "ЯЗЫК", "IDIOMA", "IDIOMA", "LINGUA" };
        public string LanguageTitle { get { return _languageTitle[(int)_language]; } }
        private string[] _languageValue = new string[] { "ENGLISH", "FRANÇAIS", "DEUTSCH", "РУССКИЙ", "ESPAÑOL", "PORTUGUÊS BRASILEIRO", "ITALIANO" };
        public string LanguageValue { get { return _languageValue[(int)_language]; } }

        private string[] _resolution = new string[] { "RESOLUTION", "RÉSOLUTION", "AUFLÖSUNG", "РАЗРЕШЕНИЕ ЭКРАНА", "RESOLUCIÓN", "RESOLUÇÃO", "RISOLUZIONE" };
        public string Resolution { get { return _resolution[(int)_language]; } }
        private string[] _fullscreen = new string[] { "FULLSCREEN", "PLEIN ÉCRAN", "VOLLBILD", "ВО ВЕСЬ ЭКРАН", "PANTALLA COMPLETA", "TELA CHEIA", "SCHERMO INTERO" };
        public string Fullscreen { get { return _fullscreen[(int)_language]; } }

        private string[] _no = new string[] { "NO", "NON", "NEIN", "НЕТ", "NO", "NÃO", "NO" };
        public string No { get { return _no[(int)_language]; } }
        private string[] _borderless = new string[] { "YES (BORDERLESS)", "OUI (SANS BORD)", "JA (BORDERLESS)", "ДА (БЕЗ РАМОК)", "SÍ (SIN MARCOS)", "SIM (SEM MARCA", "SÌ (SENZA BORDI)" };
        public string Borderless { get { return _borderless[(int)_language]; } }
        private string[] _real = new string[] { "YES (REAL)", "OUI (COMPLET)", "JA (VOLLBILD)", "ДА (СТАНДАРТНЫЙ)", "SÍ (REAL)", "SIM (REAL)", "SÌ (REALE)" };
        public string Real { get { return _real[(int)_language]; } }

        private string[] _buttons = new string[] { "PREFERRED BUTTONS", "PRÉFÉRENCE DE BOUTONS", "BEVORZUGTE TASTENBELEGUNG", "ПРЕДПОЧТИТЕЛЬНЫЕ КНОПКИ", "PREFERENCIAS DE BOTONES", "PREFERÊNCIAS DE BOTÕES", "TASTI PREFERITI" };
        public string Buttons { get { return _buttons[(int)_language]; } }
        private string[] _keyboard = new string[] { "KEYBOARD", "CLAVIER", "KEYBOARD", "КЛАВИАТУРА", "TECLADO", "TECLADO", "TASTIERA" };
        public string Keyboard { get { return _keyboard[(int)_language]; } }
        private string[] _remap = new string[] { "CHANGE MAPPING", "PERSONALISER", "BELEGUNG ÄNDERN", "ИЗМЕНИТЬ НАЗНАЧЕНИЯ", "PERSONALIZAR", "PERSONALIZAR", "CAMBIA MAPPA" };
        public string Remap { get { return _remap[(int)_language]; } }
        private string[] _microphone = new string[] { "MICROPHONE", "MICROPHONE", "BELEGUNG ÄNDERN", "ИЗМЕНИТЬ НАЗНАЧЕНИЯ", "PERSONALIZAR", "PERSONALIZAR", "CAMBIA MAPPA" };
        public string Microphone { get { return _microphone[(int)_language]; } }

        private string[] _music = new string[] { "MUSIC VOLUME", "VOLUME DES MUSIQUES", "MUSIKLAUTSTÄRKE", "ГРОМКОСТЬ МУЗЫКИ", "VOLUMEN DE MÚSICA", "VOLUME DA MÚSICA", "VOLUME MUSICA" };
        public string Music { get { return _music[(int)_language]; } }
        private string[] _sfx = new string[] { "SFX VOLUME", "VOLUME DES EFFETS", "EFFEKTLAUTSTÄRKE", "ГРОМКОСТЬ ЗВУКОВЫХ ЭФФЕКТОВ", "VOLUMEN DE EFECTOS", "VOLUME DOS EFEITOS", "VOLUME EFFETTI SONORI" };
        public string Sfx { get { return _sfx[(int)_language]; } }

        // *********************** PAUSE SCREEN ***********************

        // 55% shout, interact, roll, taunt        

        private string[] _up = new string[] { "UP", "HAUT", "HOCH", "ВВЕРХ", "ARRIBA", "PARA CIMA", "SU" };
        public string Up { get { return _up[(int)_language]; } }
        private string[] _down = new string[] { "DOWN", "BAS", "RUNTER", "ВНИЗ", "ABAJO", "PARA BAIXO", "GIÙ" };
        public string Down { get { return _down[(int)_language]; } }
        private string[] _left = new string[] { "LEFT", "GAUCHE", "LINKS", "ВЛЕВО", "IZQUIERDA", "ESQUERDA", "SINISTRA" };
        public string Left { get { return _left[(int)_language]; } }
        private string[] _right = new string[] { "RIGHT", "DROITE", "RECHTS", "ВПРАВО", "DERECHA", "DIREITA", "DESTRA" };
        public string Right { get { return _right[(int)_language]; } }

        private string[] _shout = new string[] { "SHOUT", "CRIER", "HOCH", "ВВЕРХ", "ARRIBA", "PARA CIMA", "SU" };
        public string Shout { get { return _shout[(int)_language]; } }
        private string[] _interact = new string[] { "INTERACT", "INTERAGIR", "RUNTER", "ВНИЗ", "ABAJO", "PARA BAIXO", "GIÙ" };
        public string Interact { get { return _interact[(int)_language]; } }
        private string[] _roll = new string[] { "DODGE ROLL", "ROULADE", "LINKS", "ВЛЕВО", "IZQUIERDA", "ESQUERDA", "SINISTRA" };
        public string Roll { get { return _roll[(int)_language]; } }
        private string[] _taunt = new string[] { "TAUNT", "PROVOQUER", "RECHTS", "ВПРАВО", "DERECHA", "DIREITA", "DESTRA" };
        public string Taunt { get { return _taunt[(int)_language]; } }

        private string[] _restore = new string[] { "RESTORE DEFAULT", "RÉ-INITIALISER", "STANDARD WIEDERHERSTELLEN", "ВОCСТАНОВИТЬ ПО УМОЛЧАНИЮ", "REINICIALIZAR", "REINICIALIZAR", "RIPRISTINA IMPOSTAZIONI PREDEFINITE" };
        public string Restore { get { return _restore[(int)_language]; } }

        // *********************** PAUSE SCREEN ***********************

        // 33% pause, restart

        private string[] _pause = new string[] { "PAUSE", "PAUSE", "PAUSE", "PAUSE", "PAUSE", "PAUSE", "PAUSE" };
        public string Pause { get { return _pause[(int)_language]; } }

        private string[] _restart = new string[] { "RESTART", "RESTART", "RESTART", "RESTART", "RESTART", "RESTART", "RESTART" };
        public string Restart { get { return _restart[(int)_language]; } }

        private string[] _resume = new string[] { "RESUME", "REPRENDRE", "WEITER", "ПРОДОЛЖИТЬ", "REANUDAR", "RETOMAR", "RIPRENDI" };
        public string Resume { get { return _resume[(int)_language]; } }
    }
}

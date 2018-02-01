using Microsoft.Xna.Framework.Graphics;

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
                return TextureManager.LatinFont; // depend on the language
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
                if (_language == Language.RU && !_transposedCyrillic)
                    LoadCyrillic();
            }
        }

        private bool _transposedCyrillic = false;

        private string ConvertToGame(string text)
        {
            // force uppercase
            text = text.ToUpperInvariant();
            // transpose cyrillic to decidated font table space
            text = TransposeCyrillic(text);
            // remove anything that isn't in the font table [32-255]
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\u0020-\u00FF]", string.Empty);

            return text.Trim(' ');
        }

        private string TransposeCyrillic(string text)
        {
            System.Text.StringBuilder output = new System.Text.StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == 'Ё')
                    output.Append('Ë');
                else if (text[i] >= 'А' && text[i] <= 'Я')
                    output.Append((char)(text[i] - 912));
                else
                    output.Append(text[i]);
            }

            return output.ToString();
        }

        // *********************** MAIN SCREEN ***********************

        // 100%

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

        private string[] _press = new string[] { "PRESS", "APPUYER", "DRÜCKEN", "НАЖАТЬ", "PULSA", "RETORNAR", "PREMI" };
        public string Press { get { return _press[(int)_language]; } }

        // *********************** PLAY SCREEN ***********************

        // 0% campaign, workshop, custom

        private string[] _campaign = new string[] { "MAIN CAMPAIGN", "CAMPAGNE PRINCIPALE", "HAUPTKAMPAGNE", "ГЛАВНАЯ КАМПАНИЯ", "CAMPAÑA PRINCIPAL", "MAIN CAMPAIGN", "CAMPAGNA PRINCIPALE" };
        public string Campaign { get { return _campaign[(int)_language]; } }

        private string[] _workshop = new string[] { "STEAM WORKSHOP", "WORKSHOP STEAM", "STEAM WORKSHOP", "STEAM WORKSHOP", "STEAM WORKSHOP", "STEAM WORKSHOP", "STEAM WORKSHOP" };
        public string Workshop { get { return _workshop[(int)_language]; } }

        private string[] _custom = new string[] { "COMMUNITY LEVELS", "CAMPAGNES COMMUNAUTAIRES", "COMMUNITY-LEVELS", "УРОВНИ СООБЩЕСТВА", "NIVELES DE LA COMUNIDAD", "STEAM WORKSHOP", "LIVELLI DELLA COMUNITÀ" };
        public string Custom { get { return _custom[(int)_language]; } }

        // *********************** WORKSHOP ***********************

        // 0% workshopNotice1, workshopNotice2, workshopNotice3, workshopNotice4, workshopNotice5, workshopNotice6

        private string[] _workshopNotice1 = new string[] { "THERE'S NO WORKSHOP ITEM", "IL N'Y A AUCUN NIVEAU INSTALLÉ", "ES GIBT KEINE WORKSHOP-GEGENSTÄNDE", "В МАСТЕРСКОЙ ОТСУТСТВУЮТ ЭЛЕМЕНТЫ", "NO HAY ARTÍCULOS DEL WORKSHOP", "THERE'S NO WORKSHOP ITEM", "NON CI SONO OGGETTI NEL WORKSHOP" };
        public string WorkshopNotice1 { get { return _workshopNotice1[(int)_language]; } }

        private string[] _workshopNotice2 = new string[] { "GO TO THE STEAM WORKSHOP", "ALLEZ SUR LE STEAM WORKSHOP ET ABONNEZ VOUS", "GEHE ZUM STEAM WORKSHOP", "ПЕРЕЙТИ В МАСТЕРСКУЮ STEAM", "VE AL STEAM WORKSHOP", "GO TO THE STEAM WORKSHOP", "VAI ALLO STEAM WORKSHOP" };
        public string WorkshopNotice2 { get { return _workshopNotice2[(int)_language]; } }

        private string[] _workshopNotice3 = new string[] { "AND SUBSCRIBE TO ITEMS TO SEE THEM HERE", "À DES NIVEAUX POUR LES VOIR ICI", "UND ABONNIERE GEGENSTÄNDE, UM SIE HIER ZU SEHEN", "И ОФОРМИТЬ ПОДПИСКУ НА ЭЛЕМЕНТЫ, ЧТОБЫ УВИДЕТЬ ИХ ЗДЕСЬ", "Y SUSCRÍBETE A LOS ARTÍCULOS PARA VERLOS AQUÍ", "AND SUBSCRIBE TO ITEMS TO SEE THEM HERE", "E REGISTRATI PER VEDERE GLI OGGETTI QUI" };
        public string WorkshopNotice3 { get { return _workshopNotice3[(int)_language]; } }

        private string[] _workshopNotice4 = new string[] { "THERE'S NO CUSTOM LEVELS", "IL N'Y A AUCUN NIVEAU INSTALLÉ", "ES GIBT KEINE BENUTZERDEFINIERTEN LEVELS", "ПОЛЬЗОВАТЕЛЬСКИЕ УРОВНИ ОТСУТСТВУЮТ", "NO HAY NIVELES PERSONALIZADOS", "THERE'S NO WORKSHOP ITEM", "NON CI SONO LIVELLI PERSONALIZZATI" };
        public string WorkshopNotice4 { get { return _workshopNotice1[(int)_language]; } }

        private string[] _workshopNotice5 = new string[] { "DOWNLOAD AND INSTALL LEVELS", "TÉLÉCHARGEZ ET INSTALLEZ DES NIVEAUX", "DOWNLOADE UND INSTALLIERE LEVELS,", "ЗАГРУЗИТЬ И УСТАНОВИТЬ УРОВНИ,", "DESCARGA E INSTALA NIVELES", "GO TO THE STEAM WORKSHOP", "SCARICA E INSTALLA LIVELLI" };
        public string WorkshopNotice5 { get { return _workshopNotice2[(int)_language]; } }

        private string[] _workshopNotice6 = new string[] { "TO SEE THEM HERE", "POUR LES VOIR ICI", "UM SIE HIER ZU SEHEN", "ЧТОБЫ УВИДЕТЬ ИХ ЗДЕСЬ", "PARA VERLOS AQUÍ", "AND SUBSCRIBE TO ITEMS TO SEE THEM HERE", "PER VEDERLI QUI" };
        public string WorkshopNotice6 { get { return _workshopNotice3[(int)_language]; } }

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
        private string[] _microphone = new string[] { "MICROPHONE", "MICROPHONE", "MIKROFON", "МИКРОФОН", "MICRÓFONO", "MICROPHONE", "MICROFONO" };
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

        private string[] _shout = new string[] { "SHOUT", "CRIER", "RUFEN", "КРИЧАТЬ", "GRITAR", "SHOUT", "URLO" };
        public string Shout { get { return _shout[(int)_language]; } }
        private string[] _interact = new string[] { "INTERACT", "INTERAGIR", "INTERAGIEREN", "ВЗАИМОДЕЙСТВОВАТЬ", "INTERACTUAR", "INTERACT", "INTERAGISCI" };
        public string Interact { get { return _interact[(int)_language]; } }
        private string[] _roll = new string[] { "DODGE ROLL", "ROULADE", "AUSWEICHROLLE", "УВЕРНУТЬСЯ", "ESQUIVAR RODANDO", "DODGE ROLL", "ROTOLA PER SCHIVARE" };
        public string Roll { get { return _roll[(int)_language]; } }
        private string[] _taunt = new string[] { "TAUNT", "PROVOQUER", "VERHÖHNEN", "ДРАЗНИТЬ", "BURLA", "TAUNT", "DISPETTO" };
        public string Taunt { get { return _taunt[(int)_language]; } }

        private string[] _restore = new string[] { "RESTORE DEFAULT", "RÉ-INITIALISER", "STANDARD WIEDERHERSTELLEN", "ВОCСТАНОВИТЬ ПО УМОЛЧАНИЮ", "REINICIALIZAR", "REINICIALIZAR", "RIPRISTINA IMPOSTAZIONI PREDEFINITE" };
        public string Restore { get { return _restore[(int)_language]; } }

        // *********************** PAUSE SCREEN ***********************

        // 33% pause, restart

        private string[] _pause = new string[] { "PAUSE", "PAUSE", "PAUSE", "ПАУЗА", "PAUSA", "PAUSE", "PAUSA" };
        public string Pause { get { return _pause[(int)_language]; } }

        private string[] _restart = new string[] { "RESTART", "RECOMMENCER", "NEUSTART", "ПЕРЕЗАПУСТИТЬ", "RECOMENZAR", "RESTART", "REINIZIALIZZA" };
        public string Restart { get { return _restart[(int)_language]; } }

        private string[] _resume = new string[] { "RESUME", "REPRENDRE", "WEITER", "ПРОДОЛЖИТЬ", "REANUDAR", "RETOMAR", "RIPRENDI" };
        public string Resume { get { return _resume[(int)_language]; } }

        private string[] _gameover = new string[] { "GAME OVER", "FIN DE PARTIE", "GAME OVER", "ИГРА ОКОНЧЕНА", "FIN DE PARTIDA", "FIM DA PARTIDA", "GAME OVER" };
        public string Gameover { get { return _gameover[(int)_language]; } }

        // *********************** INTER LEVEL SCREEN ***********************

        // 0% stageclear, time, money, nextstage, intro, ending1, ending2

        private string[] _stageClear = new string[] { "STAGE CLEAR", "NIVEAU TERMINÉ", "ABSCHNITT ABGESCHLOSSEN", "УРОВЕНЬ ПРОЙДЕН", "NIVEL SUPERADO", "STAGE CLEAR", "LIVELLO SUPERATO" };
        public string StageClear { get { return _stageClear[(int)_language]; } }

        private string[] _nextStage = new string[] { "NEXT STAGE", "NIVEAU SUIVANT", "NÄCHSTER ABSCHNITT", "СЛЕДУЮЩИЙ УРОВЕНЬ", "SIGUIENTE NIVEL", "NEXT STAGE", "PROSSIMO LIVELLO" };
        public string NextStage { get { return _nextStage[(int)_language]; } }

        private string[] _time = new string[] { "TIME  ", "TEMPS  ", "ZEIT  ", "ВРЕМЯ  ", "TIEMPO  ", "TIME  ", "TEMPO  " };
        public string Time { get { return _time[(int)_language]; } }

        private string[] _money = new string[] { "MONEY  ", "ARGENT  ", "GELD  ", "ДЕНЬГИ  ", "DINERO  ", "MONEY  ", "DENARO  " };
        public string Money { get { return _money[(int)_language]; } }

        private string[] _intro = new string[] { "WATCH INTRODUCTION", "REGARDER L'INTRODUCTION", "EINLEITUNG ANSEHEN", "СМОТРЕТЬ ВСТУПЛЕНИЕ", "VER PRESENTACIÓN", "WATCH INTRODUCTION", "GUARDA INTRODUZIONE" };
        public string Intro { get { return _intro[(int)_language]; } }

        private string[] _ending1 = new string[] { "WATCH ENDING", "REGARDER LA FIN", "ENDE ANSEHEN", "СМОТРЕТЬ ОКОНЧАНИЕ", "VER FINAL", "WATCH ENDING", "GUARDA FINALE" };
        public string Ending1 { get { return _ending1[(int)_language]; } }

        private string[] _ending2 = new string[] { "WATCH SECRET ENDING", "REGARDER LA FIN SECRÈTE", "GEHEIMES ENDE ANSEHEN", "СМОТРЕТЬ СЕКРЕТНОЕ ОКОНЧАНИЕ", "VER EL FINAL SECRETO", "WATCH SECRET ENDING", "GUARDA FINALE SEGRETO" };
        public string Ending2 { get { return _ending2[(int)_language]; } }

        // *********************** ENDING ***********************

        private string[] _later = new string[] { "YEARS LATER...", "DES ANNÉES PLUS TARD...", "JAHRE SPÄTER...", "НЕСКОЛЬКО ЛЕТ СПУСТЯ...", "AÑOS DESPUÉS...", "YEARS LATER...", "ANNI DOPO..." };
        public string Later { get { return _later[(int)_language]; } }

        private string[] _theEnd = new string[] { "THE END.", "FIN.", "DAS ENDE.", "КОНЕЦ.", "FIN.", "THE END.", "FINE." };
        public string TheEnd { get { return _theEnd[(int)_language]; } }

        // *********************** LANGUAGE LOADING ***********************

        private void LoadCyrillic()
        {
            if (_transposedCyrillic || _language != Language.RU)
                return;

            _transposedCyrillic = true;

            int i = (int)Language.RU;

            _play[i] = ConvertToGame(_play[i]);
            _settings[i] = ConvertToGame(_settings[i]);
            _quit[i] = ConvertToGame(_quit[i]);
            _select[i] = ConvertToGame(_select[i]);
            _back[i] = ConvertToGame(_back[i]);
            _press[i] = ConvertToGame(_press[i]);

            _campaign[i] = ConvertToGame(_campaign[i]);
            _workshop[i] = ConvertToGame(_workshop[i]);

            _workshopNotice1[i] = ConvertToGame(_workshopNotice1[i]);
            _workshopNotice2[i] = ConvertToGame(_workshopNotice2[i]);
            _workshopNotice3[i] = ConvertToGame(_workshopNotice3[i]);

            _languageTitle[i] = ConvertToGame(_languageTitle[i]);
            _languageValue[i] = ConvertToGame(_languageValue[i]);
            _resolution[i] = ConvertToGame(_resolution[i]);
            _fullscreen[i] = ConvertToGame(_fullscreen[i]);
            _no[i] = ConvertToGame(_no[i]);
            _borderless[i] = ConvertToGame(_borderless[i]);
            _real[i] = ConvertToGame(_real[i]);
            _buttons[i] = ConvertToGame(_buttons[i]);
            _keyboard[i] = ConvertToGame(_keyboard[i]);
            _remap[i] = ConvertToGame(_remap[i]);
            _microphone[i] = ConvertToGame(_microphone[i]);
            _music[i] = ConvertToGame(_music[i]);
            _sfx[i] = ConvertToGame(_sfx[i]);

            _up[i] = ConvertToGame(_up[i]);
            _down[i] = ConvertToGame(_down[i]);
            _left[i] = ConvertToGame(_left[i]);
            _right[i] = ConvertToGame(_right[i]);
            _shout[i] = ConvertToGame(_shout[i]);
            _interact[i] = ConvertToGame(_interact[i]);
            _roll[i] = ConvertToGame(_roll[i]);
            _taunt[i] = ConvertToGame(_taunt[i]);
            _restore[i] = ConvertToGame(_restore[i]);

            _resume[i] = ConvertToGame(_resume[i]);
            _restart[i] = ConvertToGame(_restart[i]);
            _pause[i] = ConvertToGame(_pause[i]);
            _gameover[i] = ConvertToGame(_gameover[i]);

            _stageClear[i] = ConvertToGame(_stageClear[i]);
            _nextStage[i] = ConvertToGame(_nextStage[i]);
            _time[i] = ConvertToGame(_time[i]);
            _money[i] = ConvertToGame(_money[i]);
            _intro[i] = ConvertToGame(_intro[i]);
            _ending1[i] = ConvertToGame(_ending1[i]);
            _ending2[i] = ConvertToGame(_ending2[i]);

            _later[i] = ConvertToGame(_later[i]);
            _theEnd[i] = ConvertToGame(_theEnd[i]);
        }
    }
}

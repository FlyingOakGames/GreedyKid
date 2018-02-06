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
        CN,

        Count,
    }

    public sealed class TextManager
    {
        private static TextManager _instance;
        private Language _language = Language.EN;
        private System.Collections.Generic.List<char> _chineseTable;

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
                case "zh": _language = Language.CN; break;
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
                case 7: _language = Language.CN; break;
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
                    LoadCyrillicAndChinese();
                if (_language == Language.CN && !_transposedChinese)
                    LoadCyrillicAndChinese();
            }
        }

        private bool _transposedCyrillic = false;
        private bool _transposedChinese = false;

        private string ConvertToGame(string text, bool isChinese)
        {
            // force uppercase
            text = text.ToUpperInvariant();
            
            if (isChinese)
            {
                // transpose chinese and build character table
                text = TransposeChinese(text);
            }
            else
            {
                // transpose cyrillic to decidated font table space
                text = TransposeCyrillic(text);
            }

            // remove anything that isn't in the font table [32-383]
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\u0020-\u017F]", string.Empty);

            return text;
        }        

        public string SafeConvert(string text)
        {
            // force uppercase
            text = text.ToUpperInvariant();

            // transpose cyrillic to decidated font table space
            text = TransposeCyrillic(text);

            // remove anything that isn't in the font table [32-255]
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[^\u0020-\u00FF]", string.Empty);

            return text.Trim();
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

        private string TransposeChinese(string text)
        {
            if (_chineseTable == null)
                _chineseTable = new System.Collections.Generic.List<char>(128);

            System.Text.StringBuilder output = new System.Text.StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= 'Ā')
                {
                    if (!_chineseTable.Contains(text[i]))
                        _chineseTable.Add(text[i]);

                    int indexOf = _chineseTable.IndexOf(text[i]);
                    output.Append((char)(256 + indexOf));
                }
                else
                    output.Append(text[i]);
            }

            return output.ToString();
        }

        // *********************** MAIN SCREEN ***********************

        // 100%

        private string[] _play = new string[] { "PLAY", "JOUER", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA", "开始" };
        public string Play { get { return _play[(int)_language]; } }

        private string[] _settings = new string[] { "SETTINGS", "PARAMÈTRES", "OPTIONEN", "НАСТРОЙКИ", "AJUSTES", "CONFIGURAÇÕES", "IMPOSTAZIONI", "设置" };
        public string Settings { get { return _settings[(int)_language]; } }

        private string[] _quit = new string[] { "QUIT", "QUITTER", "ENDE", "ВЫХОД", "SALIR", "SAIR", "ESCI", "退出" };
        public string Quit { get { return _quit[(int)_language]; } }

        private string[] _select = new string[] { "SELECT", "VALIDER", "WÄHLEN", "ВЫБРАТЬ", "SELECCIONAR", "SELECIONAR", "SELEZIONA", "选择" };
        public string Select { get { return _select[(int)_language]; } }

        private string[] _back = new string[] { "BACK", "RETOUR", "ZURÜCK", "НАЗАД", "VOLVER", "RETORNAR", "INDIETRO", "返回" };
        public string Back { get { return _back[(int)_language]; } }

        private string[] _press = new string[] { "PRESS", "APPUYER", "DRÜCKEN", "НАЖАТЬ", "PULSA", "PRESSIONE", "PREMI", "点击" };
        public string Press { get { return _press[(int)_language]; } }

        // *********************** PLAY SCREEN ***********************

        // 0% campaign, workshop, custom

        private string[] _campaign = new string[] { "MAIN CAMPAIGN", "CAMPAGNE PRINCIPALE", "HAUPTKAMPAGNE", "ГЛАВНАЯ КАМПАНИЯ", "CAMPAÑA PRINCIPAL", "CAMPANHA PRINCIPAL", "CAMPAGNA PRINCIPALE", "主要关卡" };
        public string Campaign { get { return _campaign[(int)_language]; } }

        private string[] _workshop = new string[] { "STEAM WORKSHOP", "WORKSHOP STEAM", "STEAM WORKSHOP", "STEAM WORKSHOP", "STEAM WORKSHOP", "STEAM WORKSHOP", "STEAM WORKSHOP", "STEAM WORKSHOP" };
        public string Workshop { get { return _workshop[(int)_language]; } }

        private string[] _custom = new string[] { "COMMUNITY LEVELS", "CAMPAGNES COMMUNAUTAIRES", "COMMUNITY-LEVELS", "УРОВНИ СООБЩЕСТВА", "NIVELES DE LA COMUNIDAD", "NÍVEIS DA COMUNIDADE", "LIVELLI DELLA COMUNITÀ", "社区评级" };
        public string Custom { get { return _custom[(int)_language]; } }

        // *********************** WORKSHOP ***********************

        // 0% workshopNotice1, workshopNotice2, workshopNotice3, workshopNotice4, workshopNotice5, workshopNotice6

        private string[] _workshopNotice1 = new string[] { "THERE'S NO WORKSHOP ITEM", "IL N'Y A AUCUN NIVEAU INSTALLÉ", "ES GIBT KEINE WORKSHOP-GEGENSTÄNDE", "В МАСТЕРСКОЙ ОТСУТСТВУЮТ ЭЛЕМЕНТЫ", "NO HAY ARTÍCULOS DEL WORKSHOP", "NÃO HÁ ITENS DE WORKSHOP", "NON CI SONO OGGETTI NEL WORKSHOP", "没有客户端项目" };
        public string WorkshopNotice1 { get { return _workshopNotice1[(int)_language]; } }

        private string[] _workshopNotice2 = new string[] { "GO TO THE STEAM WORKSHOP", "ALLEZ SUR LE STEAM WORKSHOP ET ABONNEZ VOUS", "GEHE ZUM STEAM WORKSHOP", "ПЕРЕЙТИ В МАСТЕРСКУЮ STEAM", "VE AL STEAM WORKSHOP", "VÁ PARA O STEAM WORKSHOP", "VAI ALLO STEAM WORKSHOP", "到Steam Workshop" };
        public string WorkshopNotice2 { get { return _workshopNotice2[(int)_language]; } }

        private string[] _workshopNotice3 = new string[] { "AND SUBSCRIBE TO ITEMS TO SEE THEM HERE", "À DES NIVEAUX POUR LES VOIR ICI", "UND ABONNIERE GEGENSTÄNDE, UM SIE HIER ZU SEHEN", "И ОФОРМИТЬ ПОДПИСКУ НА ЭЛЕМЕНТЫ, ЧТОБЫ УВИДЕТЬ ИХ ЗДЕСЬ", "Y SUSCRÍBETE A LOS ARTÍCULOS PARA VERLOS AQUÍ", "E INSCREVA-SE NOS ITENS PARA VÊ-LOS AQUI", "E REGISTRATI PER VEDERE GLI OGGETTI QUI", "并在那里订阅项目并查看它们" };
        public string WorkshopNotice3 { get { return _workshopNotice3[(int)_language]; } }

        private string[] _workshopNotice4 = new string[] { "THERE'S NO CUSTOM LEVELS", "IL N'Y A AUCUN NIVEAU INSTALLÉ", "ES GIBT KEINE BENUTZERDEFINIERTEN LEVELS", "ПОЛЬЗОВАТЕЛЬСКИЕ УРОВНИ ОТСУТСТВУЮТ", "NO HAY NIVELES PERSONALIZADOS", "NÃO HÁ NÍVEIS PERSONALIZADOS", "NON CI SONO LIVELLI PERSONALIZZATI", "没有自定义关卡" };
        public string WorkshopNotice4 { get { return _workshopNotice1[(int)_language]; } }

        private string[] _workshopNotice5 = new string[] { "DOWNLOAD AND INSTALL LEVELS", "TÉLÉCHARGEZ ET INSTALLEZ DES NIVEAUX", "DOWNLOADE UND INSTALLIERE LEVELS,", "ЗАГРУЗИТЬ И УСТАНОВИТЬ УРОВНИ,", "DESCARGA E INSTALA NIVELES", "BAIXE E INSTALE NÍVEIS", "SCARICA E INSTALLA LIVELLI", "下载并安装关卡" };
        public string WorkshopNotice5 { get { return _workshopNotice2[(int)_language]; } }

        private string[] _workshopNotice6 = new string[] { "TO SEE THEM HERE", "POUR LES VOIR ICI", "UM SIE HIER ZU SEHEN", "ЧТОБЫ УВИДЕТЬ ИХ ЗДЕСЬ", "PARA VERLOS AQUÍ", "APARA VÊ-LOS AQUI", "PER VEDERLI QUI", "在这里查看" };
        public string WorkshopNotice6 { get { return _workshopNotice3[(int)_language]; } }

        // *********************** SETTINGS SCREEN ***********************

        // 95% microphone

        private string[] _languageTitle = new string[] { "LANGUAGE", "LANGUE", "SPRACHE", "ЯЗЫК", "IDIOMA", "IDIOMA", "LINGUA", "语言" };
        public string LanguageTitle { get { return _languageTitle[(int)_language]; } }
        private string[] _languageValue = new string[] { "ENGLISH", "FRANÇAIS", "DEUTSCH", "РУССКИЙ", "ESPAÑOL", "PORTUGUÊS BRASILEIRO", "ITALIANO", "简体中文" };
        public string LanguageValue { get { return _languageValue[(int)_language]; } }

        private string[] _resolution = new string[] { "RESOLUTION", "RÉSOLUTION", "AUFLÖSUNG", "РАЗРЕШЕНИЕ ЭКРАНА", "RESOLUCIÓN", "RESOLUÇÃO", "RISOLUZIONE", "分辨率" };
        public string Resolution { get { return _resolution[(int)_language]; } }
        private string[] _fullscreen = new string[] { "FULLSCREEN", "PLEIN ÉCRAN", "VOLLBILD", "ВО ВЕСЬ ЭКРАН", "PANTALLA COMPLETA", "TELA CHEIA", "SCHERMO INTERO", "全屏" };
        public string Fullscreen { get { return _fullscreen[(int)_language]; } }

        private string[] _no = new string[] { "NO", "NON", "NEIN", "НЕТ", "NO", "NÃO", "NO", "否" };
        public string No { get { return _no[(int)_language]; } }
        private string[] _borderless = new string[] { "YES (BORDERLESS)", "OUI (SANS BORD)", "JA (BORDERLESS)", "ДА (БЕЗ РАМОК)", "SÍ (SIN MARCOS)", "SIM (SEM BORDA)", "SÌ (SENZA BORDI)", "是（无边框）" };
        public string Borderless { get { return _borderless[(int)_language]; } }
        private string[] _real = new string[] { "YES (REAL)", "OUI (COMPLET)", "JA (VOLLBILD)", "ДА (СТАНДАРТНЫЙ)", "SÍ (REAL)", "SIM (REAL)", "SÌ (REALE)", "是（真实）" };
        public string Real { get { return _real[(int)_language]; } }

        private string[] _buttons = new string[] { "PREFERRED BUTTONS", "PRÉFÉRENCE DE BOUTONS", "BEVORZUGTE TASTENBELEGUNG", "ПРЕДПОЧТИТЕЛЬНЫЕ КНОПКИ", "PREFERENCIAS DE BOTONES", "PREFERÊNCIAS DE BOTÕES", "TASTI PREFERITI", "推荐按钮" };
        public string Buttons { get { return _buttons[(int)_language]; } }
        private string[] _keyboard = new string[] { "KEYBOARD", "CLAVIER", "KEYBOARD", "КЛАВИАТУРА", "TECLADO", "TECLADO", "TASTIERA", "键盘" };
        public string Keyboard { get { return _keyboard[(int)_language]; } }
        private string[] _remap = new string[] { "CHANGE MAPPING", "PERSONALISER", "BELEGUNG ÄNDERN", "ИЗМЕНИТЬ НАЗНАЧЕНИЯ", "PERSONALIZAR", "PERSONALIZAR", "CAMBIA MAPPA", "更改地图" };
        public string Remap { get { return _remap[(int)_language]; } }
        private string[] _microphone = new string[] { "MICROPHONE", "MICROPHONE", "MIKROFON", "МИКРОФОН", "MICRÓFONO", "MICROFONE", "MICROFONO", "耳机" };
        public string Microphone { get { return _microphone[(int)_language]; } }

        private string[] _music = new string[] { "MUSIC VOLUME", "VOLUME DES MUSIQUES", "MUSIKLAUTSTÄRKE", "ГРОМКОСТЬ МУЗЫКИ", "VOLUMEN DE MÚSICA", "VOLUME DA MÚSICA", "VOLUME MUSICA", "音乐音量" };
        public string Music { get { return _music[(int)_language]; } }
        private string[] _sfx = new string[] { "SFX VOLUME", "VOLUME DES EFFETS", "EFFEKTLAUTSTÄRKE", "ГРОМКОСТЬ ЗВУКОВЫХ ЭФФЕКТОВ", "VOLUMEN DE EFECTOS", "VOLUME DOS EFEITOS", "VOLUME EFFETTI SONORI", "音效音量" };
        public string Sfx { get { return _sfx[(int)_language]; } }

        // *********************** PAUSE SCREEN ***********************

        // 55% shout, interact, roll, taunt        

        private string[] _up = new string[] { "UP", "HAUT", "HOCH", "ВВЕРХ", "ARRIBA", "PARA CIMA", "SU", "上" };
        public string Up { get { return _up[(int)_language]; } }
        private string[] _down = new string[] { "DOWN", "BAS", "RUNTER", "ВНИЗ", "ABAJO", "PARA BAIXO", "GIÙ", "下" };
        public string Down { get { return _down[(int)_language]; } }
        private string[] _left = new string[] { "LEFT", "GAUCHE", "LINKS", "ВЛЕВО", "IZQUIERDA", "ESQUERDA", "SINISTRA", "左" };
        public string Left { get { return _left[(int)_language]; } }
        private string[] _right = new string[] { "RIGHT", "DROITE", "RECHTS", "ВПРАВО", "DERECHA", "DIREITA", "DESTRA", "右" };
        public string Right { get { return _right[(int)_language]; } }

        private string[] _shout = new string[] { "SHOUT", "CRIER", "RUFEN", "КРИЧАТЬ", "GRITAR", "GRITAR", "URLO", "喊叫" };
        public string Shout { get { return _shout[(int)_language]; } }
        private string[] _interact = new string[] { "INTERACT", "INTERAGIR", "INTERAGIEREN", "ВЗАИМОДЕЙСТВОВАТЬ", "INTERACTUAR", "INTERAGIR", "INTERAGISCI", "互动" };
        public string Interact { get { return _interact[(int)_language]; } }
        private string[] _roll = new string[] { "DODGE ROLL", "ROULADE", "AUSWEICHROLLE", "УВЕРНУТЬСЯ", "ESQUIVAR RODANDO", "ESQUIVAR RODANDO", "ROTOLA PER SCHIVARE", "躲避打滚" };
        public string Roll { get { return _roll[(int)_language]; } }
        private string[] _taunt = new string[] { "TAUNT", "PROVOQUER", "VERHÖHNEN", "ДРАЗНИТЬ", "BURLA", "INSULTO", "DISPETTO", "辱骂" };
        public string Taunt { get { return _taunt[(int)_language]; } }

        private string[] _restore = new string[] { "RESTORE DEFAULT", "RÉ-INITIALISER", "STANDARD WIEDERHERSTELLEN", "ВОCСТАНОВИТЬ ПО УМОЛЧАНИЮ", "REINICIALIZAR", "REINICIALIZAR", "RIPRISTINA IMPOSTAZIONI PREDEFINITE", "重新初始化" };
        public string Restore { get { return _restore[(int)_language]; } }

        // *********************** PAUSE SCREEN ***********************

        // 33% pause, restart

        private string[] _pause = new string[] { "PAUSE", "PAUSE", "PAUSE", "ПАУЗА", "PAUSA", "PAUSAR", "PAUSA", "暂停" };
        public string Pause { get { return _pause[(int)_language]; } }

        private string[] _restart = new string[] { "RESTART", "RECOMMENCER", "NEUSTART", "ПЕРЕЗАПУСТИТЬ", "RECOMENZAR", "REINICIAR", "REINIZIALIZZA", "重启" };
        public string Restart { get { return _restart[(int)_language]; } }

        private string[] _resume = new string[] { "RESUME", "REPRENDRE", "WEITER", "ПРОДОЛЖИТЬ", "REANUDAR", "RETOMAR", "RIPRENDI", "重新开始" };
        public string Resume { get { return _resume[(int)_language]; } }

        private string[] _gameover = new string[] { "GAME OVER", "FIN DE PARTIE", "GAME OVER", "ИГРА ОКОНЧЕНА", "FIN DE PARTIDA", "FIM DO JOGO", "GAME OVER", "游戏结束" };
        public string Gameover { get { return _gameover[(int)_language]; } }

        // *********************** INTER LEVEL SCREEN ***********************

        // 0% stageclear, time, money, nextstage, intro, ending1, ending2

        private string[] _stageClear = new string[] { "STAGE CLEAR", "NIVEAU TERMINÉ", "ABSCHNITT ABGESCHLOSSEN", "УРОВЕНЬ ПРОЙДЕН", "NIVEL SUPERADO", "NÍVEL COMPLETADO", "LIVELLO SUPERATO", "直接过关" };
        public string StageClear { get { return _stageClear[(int)_language]; } }

        private string[] _nextStage = new string[] { "NEXT STAGE", "NIVEAU SUIVANT", "NÄCHSTER ABSCHNITT", "СЛЕДУЮЩИЙ УРОВЕНЬ", "SIGUIENTE NIVEL", "PRÓXIMO NÍVEL", "PROSSIMO LIVELLO", "下一关" };
        public string NextStage { get { return _nextStage[(int)_language]; } }

        private string[] _time = new string[] { "TIME  ", "TEMPS  ", "ZEIT  ", "ВРЕМЯ  ", "TIEMPO  ", "TEMPO  ", "TEMPO  ", "时间  " };
        public string Time { get { return _time[(int)_language]; } }

        private string[] _money = new string[] { "MONEY  ", "ARGENT  ", "GELD  ", "ДЕНЬГИ  ", "DINERO  ", "DINHEIRO  ", "DENARO  ", "钱  " };
        public string Money { get { return _money[(int)_language]; } }

        private string[] _intro = new string[] { "WATCH INTRODUCTION", "REGARDER L'INTRODUCTION", "EINLEITUNG ANSEHEN", "СМОТРЕТЬ ВСТУПЛЕНИЕ", "VER PRESENTACIÓN", "ASSISTIR INTRODUÇÃO", "GUARDA INTRODUZIONE", "观看介绍" };
        public string Intro { get { return _intro[(int)_language]; } }

        private string[] _ending1 = new string[] { "WATCH ENDING", "REGARDER LA FIN", "ENDE ANSEHEN", "СМОТРЕТЬ ОКОНЧАНИЕ", "VER FINAL", "ASSISTIR FINAL", "GUARDA FINALE", "观看结局" };
        public string Ending1 { get { return _ending1[(int)_language]; } }

        private string[] _ending2 = new string[] { "WATCH SECRET ENDING", "REGARDER LA FIN SECRÈTE", "GEHEIMES ENDE ANSEHEN", "СМОТРЕТЬ СЕКРЕТНОЕ ОКОНЧАНИЕ", "VER EL FINAL SECRETO", "ASSISTIR FINAL SECRETO", "GUARDA FINALE SEGRETO", "观看神秘的结局" };
        public string Ending2 { get { return _ending2[(int)_language]; } }

        // *********************** ENDING ***********************

        private string[] _later = new string[] { "YEARS LATER...", "DES ANNÉES PLUS TARD...", "JAHRE SPÄTER...", "НЕСКОЛЬКО ЛЕТ СПУСТЯ...", "AÑOS DESPUÉS...", "ANOS DEPOIS...", "ANNI DOPO...", "数年后..." };
        public string Later { get { return _later[(int)_language]; } }

        private string[] _theEnd = new string[] { "THE END.", "FIN.", "DAS ENDE.", "КОНЕЦ.", "FIN.", "FIM.", "FINE.", "结束。" };
        public string TheEnd { get { return _theEnd[(int)_language]; } }

        // *********************** LANGUAGE LOADING ***********************

        private void LoadCyrillicAndChinese()
        {
            if (_language == Language.RU && _transposedCyrillic)
                return;
            if (_language == Language.CN && _transposedChinese)
                return;

            if (_language == Language.RU)
                _transposedCyrillic = true;
            if (_language == Language.CN)
                _transposedChinese = true;

            int i = (int)_language;

            bool isChinese = (_language == Language.CN);

            _play[i] = ConvertToGame(_play[i], isChinese);
            _settings[i] = ConvertToGame(_settings[i], isChinese);
            _quit[i] = ConvertToGame(_quit[i], isChinese);
            _select[i] = ConvertToGame(_select[i], isChinese);
            _back[i] = ConvertToGame(_back[i], isChinese);
            _press[i] = ConvertToGame(_press[i], isChinese);

            _campaign[i] = ConvertToGame(_campaign[i], isChinese);
            _workshop[i] = ConvertToGame(_workshop[i], isChinese);
            _custom[i] = ConvertToGame(_custom[i], isChinese);

            _workshopNotice1[i] = ConvertToGame(_workshopNotice1[i], isChinese);
            _workshopNotice2[i] = ConvertToGame(_workshopNotice2[i], isChinese);
            _workshopNotice3[i] = ConvertToGame(_workshopNotice3[i], isChinese);
            _workshopNotice4[i] = ConvertToGame(_workshopNotice4[i], isChinese);
            _workshopNotice5[i] = ConvertToGame(_workshopNotice5[i], isChinese);
            _workshopNotice6[i] = ConvertToGame(_workshopNotice6[i], isChinese);

            _languageTitle[i] = ConvertToGame(_languageTitle[i], isChinese);
            _languageValue[i] = ConvertToGame(_languageValue[i], isChinese);
            _resolution[i] = ConvertToGame(_resolution[i], isChinese);
            _fullscreen[i] = ConvertToGame(_fullscreen[i], isChinese);
            _no[i] = ConvertToGame(_no[i], isChinese);
            _borderless[i] = ConvertToGame(_borderless[i], isChinese);
            _real[i] = ConvertToGame(_real[i], isChinese);
            _buttons[i] = ConvertToGame(_buttons[i], isChinese);
            _keyboard[i] = ConvertToGame(_keyboard[i], isChinese);
            _remap[i] = ConvertToGame(_remap[i], isChinese);
            _microphone[i] = ConvertToGame(_microphone[i], isChinese);
            _music[i] = ConvertToGame(_music[i], isChinese);
            _sfx[i] = ConvertToGame(_sfx[i], isChinese);

            _up[i] = ConvertToGame(_up[i], isChinese);
            _down[i] = ConvertToGame(_down[i], isChinese);
            _left[i] = ConvertToGame(_left[i], isChinese);
            _right[i] = ConvertToGame(_right[i], isChinese);
            _shout[i] = ConvertToGame(_shout[i], isChinese);
            _interact[i] = ConvertToGame(_interact[i], isChinese);
            _roll[i] = ConvertToGame(_roll[i], isChinese);
            _taunt[i] = ConvertToGame(_taunt[i], isChinese);
            _restore[i] = ConvertToGame(_restore[i], isChinese);

            _resume[i] = ConvertToGame(_resume[i], isChinese);
            _restart[i] = ConvertToGame(_restart[i], isChinese);
            _pause[i] = ConvertToGame(_pause[i], isChinese);
            _gameover[i] = ConvertToGame(_gameover[i], isChinese);

            _stageClear[i] = ConvertToGame(_stageClear[i], isChinese);
            _nextStage[i] = ConvertToGame(_nextStage[i], isChinese);
            _time[i] = ConvertToGame(_time[i], isChinese);
            _money[i] = ConvertToGame(_money[i], isChinese);
            _intro[i] = ConvertToGame(_intro[i], isChinese);
            _ending1[i] = ConvertToGame(_ending1[i], isChinese);
            _ending2[i] = ConvertToGame(_ending2[i], isChinese);

            _later[i] = ConvertToGame(_later[i], isChinese);
            _theEnd[i] = ConvertToGame(_theEnd[i], isChinese);

#if DEBUG
            if (_language == Language.CN && _chineseTable != null)
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter("chinese_table.txt", false, System.Text.Encoding.UTF8))
                {
                    for (int j = 0; j < _chineseTable.Count; j++)
                    {
                        if (j % 16 == 0)
                            writer.WriteLine("");
                        writer.Write(_chineseTable[j] + "  ");
                    }
                }                
            }
#endif
        }
    }
}

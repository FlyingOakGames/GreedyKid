using Microsoft.Xna.Framework.Graphics;
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

        private string[] _play = new string[] { "PLAY", "JOUER", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Play { get { return _play[(int)_language]; } }

        private string[] _settings = new string[] { "SETTINGS", "PARAMÈTRES", "OPTIONEN", "НАСТРОЙКИ", "AJUSTES", "AJUSTES", "IMPOSTAZIONI" };
        public string Settings { get { return _settings[(int)_language]; } }

        private string[] _quit = new string[] { "QUIT", "QUITTER", "ENDE", "ВЫХОД", "SALIR", "SAIR", "ESCI" };
        public string Quit { get { return _quit[(int)_language]; } }

        // *********************** PLAY SCREEN ***********************

        private string[] _campaign = new string[] { "MAIN CAMPAIGN", "CAMPAGNE PRINCIPALE", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Campaign { get { return _campaign[(int)_language]; } }

        private string[] _workshop = new string[] { "STEAM WORKSHOP", "WORKSHOP STEAM", "SPIELEN", "ИГРАТЬ", "JUGAR", "JOGAR", "GIOCA" };
        public string Workshop { get { return _workshop[(int)_language]; } }

        private string[] _back = new string[] { "BACK", "RETOUR", "ZURÜCK", "НАЗАД", "VOLVER", "RETORNAR", "INDIETRO" };
        public string Back { get { return _back[(int)_language]; } }
    }
}

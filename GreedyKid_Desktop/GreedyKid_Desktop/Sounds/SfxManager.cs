using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GreedyKid
{
    public enum Sfx
    {
        // splash only
        SplashBonus,

        // gameplay
        MenuBlip,
        SoundTest,

        Count,
    }

    public sealed class SfxManager
    {
        private static SfxManager _instance;

        public static SfxManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SfxManager();
                return _instance;
            }
        }

        private const string _sfxPath = "Sfx/";
        private SoundEffect[] _sfx;

        private float _volume = 1.0f;

        private SfxManager()
        {
            _sfx = new SoundEffect[(int)Sfx.Count];
            SetVolume(1.0f);
        }

        public static ContentManager Content
        {
            get;
            set;
        }

        public void LoadSplashSfx()
        {
            LoadSfx((int)Sfx.SplashBonus);
        }

        public void LoadGameplaySfx()
        {
            for (int i = (int)Sfx.MenuBlip; i < (int)Sfx.Count; i++)
            {
                LoadSfx(i);
            }
        }

        private void LoadSfx(int id)
        {
            if (_sfx[id] == null)
            {
                string fileName = "";
#if XBOXONE
                fileName = EnumToString.Sfx[id];
#else
                fileName = ((Sfx)id).ToString();
#endif
                _sfx[id] = Content.Load<SoundEffect>(_sfxPath + fileName);
            }
        }

        public void Unload()
        {
            for (int i = 0; i < _sfx.Length; i++)
            {
                if (_sfx[i] != null)
                {
                    _sfx[i].Dispose();
                    _sfx[i] = null;
                }
            }            
        }

        public void SetVolume(float volume)
        {
            _volume = volume;
        }

        public void Play(Sfx sfx)
        {
            int id = (int)sfx;
            if (_sfx[id] != null)
            {
                try
                {
                    _sfx[id].Play(_volume, 0.0f, 0.0f);
                }
                catch (InstancePlayLimitException)
                {
                    // silently catch instance limit
                }
            }
        }
    }
}

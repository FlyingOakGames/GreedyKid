using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;

namespace GreedyKid
{
    public enum Sfx
    {
        // splash only
        SplashBonus,

        // gameplay
        MenuBlip,
        SoundTest,

        DoorOpen,
        DoorClose,
        DoorOpenClose,
        ElevatorOpen,

        Hide,
        Show,
        PlayerRoll,
        Taunt1,
        Taunt2,
        Shout1,
        Shout2,
        Shout3,
        Shout4,
        Shout5,
        Hit,
        Cry,
        HeavyHit,
        TaserHit,
        SwatHit,
        RoboHit,
        MoneyGrab,
        MoneyLoot,
        HealthPack,

        CopRoll,
        CopSurprise,
        CopSwing,
        CopTaser,
        
        WidowsBreak,
        GrapplingUp,
        GrapplingDown,
        SwatFire,

        RoboLanding,
        RoboUp,
        RoboDown,
        RoboFire,

        RAngryH,
        RAngryF,
        RBooH,
        RBooF,
        RKOH,
        RKOF,
        Revive,

        NAngryH,
        NAngryF,
        NBooH,
        NBooF,
        NKOH,
        NKOF,
        Help,

        TV,

        OneStar,
        TwoStars,
        ThreeStars,

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

        public void LoadGameplaySfx(bool force = false)
        {
            for (int i = (int)Sfx.MenuBlip; i < (int)Sfx.Count; i++)
            {
                LoadSfx(i, force);
            }
        }

        private void LoadSfx(int id, bool force = false)
        {
            if (_sfx[id] == null || force)
            {
                string fileName = "";
#if XBOXONE
                fileName = EnumToString.Sfx[id];
#else
                fileName = ((Sfx)id).ToString();
#endif

                if (force && _sfx[id] != null)
                {
                    // was it a wav?
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "Content/" + _sfxPath);
                    if (File.Exists(path + fileName + ".wav")) // MacOS hack
                    {
                        _sfx[id].Dispose();
                        _sfx[id] = null;
                        LoadSfxDebug(id, fileName);
                        return;
                    }
                    else
                    {
                        path = Content.RootDirectory + "/" + _sfxPath + fileName + ".wav";
                        if (File.Exists(path)) // MacOS hack
                        {
                            _sfx[id].Dispose();
                            _sfx[id] = null;
                            LoadSfxDebug(id, fileName);
                            return;
                        }
                    }
                }

                try
                {
                    _sfx[id] = Content.Load<SoundEffect>(_sfxPath + fileName);
                }
                catch (Exception)
                {
                    // development fallback
                    LoadSfxDebug(id, fileName);
                }
            }
        }

        private void LoadSfxDebug(int id, string fileName)
        {
            string path = Content.RootDirectory + "/" + _sfxPath;
            if (!File.Exists(path + fileName + ".wav")) // MacOS hack
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "Content/" + _sfxPath);
            }
            _sfx[id] = SoundEffect.FromStream(File.OpenRead(path + fileName + ".wav"));
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

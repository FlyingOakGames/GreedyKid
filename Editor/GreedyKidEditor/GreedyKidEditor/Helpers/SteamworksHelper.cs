using System;
using Steamworks;

namespace GreedyKidEditor.Helpers
{
    public enum SteamworksReturn
    {
        SteamworksNotAvailable,
        RestartingThroughSteam,
        Ok,
        CantInit
    }

    public sealed class SteamworksHelper
    {
        private static SteamworksHelper _instance;

        public static SteamworksHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SteamworksHelper();
                return _instance;
            }
        }

        private SteamworksHelper()
        {

        }

        // ************************* STEAM INIT *************************

        public bool _steamworksReady = false;

        private CGameID m_GameID;

        public SteamworksReturn Init()
        {
            _steamworksReady = false;

            InitDllDirectory();

            if (!Packsize.Test())
            {
                Console.WriteLine("STEAM: Wrong Packsize");
                return SteamworksReturn.SteamworksNotAvailable;
            }

            if (!DllCheck.Test())
            {
                Console.WriteLine("STEAM: Wrong Dll");
                return SteamworksReturn.SteamworksNotAvailable;
            }

            try
            {
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(770630)))
                {
                    Console.WriteLine("STEAM: Steam not launched, restarting");
                    return SteamworksReturn.RestartingThroughSteam;
                }
                Console.WriteLine("STEAM: Steam launched, all clear");
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("STEAM: Can't find Dll");
                return SteamworksReturn.SteamworksNotAvailable;
            }
            catch (Exception)
            {
                return SteamworksReturn.SteamworksNotAvailable;
            }

            try
            {
                if (!SteamAPI.Init())
                {
                    Console.WriteLine("STEAM: Oh, looks like Steam isn't launched after all, or couldn't be found.");
                    return SteamworksReturn.CantInit;
                }
            }
            catch (Exception)
            {
                return SteamworksReturn.SteamworksNotAvailable;
            }

            Console.WriteLine("STEAM: Everything is fine");

            _steamworksReady = true;

            m_GameID = new CGameID(SteamUtils.GetAppID());

            if (!SteamUserStats.RequestCurrentStats())
            {
                _shouldRetryRequestStats = true;
                _currentRetryRequestStatsTime = 0.0f;
            }

            return SteamworksReturn.Ok;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        private void InitDllDirectory()
        {
            PlatformID pid = Environment.OSVersion.Platform;
            bool isWindows = false;
            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    isWindows = true;
                    break;
                default: isWindows = false; break;
            }

            if (isWindows)
            {
                string executingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (Environment.Is64BitProcess)
                {
                    SetDllDirectory(System.IO.Path.Combine(executingDirectory, "x64"));
                }
                else
                {
                    SetDllDirectory(System.IO.Path.Combine(executingDirectory, "x86"));
                }
            }
        }

        // ************************* STEAM UPDATE *************************

        private float _currentCallbackTime = 0.0f;
        private const float _callbackTime = 0.5f;

        private bool _shouldRetryRequestStats = false;
        private float _currentRetryRequestStatsTime = 0.0f;
        private const float _retryRequestStatsTime = 5.0f;

        private bool _shouldRequestStats = false;

        public void Update(float gameTime)
        {
            if (_steamworksReady)
            {
                _currentCallbackTime += gameTime;
                if (_currentCallbackTime > _callbackTime)
                {
                    _currentCallbackTime -= _callbackTime;
                    RunCallbacks();
                }

                if (_shouldRetryRequestStats)
                {
                    _currentRetryRequestStatsTime += gameTime;
                    if (_currentRetryRequestStatsTime > _retryRequestStatsTime)
                    {
                        _currentRetryRequestStatsTime = 0.0f;
                        _shouldRequestStats = true;
                    }
                }
            }
            if (_callbackException != null)
            {
                _callbackException = null;
                Console.WriteLine("STEAM crashed: " + _callbackException.Message);
                //ResetWithError();
            }
        }

        private Exception _callbackException = null;

        private void RunCallbacks()
        {
            if (_steamworksReady)
            {
                try
                {
                    SteamAPI.RunCallbacks();

                    if (_shouldRequestStats)
                    {
                        if (!SteamUserStats.RequestCurrentStats())
                        {
                            _shouldRetryRequestStats = true;
                            _currentRetryRequestStatsTime = 0.0f;
                        }
                    }
                }
                catch (Exception e)
                {
                    _callbackException = e;
                }
            }
        }    
    }
}

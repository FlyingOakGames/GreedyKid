// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
#if STEAM

using System;
using Steamworks;

namespace GreedyKid.Helper
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

        private const uint _appID = 0; // put your own Steam AppID here

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

        private bool _steamworksReady = false;

        private CGameID m_GameID;

        public bool IsReady
        {
            get { return _steamworksReady; }
        }

        public SteamworksReturn Init()
        {
            _steamworksReady = false;

            /*
            if (Program.EditorMode)
            {
                Console.WriteLine("STEAM: Starting from editor");
                return SteamworksReturn.SteamworksNotAvailable;
            }
            */

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
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(_appID)))
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

            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

            m_Achievements = new Achievement_t[(int)Achievement.COUNT];
            for (int i = 0; i < (int)Achievement.COUNT; i++)
                m_Achievements[i] = new Achievement_t((Achievement)i, "", "");            

            m_GameID = new CGameID(SteamUtils.GetAppID());

            if (!SteamUserStats.RequestCurrentStats())
            {
                _shouldRetryRequestStats = true;
                _currentRetryRequestStatsTime = 0.0f;
            }

            return SteamworksReturn.Ok;
        }

        // ************************* STEAM UPDATE *************************

        private float _currentCallbackTime = 0.0f;
        private const float _callbackTime = 0.5f;

        private bool _shouldRetrySendStats = false;
        private float _currentRetrySendStatsTime = 0.0f;
        private const float _retrySendStatsTime = 10.0f;

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

                if (_shouldRetrySendStats)
                {
                    _currentRetrySendStatsTime += gameTime;
                    if (_currentRetrySendStatsTime > _retrySendStatsTime)
                    {
                        _currentRetrySendStatsTime = 0.0f;
                        _shouldStoreStats = true;
                    }
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
                    else if (_shouldStoreStats)
                        SendStats();
                }
                catch (Exception e)
                {
                    _callbackException = e;
                }
            }
        }

        // ************************* STEAM WORKSHOP *************************

        private string _macPath = null;

        public string WorkshopPath
        {
            get
            {
                if (_macPath == null)
                    _macPath = System.IO.Path.Combine("..", "..", "..", "..", "..", "workshop/content/" + _appID + "/");
                if (System.IO.Directory.Exists(_macPath))
                    return _macPath;
                return "../../workshop/content/" + _appID + "/";
            }
        }

        // ************ ACHIEVEMENTS ****************

        private Callback<UserStatsReceived_t> m_UserStatsReceived;
        private Callback<UserStatsStored_t> m_UserStatsStored;
        private Callback<UserAchievementStored_t> m_UserAchievementStored;

        private bool _shouldStoreStats = false;

        private void SendStats()
        {
            if (_steamworksReady && _shouldStoreStats)
            {
                _shouldStoreStats = false;
                if (!SteamUserStats.StoreStats())
                {
                    // failed, try again later
                    _shouldRetrySendStats = true;
                    _currentRetrySendStatsTime = 0.0f;
                }
            }
        }

        public void SetAchievement(Achievement id)
        {
            if (_steamworksReady && !m_Achievements[(int)id].m_bAchieved)
            {
                m_Achievements[(int)id].m_bAchieved = true;
                SteamUserStats.SetAchievement(id.ToString());
                _shouldStoreStats = true;
            }
        }

        private void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            if (!_steamworksReady)
                return;

            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    // load achievements
                    foreach (Achievement_t ach in m_Achievements)
                    {
                        bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
                        if (ret)
                        {
                            ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
                            ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
                        }
                        else
                        {
                            // failed to get achievement
                            _shouldRetryRequestStats = true;
                            _currentRetryRequestStatsTime = 0.0f;
                        }
                    }

                    // load stats

                }
                else
                {
                    _shouldRetryRequestStats = true;
                    _currentRetryRequestStatsTime = 0.0f;
                }
            }
        }

        private void OnUserStatsStored(UserStatsStored_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    // ok
                }
                else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
                {
                    // One or more stats we set broke a constraint. They've been reverted,
                    // and we should re-iterate the values now to keep in sync.

                    // Fake up a callback here so that we re-load the values.
                    UserStatsReceived_t callback = new UserStatsReceived_t();
                    callback.m_eResult = EResult.k_EResultOK;
                    callback.m_nGameID = (ulong)m_GameID;
                    OnUserStatsReceived(callback);
                }
                else
                {
                    // bug
                    _shouldRetrySendStats = true;
                }
            }
        }

        private void OnAchievementStored(UserAchievementStored_t pCallback)
        {
            // We may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (0 == pCallback.m_nMaxProgress)
                {
                    // achievement unlocked                    
                }
                else
                {
                    // achievement progressed, but not unlocked yet                    
                }
            }
        }

        private class Achievement_t
        {
            public Achievement m_eAchievementID;
            public string m_strName;
            public string m_strDescription;
            public bool m_bAchieved;
            public bool m_successfullyUploaded;

            /// <summary>
            /// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
            /// </summary>
            /// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
            /// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
            /// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
            public Achievement_t(Achievement achievementID, string name, string desc)
            {
                m_eAchievementID = achievementID;
                m_strName = name;
                m_strDescription = desc;
                m_bAchieved = false;
            }
        }

        private Achievement_t[] m_Achievements;
    }
}
#endif

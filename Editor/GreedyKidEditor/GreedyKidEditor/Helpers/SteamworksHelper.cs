using System;
using Steamworks;
using System.IO;
using System.IO.Compression;

namespace GreedyKidEditor.Helpers
{
    public enum SteamworksReturn
    {
        SteamworksNotAvailable,
        RestartingThroughSteam,
        Ok,
        CantInit
    }

    public enum WorkshopUploadReturn
    {
        None,

        // create item
        Banned,
        Timeout,
        NotLoggedIn,
        NeedLegalAgreement,

        // upload item
        NotEnoughSpace,

        UnknownError,
    }

    public enum WorkshopUploadStatus
    {
        None,
        Uploading,
        Success,
        Error
    }

    public enum WorkshopItemVisibility
    {
        Public,
        FriendsOnly,
        Private,
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

        private bool _steamworksReady = false;

        private CGameID m_GameID;

        public bool IsReady
        {
            get { return _steamworksReady; }
        }

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

            // field init
            m_createItemResult = new CallResult<CreateItemResult_t>();
            m_submitItemUpdateResult = new CallResult<SubmitItemUpdateResult_t>();

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

        // ************************* WORKSHOP UPLOAD *************************

        private Building _buildingToUpload = null;

        private CallResult<CreateItemResult_t> m_createItemResult;
        private CallResult<SubmitItemUpdateResult_t> m_submitItemUpdateResult;

        private WorkshopUploadStatus _uploadStatus = WorkshopUploadStatus.None;
        private WorkshopUploadReturn _uploadResult = WorkshopUploadReturn.None;

        public WorkshopUploadStatus UploadStatus
        {
            get { return _uploadStatus; }
        }

        public WorkshopUploadReturn UploadResult
        {
            get { return _uploadResult; }
        }

        public string UploadID
        {
            get { return _buildingToUpload.Identifier; }
        }

        public void UploadBuilding(Building building)
        {
            if (_uploadStatus != WorkshopUploadStatus.Uploading)
            {
                _uploadStatus = WorkshopUploadStatus.Uploading;
                _uploadResult = WorkshopUploadReturn.None;
                _buildingToUpload = building;

                PublishedFileId_t itemID = new PublishedFileId_t();
                bool exist = false;
                try
                {
                    ulong raw = ulong.Parse(_buildingToUpload.Identifier);
                    itemID = new PublishedFileId_t(raw);
                    exist = true;
                }
                catch (Exception)
                {

                }

                if (exist)  // skip to update if ID already ok
                    UpdateItem(itemID);
                else
                    CreateItem();
            }
        }

        private void CreateItem()
        {
            if (_steamworksReady)
            {
                try
                {
                    SteamAPICall_t hSteamAPICall = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
                    m_createItemResult.Set(hSteamAPICall, OnCreateItemResult);
                }
                catch (Exception e)
                {
                    _callbackException = e;
                    _uploadStatus = WorkshopUploadStatus.Error;
                    _uploadResult = WorkshopUploadReturn.UnknownError;
                }
            }
        }

        private void OnCreateItemResult(CreateItemResult_t pCallback, bool failure)
        {
            if (pCallback.m_eResult != EResult.k_EResultOK || failure)
            {
                // error
                _uploadStatus = WorkshopUploadStatus.Error;
                if (pCallback.m_eResult == EResult.k_EResultInsufficientPrivilege)
                    _uploadResult = WorkshopUploadReturn.Banned;
                else if (pCallback.m_eResult == EResult.k_EResultTimeout)
                    _uploadResult = WorkshopUploadReturn.Timeout;
                else if (pCallback.m_eResult == EResult.k_EResultNotLoggedOn)
                    _uploadResult = WorkshopUploadReturn.NotLoggedIn;
                else
                    _uploadResult = WorkshopUploadReturn.UnknownError;
                return;
            }

            ulong itemID = pCallback.m_nPublishedFileId.m_PublishedFileId;

            _buildingToUpload.Identifier = itemID.ToString();

            // must save before upload with new ID
            if (File.Exists(Path.GetTempPath() + "GreedyKidWorkshopUpload" + "\\building"))
            {
                try
                {
                    File.Delete(Path.GetTempPath() + "GreedyKidWorkshopUpload" + "\\building");
                }
                catch (Exception) { }
            }
            using (FileStream fs = new FileStream(Path.GetTempPath() + "GreedyKidWorkshopUpload" + "\\building", FileMode.OpenOrCreate))
            {
                using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Compress))
                {
                    using (BinaryWriter writer = new BinaryWriter(gzipStream))
                    {
                        _buildingToUpload.Save(writer, true);
                    }
                }
            }

            UpdateItem(pCallback.m_nPublishedFileId);
        }

        private void UpdateItem(PublishedFileId_t itemID)
        {
            if (_steamworksReady)
            {
                try
                {
                    UGCUpdateHandle_t UGCHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), itemID);

                    ERemoteStoragePublishedFileVisibility visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
                    if (_buildingToUpload.Visibility == WorkshopItemVisibility.FriendsOnly)
                        visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly;
                    else if (_buildingToUpload.Visibility == WorkshopItemVisibility.Public)
                        visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic;

                    // configure item for upload
                    if (SteamUGC.SetItemTitle(UGCHandle, _buildingToUpload.Name)
                        && SteamUGC.SetItemDescription(UGCHandle, _buildingToUpload.Description)
                        && SteamUGC.SetItemUpdateLanguage(UGCHandle, _buildingToUpload.LanguageCode)
                        && SteamUGC.SetItemVisibility(UGCHandle, visibility)
                        && SteamUGC.SetItemContent(UGCHandle, System.IO.Path.GetTempPath() + "GreedyKidWorkshopUpload")
                        && SteamUGC.SetItemPreview(UGCHandle, _buildingToUpload.PreviewImagePath))
                    {
                        // upload
                        UploadItem(UGCHandle);
                    }
                    else
                    {
                        _uploadStatus = WorkshopUploadStatus.Error;
                        _uploadResult = WorkshopUploadReturn.UnknownError;
                    }
                }
                catch (Exception e)
                {
                    _uploadStatus = WorkshopUploadStatus.Error;
                    _uploadResult = WorkshopUploadReturn.UnknownError;
                }
            }
        }

        private void UploadItem(UGCUpdateHandle_t UGCHandle)
        {
            if (_steamworksReady)
            {
                try
                {
                    SteamAPICall_t hSteamAPICall = SteamUGC.SubmitItemUpdate(UGCHandle, "");
                    m_submitItemUpdateResult.Set(hSteamAPICall, OnSubmitItemUpdateResult);
                }
                catch (Exception e)
                {
                    _uploadStatus = WorkshopUploadStatus.Error;
                    _uploadResult = WorkshopUploadReturn.UnknownError;
                }
            }
        }

        private void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool failure)
        {
            if (pCallback.m_eResult != EResult.k_EResultOK || failure)
            {
                // error
                _uploadStatus = WorkshopUploadStatus.Error;
                if (pCallback.m_eResult == EResult.k_EResultAccessDenied)
                    _uploadResult = WorkshopUploadReturn.Banned;
                else if (pCallback.m_eResult == EResult.k_EResultLimitExceeded)
                    _uploadResult = WorkshopUploadReturn.NotEnoughSpace;
                else
                    _uploadResult = WorkshopUploadReturn.UnknownError;
                return;
            }

            // everything's fine
            _uploadStatus = WorkshopUploadStatus.Success;
            _uploadResult = WorkshopUploadReturn.None;

            if (pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
                _uploadResult = WorkshopUploadReturn.NeedLegalAgreement;
        }
    }
}

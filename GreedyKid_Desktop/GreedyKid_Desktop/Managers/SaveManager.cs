using Ionic.Zlib;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class SaveManager
    {
        private string _savePath = "Scores";
        private static int[] _minVersion = new int[] { 1, 0, 0, 0 };

        private static SaveManager _instance;

        private string _currentBuildingIdentifier = "";

        private bool[] _isLevelDone;
        private int[] _levelTime;
        private int[] _levelMoney;
        private int[] _levelStars;

        private bool _hasSeenIntro = false;
        private bool _hasSeenEnding1 = false;
        private bool _hasSeenEnding2 = false;

        // stats
        private uint _accumalatedMoney = 0;
        private uint _booCount = 0;
        private uint _hideCount = 0;
        private uint _rollCount = 0;

        private SaveManager()
        {
#if DESKTOP
            _savePath = Path.Combine(SettingsManager.Instance.SaveDirectory, _savePath);
#endif
        }

        public bool IsLevelDone(int level)
        {
            if (level == -1)
                return _hasSeenIntro;
            else if (level < _isLevelDone.Length)
                return _isLevelDone[level];
            else if (level == _isLevelDone.Length)
                return _hasSeenEnding1;
            else if (level == _isLevelDone.Length + 1)
                return _hasSeenEnding2;
            return false;
        }

        public void DebugUnlockNextLevel()
        {
            if (!_hasSeenIntro)
                _hasSeenIntro = true;
            else
            {
                bool hasIncreased = false;
                for (int i = 0; i < _isLevelDone.Length; i++)
                {
                    if (_isLevelDone[i] == false)
                    {
                        _isLevelDone[i] = true;
                        _levelStars[i] = 3;
                        hasIncreased = true;
                        break;
                    }
                }
                if (!hasIncreased)
                {
                    if (!_hasSeenEnding1)
                        _hasSeenEnding1 = true;
                    else if (!_hasSeenEnding2)
                        _hasSeenEnding2 = true;
                }
            }
        }

        public int LevelTime(int level)
        {
            return _levelTime[level];
        }

        public int LevelMoney(int level)
        {
            return _levelMoney[level];
        }

        public int LevelStars(int level)
        {
            if(_levelStars[level] < 4)
                return _levelStars[level];
            return 0;
        }

        public void AddMoney(int money)
        {
            if (_currentBuildingIdentifier == Building.MainCampaignIdentifier)
            {
                _accumalatedMoney += (uint)money;
                if (_accumalatedMoney >= Helper.AchievementHelper.MoneyThreshold)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_18);
            }
        }

        public void AddBoo()
        {
            if (_currentBuildingIdentifier == Building.MainCampaignIdentifier)
            {
                _booCount++;
                if (_booCount == Helper.AchievementHelper.BooThreshold)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_2);
            }
        }

        public void AddHide()
        {
            if (_currentBuildingIdentifier == Building.MainCampaignIdentifier)
                _hideCount++;
            if (_hideCount == Helper.AchievementHelper.HideThreshold)
                Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_10);
        }

        public void AddRoll()
        {
            if (_currentBuildingIdentifier == Building.MainCampaignIdentifier)
                _rollCount++;
            if (_rollCount == Helper.AchievementHelper.RollThreshold)
                Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_11);
        }

        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SaveManager();
                return _instance;
            }
        }

        public void SetScore(int level, int money, int time, int stars)
        {
            _isLevelDone[level] = true;
            // shoud handle best score here
            if (stars >= _levelStars[level])
            {
                _levelMoney[level] = money;
                if (stars == 3 && _levelTime[level] > 0)
                    _levelTime[level] = Math.Min(time, _levelTime[level]);
                else
                    _levelTime[level] = time;
                _levelStars[level] = stars;
            }

            if (_currentBuildingIdentifier == Building.MainCampaignIdentifier)
            {
                Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_3);

                int completedLevels = 0;
                int totalStars = 0;
                for (int i = 0; i < 0; i++)
                {
                    if (_isLevelDone[i])
                        completedLevels++;
                    totalStars += _levelStars[i];
                }

                if (completedLevels >= _isLevelDone.Length / 3)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_4);
                if (completedLevels >= _isLevelDone.Length * 2 / 3)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_5);
                if (completedLevels == _isLevelDone.Length)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_6);

                if (totalStars >= _isLevelDone.Length)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_7);
                if (totalStars >= _isLevelDone.Length * 2)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_8);
                if (totalStars == _isLevelDone.Length * 3)
                    Helper.AchievementHelper.Instance.UnlockAchievement(Helper.Achievement.GD_ACHIEVEMENT_9);

            }
        }

        public void SetIntro()
        {
            _hasSeenIntro = true;
        }

        public void SetEnding1()
        {
            _hasSeenEnding1 = true;
        }

        public void SetEnding2()
        {
            _hasSeenEnding2 = true;
        }

        public bool HasAllStars()
        {
            int count = 0;
            for (int i = 0; i < _levelStars.Length; i++)
                count += _levelStars[i];

            return count == _levelStars.Length * 3;
        }

        public void Load(Building building)
        {
            if (_currentBuildingIdentifier == building.Identifier)
                return;

            _currentBuildingIdentifier = building.Identifier;

            string path = _savePath + "_" + building.Identifier;

            _isLevelDone = new bool[building.LevelCount];
            _levelTime = new int[building.LevelCount];
            _levelMoney = new int[building.LevelCount];
            _levelStars = new int[building.LevelCount];

            _hasSeenIntro = false;
            _hasSeenEnding1 = false;
            _hasSeenEnding2 = false;

#if PLAYSTATION4
            path = PlatformHelper.PlayStation4.BeginSave(_statsPath, true);
            if (path == null)
                return;
#endif

#if DESKTOP || PLAYSTATION4
            if (File.Exists(path))
#elif XBOXONE
            byte[] buffer;
            if (PlatformHelper.XboxOne.LoadData(path, path, out buffer))
#endif
            {
                try
                {
#if DESKTOP || PLAYSTATION4
                    using (FileStream stream = new FileStream(path, FileMode.Open))
#elif XBOXONE
                    using (MemoryStream stream = new MemoryStream(buffer))
#endif
                    {
#if DESKTOP || PLAYSTATION4
                        using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, false, true))
                        {
                            using (BinaryReader reader = new BinaryReader(gzipStream))
#elif XBOXONE
                        {
                            using (BinaryReader reader = new BinaryReader(stream))
#endif
                            {
                                string currentVersion = System.Reflection.Assembly.GetExecutingAssembly()
                                    .GetName()
                                    .Version
                                    .ToString();

                                string version = reader.ReadString();

                                bool goodVersion = true;

                                string[] split = version.Split('.');
                                int[] versionSplit = new int[split.Length];
                                for (int i = 0; i < split.Length; i++)
                                {
                                    if (!Int32.TryParse(split[i], out versionSplit[i]))
                                        goodVersion = false;
                                }
                                if (split.Length != 4)
                                    goodVersion = false;

                                if (goodVersion)
                                {
                                    if ((versionSplit[0] < _minVersion[0]) ||
                                        (versionSplit[0] == _minVersion[0] && versionSplit[1] < _minVersion[1]) ||
                                        (versionSplit[0] == _minVersion[0] && versionSplit[1] == _minVersion[1] && versionSplit[2] < _minVersion[2]) ||
                                        (versionSplit[0] == _minVersion[0] && versionSplit[1] == _minVersion[1] && versionSplit[2] == _minVersion[2] && versionSplit[3] < _minVersion[3]))
                                    {
                                        goodVersion = false;
                                    }
                                }

                                string buildingIdentifier = reader.ReadString();

                                if (goodVersion && _currentBuildingIdentifier == buildingIdentifier)
                                {
                                    for (int i = 0; i < building.LevelCount; i++)
                                    {
                                        _isLevelDone[i] = reader.ReadBoolean();
                                        _levelTime[i] = reader.ReadInt32();
                                        _levelMoney[i] = reader.ReadInt32();
                                        _levelStars[i] = reader.ReadInt32();
                                    }
                                    _hasSeenIntro = reader.ReadBoolean();
                                    _hasSeenEnding1 = reader.ReadBoolean();
                                    _hasSeenEnding2 = reader.ReadBoolean();

                                    if (_currentBuildingIdentifier == Building.MainCampaignIdentifier)
                                    {
                                        _accumalatedMoney = reader.ReadUInt32();
                                        _booCount = reader.ReadUInt32();
                                        _hideCount = reader.ReadUInt32();
                                        _rollCount = reader.ReadUInt32();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // can't load save
                }
            }

#if PLAYSTATION4
            PlatformHelper.PlayStation4.EndSave();
#endif
        }

        public void Save(Building building)
        {
            string path = _savePath + "_" + building.Identifier;

#if PLAYSTATION4
            path = PlatformHelper.PlayStation4.BeginSave(_statsPath, false);
            if (path == null)
                return;
#endif

#if XBOXONE
            using (MemoryStream stream = new MemoryStream())
#elif DESKTOP || PLAYSTATION4
            using (FileStream stream = new FileStream(path, FileMode.Create))
#endif
            {
#if DESKTOP || PLAYSTATION4
                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    using (BinaryWriter writer = new BinaryWriter(gzipStream))
#elif XBOXONE
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
#endif
                    {
                        string currentVersion = System.Reflection.Assembly.GetExecutingAssembly()
                                    .GetName()
                                    .Version
                                    .ToString();

                        writer.Write(currentVersion);

                        writer.Write(_currentBuildingIdentifier);

                        for (int i = 0; i < building.LevelCount; i++)
                        {
                            writer.Write(_isLevelDone[i]);
                            writer.Write(_levelTime[i]);
                            writer.Write(_levelMoney[i]);
                            writer.Write(_levelStars[i]);
                        }

                        writer.Write(_hasSeenIntro);
                        writer.Write(_hasSeenEnding1);
                        writer.Write(_hasSeenEnding2);

                        if (_currentBuildingIdentifier == Building.MainCampaignIdentifier)
                        {
                            writer.Write(_accumalatedMoney);
                            writer.Write(_booCount);
                            writer.Write(_hideCount);
                            writer.Write(_rollCount);
                        }

                        writer.Flush();
#if DESKTOP || PLAYSTATION4
                        gzipStream.Flush();
#endif
                        stream.Flush();

#if XBOXONE
                        byte[] buffer = stream.GetBuffer();
                        PlatformHelper.XboxOne.SaveData(path, path, buffer, buffer.Length);
#endif
                    }
                }
            }
#if PLAYSTATION4
            PlatformHelper.PlayStation4.EndSave();
#endif
        }
    }    
}

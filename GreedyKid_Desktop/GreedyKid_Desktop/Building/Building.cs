using Ionic.Zlib;
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Building
    {
        public const string MainCampaignIdentifier = "MainCampaign";

        public const string LocalWorkshopPath = "Content/Workshop/";
        private const string _defaultBuildingPath = "Content\\building";
        private const string _defaultLevelPath = "Content\\level_";

        private string _currentBuildingPath = "";
        private string _currentLevelPath = "";

        public string Identifier = "";
        public uint Version = 0;
        public string Name = "";
        public int LevelCount = 0;

        public int[] TargetTime;
        public int[] TargetMoney;

        public Level CurrentLevel;

        public static void GetName(string buildingFolder, out string identifier, out string name)
        {
            using (FileStream stream = new FileStream(buildingFolder + "\\building", FileMode.Open))
            {
                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, false, true))
                {
                    using (BinaryReader reader = new BinaryReader(gzipStream))
                    {
                        identifier = reader.ReadString();
                        reader.ReadUInt32(); // version

                        name = reader.ReadString();
                        name = name.ToUpperInvariant();
                    }
                }
            }
        }

        public void Load(string buildingIdentifier, bool isSteamWorkshop)
        {
            _currentBuildingPath = _defaultBuildingPath;
            _currentLevelPath = _defaultLevelPath;

            if (buildingIdentifier != MainCampaignIdentifier)
            {
                if (isSteamWorkshop)
                {
                    string workshopPath = Helper.SteamworksHelper.Instance.WorkshopPath;
                    _currentBuildingPath = workshopPath + buildingIdentifier + "/building"; // wrong dir
                    _currentLevelPath = workshopPath + buildingIdentifier + "/level_"; // wrong dir
                }
                else
                {
                    _currentBuildingPath = LocalWorkshopPath + buildingIdentifier + "/building";
                    _currentLevelPath = LocalWorkshopPath + buildingIdentifier + "/level_";
                }
            }

            using (FileStream stream = new FileStream(_currentBuildingPath, FileMode.Open))
            {
                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    using (BinaryReader reader = new BinaryReader(gzipStream))
                    {
                        Identifier = reader.ReadString();
                        Version = reader.ReadUInt32();

                        Name = reader.ReadString();

                        LevelCount = reader.ReadInt32();

                        TargetTime = new int[LevelCount];
                        for (int i = 0; i < LevelCount; i++)
                        {
                            TargetTime[i] = reader.ReadInt32();
                        }

                        TargetMoney = new int[LevelCount];
                        for (int i = 0; i < LevelCount; i++)
                        {
                            TargetMoney[i] = reader.ReadInt32();
                        }
                    }
                }
            }

            CurrentLevel = null;

            // clean previous building if any
            GC.Collect();
        }

        public void LoadLevel(int level)
        {
            using (FileStream stream = new FileStream(_currentLevelPath + level, FileMode.Open))
            {
                using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    using (BinaryReader reader = new BinaryReader(gzipStream))
                    {
                        CurrentLevel = new Level();
                        CurrentLevel.Load(reader);

                        if (CurrentLevel.Version != Version || CurrentLevel.Identifier != Identifier)
                            throw new Exception("Tentative to hack levels.");
                    }
                }
            }
        }
    }
}

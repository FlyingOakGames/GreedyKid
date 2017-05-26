using Ionic.Zlib;
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class Building
    {
        private const string _defaultBuildingPath = "Content\\building";
        private const string _defaultLevelPath = "Content\\level_";

        private string _currentBuildingPath = "";
        private string _currentLevelPath = "";

        public string Identifier = "";
        public string Name = "";
        public int LevelCount = 0;

        public Level CurrentLevel;        

        public void Load(string buildingIdentifier)
        {
            _currentBuildingPath = _defaultBuildingPath;
            _currentLevelPath = _defaultLevelPath;

            if (buildingIdentifier != "Default")
            {
                _currentBuildingPath = "Content\\Workshop\\" + buildingIdentifier + "\\building";
                _currentLevelPath = "Content\\Workshop\\" + buildingIdentifier + "\\level_";
            }

            using (GZipStream gzipStream = new GZipStream(TitleContainer.OpenStream(_currentBuildingPath), CompressionMode.Decompress))
            {
                using (BinaryReader reader = new BinaryReader(gzipStream))
                {
                    Identifier = reader.ReadString();

                    Name = reader.ReadString();

                    LevelCount = reader.ReadInt32();
                }
            }

            CurrentLevel = null;

            // clean previous building if any
            GC.Collect();
        }

        public void LoadLevel(int level)
        {
            using (GZipStream gzipStream = new GZipStream(TitleContainer.OpenStream(_currentLevelPath + level), CompressionMode.Decompress))
            {
                using (BinaryReader reader = new BinaryReader(gzipStream))
                {
                    CurrentLevel = new Level();
                    CurrentLevel.Load(reader);
                }
            }
        }
    }
}

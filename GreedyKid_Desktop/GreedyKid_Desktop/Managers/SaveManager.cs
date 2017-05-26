using Ionic.Zlib;
using System;
using System.IO;

namespace GreedyKid
{
    public sealed class SaveManager
    {
        private string _savePath = "Save";

        private static SaveManager _instance;

        private string _currentBuildingIdentifier = "";

        private bool[] _isLevelDone;
        private float[] _levelTime;
        private int[] _levelMoney;

        private SaveManager()
        {
#if DESKTOP
            _savePath = Path.Combine(SettingsManager.Instance.SaveDirectory, _savePath);
#endif
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

        public void Load(Building building)
        {
            if (_currentBuildingIdentifier == building.Identifier)
                return;

            _currentBuildingIdentifier = building.Identifier;

            string path = _savePath + "_" + building.Identifier;

            _isLevelDone = new bool[building.LevelCount];
            _levelTime = new float[building.LevelCount];
            _levelMoney = new int[building.LevelCount];

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
                                for (int i = 0; i < building.LevelCount; i++)
                                {
                                    _isLevelDone[i] = reader.ReadBoolean();
                                    _levelTime[i] = reader.ReadSingle();
                                    _levelMoney[i] = reader.ReadInt32();
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
                        for (int i = 0; i < building.LevelCount; i++)
                        {
                            writer.Write(_isLevelDone[i]);
                            writer.Write(_levelTime[i]);
                            writer.Write(_levelMoney[i]);
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

using Ionic.Zlib;
using Microsoft.Xna.Framework;
using System.IO;

namespace GreedyKid
{
    public sealed class Building
    {
        private const string _defaultBuildingPath = "Content\\building";

        public string Name = "";

        public Level[] Levels;

        public void Load(string building = null)
        {
            if (building == null)
                building = _defaultBuildingPath;
            else
                building = "Content\\" + building;

            using (GZipStream gzipStream = new GZipStream(TitleContainer.OpenStream(building), CompressionMode.Decompress))
            {
                using (BinaryReader reader = new BinaryReader(gzipStream))
                {
                    Name = reader.ReadString();

                    int n = reader.ReadInt32();
                    Levels = new Level[n];
                    for (int i = 0; i < n; i++)
                    {
                        Levels[i] = new Level();
                        Levels[i].Load(reader);
                    }
                }
            }
        }
    }
}

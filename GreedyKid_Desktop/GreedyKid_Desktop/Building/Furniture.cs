using System.IO;

namespace GreedyKid
{
    public sealed class Furniture
    {
        public const int FurnitureCount = 3;
        public const int FurnitureFrames = 14;
        public const int FurniturePerLine = 4;

        public int Type = 0;
        public int X = 0;

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
        }
    }
}

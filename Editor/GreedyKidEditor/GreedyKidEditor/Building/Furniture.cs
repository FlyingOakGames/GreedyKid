using System.IO;

namespace GreedyKidEditor
{
    public sealed class Furniture
    {
        public const int FurnitureCount = 3;
        public const int FurnitureFrames = 14;
        public const int FurniturePerLine = 4;

        public int Type = 0;
        public int X = 0;

        public Furniture()
        {

        }

        public Furniture(int x)
        {
            X = x;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(X);
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
        }
    }
}

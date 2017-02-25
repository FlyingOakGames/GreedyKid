using System.IO;

namespace GreedyKidEditor
{
    public sealed class Retired
    {
        public const int RetiredCount = 1;

        public int Type = 0;
        public int X = 0;

        public int Life = 1;

        public Retired()
        {

        }

        public Retired(int x)
        {
            X = x;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(X);
            writer.Write(Life);
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
            Life = reader.ReadInt32();
        }
    }
}

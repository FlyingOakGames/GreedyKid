using System.IO;

namespace GreedyKidEditor
{
    public sealed class Cop
    {
        public const int CopCount = 1;

        public int Type = 0;
        public int X = 0;

        public Cop()
        {

        }

        public Cop(int x)
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

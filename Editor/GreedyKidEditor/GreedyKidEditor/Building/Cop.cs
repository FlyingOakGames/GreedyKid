using System.IO;

namespace GreedyKidEditor
{
    public sealed class Cop : IMovable
    {
        public const int CopCount = 4;

        public int Type = 0;
        public int X = 0;

        public Cop()
        {

        }

        public Cop(int x)
        {
            X = x;
        }

        public void Move(int x)
        {
            X = x;
        }

        public int GetX()
        {
            return X;
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

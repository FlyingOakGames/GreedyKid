using System.IO;

namespace GreedyKidEditor
{
    public sealed class Detail
    {
        public const int NormalDetailCount = 20;
        public const int AnimatedDetailCount = 7;
        public const int AnimatedDetailFrames = 5;

        public int Type = 0;
        public int X = 0;

        public Detail()
        {

        }

        public Detail(int x)
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

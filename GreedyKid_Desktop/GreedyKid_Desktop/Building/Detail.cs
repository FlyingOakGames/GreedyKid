using System.IO;

namespace GreedyKid
{
    public sealed class Detail
    {
        public const int NormalDetailCount = 20;
        public const int AnimatedDetailCount = 7;
        public const int AnimatedDetailFrames = 4;

        public int Type = 0;
        public int X = 0;

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
        }
    }
}

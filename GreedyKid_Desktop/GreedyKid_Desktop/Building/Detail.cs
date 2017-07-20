using System.IO;

namespace GreedyKid
{
    public sealed class Detail
    {
        public const int NormalDetailCount = 19;
        public const int AnimatedDetailCount = 9;
        public const int AnimatedDetailFrames = 4;
        public const int TutorialCount = 25;

        public int Type = 0;
        public int X = 0;

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
        }
    }
}

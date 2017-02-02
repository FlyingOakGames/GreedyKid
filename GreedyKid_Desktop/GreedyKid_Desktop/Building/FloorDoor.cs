using System.IO;

namespace GreedyKid
{
    public sealed class FloorDoor
    {
        public const int DoorCount = 5;
        public const int DoorPerLine = 6;

        public const int DoorFrames = 6;

        public int Color = 0;
        public int X = 0;

        public void Load(BinaryReader reader)
        {
            Color = reader.ReadInt32();
            X = reader.ReadInt32();
        }
    }
}

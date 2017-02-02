using System.IO;

namespace GreedyKid
{
    public sealed class RoomDoor
    {
        public const int DoorFrames = 18;
        public const int FramePerLine = 18;

        public int X = 0;

        public void Load(BinaryReader reader)
        {
            X = reader.ReadInt32();
        }
    }
}

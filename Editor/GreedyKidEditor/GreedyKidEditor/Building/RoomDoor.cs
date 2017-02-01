using System.IO;

namespace GreedyKidEditor
{
    public sealed class RoomDoor
    {
        public const int DoorFrames = 18;
        public const int FramePerLine = 18;

        public int X = 0;

        public RoomDoor()
        {

        }

        public RoomDoor(int x)
        {
            X = x;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(X);
        }

        public void Load(BinaryReader reader)
        {
            X = reader.ReadInt32();
        }
    }
}

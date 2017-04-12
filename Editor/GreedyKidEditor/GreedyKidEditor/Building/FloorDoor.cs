using System.IO;

namespace GreedyKidEditor
{
    public sealed class FloorDoor : IMovable
    {
        public const int DoorCount = 5;
        public const int DoorPerLine = 6;

        public const int DoorFrames = 6;

        public int Color = 0;
        public int X = 0;

        public FloorDoor()
        {

        }

        public FloorDoor(int x)
        {
            X = x;
        }

        public void Move(int x)
        {
            X = x;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Color);
            writer.Write(X);
        }

        public void Load(BinaryReader reader)
        {
            Color = reader.ReadInt32();
            X = reader.ReadInt32();
        }
    }
}

using System.IO;

namespace GreedyKidEditor
{
    public enum RoomDoorState
    {
        Closed,
        OpeningToLeft,
        OpenLeft,
        OpeningToRight,
        OpenRight,
        ClosingFromLeft,
        ClosingFromRight,

        Count
    }

    public sealed class RoomDoor : IMovable
    {
        public const int DoorFrames = 18;
        public const int FramePerLine = 18;

        public int X = 0;
        public RoomDoorState State = RoomDoorState.Closed;

        public RoomDoor()
        {

        }

        public RoomDoor(int x)
        {
            X = x;
        }

        public void Move(int x)
        {
            X = x;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write((int)State);
        }

        public int GetX()
        {
            return X;
        }

        public void Load(BinaryReader reader)
        {
            X = reader.ReadInt32();
            State = (RoomDoorState)reader.ReadInt32();
        }
    }
}

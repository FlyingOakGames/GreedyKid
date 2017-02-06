using System.IO;

namespace GreedyKid
{
    public sealed class Room
    {
        public const int PaintCount = 4;
        public const int DecorationCount = 5;

        public string Name = "";

        public int BackgroundColor = 0;

        public int LeftMargin = 1;
        public int RightMargin = 1;

        public int LeftDecoration = 0;
        public int RightDecoration = 0;

        public Detail[] Details;
        public FloorDoor[] FloorDoors;
        public RoomDoor[] RoomDoors;
        public Furniture[] Furnitures;

        public const int ElevatorFrames = 5;

        public bool HasStart = false;
        public int StartX = 0;
        public bool HasExit = false;
        public int ExitX = 0;

        public int Y = 0;

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            BackgroundColor = reader.ReadInt32();

            LeftMargin = reader.ReadInt32();
            RightMargin = reader.ReadInt32();

            LeftDecoration = reader.ReadInt32();
            RightDecoration = reader.ReadInt32();

            int n = reader.ReadInt32();
            Details = new Detail[n];
            for (int i = 0; i < n; i++)
            {
                Details[i] = new Detail();
                Details[i].Load(reader);
            }
            n = reader.ReadInt32();
            FloorDoors = new FloorDoor[n];
            for (int i = 0; i < n; i++)
            {
                FloorDoors[i] = new FloorDoor();
                FloorDoors[i].Load(reader);
            }
            n = reader.ReadInt32();
            RoomDoors = new RoomDoor[n];
            for (int i = 0; i < n; i++)
            {
                RoomDoors[i] = new RoomDoor();
                RoomDoors[i].Load(reader);
            }
            n = reader.ReadInt32();
            Furnitures = new Furniture[n];
            for (int i = 0; i < n; i++)
            {
                Furnitures[i] = new Furniture();
                Furnitures[i].Load(reader);
            }

            HasStart = reader.ReadBoolean();
            StartX = reader.ReadInt32();
            HasExit = reader.ReadBoolean();
            ExitX = reader.ReadInt32();
        }
    }
}

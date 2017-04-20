using System.Collections.Generic;
using System.IO;

namespace GreedyKid
{
    public sealed class Room
    {
        public const int PaintCount = 4;
        public const int DecorationCount = 5;

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

        public List<Retired> Retireds;
        public List<Nurse> Nurses;
        public List<Cop> Cops;

        public List<Droppable> Drops;

        public void Load(BinaryReader reader)
        {
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
                FloorDoors[i].Room = this;
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
            n = reader.ReadInt32();
            Retireds = new List<Retired>(10);
            for (int i = 0; i < n; i++)
            {
                Retired retired = new Retired();
                retired.Load(reader);
                retired.Room = this;
                Retireds.Add(retired);
            }
            n = reader.ReadInt32();
            Nurses = new List<Nurse>(10);
            for (int i = 0; i < n; i++)
            {
                Nurse nurse = new Nurse();
                nurse.Load(reader);
                nurse.Room = this;
                Nurses.Add(nurse);
            }
            n = reader.ReadInt32();
            Cops = new List<Cop>(10);
            for (int i = 0; i < n; i++)
            {
                Cop cop = new Cop();
                cop.Load(reader);
                cop.Room = this;
                Cops.Add(cop);
            }

            HasStart = reader.ReadBoolean();
            StartX = reader.ReadInt32();
            HasExit = reader.ReadBoolean();
            ExitX = reader.ReadInt32();

            Drops = new List<Droppable>();
        }        

        public void AddDrop(ObjectType type, float x)
        {
            Droppable drop = new Droppable(type);
            drop.Room = this;
            Drops.Add(drop);
            drop.Drop(x + 8.0f);
        }
    }
}

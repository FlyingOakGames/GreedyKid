using System.Collections.Generic;
using System.IO;

namespace GreedyKidEditor
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

        public List<Detail> Details = new List<Detail>();
        public List<FloorDoor> FloorDoors = new List<FloorDoor>();
        public List<RoomDoor> RoomDoors = new List<RoomDoor>();
        public List<Furniture> Furnitures = new List<Furniture>();

        public const int ElevatorFrames = 5;

        public bool HasStart = false;
        public int StartX = 0;
        public bool HasExit = false;
        public int ExitX = 0;

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);

            writer.Write(BackgroundColor);

            writer.Write(LeftMargin);
            writer.Write(RightMargin);

            writer.Write(LeftDecoration);
            writer.Write(RightDecoration);

            writer.Write(Details.Count);
            for (int i = 0; i < Details.Count; i++)
                Details[i].Save(writer);
            writer.Write(FloorDoors.Count);
            for (int i = 0; i < FloorDoors.Count; i++)
                FloorDoors[i].Save(writer);
            writer.Write(RoomDoors.Count);
            for (int i = 0; i < RoomDoors.Count; i++)
                RoomDoors[i].Save(writer);
            writer.Write(Furnitures.Count);
            for (int i = 0; i < Furnitures.Count; i++)
                Furnitures[i].Save(writer);

            writer.Write(HasStart);
            writer.Write(StartX);
            writer.Write(HasExit);
            writer.Write(ExitX);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            BackgroundColor = reader.ReadInt32();

            LeftMargin = reader.ReadInt32();
            RightMargin = reader.ReadInt32();

            LeftDecoration = reader.ReadInt32();
            RightDecoration = reader.ReadInt32();

            int n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                Detail d = new Detail();
                d.Load(reader);
                Details.Add(d);
            }
            n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                FloorDoor d = new FloorDoor();
                d.Load(reader);
                FloorDoors.Add(d);
            }
            n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                RoomDoor d = new RoomDoor();
                d.Load(reader);
                RoomDoors.Add(d);
            }
            n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                Furniture d = new Furniture();
                d.Load(reader);
                Furnitures.Add(d);
            }

            HasStart = reader.ReadBoolean();
            StartX = reader.ReadInt32();
            HasExit = reader.ReadBoolean();
            ExitX = reader.ReadInt32();
        }
    }
}

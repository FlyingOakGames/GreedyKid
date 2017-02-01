using System.Collections.Generic;

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
    }
}

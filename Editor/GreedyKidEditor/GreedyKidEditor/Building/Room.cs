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
    }
}

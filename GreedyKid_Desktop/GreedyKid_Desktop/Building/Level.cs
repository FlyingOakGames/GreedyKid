using System.IO;

namespace GreedyKid
{
    public sealed class Level
    {
        public string Name = "";

        public Floor[] Floors;

        // cop sequences
        public int TimeBeforeCop = 0;
        public int Cop1Count = 0;
        public int Cop2Count = 0;

        // swat
        public int TimeBeforeSwat = 0;
        public int Swat1Count = 0;

        // robocop
        public int TimeBeforeRobocop = 0;
        public int RobocopCount = 0;

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            int n = reader.ReadInt32();
            Floors = new Floor[n];
            for (int i = 0; i < n; i++)
            {
                Floors[i] = new Floor();
                Floors[i].Y = i;
                Floors[i].Load(reader);
            }

            TimeBeforeCop = reader.ReadInt32();
            Cop1Count = reader.ReadInt32();
            Cop2Count = reader.ReadInt32();

            TimeBeforeSwat = reader.ReadInt32();
            Swat1Count = reader.ReadInt32();

            ConnectSisterDoors();

            // upper/lower floors for room
            for (int f = 0; f < Floors.Length; f++)
            {
                Floor floor = Floors[f];

                for (int r = 0; r < floor.Rooms.Length; r++)
                {
                    Room room = floor.Rooms[r];

                    if (f > 0)
                    {
                        room.LowerFloor = Floors[f - 1];
                    }
                    else
                    {
                        room.LowerFloor = null;
                    }

                    if (f < floor.Rooms.Length - 1)
                    {
                        room.UpperFloor = Floors[f - 1];
                    }
                    else
                    {
                        room.UpperFloor = null;
                    }
                }
            }
        }

        private void ConnectSisterDoors()
        {
            for (int f = 0; f < Floors.Length; f++)
            {
                Floor floor = Floors[f];

                for (int r = 0; r < floor.Rooms.Length; r++)
                {
                    Room room = floor.Rooms[r];

                    for (int ff = 0; ff < room.FloorDoors.Length; ff++)
                    {
                        FloorDoor floorDoor = room.FloorDoors[ff];

                        if (floorDoor.SisterDoor != null && floorDoor.SisterDoor.SisterDoor != null)
                            continue;


                        for (int f2 = 0; f2 < Floors.Length; f2++)
                        {
                            Floor floor2 = Floors[f2];

                            for (int r2 = 0; r2 < floor2.Rooms.Length; r2++)
                            {
                                Room room2 = floor2.Rooms[r2];

                                for (int ff2 = 0; ff2 < room2.FloorDoors.Length; ff2++)
                                {
                                    FloorDoor floorDoor2 = room2.FloorDoors[ff2];

                                    if (floorDoor.Color > 0 && floorDoor2.Color == floorDoor.Color)
                                    {
                                        floorDoor.SisterDoor = floorDoor2;
                                        floorDoor2.SisterDoor = floorDoor;
                                    }
                                    else if (floorDoor.Color == 0 && floorDoor2.Color == floorDoor.Color && floorDoor2.X == floorDoor.X)
                                    {
                                        floorDoor.SisterDoor = floorDoor2;
                                        floorDoor2.SisterDoor = floorDoor;
                                    }
                                }
                            }
                        }                        
                    }
                }
            }
        }
    }
}

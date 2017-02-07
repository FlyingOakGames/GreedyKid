using System.IO;

namespace GreedyKid
{
    public sealed class Level
    {
        public string Name = "";

        public Floor[] Floors;

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

            ConnectSisterDoors();
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

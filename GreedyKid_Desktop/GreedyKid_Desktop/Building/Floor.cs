// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using System.IO;

namespace GreedyKid
{
    public sealed class Floor
    {
        public Room[] Rooms;

        public int Y = 0;

        public int Load(BinaryReader reader)
        {
            int money = 0;

            int n = reader.ReadInt32();
            Rooms = new Room[n];
            for (int i = 0; i < n; i++)
            {
                Rooms[i] = new Room();
                Rooms[i].Y = Y;
                money += Rooms[i].Load(reader);
            }

            return money;
        }
    }
}

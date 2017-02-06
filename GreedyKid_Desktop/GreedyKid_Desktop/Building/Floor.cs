﻿using System.IO;

namespace GreedyKid
{
    public sealed class Floor
    {
        public string Name = "";

        public Room[] Rooms;

        public int Y = 0;

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            int n = reader.ReadInt32();
            Rooms = new Room[n];
            for (int i = 0; i < n; i++)
            {
                Rooms[i] = new Room();
                Rooms[i].Y = Y;
                Rooms[i].Load(reader);
            }
        }
    }
}
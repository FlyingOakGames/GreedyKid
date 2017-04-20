using System.IO;

namespace GreedyKid
{
    public sealed class Floor
    {
        public Room[] Rooms;

        public int Y = 0;

        public void Load(BinaryReader reader)
        {
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

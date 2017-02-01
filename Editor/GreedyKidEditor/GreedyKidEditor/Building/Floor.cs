using System.Collections.Generic;
using System.IO;

namespace GreedyKidEditor
{
    public sealed class Floor
    {
        public string Name = "";

        public List<Room> Rooms = new List<Room>();

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);

            writer.Write(Rooms.Count);
            for (int i = 0; i < Rooms.Count; i++)
                Rooms[i].Save(writer);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            int n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                Room r = new Room();
                r.Load(reader);
                Rooms.Add(r);
            }
        }
    }
}

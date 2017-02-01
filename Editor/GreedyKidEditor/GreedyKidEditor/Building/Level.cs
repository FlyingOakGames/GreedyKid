using System.Collections.Generic;
using System.IO;

namespace GreedyKidEditor
{
    public sealed class Level
    {
        public string Name = "";

        public List<Floor> Floors = new List<Floor>();

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);

            writer.Write(Floors.Count);
            for (int i = 0; i < Floors.Count; i++)
                Floors[i].Save(writer);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            int n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                Floor r = new Floor();
                r.Load(reader);
                Floors.Add(r);
            }
        }
    }
}

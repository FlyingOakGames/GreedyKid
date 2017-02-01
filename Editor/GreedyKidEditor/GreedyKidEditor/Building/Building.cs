using System.Collections.Generic;
using System.IO;

namespace GreedyKidEditor
{
    public sealed class Building
    {
        public string Name = "";

        public List<Level> Levels = new List<Level>();

        public Building()
        {

        }

        public Building(string name)
        {
            Name = name;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);

            writer.Write(Levels.Count);
            for (int i = 0; i < Levels.Count; i++)
                Levels[i].Save(writer);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            int n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                Level r = new Level();
                r.Load(reader);
                Levels.Add(r);
            }
        }
    }
}

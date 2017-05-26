using System.Collections.Generic;
using System.IO;

namespace GreedyKidEditor
{
    public sealed class Building
    {
        public string Identifier = "Default";

        public string Name = "";

        public List<Level> Levels = new List<Level>();

        public Building()
        {

        }

        public Building(string name)
        {
            Name = name;
            Levels.Add(new Level());
        }

        public void Save(BinaryWriter writer, bool export = false)
        {
            writer.Write(Identifier);

            writer.Write(Name);

            writer.Write(Levels.Count);

            if (!export)
            {
                for (int i = 0; i < Levels.Count; i++)
                    Levels[i].Save(writer);
            }
        }

        public void Load(BinaryReader reader)
        {
            Levels.Clear();

            Identifier = reader.ReadString();

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

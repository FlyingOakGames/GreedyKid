using GreedyKidEditor.Helpers;
using System.Collections.Generic;
using System.IO;

namespace GreedyKidEditor
{
    public sealed class Building
    {
        // workshop data
#if DEVMODE
        public string Identifier = "Default";
#else
        public string Identifier = "NotUploadedToWorkshopYet";
#endif
        public string Description = "Describe your building here";
        public string LanguageCode = "english";
        public WorkshopItemVisibility Visibility = WorkshopItemVisibility.Private;
        public string PreviewImagePath = "";


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

            if (!export)
            {
                writer.Write(Description);
                writer.Write(LanguageCode);
                writer.Write((int)Visibility);
                writer.Write(PreviewImagePath);
            }

            writer.Write(Name);

            writer.Write(Levels.Count);

            if (!export)
            {
                for (int i = 0; i < Levels.Count; i++)
                    Levels[i].Save(writer);
            }
            else
            {
                // target times
                for (int i = 0; i < Levels.Count; i++)
                    writer.Write(Levels[i].TargetTime);
                // target money
                for (int i = 0; i < Levels.Count; i++)
                    writer.Write(Levels[i].GetTargetMoney());
            }
        }

        public void Load(BinaryReader reader)
        {
            Levels.Clear();

            Identifier = reader.ReadString();
            Description = reader.ReadString();
            LanguageCode = reader.ReadString();
            Visibility = (WorkshopItemVisibility)reader.ReadInt32();
            PreviewImagePath = reader.ReadString();

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

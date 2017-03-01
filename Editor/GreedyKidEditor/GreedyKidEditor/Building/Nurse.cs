using System.IO;

namespace GreedyKidEditor
{
    public sealed class Nurse
    {
        public const int NurseCount = 1;

        public int Type = 0;
        public int X = 0;

        public int Life = 1;

        public Nurse()
        {

        }

        public Nurse(int x)
        {
            X = x;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(X);
            writer.Write(Life);
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
            Life = reader.ReadInt32();
        }
    }
}

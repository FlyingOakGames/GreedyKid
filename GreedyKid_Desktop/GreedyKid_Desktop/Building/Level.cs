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
        }
    }
}

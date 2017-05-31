using System.Collections.Generic;
using System.IO;

namespace GreedyKidEditor
{
    public sealed class Level
    {
        public string Name = "";

        public List<Floor> Floors = new List<Floor>();

        // cop sequences
        public int TimeBeforeCop = 0;
        public int Cop1Count = 0;
        public int Cop2Count = 0;

        // swat
        public int TimeBeforeSwat = 0;
        public int Swat1Count = 0;

        // robocop
        public int TimeBeforeRobocop = 0;
        public int RobocopCount = 0;

        // score
        public int TargetTime = 0;

        public void Save(BinaryWriter writer)
        {
            // removing empty floors
            for (int f = Floors.Count - 1; f >= 0; f--)
            {
                if (Floors[f].Rooms.Count == 0)
                    Floors.RemoveAt(f);
                else
                    break;
            }

            writer.Write(Name);

            writer.Write(TargetTime);

            writer.Write(Floors.Count);
            for (int i = 0; i < Floors.Count; i++)
                Floors[i].Save(writer);

            writer.Write(TimeBeforeCop);
            writer.Write(Cop1Count);
            writer.Write(Cop2Count);

            writer.Write(TimeBeforeSwat);
            writer.Write(Swat1Count);

            writer.Write(TimeBeforeRobocop);
            writer.Write(RobocopCount);
        }

        public void Load(BinaryReader reader)
        {
            Name = reader.ReadString();

            TargetTime = reader.ReadInt32();

            int n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                Floor r = new Floor();
                r.Load(reader);
                Floors.Add(r);
            }

            TimeBeforeCop = reader.ReadInt32();
            Cop1Count = reader.ReadInt32();
            Cop2Count = reader.ReadInt32();

            TimeBeforeSwat = reader.ReadInt32();
            Swat1Count = reader.ReadInt32();

            TimeBeforeRobocop = reader.ReadInt32();
            RobocopCount = reader.ReadInt32();
        }

        public int HasStart()
        {
            int startCount = 0;
            
            for (int i = 0; i < Floors.Count; i++)
            {
                for (int n = 0; n < Floors[i].Rooms.Count; n++)
                {
                    if (Floors[i].Rooms[n].HasStart)
                        startCount++;
                }
            }

            return startCount;
        }

        public int HasExit()
        {
            int exitCount = 0;

            for (int i = 0; i < Floors.Count; i++)
            {
                for (int n = 0; n < Floors[i].Rooms.Count; n++)
                {
                    if (Floors[i].Rooms[n].HasExit)
                        exitCount++;
                }
            }

            return exitCount;
        }

        public int GetTargetMoney()
        {
            int money = 0;

            for (int i = 0; i < Floors.Count; i++)
            {
                for (int n = 0; n < Floors[i].Rooms.Count; n++)
                {
                    for (int r = 0; r < Floors[i].Rooms[n].Retireds.Count; r++)
                    {
                        money += Floors[i].Rooms[n].Retireds[r].Money;
                    }
                }
            }

            return money;
        }
    }
}

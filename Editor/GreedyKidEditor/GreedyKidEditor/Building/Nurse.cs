﻿using System.IO;

namespace GreedyKidEditor
{
    public sealed class Nurse : IMovable
    {
        public const int NurseCount = 4;

        public int Type = 0;
        public int X = 0;

        public int Life = 2;

        public Nurse()
        {

        }

        public Nurse(int x)
        {
            X = x;
        }

        public void Move(int x)
        {
            X = x;
        }

        public int GetX()
        {
            return X;
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

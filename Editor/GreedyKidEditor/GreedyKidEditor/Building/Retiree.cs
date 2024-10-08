﻿using System.IO;

namespace GreedyKidEditor
{
    public sealed class Retiree : IMovable
    {
        public const int RetireeCount = 8;
        public const int MaxMoney = 20;

        public int Type = 0;
        public int X = 0;

        public int Life = 2;
        public int Money = 5;

        public Retiree()
        {
            
        }

        public Retiree(int x)
        {
            X = x;
            System.Random r = new System.Random();
            Money = r.Next(1, MaxMoney);
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
            writer.Write(Money);
        }

        public void Load(BinaryReader reader)
        {
            Type = reader.ReadInt32();
            X = reader.ReadInt32();
            Life = reader.ReadInt32();
            Money = reader.ReadInt32();
        }
    }
}

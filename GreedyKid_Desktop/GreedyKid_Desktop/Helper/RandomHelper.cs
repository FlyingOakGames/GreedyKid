using System;

namespace GreedyKid
{
    public static class RandomHelper
    {
        private static Random _random;

        static RandomHelper()
        {
            _random = new Random();
        }

        public static float Next()
        {
            return (float)_random.NextDouble();
        }

        public static int Next(int max)
        {
            return _random.Next(max);
        }
    }
}

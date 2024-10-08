﻿// Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.
using System;

namespace GreedyKid
{
    public static class EasingHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Current time</param>
        /// <param name="b">Initial value</param>
        /// <param name="c">Difference</param>
        /// <param name="d">Total time</param>
        /// <returns></returns>
        public static float EaseOutExpo(float t, float b, float c, float d)
        {
            return (t == d) ? b + c : c * (-(float)Math.Pow(2, -10 * t / d) + 1) + b;
        }
    }
}

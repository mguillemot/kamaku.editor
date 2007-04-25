using System;

namespace Kamaku
{
    public class Rand
    {
        private static Random _r = new Random();

        private Rand()
        {
        }

        public static int NextInt(int min, int max)
        {
            return _r.Next(min, max);
        }

        public static float NextFloat()
        {
            return Convert.ToSingle(_r.NextDouble());
        }
    }
}

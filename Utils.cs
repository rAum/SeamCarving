using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeamCarving
{
    /// <summary>
    /// Szwajcarski scyzoreeek
    /// </summary>
    class Utils
    {
        public static T Clamp<T>(T c, T min, T max) where T : System.IComparable<T>
        {
            if (c.CompareTo(min) < 0)
                return min;

            if (c.CompareTo(max) > 0)
                return max;

            return c;
        }

        public static ulong Min(ulong a, ulong b)
        {
            return Math.Min(a, b);
        }

        public static ulong Min(ulong a, ulong b, ulong c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        public static double Lerp(double value, double a, double b)
        {
            return a + value / (b - a);
        }

    }
}

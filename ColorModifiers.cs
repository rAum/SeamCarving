using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SeamCarving
{
    class ColorModifiers
    {
        public static Color ToGrayscaleSimple(Color c)
        {
            int gs = ((int)c.R + c.G + c.B) / 3;
            return Color.FromArgb(gs,gs,gs);
        }

        public static Color ToGrayscale(Color c)
        {
            int gs = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
            return Color.FromArgb(gs, gs, gs);
        }

        public static Color ToSepia(Color c)
        {
            byte gs = ToGrayscale(c).R;
            return Color.FromArgb(gs, (byte)(0.95 * gs), (byte)(0.82 * gs));
        }
    }
}

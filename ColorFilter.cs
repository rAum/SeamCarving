using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SeamCarving
{
    class ColorFilter
    {
        private double gsr, gsb, gsg;
        public enum Type { Fast, Standard, Custom };

        private void Normalize()
        {
            double dst = Math.Sqrt(gsr * gsr + gsb * gsb + gsg * gsg);
            gsr /= dst;
            gsb /= dst;
            gsg /= dst;
        }

        public double RedValue
        {
            get { return gsr; }
            set { gsr = Utils.Clamp<double>(value, 0, 1); Normalize(); } 
        }

        public double GreenValue
        {
            get { return gsg; }
            set { gsg = Utils.Clamp<double>(value, 0, 1); Normalize();  }
        }

        public double BlueValue
        {
            get { return gsb; }
            set { gsb = Utils.Clamp<double>(value, 0, 1); Normalize(); }
        }

        static private Color Fast(Color a)
        {
            int c = (a.R + a.B + a.G) / 3;
            return Color.FromArgb(c,c,c);
        }

        static private Color Standard(Color c)
        {
            byte col = (byte) (0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
            return Color.FromArgb(col, col, col);
        }

        private Color Custom(Color c)
        {
            byte col = (byte)(gsr * c.R + gsg * c.G + gsb * c.B);
            return Color.FromArgb(col, col, col);
        }

        private delegate Color ModDelegate(Color a);

        private ModDelegate func;

        public ColorFilter(Type type)
        {
            switch (type)
            {
                case Type.Fast:
                    func = new ModDelegate(Fast);
                    break;
                case Type.Standard:
                    func = new ModDelegate(Standard);
                    break;
                case Type.Custom:
                    func = new ModDelegate(Custom);
                    break;
            }
        }

        public Color Convert(Color cin)
        {
            return func(cin);
        }

    }
}

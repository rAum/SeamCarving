using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SeamCarving
{
    /// <summary>
    /// Implementacja Look Up Table
    /// Znacznie przyspiesza przetwarzanie.
    /// </summary>
    class LUT : IImageMod, ICloneable
    {
        private Color[,,] lut;

        public delegate Color ColorConversion(Color color_in);

        public LUT(ColorConversion conversion = null)
        {
            lut = new Color[256, 256, 256];
            CreateLookupTable(conversion);
        }

        public void CreateLookupTable(ColorConversion conversion)
        {
            if (conversion == null)
                return;

            for (int r = 255; r >= 0; --r)
            {
                for (int g = 255; g >= 0; --g)
                {
                    for (int b = 255; b >= 0; --b)
                    {
                        lut[r, g, b] = conversion(Color.FromArgb(r, g, b));
                    }
                }
            }
        }

        public Color Lookup(Color color)
        {
            return lut[color.R, color.G, color.B];
        }

        public RawImage Modify(RawImage rim)
        {
            RawImage tmp = (RawImage)rim.Clone();
            ModifyInPlace(ref rim);
            return tmp;
        }

        public void ModifyInPlace(ref RawImage rim)
        {
            rim.Lock();

            for (int x = 0; x < rim.Width; ++x)
            {
                for (int y = 0; y < rim.Height; ++y)
                {
                    rim[x, y] = Lookup(rim[x,y]);
                }
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
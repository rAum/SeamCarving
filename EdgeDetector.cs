using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SeamCarving
{
    class EdgeDetector : IImageMod
    {
        public enum EdgeDetectorType { Laplacian };
        private EdgeDetectorType edge;

        public RawImage Modify(RawImage rim)
        {
            RawImage tmp = new RawImage(rim.Width, rim.Height);
            Edge(ref tmp,rim);
            return tmp;
        }

        private int width, height;

        private int MirrorX(int x)
        {
            if (x < 0)
                return Math.Abs(x);
            else if (x >= width)
                return width - (x - width) - 1;
            return x;
        }

        private int MirrorY(int y)
        {
            if (y < 0)
                return Math.Abs(y);
            else if (y >= height)
                return height - (y - height) - 1;
            return y;
        }

        public void ModifyInPlace(ref RawImage rim)
        {
            rim = Modify(rim);
        }

        private void Edge(ref RawImage rim, RawImage org)
        {
            width = rim.Width;
            height = rim.Height;
            int val;

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    val = org[MirrorX(x - 1), y, RawImage.Channel.Red] + org[MirrorX(x + 1), y, RawImage.Channel.Red]
                        + org[MirrorX(x - 1), MirrorY(y - 1), RawImage.Channel.Red] + org[MirrorX(x + 1), MirrorY(y - 1), RawImage.Channel.Red]
                        + org[MirrorX(x - 1), MirrorY(y + 1), RawImage.Channel.Red] + org[MirrorX(x + 1), MirrorY(y + 1), RawImage.Channel.Red]
                        + org[x, MirrorY(y - 1), RawImage.Channel.Red] + org[x, MirrorY(y - 1), RawImage.Channel.Red];

                    val -= 8 * org[x, y, RawImage.Channel.Red];
                    val = Utils.Clamp<int>(val, 0, 255);
                    rim[x, y] = Color.FromArgb(val, val, val);
                }
            }


            
        }
    }
}

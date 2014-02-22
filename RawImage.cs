using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace SeamCarving
{
    /// <summary>
    /// Klasa do szybkiej modyfikacji pikseli w bitmapi
    /// </summary>
    class RawImage : ICloneable
    {
        private Bitmap bmp;
        private BitmapData data;
        private unsafe byte* ptr;
        private int offset;
        private bool locked = false;
        private int width = 0, height = 0;

        public RawImage() { }
        public RawImage(Bitmap bmp) { Image = new Bitmap(bmp); }
        public RawImage(int width, int height)
        {
            Image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }

        public unsafe byte* Pointer
        {
            get { return ptr; }
        }

        public int Stride
        {
            get { return offset - 3 * Width; }
        }

        public unsafe Color this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return Color.Black;

                int addr = 3 * x + y * offset;
                return Color.FromArgb((int)ptr[addr + 2], (int)ptr[addr + 1], (int)ptr[addr]);
            }

            set
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return;

                int addr = 3 * x + y * offset;
                ptr[addr] = value.B;
                ptr[addr + 1] = value.G;
                ptr[addr + 2] = value.R;
            }
        }

        public enum Channel { Blue = 0, Green = 1, Red = 2 };

        public unsafe byte this[int x, int y, Channel color]
        {
            get
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return 0;
                return ptr[3 * x + (int)color + y * offset];
            }
            set
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return;
                ptr[3 * x + (int)color + y * offset] = value;
            }
        }

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Bitmap Image
        {
            get { Unlock(); return bmp; }
            set 
            {
                bmp = value;
                if (bmp == null)
                {
                    width = 0;
                    height = 0;
                }
                else
                {
                    width = bmp.Width;
                    height = bmp.Height;
                    Lock();
                }
            }
        }

        public unsafe void Lock()
        {
            if (!locked && bmp != null)
            {
                data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                offset = data.Stride;
                ptr = (byte*)data.Scan0;
                locked = true;
            }
        }

        public void Unlock()
        {
            if (locked && bmp != null)
            {
                bmp.UnlockBits(data);
                locked = false;
            }
        }

        public object Clone()
        {
            RawImage img = (RawImage)this.MemberwiseClone();
            img.Lock();
            return img;
        }
    }
}


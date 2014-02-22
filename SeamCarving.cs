using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SeamCarving
{
    class SeamCarving
    {
        private RawImage img;
        private int[,] energy;
        private int[,] gs;
        private int[,] grad;

        public SeamCarving(Image image)
        {
            img = new RawImage((Bitmap)image);
            Allocate();
        }

        private void Allocate(int dx = 0, int dy = 0)
        {
            dx += img.Width;
            dy += img.Height;
            energy = new int[dy, dx];
            gs = new int[dy, dx];
            grad = new int[dy, dx];
        }

        private int ClampColor(int a)
        {
            if (a < 0) return 0;
            if (a > 255) return 255;
            return a;
        }

        private Color Mixture(Color a, Color b)
        {
            return Color.FromArgb(ClampColor(((int)a.R + b.R) / 2),
                                  ClampColor(((int)a.G + b.G) / 2),
                                  ClampColor(((int)a.B + b.B) / 2 ));
        }

        internal Image Retarget(int nwidth, int nheight, System.ComponentModel.BackgroundWorker bg)
        {
            int hdiff = img.Width - nwidth;
            int vdiff = img.Height - nheight;
            int steps = 0;
            int step = 0;

            steps += Math.Abs(hdiff);
            steps += Math.Abs(vdiff);

            if (hdiff > 0)
            {
                while (hdiff-- > 0)
                {
                    ComputeGradient();
                    ComputeVerticalEnergy();
                    img = new RawImage(RemoveVerticalSeam(FindVerticalSeam()));

                    ++step;

                    if (bg != null)
                        bg.ReportProgress(step * 100 / steps);
                }
            }
            else if (hdiff != 0)
            {
                int founded = Math.Abs(hdiff);
                int need = founded;

                do
                {
                    Allocate(0, founded);
                    ComputeGradient();
                    ComputeVerticalEnergy();
                    
                    RawImage tmp = new RawImage(AddVerticalSeams(FindVerticalSeams(founded, ref founded)));

                    step += founded;
                    need -= founded;

                    if (founded > 0)
                        img = tmp;

                    if (bg != null)
                        bg.ReportProgress(step * 100 / steps);

                } while (need > 0);
            }
           

            if (vdiff > 0)
            {
                while (vdiff-- > 0)
                {
                    ComputeGradient();
                    ComputeHorizontalEnergy();
                    img = new RawImage(RemoveHorizontalSeam(FindHorizontalSeam()));

                    ++step;

                    if (bg != null)
                        bg.ReportProgress(step * 100 / steps);
                }
            }
            
            if (bg != null)
                bg.ReportProgress(100);

            return img.Image;
        }


        public Image Retarget(int nwidth, int nheight)
        {
            return Retarget(nwidth, nheight, null);
        }


        private List<List<int>> FindVerticalSeams(int num, ref int founded)
        {
            List<List<int>> seams = new List<List<int>>();
            int last = img.Height - 1;
            int minValue, minIndex;

            int need = num;
            for (int i = 0; i < need; )
            {
                minValue = energy[last, 0];
                minIndex = 0;

                for (int x = 1; x < img.Width; ++x)
                {
                    if (energy[last, x] < minValue)
                    {
                        minIndex = x;
                        minValue = energy[last, x];
                    }
                }

                if (minValue == int.MaxValue)
                    break;

                energy[last, minIndex] = int.MaxValue;
                
                int lim;
                List<int> seam = new List<int>(img.Height);
                seam.Add(minIndex);
                for (int y = last - 1; y >= 0; --y)
                {
                    //minValue = int.MaxValue;
                    lim = minIndex + 2;
                    for (int dx = minIndex - 1; dx < lim; ++dx)
                    {
                        if (dx < 0 || dx >= img.Width)
                            continue;
                        if (energy[y, dx] < minValue)
                        {
                            minValue = energy[y, dx];
                            minIndex = dx;
                        }
                    }

                    if (energy[y, minIndex] != int.MaxValue)
                    {
                        seam.Add(minIndex);
                        energy[y, minIndex] = int.MaxValue;
                    }
                    else
                        break;
                }

                if (seam.Count == img.Height)
                {
                    seams.Add(seam);

                    for (int y = last; y >= 0; --y)
                        energy[y, seam[y]] = int.MaxValue;

                    ++i;
                }
            }

            founded = seams.Count;
            return seams;
        }

        private Color LinearGradient(Color a, Color b, float c)
        {
            if (c > 1) c = 1;
            return Color.FromArgb((byte)((1 - c) * a.R + c * b.R), (byte)((1 - c) * a.G + c * b.G), (byte)((1 - c) * a.B + c * b.B));
        }

        public Bitmap AddVerticalSeams(List<List<int>> seams)
        {
            RawImage ni = new RawImage(img.Width + seams.Count, img.Height);

            for (int y = img.Height - 1; y >= 0; --y)
            {
                for (int x = 0; x < img.Width; ++x)
                    ni[x, y] = img[x, y];
            }

            int tmpWidth = img.Width;

            //foreach (var seam in seams)
            {
                //tmpWidth += 1;
                for (int y = img.Height - 1; y >= 0; --y)
                {
                    //int add = seam[y];

                    List<int> tmp = new List<int>(seams.Count);
                    foreach (var s in seams)
                        tmp.Add(s[y]);

                    tmp.Sort();
                    tmp.Reverse();

                    for (int dx = 0; dx < tmp.Count; ++dx)
                    { 
                        for (int x = tmpWidth + tmp.Count; x > tmp[dx]; --x)
                            ni[x, y] = ni[x - 1, y];
                        ni[tmp[dx], y] = LinearGradient(ni[tmp[dx] + 1, y], ni[tmp[dx], y], 0.75f);
                    }
                }
            }

            return ni.Image;
        }

        public Bitmap RemoveHorizontalSeam(List<int> seam)
        {
            int x = img.Width - 1;
            RawImage ni = new RawImage(img.Width, img.Height - 1);

            foreach (var skip in seam)
            {
                for (int y = 0; y < skip - 1; ++y)
                    ni[x, y] = img[x, y];

                for (int y = skip + 1; y < img.Height; ++y)
                    ni[x, y - 1] = img[x, y];
                
                --x;
            }

            return ni.Image;
        }

        public Bitmap RemoveVerticalSeam(List<int> seam)
        {
            int y = img.Height - 1;
            RawImage ni = new RawImage(img.Width - 1, img.Height);

            foreach(var skip in seam)
            {
                for (int x = 0; x < skip; ++x)
                    ni[x, y] = img[x, y];

                for (int x = skip + 1; x < img.Width; ++x)
                    ni[x - 1, y] = img[x, y];
                --y;
            }

            return ni.Image;
        }

        private void ComputeVerticalEnergy()
        {
            for (int x = 0; x < img.Width; ++x)
                energy[0, x] = grad[0, x];

            for (int y = 1; y < img.Height; ++y)
            {
                energy[y, 0] = grad[y, 0] + Math.Min(energy[y - 1, 0], energy[y - 1, 1]);
                energy[y, img.Width - 1] = grad[y, img.Width - 1] + Math.Min(energy[y - 1, img.Width - 2], energy[y - 1, img.Width - 1]);

                for (int x = 1; x < img.Width - 1; ++x)
                    energy[y, x] = grad[y, x] + Math.Min(energy[y - 1, x - 1], Math.Min(energy[y - 1, x], energy[y - 1, x + 1]));
            }
        }

        private void ComputeHorizontalEnergy()
        {
            for (int y = 0; y < img.Height; ++y)
                energy[y, 0] = grad[y, 0];

            for (int x = 1; x < img.Width; ++x)
            {
                energy[0, x] = grad[0, x] + Math.Min(energy[0, x - 1], energy[1, x - 1]);
                energy[img.Height - 1, x] = grad[img.Height - 1, x] + Math.Min(energy[img.Height - 1, x - 1], energy[img.Height - 1, x - 1]);

                for (int y = 1; y < img.Height - 1; ++y)
                    energy[y, x] = grad[y, x] + Math.Min(energy[y - 1, x - 1], Math.Min(energy[y, x - 1], energy[y + 1, x - 1]));
            }
        }

        private List<int> FindHorizontalSeam()
        {
            int min = int.MaxValue;
            int my = 0;
            int tmp = img.Width - 1;

            for (int y = 0; y < img.Height; ++y)
            {
                if (min > energy[y, tmp])
                {
                    min = energy[y, tmp];
                    my = y;
                }
            }

            List<int> seam = new List<int>();
            seam.Add(my);
            // Mark seam
            int dy;
            for (int x = tmp - 1; x >= 0; --x)
            {
                min = int.MaxValue;
                dy = my + 2;
                for (int y = my - 1; y < dy; ++y)
                {
                    if (y < 0 || y > img.Height - 1)
                        continue;
                    if (min > energy[y, x])
                    {
                        min = energy[y, x];
                        my = y;
                    }
                }

                seam.Add(my);
            }

            return seam;
        }

        private List<int> FindVerticalSeam()
        {
            int min = int.MaxValue;
            int mx = 0;
            int tmp = img.Height - 1;

            for (int x = 0; x < img.Width; ++x)
            {
                if (min > energy[tmp, x])
                {
                    min = energy[tmp, x];
                    mx = x;
                }
            }

            List<int> seam = new List<int>();
            seam.Add(mx);
            // Mark seam
            int dx;
            for (int y = tmp - 1; y >= 0; --y)
            {
                min = int.MaxValue;
                dx = mx+2;
                for (int x = mx - 1; x < dx; ++x)
                {
                    if ( x < 0 || x > img.Width - 1)
                        continue;
                    if (min > energy[y, x])
                    {
                        min = energy[y, x];
                        mx = x;
                    }
                }

                seam.Add(mx);
            }

            return seam;
        }


        /// <summary>
        /// Oblicza gradient
        /// TODO: sobel! or laplace
        /// </summary>
        private void ComputeGradient()
        {
            int vgrad, hgrad;
            Color a, b;
            Color c;
            
            for (int y = 0; y < img.Height; ++y)
            {
                for (int x = 0; x < img.Width; ++x)
                {
                    a = img[x, Math.Abs(y - 1)];
                    b = img[Math.Abs(x - 1),y];
                    c = img[x,y];
                    vgrad = Math.Abs((a.R - c.R)) + Math.Abs((a.G - c.G)) + Math.Abs((a.B - c.B));
                    hgrad = Math.Abs((b.R - c.R)) + Math.Abs((b.G - c.G)) + Math.Abs((b.B - c.B));

                    grad[y, x] = vgrad + hgrad;
                }
            }


            /// blur (rozmycie gaussa)
            //for (int y = 1; y < img.Height - 1; ++y)
            //{
            //    for (int x = 1; x < img.Width - 1; ++x)
            //    {
            //        grad[y, x] = (grad[y - 1, x - 1] + 2 * grad[y - 1, x] + grad[y - 1, x + 1] +
            //                       2 * grad[y, x - 1] + 4 * grad[y, x] + 2 * grad[y, x + 1] +
            //                       grad[y + 1, x - 1] + 2 * grad[y + 1, x] + grad[y + 1, x + 1]) / 16;
            //    }
            //}
        }
    }
}

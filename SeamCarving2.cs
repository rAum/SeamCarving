using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SeamCarving
{
    class SeamCarving2
    {
        private RawImage imgOriginal;
        private RawImage img;
        private RawImage imgEnergy;
        private static LUT gsLUT;
        private UInt64[,] energy;
        int tmpWidth, tmpHeight;

        public SeamCarving2()
        {
            if (gsLUT == null)
                gsLUT = new LUT(ColorModifiers.ToGrayscale);
        }

        public Bitmap EnergyVisualisation
        {
            get { return imgEnergy.Image; }
        }

        public Bitmap Input
        {
            set { imgOriginal = new RawImage(value); }
            get { return imgOriginal.Image; }
        }

        public Bitmap Output
        {
            get { return img.Image; }
        }

        public void Preprocess()
        {
            gsLUT.ModifyInPlace(ref imgOriginal);
            EdgeDetector edge = new EdgeDetector();
            edge.ModifyInPlace(ref imgOriginal);

            tmpWidth = imgOriginal.Width;
            tmpHeight = imgOriginal.Height;
        }

        Color energy_to_color(Color en)
        {   // do zapisywania energii w
            // postaci obrazka
            int energy = en.R;
            int r,g,b;
            r = g = b = 0;
            if (energy == 255)
            {
                r = g = b = 255;
            }
            else if (0 <= energy && energy < 5)
            { // black -> dark blue
                b = (int)Utils.Lerp(energy / 5.0, 0, 0xAA);
            }
            else if (5 <= energy && energy < 10)
            { // dark blue -> light blue
                g = (int)Utils.Lerp((energy - 5) / 5.0, 0x00, 0x55);
                b = (int)Utils.Lerp((energy - 5) / 5.0, 0xAA, 0xFF);
            }
            else if (10 <= energy && energy < 20)
            { // light blue -> dark green
                g = (int)Utils.Lerp((energy - 10) / 10.0, 0x55, 0xAA);
                b = (int)Utils.Lerp((energy - 10) / 10.0, 0xFF, 0x00);
            }
            else if (20 <= energy && energy < 30)
            { // dark green -> light green
                g = (int)Utils.Lerp((energy - 20) / 10.0, 0xAA, 0xFF);
            }
            else if (30 <= energy && energy < 40)
            { // light green -> dark yellow
                g = (int)Utils.Lerp((energy - 30) / 10.0, 0xFF, 0xAA);
                r = (int)Utils.Lerp((energy - 30) / 10.0, 0x00, 0xAA);
            }
            else if (40 <= energy && energy < 70)
            { // dark yellow -> light yellow
                g = (int)Utils.Lerp((energy - 40) / 30.0, 0xAA, 0xFF);
                r = (int)Utils.Lerp((energy - 40) / 30.0, 0xAA, 0xFF);
            }
            else if (70 <= energy && energy < 130)
            { // light yellow -> orange
                g = (int)Utils.Lerp((energy - 70) / 60.0, 0xFF, 0xAA);
                r = 0xFF;
            }
            else if (130 <= energy && energy < 256)
            { // orange -> red
                r = 0xFF;
                g = (int)Utils.Lerp((energy - 130) / 125.0, 0xAA, 0x00);
            }

            return Color.FromArgb(r,g,b);
        }

        #region obliczanie energii 
        void energyVertical()
        {
            int px = tmpWidth - 1;
            for (int x = 0; x < tmpWidth; ++x)
                energy[x, 0] = imgOriginal[x, 0, RawImage.Channel.Red];

            for (int y = 1; y < tmpHeight; ++y)
            {
                energy[0, y] = Utils.Min(energy[0, y - 1], energy[1, y - 1]) + imgOriginal[0, y, RawImage.Channel.Red];

                for (int x = 1; x < px; ++x)
                    energy[x, y] = Utils.Min(energy[x - 1, y - 1], energy[x, y - 1], energy[x + 1, y - 1]) + imgOriginal[x, y, RawImage.Channel.Red];

                energy[px, y] = Utils.Min(energy[px, y - 1], energy[px, y - 1]) + imgOriginal[px, y, RawImage.Channel.Red];
            }
        }

        void energyHorizontal()
        {
            int py = tmpHeight - 1;
            for (int y = 0; y < tmpHeight; ++y)
                energy[0, y] = imgOriginal[0, y, RawImage.Channel.Red];

            for (int x = 1; x < tmpWidth; ++x)
            {
                energy[x, 0] = Utils.Min(energy[x - 1, 0], energy[x - 1, 1]) + imgOriginal[x, 0, RawImage.Channel.Red];

                for (int y = 1; y < py; ++y)
                    energy[x, y] = Utils.Min(energy[x - 1, y - 1], energy[x - 1, y], energy[x - 1, y + 1]) + imgOriginal[x, y, RawImage.Channel.Red];

                energy[x, py] = Utils.Min(energy[x - 1, py], energy[x - 1, py]) + imgOriginal[x, py, RawImage.Channel.Red];
            }
        } 
        #endregion

        int minEnergy()
        {
            ulong min = ulong.MaxValue;
            int last = tmpHeight - 1;
            int minIndex = -1;

            for (int x = 0; x < tmpWidth; ++x)
            {
                if (min > energy[x, last])
                {
                    min = energy[x, last];
                    minIndex = x;
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Tries to find vertical (up-down) seam with minimal energy
        /// </summary>
        /// <param name="number">how many seams needed</param>
        /// <returns>object representing founded seams</returns>
        SeamList FindVerticalSeams(int need)
        {
            int dx;
            int height = tmpHeight;
            int last = height - 1;
            List<int> blacklist = new List<int>(need);

            SeamList founded_seams = new SeamList(height, SeamList.SeamType.Vertical);

            while (founded_seams.Count != need)
            {
                Seam seam = new Seam(tmpHeight);

                // find start index
                dx = minEnergy();

                // no other minimum - quit
                if (dx == -1 || blacklist.Contains(dx))
                    break;

                seam.PushBack(dx);
                blacklist.Add(dx); // mark as visited

                // build minimum seam (going up)
                ulong min;
                int minIndex = -1;
                for (int y = height - 1; y >= 0; --y)
                {
                    min = ulong.MaxValue;

                    /// find location of min
                    for (int x = dx - 1; x <= dx + 1; ++x)
                    {
                        if (x < 0 || x > tmpWidth)
                            continue;

                        if (energy[x, y] < min)
                        {
                            min = energy[x, y];
                            minIndex = x;
                        }
                    }

                    if (min == ulong.MaxValue)
                        break;

                    dx = minIndex;
                    seam.PushBack(dx);
                }

                // if seam is ok, then add and try to found another (if needed)
                if (seam.IsOK)
                    founded_seams.AddSeam(seam);
            }

            return founded_seams;
        }

        public void Resize(int nw, int nh)
        {
            int diffw = nw - imgOriginal.Width;
            int diffh = nh - imgOriginal.Height;
            int need;

            if (diffw < 0)
            {
                need = Math.Abs(diffw);

                while (need > 0)
                {
                    SeamList sl = FindVerticalSeams(diffw);
                    
                    sl.RemoveSeams(imgOriginal);

                    need -= sl.Count;
                }
            }
            else if (diffw > 0)
            {
                need = diffw;

                while (need > 0)
                {
                    SeamList sl = FindVerticalSeams(need);

                    sl.MarkSeams(imgOriginal, Color.Red);
                    //sl.DuplicateSeams(imgOriginal);

                    need -= sl.Count;
                }
            }

            if (diffh < 0)
            {
                need = Math.Abs(diffh);

                while (need > 0)
                {
                    SeamList sl = FindVerticalSeams(need);
                    sl.RemoveSeams(imgOriginal);

                    need -= sl.Count;
                }
            }
            else if (diffh > 0)
            {
                need = diffh;

                while (need > 0)
                {
                    SeamList sl = FindVerticalSeams(need);

                    need -= sl.Count;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SeamCarving
{
    public partial class WindowConvultion : Form
    {
        int[,] filter;
        RawImage original;
        RawImage mod;
        WindowPicture pic;

        public WindowConvultion(WindowPicture wnd)
        {
            InitializeComponent();

            filter = new int[5, 5];
            filter[3, 3] = 1;

            pic = wnd;
            mod = new RawImage((Bitmap) wnd.Image);
            original = new RawImage((Bitmap) wnd.Image);

            for (int i = 0; i < 5; ++i)
                filterData.Columns.Add("", "");

            for (int i = 0; i < 5; ++i)
                filterData.Rows.Add(0, 0, 0, 0, 0);

            filterData[2, 2].Value = 1;
        
            filterData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            filterData.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            filterData.ColumnHeadersVisible = false;
        }

        private void WindowConvultion_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FilterImage();
            
            if ((sender as Button).Text == "Apply")
                Close();
        }

        private void FilterImage()
        {
            int res;
            int wages = 0;

            for (int i = 0; i < 5; ++i)
            {
                for (int j = 0; j < 5; ++j)
                {
                    int.TryParse(filterData[i, j].Value.ToString(), out res);
                    filter[i, j] = res;
                    wages += res;
                }
            }

            mod.Lock();
            original.Lock();

            float alpha = tbBlend.Value / 100.0f;

            int r, g, b;
            int f, mi, mj;
            for (int y = 0; y < original.Height; ++y)
            {
                for (int x = 0; x < original.Width; ++x)
                {
                    r = g = b = 0;

                    for (int i = 0; i < 5; ++i)
                    {
                        mi = i - 2;
                        for (int j = 0; j < 5; ++j)
                        {
                            f = filter[i, j];
                            mj = j - 2;

                            r += original[x - mi, y - mj, RawImage.Channel.Red] * f;
                            g += original[x - mi, y - mj, RawImage.Channel.Green] * f;
                            b += original[x - mi, y - mj, RawImage.Channel.Blue] * f;
                        }
                    }

                    if (wages != 0)
                    {
                        r /= wages;
                        g /= wages;
                        b /= wages;
                    }

                    Color o = original[x, y];
                    Color w = Clamp(Math.Abs(r), Math.Abs(g), Math.Abs(b));
                    mod[x, y] = Color.FromArgb((byte)(alpha * o.R + (1 - alpha) * w.R),
                                                (byte)(alpha * o.G + (1 - alpha) * w.G),
                                                (byte)(alpha * o.B + (1 - alpha) * w.B));
                }
            }


            pic.Image = mod.Image;
        }

        private Color Clamp(int r, int g, int b)
        {
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            return Color.FromArgb(r, g, b);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pic.Image = original.Image;
            Close();
        }

        private void filterData_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tbBlend_ValueChanged(object sender, EventArgs e)
        {
            FilterImage();
        }
    }
}

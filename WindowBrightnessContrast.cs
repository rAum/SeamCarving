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
    public partial class WindowBrightnessContrast : Form
    {
        private RawImage original;
        private RawImage mod;
        private WindowPicture pic;
        private Modification type;

        public enum Modification { Brightness, Contrast, Gamma, Exposition, Threshold };

        public WindowBrightnessContrast(WindowPicture wnd, Modification mt)
        {
            InitializeComponent();

            mod = new RawImage((Bitmap)wnd.Image);
            pic = wnd;
            original = new RawImage((Bitmap)wnd.Image);
            type = mt;
            apply = false;

            switch (mt)
            {
                case Modification.Brightness:
                    tb.Minimum = -255;
                    tb.Maximum = 255;
                    tb.Value = 0;
                    Text = label1.Text = "Brightness";
                    break;
                case Modification.Contrast:
                    tb.Maximum = 50;
                    tb.Minimum = -50;
                    tb.Value = 0;
                    Text = label1.Text = "Contrast";
                    break;
                case Modification.Gamma:
                    tb.Minimum = 10;
                    tb.Maximum = 200;
                    Text = label1.Text = "Gamma";
                    tb.Value = 100;
                    break;
                case Modification.Exposition:
                    tb.Minimum = -2000;
                    tb.Maximum = 2000;
                    tb.Value = 0;
                    Text = label1.Text = "Exposition";
                    break;
                case Modification.Threshold:
                    tb.Minimum = 0;
                    tb.Maximum = 255;
                    tb.Value = 128;
                    Text = label1.Text = "Threshold";
                    break;
            }
        }

        private bool apply;

        private void button1_Click(object sender, EventArgs e)
        {
            if (tb.Value != 0)
                apply = true;

            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (tb.Value != 0)
                pic.Image = original.Image;

            Close();
        }

        private void ChangeContrast()
        {
            mod.Lock();
            int v = tb.Value;
            float contrast = 1 + Math.Abs(v) / 10.0f;
            Color c;

            if (v < 0)
                contrast = 1.0f / contrast;

            for (int y = 0; y < mod.Height; ++y)
            {
                for (int x = 0; x < mod.Width; ++x)
                {
                    c = original[x, y];

                    mod[x, y] = Color.FromArgb( Clamp((c.R - 128) * contrast + 128),
                                                Clamp((c.G - 128) * contrast + 128),
                                                Clamp((c.B - 128) * contrast + 128));
                }
            }

            pic.Image = mod.Image;
        }


        private void ChangeExposition()
        {
            mod.Lock();
            int v = tb.Value;
            float expo = 1 + Math.Abs(v) / 1000.0f;
            Color c;

            if (v < 0)
                expo = 1.0f / expo;

            for (int y = 0; y < mod.Height; ++y)
            {
                for (int x = 0; x < mod.Width; ++x)
                {
                    c = original[x, y];

                    mod[x, y] = Color.FromArgb(Clamp(c.R * expo),
                                                Clamp(c.G * expo),
                                                Clamp(c.B * expo));
                }
            }

            pic.Image = mod.Image;
        }


        private void ChangeBrightness()
        {
            mod.Lock();
            int brightness = tb.Value;
            Color c;

            for (int y = 0; y < mod.Height; ++y)
            {
                for (int x = 0; x < mod.Width; ++x)
                {
                    c = original[x, y];

                    mod[x, y] = Color.FromArgb( Clamp(c.R + brightness),
                                                Clamp(c.G + brightness),
                                                Clamp(c.B + brightness));
                }
            }

            pic.Image = mod.Image;
        }


        private void GammaCorrection()
        {
            mod.Lock();
            float gamma = tb.Value / 100.0f;
            Color c;

            for (int y = 0; y < mod.Height; ++y)
            {
                for (int x = 0; x < mod.Width; ++x)
                {
                    c = original[x, y];

                    mod[x, y] = Color.FromArgb( Clamp( (float)(255 * Math.Pow(c.R / 255.0f, gamma) ) ),
                                                Clamp((float)(255 * Math.Pow(c.G / 255.0f, gamma))),
                                                Clamp((float)(255 * Math.Pow(c.B / 255.0f, gamma))) ); 
                }
            }

            pic.Image = mod.Image;
        }

        
        private byte Clamp(float p)
        {
            if (p < 0) return 0;
            if (p > 255) return 255;
            return (byte)p;
        }

        private byte Clamp(int p)
        {
            if (p < 0) return 0;
            if (p > 255) return 255;
            return (byte)p;
        }

        private void WindowBrightnessContrast_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!apply)
                button2_Click(sender, e);
        }

        private void tbValueChanged(object sender, EventArgs e)
        {
            switch (type)
            {
                case Modification.Contrast:
                    ChangeContrast();
                    break;
                case Modification.Brightness:
                    ChangeBrightness();
                    break;
                case Modification.Gamma:
                    GammaCorrection();
                    break;
                case Modification.Exposition:
                    ChangeExposition();
                    break;
                case Modification.Threshold:
                    Threshold();
                    break;
            }
        }

        private void Threshold()
        {
            mod.Lock();
            int threshold = tb.Value;
            int gs;
            Color c;

            for (int y = 0; y < mod.Height; ++y)
            {
                for (int x = 0; x < mod.Width; ++x)
                {
                    c = original[x, y];
                    gs = (c.R + c.G + c.B) / 3;
                    
                    if (gs < threshold)
                        mod[x, y] = Color.Black;
                    else
                        mod[x,y] = Color.White;
                }
            }

            pic.Image = mod.Image;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using AForge.Imaging;

namespace SeamCarving
{
    public partial class MainWindow : Form
    {
        LUT lut;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            saveToolStripMenuItem.Enabled = false;
            bApply.Enabled = false;
            nudWidth.Enabled = false;
            nudHeight.Enabled = false;
            cat.Visible = false;
            lWait.Visible = false;
            adjusmentToolStripMenuItem.Enabled = false;
            lut = new LUT(ColorModifiers.ToSepia);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WindowPicture pic = new WindowPicture();
                pic.SetImage(openDialog.FileName);
                pic.File = openDialog.SafeFileName;
                pic.MdiParent = this;
                pic.Show();
            }
        }

        private void MainWindow_MdiChildActivate(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;
            if (f != null && f is WindowPicture)
            {
                saveToolStripMenuItem.Enabled = true;
                adjusmentToolStripMenuItem.Enabled = true;

                WindowPicture pic = (f as WindowPicture);
                labelFileName.Text = pic.File;

                bApply.Enabled = true;

                nudWidth.Enabled = true;
                nudHeight.Enabled = true;
                nudWidth.Value = pic.Image.Width;
                nudHeight.Value = pic.Image.Height;
            }
            else
            {
                saveToolStripMenuItem.Enabled = false;
                bApply.Enabled = false;
                nudWidth.Enabled = false;
                nudHeight.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (bgWorker.IsBusy)
                MessageBox.Show("Nie masz wystarczającej ilości chomików by uruchomić kolejne skalowanie... :(", "Cholercia!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                progressBar.Visible = true;
                lWait.Visible = true;
                progressBar.Value = 0;
                cat.Visible = false;
                bgWorker.RunWorkerAsync();
            }
        }

        private void nudWidth_ValueChanged(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;
            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                nudColorize(pic);
            }
        }

        private void nudColorize(WindowPicture pic)
        {
            if ((int)nudWidth.Value < pic.Image.Width)
            {
                nudWidth.BackColor = Color.Green;
            }
            else if ((int)nudWidth.Value > pic.Image.Width)
            {
                nudWidth.BackColor = Color.Red;
            }
            else
                nudWidth.BackColor = Color.White;

            if ((int)nudHeight.Value < pic.Image.Height)
            {
                nudHeight.BackColor = Color.Green;
            }
            else if ((int)nudHeight.Value > pic.Image.Height)
            {
                nudHeight.BackColor = Color.Red;
            }
            else
                nudHeight.BackColor = Color.White;
        }

        private void nudHeight_ValueChanged(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;
            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                nudColorize(pic);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;
            if (f != null && f is WindowPicture)
            {
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    (f as WindowPicture).Image.Save(saveDialog.FileName);
                }
            }
        }

        private int Clamp(int i)
        {
            if (i > 100)
                return 100;
            return i;
        }
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = Clamp(e.ProgressPercentage);
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = sender as BackgroundWorker;

            Form f = ActiveMdiChild;
            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                SeamCarving seam = new SeamCarving(pic.Image);

                Bitmap tmp = (Bitmap)seam.Retarget((int)nudWidth.Value, (int)nudHeight.Value, bg);
                e.Result = new KeyValuePair<WindowPicture, Bitmap>(pic, tmp);
            }

        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            KeyValuePair<WindowPicture, Bitmap> pair = (KeyValuePair<WindowPicture, Bitmap>) e.Result;
            WindowPicture pic = pair.Key;

            if (MdiChildren.Contains(pic))
            {
                pic.Image = pair.Value;

                nudWidth.Value = pic.Image.Width;
                nudHeight.Value = pic.Image.Height;
                nudColorize(pic);
            }

            progressBar.Visible = false;
            cat.Visible = true;
            lWait.Visible = false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            WindowPicture pic = ActiveMdiChild as WindowPicture;

            if (MdiChildren.Contains(pic))
            {
                SeamCarving seam = new SeamCarving((Bitmap)pic.Image);
                //seam.Image = (Bitmap)pic.Image;
                pic.Image = seam.Retarget(pic.Image.Width - 1, pic.Image.Height);

                //WindowPicture wen = new WindowPicture();
                //wen.Image = seam.EnergyImage;
                //wen.File = "Energy";
                //wen.MdiParent = this;
                //wen.Show();

                //pic.Image = seam.ResultImage;
                //pic.RefreshPicture();
                


                nudWidth.Value = pic.Image.Width;
                nudHeight.Value = pic.Image.Height;
                nudColorize(pic);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WindowPicture pic = ActiveMdiChild as WindowPicture;

            if (MdiChildren.Contains(pic))
            {
                SeamCarving2 seam = new SeamCarving2();
                seam.Input = (Bitmap) pic.Image;
                seam.Preprocess();
                pic.Image = seam.Input;


                pic.Refresh();


                nudWidth.Value = pic.Image.Width;
                nudHeight.Value = pic.Image.Height;
                nudColorize(pic);
            }

        }

        private void brightnessContrastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;

            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                WindowBrightnessContrast mod = new WindowBrightnessContrast(pic, WindowBrightnessContrast.Modification.Brightness);
                mod.ShowDialog();
            }
        }

        private void negativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;
            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;

                RawImage mod = new RawImage((Bitmap) pic.Image);
                Color c;
                
                for (int y = 0; y < mod.Height; ++y)
                {
                    for (int x = 0; x < mod.Width; ++x)
                    {
                        c = mod[x,y];
                        mod[x, y] = Color.FromArgb( 255 - c.R, 255 - c.G, 255 - c.B );
                    }
                }

                pic.Image = mod.Image;
            }
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;
            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;

                RawImage mod = new RawImage((Bitmap)pic.Image);
                Color c;
                byte gs;

                for (int y = 0; y < mod.Height; ++y)
                {
                    for (int x = 0; x < mod.Width; ++x)
                    {
                        c = mod[x, y];
                        gs = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                        mod[x, y] = Color.FromArgb(gs,gs,gs);
                    }
                }

                pic.Image = mod.Image;
            }
        }

        private void contrastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;

            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                WindowBrightnessContrast mod = new WindowBrightnessContrast(pic, WindowBrightnessContrast.Modification.Contrast);
                mod.ShowDialog();
            }
        }

        private void gammaCorrectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;

            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                WindowBrightnessContrast mod = new WindowBrightnessContrast(pic, WindowBrightnessContrast.Modification.Gamma);
                mod.ShowDialog();
            }
        }



        private void expositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;

            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                WindowBrightnessContrast mod = new WindowBrightnessContrast(pic, WindowBrightnessContrast.Modification.Exposition);
                mod.ShowDialog();
            }
        }

        private void filtersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;

            if (f != null && f is WindowPicture)
            {
                WindowConvultion w = new WindowConvultion(f as WindowPicture);
                w.ShowDialog();
            }
        }

        private void thresholdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;

            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                WindowBrightnessContrast mod = new WindowBrightnessContrast(pic, WindowBrightnessContrast.Modification.Threshold);
                mod.ShowDialog();
            }
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = ActiveMdiChild;

            if (f != null && f is WindowPicture)
            {
                WindowPicture pic = f as WindowPicture;
                RawImage raw = new RawImage((Bitmap)pic.Image);

                pic.Image = lut.Modify(raw).Image;
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace SeamCarving
{
    public partial class WindowPicture : Form
    {
        private int oldWidth, oldHeight;
        private Image img;
        public WindowPicture()
        {
            InitializeComponent();
        }

        public Image Image
        {
            get { return img; }
            set 
            {
                img = value;
                pictureBox.Image = img;
                pictureBox.Width = img.Width;
                pictureBox.Height = img.Height;
                pictureBox.Invalidate();
                Text = File;
            }
        }

        public void RefreshPicture() { pictureBox.Invalidate(); }

        private void WindowPicture_ResizeEnd(object sender, EventArgs e)
        {
        }

        private void WindowPicture_ResizeBegin(object sender, EventArgs e)
        {
            oldWidth = Width;
            oldHeight = Height;
        }

        private void WindowPicture_Load(object sender, EventArgs e)
        {
            pictureBox.Image = Image;
            pictureBox.Width = Image.Width;
            pictureBox.Height = Image.Height;
            Image = pictureBox.Image;
        }

        internal void SetImage(string file)
        {
            pictureBox.Load(file);
            this.Image = pictureBox.Image;
        }

        public string File { get; set; }

        private void pictureBox_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {

        }

        private void WindowPicture_ClientSizeChanged(object sender, EventArgs e)
        {

        }

        private void WindowPicture_SizeChanged(object sender, EventArgs e)
        {
            /*
            if (pictureBox.Height < panel1.Height || pictureBox.Width < panel1.Width)
            {
                pictureBox.Dock = DockStyle.Fill;
                pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                pictureBox.Dock = DockStyle.None;
                pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            }*/
        }
    }
}

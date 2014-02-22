using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeamCarving
{
    class Seam
    {
        List<int> seam;
        int length;

        public Seam(int size)
        {
            seam = new List<int>(size);
            length = size;
        }

        public bool IsOK
        {
            get { return seam.Count == length; }
        }

        public void PushBack(int i)
        {
            seam.Add(i);
        }

        public int this[int i]
        {
            get { return seam[i]; }
            set { seam[i] = value; }
        }

        public int Count
        {
            get { return seam.Count; }
        }
    }
}

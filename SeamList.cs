using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SeamCarving
{
    class SeamList
    {
        List<int>[] seams;
        int len;
        int seamsCount;
        SeamType type;

        public enum SeamType { Horizontal, Vertical };

        public SeamType Type
        {
            get { return type; }
        }

        public int Count
        {
            get { return seamsCount; }
        }

        public SeamList(int length, SeamType seamType)
        {
            seams = new List<int>[length];
            len = length;
            seamsCount = 0;
            type = seamType;
        }

        public void AddSeam(Seam seam)
        {
            for (int i = len - 1; i >= 0; --i)
            {
                List<int> list = seams[len - i - 1];
                list.Add(seam[i]);
                Sort(list);
            }

            ++seamsCount;
        }

        private static void Sort(List<int> list)
        {
            int tmp;

            for (int j = list.Count - 2; j >= 0; --j)
            {
                if (list[j] > list[j + 1])
                {
                    tmp = list[j];
                    list[j] = list[j + 1];
                    list[j + 1] = tmp;
                }
                else break;
            }
        }

        public IEnumerable<List<int>> Rows()
        {
            foreach (var row in seams)
            {
                yield return row;
            }
        }

        public RawImage RemoveSeams(RawImage img)
        {
            switch (type)
            {
                case SeamType.Vertical:
                    return RemoveVerticalSeamsFromImage(img);
                default:
                    return RemoveHorizontalSeamsFromImage(img);
            }
        }

        public RawImage MarkSeams(RawImage img, Color color)
        {
            switch (type)
            {
                case SeamType.Vertical:
                    return MarkVerticalSeams(img, color);
                default:
                    return MarkVerticalSeams(img, color);
            }
        }

        private RawImage MarkVerticalSeams(RawImage img, Color color)
        {
            RawImage rim = new RawImage(img.Width, img.Height);

            for (int y = 0; y < img.Height; ++y)
            {
                List<int> list = seams[y];

                for (int x = 0; x < img.Width; ++x)
                {
                    //TODO: do quicker
                    if (list.Contains(x))
                        rim[x, y] = color;
                    else
                        rim[x, y] = img[x, y];
                }
            }

            return rim;
        }

        private RawImage MarkHorizontalSeams(RawImage img, Color color)
        {
            RawImage rim = new RawImage(img.Width, img.Height);

            for (int x = 0; x < img.Width; ++x)
            {
                List<int> list = seams[x];

                for (int y = 0; y < img.Height; ++y)
                {
                    if (list.Contains(y))
                        rim[x,y] = color;
                    else
                        rim[x,y] = img[x,y];
                }
            }

            return rim;
        }

        private RawImage RemoveVerticalSeamsFromImage(RawImage img)
        {
            RawImage rim = new RawImage(img.Width - seamsCount, img.Height);

            for (int y = 0; y < img.Height; ++y)
            {
                List<int> list = seams[y];
                int dx = 0;

                for (int x = 0; x < img.Width; ++x)
                {
                    //TODO: do quicker
                    if (list.Contains(x))
                        continue;

                    rim[dx, y] = img[x, y];
                    ++dx;
                }
            }

            return rim;
        }

        private RawImage RemoveHorizontalSeamsFromImage(RawImage img)
        {
            RawImage rim = new RawImage(img.Width, img.Height - seamsCount);

            for (int x = 0; x < img.Width; ++x)
            {
                List<int> list = seams[x];
                int dy = 0;

                for (int y = 0; y < img.Height; ++y)
                {
                    //TODO: do quicker
                    if (list.Contains(y))
                        continue;

                    rim[x, dy] = img[x, y];
                    ++dy;
                }
            }

            return rim;
        }
    }
}

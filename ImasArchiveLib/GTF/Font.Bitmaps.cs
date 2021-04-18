using System;
using System.Drawing;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        private GTF gtf;
        private Bitmap nonGtfBitmap;

        private void SetGtf(GTF value)
        {
            nonGtfBitmap?.Dispose();
            gtf = value;
        }

        public Bitmap BigBitmap
        {
            get => gtf != null ? gtf.Bitmap : nonGtfBitmap;
            set
            {
                gtf?.Dispose();
                nonGtfBitmap = value;
            }
        }

        private void ClearBigBitmap()
        {
            gtf?.Dispose();
            gtf = null;
            nonGtfBitmap?.Dispose();
            nonGtfBitmap = null;
        }

        private bool charsHaveBitmaps;

        private Bitmap GetCharBitmap(CharData c)
        {
            Bitmap charBitmap = new Bitmap(c.datawidth, c.dataheight);
            for (int y = 0; y < c.dataheight; y++)
            {
                for (int x = 0; x < c.datawidth; x++)
                {
                    int xx = c.datax + x;
                    int yy = c.datay + y;
                    Color color;
                    if (xx < 0 || yy < 0 || xx >= BigBitmap.Width || yy >= BigBitmap.Height)
                        color = Color.Transparent;
                    else
                        color = BigBitmap.GetPixel(xx, yy);
                    charBitmap.SetPixel(x, y, color);
                }
            }
            return charBitmap;
        }

        private void UseCharBitmaps()
        {
            if (!charsHaveBitmaps && BigBitmap != null)
            {
                foreach (CharData c in chars)
                {
                    c.bitmap = GetCharBitmap(c);
                }
            }
            ClearBigBitmap();
            charsHaveBitmaps = true;
        }

        public void UseBlackBitmaps()
        {
            UseCharBitmaps();
            foreach (CharData c in chars)
            {
                if (c.isEmoji == 0)
                {
                    for (int y = 0; y < c.bitmap.Height; y++)
                    {
                        for (int x = 0; x < c.bitmap.Width; x++)
                        {
                            int a = c.bitmap.GetPixel(x, y).A;
                            c.bitmap.SetPixel(x, y, Color.FromArgb(a, Color.Black));
                        }
                    }
                }
            }
        }

        // have 2 pixels of space between chars
        // 1 pixel led to artifacts around the chars :(
        private void UseBigBitmap()
        {
            if (BigBitmap == null && charsHaveBitmaps)
            {
                BigBitmap = new Bitmap(2048, 2048);
                Array.Sort(chars, (c1, c2) =>
                {
                    int result = c2.dataheight.CompareTo(c1.dataheight);
                    if (result != 0)
                        return result;
                    else
                        return c1.index.CompareTo(c2.index);
                }
                );
                short x = 2049;
                short y = -1;
                byte lineHeight = 0;
                foreach (CharData c in chars)
                {
                    if (x + c.datawidth > 2048 + 1)
                    {
                        y += lineHeight;
                        lineHeight = c.dataheight;
                        //lineHeight--;
                        x = -1;
                    }
                    CopyCharBitmap(BigBitmap, c, x, y);
                    c.datax = x;
                    c.datay = y;
                    x += c.datawidth;
                    //x--;
                }
                Array.Sort(chars);
                foreach (CharData c in chars)
                {
                    c.bitmap?.Dispose();
                    c.bitmap = null;
                }
                charsHaveBitmaps = false;
            }
        }

        private void CopyCharBitmap(Bitmap bigBitmap, CharData c, int x, int y)
        {
            for (int yy = 1; yy < c.dataheight - 1; yy++)
            {
                for (int xx = 1; xx < c.datawidth - 1; xx++)
                {
                    bigBitmap.SetPixel(x + xx, y + yy, c.bitmap.GetPixel(xx, yy));
                }
            }
        }

        public void RecreateBigBitmap()
        {
            UseCharBitmaps();
            UseBigBitmap();
        }
    }
}
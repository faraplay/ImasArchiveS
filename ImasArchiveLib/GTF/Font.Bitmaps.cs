using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        private GTF gtf;
        private Bitmap NonGtfBitmap { get; set; }

        private void SetGtf(GTF value)
        {
            NonGtfBitmap?.Dispose();
            gtf = value;
        }

        public Bitmap BigBitmap
        {
            get => gtf != null ? gtf.Bitmap : NonGtfBitmap;
            set
            {
                gtf?.Dispose();
                NonGtfBitmap = value;
                _bigBitmapPixelData = GetPixelData(BigBitmap, out _, out _);
            }
        }
        private int BigBitmapWidth => 2048;
        private int BigBitmapHeight => 2048;
        private int BigBitmapStride => 2048;

        private int[] _bigBitmapPixelData;
        private int[] BigBitmapPixelData
        {
            get
            {
                if (_bigBitmapPixelData == null)
                    _bigBitmapPixelData = GetPixelData(BigBitmap, out _, out _);
                return _bigBitmapPixelData;
            }
        }
        private void UpdateBigBitmapPixelData() => _bigBitmapPixelData = GetPixelData(BigBitmap, out _, out _);
        private void UpdateBigBitmap()
        {
            if (_bigBitmapPixelData == null)
                return;
            if (BigBitmap == null)
                NonGtfBitmap = new Bitmap(BigBitmapWidth, BigBitmapHeight, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = BigBitmap.LockBits(
                new Rectangle(0, 0, BigBitmap.Width, BigBitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(_bigBitmapPixelData, 0, bitmapPtr, BigBitmapWidth * BigBitmapHeight);
            BigBitmap.UnlockBits(bitmapData);
        }

        private void ClearBigBitmap()
        {
            gtf?.Dispose();
            gtf = null;
            NonGtfBitmap?.Dispose();
            NonGtfBitmap = null;
        }

        private int[] GetCharPixelData(int[] bigBitmap, CharData c)
        {
            int[] pixelData = new int[c.datawidth * c.dataheight];
            for (int y = 0; y < c.dataheight; y++)
            {
                for (int x = 0; x < c.datawidth; x++)
                {
                    int xx = c.datax + x;
                    int yy = c.datay + y;
                    int color;
                    if (xx < 0 || yy < 0 ||
                        xx >= BigBitmapWidth || yy >= BigBitmapHeight)
                        color = Color.Transparent.ToArgb();
                    else
                        color = bigBitmap[yy * BigBitmapStride + xx];
                    pixelData[y * c.datawidth + x] = color;
                }
            }
            return pixelData;
        }

        private Dictionary<ushort, int[]> GetCharBitmaps(int[] bigBitmap, IEnumerable<CharData> chars)
        {
            Dictionary<ushort, int[]> charBitmaps = new Dictionary<ushort, int[]>();
            foreach (CharData c in chars)
            {
                charBitmaps.Add(c.key, GetCharPixelData(bigBitmap, c));
            }
            return charBitmaps;
        }

        public void UseBlackBitmaps()
        {
            var charBitmaps = GetCharBitmaps(BigBitmapPixelData, chars);
            foreach (CharData c in chars)
            {
                if (c.isEmoji == 0)
                {
                    for (int y = 0; y < c.dataheight; y++)
                    {
                        for (int x = 0; x < c.datawidth; x++)
                        {
                            charBitmaps[c.key][y * c.datawidth + x] &= -0x01000000;
                        }
                    }
                }
            }
            BuildBigBitmap(charBitmaps, chars);
        }

        // have 2 pixels of space between chars
        // 1 pixel led to artifacts around the chars :(
        private void BuildBigBitmap(Dictionary<ushort, int[]> charBitmaps, IEnumerable<CharData> chars)
        {
            int[] newPixelData = new int[BigBitmapWidth * BigBitmapHeight];
            short x = 2049;
            short y = -1;
            byte lineHeight = 0;
            foreach (CharData c in chars.OrderBy(c => c.key - (c.dataheight << 16)))
            {
                if (x + c.datawidth > 2048 + 1)
                {
                    y += lineHeight;
                    lineHeight = c.dataheight;
                    x = -1;
                }
                CopyCharBitmap(charBitmaps[c.key], c, newPixelData, BigBitmapStride, x, y);
                c.datax = x;
                c.datay = y;
                x += c.datawidth;
            }
            _bigBitmapPixelData = newPixelData;
            UpdateBigBitmap();
        }

        private void CopyCharBitmap(int[] charBitmap, CharData c, int[] destBitmap, int stride, int destX, int destY)
        {
            for (int yy = 1; yy < c.dataheight - 1; yy++)
            {
                for (int xx = 1; xx < c.datawidth - 1; xx++)
                {
                    destBitmap[(destY + yy) * stride + (destX + xx)] = charBitmap[yy * c.datawidth + xx];
                }
            }
        }

        public void RecreateBigBitmap()
        {
            var charBitmaps = GetCharBitmaps(BigBitmapPixelData, chars);
            BuildBigBitmap(charBitmaps, chars);
        }

        private static int[] GetPixelData(Bitmap bitmap, out int width, out int height)
        {
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            width = bitmap.Width;
            height = bitmap.Height;
            int stride = bitmapData.Stride / 4;
            int[] pixelData = new int[stride * height];
            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapPtr, pixelData, 0, stride * height);
            bitmap.UnlockBits(bitmapData);
            if (stride == width)
                return pixelData;

            int[] condensedPixelData = new int[width * height];
            for (int y = 0; y < height; ++y)
            {
                Array.Copy(pixelData, y * stride, condensedPixelData, y * width, width);
            }
            return condensedPixelData;
        }

        private static void SavePngFromPixelData(Stream outStream, int[] pixelData, int width, int height)
        {
            int stride = width;
            IntPtr bitmapDataPtr = Marshal.AllocHGlobal(4 * stride * height);
            using (Bitmap bitmap = new Bitmap(width, height, 4 * stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmapDataPtr))
            {
                Marshal.Copy(pixelData, 0, bitmapDataPtr, stride * height);
                bitmap.Save(outStream, ImageFormat.Png);
            }
            Marshal.FreeHGlobal(bitmapDataPtr);
        }

        public void SaveBigBitmap(string saveLocation)
        {
            using FileStream fileStream = new FileStream(saveLocation, FileMode.Create);
            SavePngFromPixelData(fileStream, BigBitmapPixelData, BigBitmapWidth, BigBitmapHeight);
        }
    }
}
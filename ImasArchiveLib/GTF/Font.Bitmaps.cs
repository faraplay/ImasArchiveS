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
        private const int BigBitmapWidth = 2048;
        private const int BigBitmapHeight = 2048;
        private const int BigBitmapStride = 2048;

        private int[] GetCharPixelData(int[] bigBitmap, CharData c)
        {
            int[] pixelData = new int[c.paddedBbWidth * c.paddedBbHeight];
            for (int y = 0; y < c.paddedBbHeight; y++)
            {
                for (int x = 0; x < c.paddedBbWidth; x++)
                {
                    int xx = c.dataX + x;
                    int yy = c.dataY + y;
                    int color;
                    if (xx < 0 || yy < 0 ||
                        xx >= BigBitmapWidth || yy >= BigBitmapHeight)
                        color = Color.Transparent.ToArgb();
                    else
                        color = bigBitmap[yy * BigBitmapStride + xx];
                    pixelData[y * c.paddedBbWidth + x] = color;
                }
            }
            return pixelData;
        }

        private Dictionary<ushort, int[]> GetCharBitmaps()
        {
            Dictionary<ushort, int[]> charBitmaps = new Dictionary<ushort, int[]>();
            foreach (CharData c in chars)
            {
                charBitmaps.Add(c.key, GetCharPixelData(Gtf.PixelData, c));
            }
            return charBitmaps;
        }

        public void UseBlackBitmaps()
        {
            var charBitmaps = GetCharBitmaps();
            foreach (CharData c in chars)
            {
                if (c.isEmoji == 0)
                {
                    for (int y = 0; y < c.paddedBbHeight; y++)
                    {
                        for (int x = 0; x < c.paddedBbWidth; x++)
                        {
                            charBitmaps[c.key][y * c.paddedBbWidth + x] &= -0x01000000;
                        }
                    }
                }
            }
            RebuildMyBitmap(charBitmaps, chars);
        }

        private void RebuildMyBitmap(Dictionary<ushort, int[]> charBitmaps, IEnumerable<CharData> chars)
        {
            Gtf.LoadPixelData(BuildBitmap(charBitmaps, chars));
        }

        // have 2 pixels of space between chars
        // 1 pixel led to artifacts around the chars :(
        private static int[] BuildBitmap(Dictionary<ushort, int[]> charBitmaps, IEnumerable<CharData> chars)
        {
            int[] newPixelData = new int[BigBitmapWidth * BigBitmapHeight];
            short x = 2049;
            short y = -1;
            byte lineHeight = 0;
            foreach (CharData c in chars.OrderBy(c => c.key - (c.paddedBbHeight << 16)))
            {
                if (x + c.paddedBbWidth > 2048 + 1)
                {
                    y += lineHeight;
                    lineHeight = c.paddedBbHeight;
                    x = -1;
                }
                CopyCharBitmap(charBitmaps[c.key], c, newPixelData, BigBitmapStride, x, y);
                c.dataX = x;
                c.dataY = y;
                x += c.paddedBbWidth;
            }
            return newPixelData;
        }

        private static void CopyCharBitmap(int[] charBitmap, CharData c, int[] destBitmap, int stride, int destX, int destY)
        {
            for (int yy = 1; yy < c.paddedBbHeight - 1; yy++)
            {
                for (int xx = 1; xx < c.paddedBbWidth - 1; xx++)
                {
                    destBitmap[(destY + yy) * stride + (destX + xx)] = charBitmap[yy * c.paddedBbWidth + xx];
                }
            }
        }

        public void CompressBitmap()
        {
            var charBitmaps = GetCharBitmaps();
            RebuildMyBitmap(charBitmaps, chars);
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
            using (Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapDataPtr))
            {
                Marshal.Copy(pixelData, 0, bitmapDataPtr, stride * height);
                bitmap.Save(outStream, ImageFormat.Png);
            }
            Marshal.FreeHGlobal(bitmapDataPtr);
        }

        public void SaveBigBitmap(string saveLocation)
        {
            using FileStream fileStream = new FileStream(saveLocation, FileMode.Create);
            Gtf.SavePngTo(fileStream);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Imas
{
    class MyQuantizer
    {
        public static Bitmap QuantizeImage(Bitmap image)
        {
            Color[] palette = new Color[256];
            nQuant.WuQuantizer wuQuantizer = new nQuant.WuQuantizer();
            using (Bitmap qBitmap = (Bitmap)wuQuantizer.QuantizeImage(image)) {
                qBitmap.Palette.Entries.CopyTo(palette, 0);
            }

            int width = image.Width;
            int height = image.Height;

            // get raw data of image
            if (image.PixelFormat != PixelFormat.Format32bppArgb)
                throw new NotSupportedException("Wrong pixel format - expected 32bppArgb");

            BitmapData bitmapData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            IntPtr bitmapPtr = bitmapData.Scan0;
            int stride = bitmapData.Stride / 4;
            int[] bitmapArray = new int[stride * image.Height];
            Marshal.Copy(bitmapPtr, bitmapArray, 0, stride * image.Height);
            image.UnlockBits(bitmapData);

            // first pass: count colours
            //Dictionary<uint, int> colors = new Dictionary<uint, int>();
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        uint b = (uint)bitmapArray[y * stride + x];
            //        if (colors.ContainsKey(b))
            //        {
            //            colors[b]++;
            //        }
            //        else
            //        {
            //            colors.Add(b, 1);
            //        }
            //    }
            //}

            int indexedStride = (width + 3) & -4;
            IntPtr indexedBitmapPtr = Marshal.AllocHGlobal(indexedStride * height);
            byte[] indexedBitmapArray = new byte[indexedStride * height];

            // nearest neighbour
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uint c = (uint)bitmapArray[y * stride + x];
                    int bestIndex = 0;
                    long bestDistance = long.MaxValue;
                    for (int i = 0; i < 256; i++)
                    {
                        long a1 = ((c & 0xFF000000) >> 24);
                        long r1 = ((c & 0x00FF0000) >> 16);
                        long g1 = ((c & 0x0000FF00) >> 8);
                        long b1 = ((c & 0x000000FF) >> 0);
                        b1 *= a1;
                        g1 *= a1;
                        r1 *= a1;
                        a1 *= 255;
                        long a2 = palette[i].A;
                        long r2 = palette[i].R;
                        long g2 = palette[i].G;
                        long b2 = palette[i].B;
                        b2 *= a2;
                        g2 *= a2;
                        r2 *= a2;
                        a2 *= 255;
                        long dist = (a1 - a2) * (a1 - a2)
                                + (r1 - r2) * (r1 - r2)
                                + (g1 - g2) * (g1 - g2)
                                + (b1 - b2) * (b1 - b2);
                        if (dist < bestDistance)
                        {
                            bestIndex = i;
                            bestDistance = dist;
                        }
                    }
                    indexedBitmapArray[y * indexedStride + x] = (byte)bestIndex;
                }
            }

            Marshal.Copy(indexedBitmapArray, 0, indexedBitmapPtr, indexedStride * height);
            Bitmap indexedBitmap = new Bitmap(width, height, indexedStride, PixelFormat.Format8bppIndexed, indexedBitmapPtr);

            ColorPalette colorPalette = indexedBitmap.Palette;
            for (int i = 0; i < 0x100; i++)
                colorPalette.Entries[i] = palette[i];
            indexedBitmap.Palette = colorPalette;

            return indexedBitmap;
        }
    }
}

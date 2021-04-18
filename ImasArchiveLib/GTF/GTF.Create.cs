
using System;
using System.Drawing;
using System.IO;

namespace Imas.Gtf
{
    public partial class GTF
    {
        public static GTF CreateFromPixelData(int[] pixelData, int encodingType, int width, int height, int stride)
        {
            GTF gtf = new GTF(pixelData, encodingType, width, height, stride);
            gtf.CopyPixelDataToBitmap();
            return gtf;
        }
        public static GTF CreateFromBitmap(Bitmap bitmap, int encodingType)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = width;
            int[] pixelData = new int[stride * height];
            GTF gtf = new GTF(pixelData, encodingType, width, height, stride);
            gtf.LoadBitmap(bitmap);
            return gtf;
        }
        public static GTF CreateFromGtfStream(Stream stream)
        {
            ReadHeader(stream, out int type, out int width, out int height, out Color[] palette);

            int stride = width;
            int[] pixelData = (type & 15) switch
            {
                1 => ReadPixels(stream, width, height, stream => GetPixelIndexed(stream, palette), stride),
                2 => ReadPixels(stream, width, height, GetPixel1555, stride),
                3 => ReadPixels(stream, width, height, GetPixel4444, stride),
                5 => ReadPixels(stream, width, height, GetPixel8888, stride),
                6 => ReadBlocks(stream, width, height, GetBlockNoAlpha, stride),
                7 => ReadBlocks(stream, width, height, GetBlock4Alpha, stride),
                8 => ReadBlocks(stream, width, height, GetBlock8RelAlpha, stride),
                0xE => ReadPixels(stream, width, height, GetPixel8888, stride),
                _ => throw new NotSupportedException()
            };
            return CreateFromPixelData(pixelData, type, width, height, stride);
        }

    }
}
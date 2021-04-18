using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Imas.Gtf
{
    public partial class GTF
    {
        public static GTF ReadGTF(Stream stream)
        {
            ReadHeader(stream, out int type, out int width, out int height, out Color[] palette);

            int stride = width;
            int[] bitmapArray = (type & 15) switch
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

            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapPtr);
            return new GTF(bitmap, bitmapPtr, bitmapArray, type, width, height, stride);
        }

        private static void ReadHeader(Stream stream, out int type, out int width, out int height, out Color[] palette)
        {
            long pos = stream.Position;
            Binary binary = new Binary(stream, true);

            binary.ReadUInt32(); // version (1.1.0.0, 2.2.0.-1 or 2.2.0.0)
            binary.ReadUInt32(); // size of file minus header
            int partCount = binary.ReadInt32();

            binary.ReadUInt32(); // 0 : index of first part
            binary.ReadUInt32(); // 0x80 : offset of first part
            binary.ReadUInt32(); // size of first part
            type = binary.ReadByte();
            binary.ReadByte(); // mipmap count
            binary.ReadUInt16(); // 0x0200
            binary.ReadUInt32(); // part type??
            width = binary.ReadUInt16();
            height = binary.ReadUInt16();
            binary.ReadUInt16(); // 1
            binary.ReadUInt16(); // 0
            binary.ReadUInt32(); // stride
            binary.ReadUInt32(); // 0

            binary.ReadUInt32(); // 1 : index of second part
            int paletteData = binary.ReadInt32(); // offset of second part
            binary.ReadInt32(); // size of second part
            binary.ReadByte(); // 0x85 : type of second part
            binary.ReadByte(); // 1 : mipmap count
            binary.ReadUInt16(); // 0x0200
            binary.ReadUInt32(); // 0xAA6C : part type??
            int paletteWidth = binary.ReadUInt16();
            binary.ReadUInt16(); // 1 : height
            binary.ReadUInt16(); // 1
            binary.ReadUInt16(); // 0
            binary.ReadUInt32(); // 0 : stride
            binary.ReadUInt32(); // 0

            stream.Position += 0x2C;

            palette = null;
            if (partCount == 2)
            {
                int paletteRepeat = paletteWidth / 0x100;
                palette = new Color[0x100];
                stream.Position = pos + paletteData;
                for (int n = 0; n < paletteWidth; n++)
                {
                    int b0 = stream.ReadByte();
                    int b1 = stream.ReadByte();
                    int b2 = stream.ReadByte();
                    int b3 = stream.ReadByte();
                    palette[n / paletteRepeat] = Color.FromArgb(b0, b3, b2, b1);
                }
                stream.Position = pos + 128;
            }
        }

        private static int[] ReadPixels(Stream stream, int width, int height, Func<Stream, uint> getPixel, int stride)
        {
            int[] bitmapArray = new int[stride * height];
            Order order = new Order(width, height);
            for (int n = 0; n < width * height; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = (int)getPixel(stream);
            }
            return bitmapArray;
        }

        private static int[] ReadBlocks(Stream stream, int width, int height, Action<Binary, int[,], int[]> getBlock, int stride)
        {
            int[] bitmapArray = new int[stride * height];
            Binary binary = new Binary(stream, false);
            for (int y = 0; y < height / 4; y++)
            {
                for (int x = 0; x < width / 4; x++)
                {
                    int[,] alphas = new int[4, 4];
                    int[] colorVals = new int[4];
                    getBlock(binary, alphas, colorVals);

                    for (int yy = 0; yy < 4; yy++)
                    {
                        byte k = binary.ReadByte();
                        for (int xx = 0; xx < 4; xx++)
                        {
                            int t = k & 3;
                            bitmapArray[(4 * y + yy) * stride + 4 * x + xx] = (alphas[xx, yy] << 24) | colorVals[t];
                            k >>= 2;
                        }
                    }
                }
            }
            return bitmapArray;
        }

        private static uint GetPixelIndexed(Stream stream, Color[] palette)
        {
            return (uint)palette[Binary.ReadByte(stream, true)].ToArgb();
        }

        private static uint GetPixel1555(Stream stream)
        {
            ushort b = Binary.ReadUInt16(stream, true);
            return (uint)ColorHelp.From1555(b).ToArgb();
        }

        private static uint GetPixel4444(Stream stream)
        {
            uint b = Binary.ReadUInt16(stream, true); // 0x0000abcd
            b = (b ^ (b << 8)) & 0x00FF00FF;          // 0x00ab00cd
            b = (b ^ (b << 4)) & 0x0F0F0F0F;          // 0x0a0b0c0d
            b ^= b << 4;                              // 0xaabbccdd
            return b;
        }

        private static uint GetPixel8888(Stream stream)
        {
            return Binary.ReadUInt32(stream, true);
        }

        private static void GetBlockNoAlpha(Binary binary, int[,] alphas, int[] colorVals)
        {
            ushort c0 = binary.ReadUInt16();
            ushort c1 = binary.ReadUInt16();

            Color[] color = new Color[4];
            color[0] = ColorHelp.From565(c0);
            color[1] = ColorHelp.From565(c1);
            if (c0 <= c1)
            {
                color[2] = ColorHelp.MixRatio(color[0], color[1], 1, 1);
                color[3] = Color.FromArgb(0, 0, 0);
            }
            else
            {
                color[2] = ColorHelp.MixRatio(color[0], color[1], 2, 1);
                color[3] = ColorHelp.MixRatio(color[0], color[1], 1, 2);
            }
            for (int i = 0; i < 4; i++)
                colorVals[i] = color[i].ToArgb();
        }

        private static void GetBlock4Alpha(Binary binary, int[,] alphas, int[] colorVals)
        {
            ulong n = binary.ReadUInt64();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    alphas[j, i] = (int)((n & 15) * 17);
                    n >>= 4;
                }
            }

            ushort c0 = binary.ReadUInt16();
            ushort c1 = binary.ReadUInt16();

            Color[] color = new Color[4];
            color[0] = ColorHelp.From565(c0);
            color[1] = ColorHelp.From565(c1);
            color[2] = ColorHelp.MixRatio(color[0], color[1], 2, 1);
            color[3] = ColorHelp.MixRatio(color[0], color[1], 1, 2);
            for (int i = 0; i < 4; i++)
                colorVals[i] = color[i].ToArgb() & 0x00FFFFFF;
        }

        private static void GetBlock8RelAlpha(Binary binary, int[,] alphas, int[] colorVals)
        {
            ulong n = binary.ReadUInt64();
            int a0 = (byte)(n & 0xFF);
            n >>= 8;
            int a1 = (byte)(n & 0xFF);
            n >>= 8;
            int[] a = new int[8];
            if (a0 <= a1)
            {
                a[0] = a0;
                a[1] = a1;
                a[2] = (4 * a0 + 1 * a1) / 5;
                a[3] = (3 * a0 + 2 * a1) / 5;
                a[4] = (2 * a0 + 3 * a1) / 5;
                a[5] = (1 * a0 + 4 * a1) / 5;
                a[6] = 0;
                a[7] = 255;
            }
            else
            {
                a[0] = a0;
                a[1] = a1;
                a[2] = (6 * a0 + 1 * a1) / 7;
                a[3] = (5 * a0 + 2 * a1) / 7;
                a[4] = (4 * a0 + 3 * a1) / 7;
                a[5] = (3 * a0 + 4 * a1) / 7;
                a[6] = (2 * a0 + 5 * a1) / 7;
                a[7] = (1 * a0 + 6 * a1) / 7;
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    alphas[j, i] = a[n & 7];
                    n >>= 3;
                }
            }

            ushort c0 = binary.ReadUInt16();
            ushort c1 = binary.ReadUInt16();

            Color[] color = new Color[4];
            color[0] = ColorHelp.From565(c0);
            color[1] = ColorHelp.From565(c1);
            color[2] = ColorHelp.MixRatio(color[0], color[1], 2, 1);
            color[3] = ColorHelp.MixRatio(color[0], color[1], 1, 2);
            for (int i = 0; i < 4; i++)
                colorVals[i] = color[i].ToArgb() & 0x00FFFFFF;
        }

    }
}
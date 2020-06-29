using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    static class GTF
    {

        public static Bitmap ReadGTF(Stream stream)
        {
            stream.Position = 24;
            int type = stream.ReadByte();

            stream.Position = 32;
            int width = Utils.GetUShort(stream);
            int height = Utils.GetUShort(stream);

            stream.Position = 52;
            int paletteData = (int)Utils.GetUInt(stream);
            stream.Position = 128;
            return type switch
            {
                0x81 => GTFPaletteInterlace(stream, width, height, paletteData),
                0xA1 => GTFPalette(stream, width, height, paletteData),
                0x83 => GTF12ColorInterlace(stream, width, height),
                0x85 => GTFInterlace(stream, width, height),
                0xA5 => GTFNormal(stream, width, height),
                0x86 => GTF4x4(stream, width, height),
                0xA6 => GTF4x4(stream, width, height),
                0x88 => GTF4x4Alpha(stream, width, height),
                0xA8 => GTF4x4Alpha(stream, width, height),
                _ => throw new NotSupportedException()
            };
        }

        private static Bitmap GTFPalette(Stream stream, int width, int height, int paletteData)
        {
            Bitmap bitmap = new Bitmap(width, height);

            Color[] colors = new Color[0x100];
            stream.Position = paletteData;
            for (int n = 0; n < 0x400; n++)
            {
                int b0 = stream.ReadByte();
                int b1 = stream.ReadByte();
                int b2 = stream.ReadByte();
                int b3 = stream.ReadByte();
                colors[n / 4] = Color.FromArgb(b0, b3, b2, b1);
            }

            stream.Position = 128;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int b0 = stream.ReadByte();
                    bitmap.SetPixel(x, y, colors[b0]);
                }
            }

            return bitmap;
        }

        private static Bitmap GTFPaletteInterlace(Stream stream, int width, int height, int paletteData)
        {
            Bitmap bitmap = new Bitmap(width, height);

            Color[] colors = new Color[0x100];
            stream.Position = paletteData;
            for (int n = 0; n < 0x400; n++)
            {
                int b0 = stream.ReadByte();
                int b1 = stream.ReadByte();
                int b2 = stream.ReadByte();
                int b3 = stream.ReadByte();
                colors[n / 4] = Color.FromArgb(b0, b3, b2, b1);
            }

            stream.Position = 128;

            int size = width;
            int p = -1;
            while (size > 0)
            {
                size >>= 1;
                p++;
            }

            for (int n = 0; n < width * height; n++)
            {
                int x, y;
                (x, y) = GetXY(n, p);
                int b0 = stream.ReadByte();
                bitmap.SetPixel(x, y, colors[b0]);
            }

            return bitmap;
        }

        private static Bitmap GTFInterlace(Stream stream, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            int size = width;
            int p = -1;
            while (size > 0)
            {
                size >>= 1;
                p++;
            }

            for (int n = 0; n < width * height; n++)
            {
                int x, y;
                (x, y) = GetXY(n, p);
                int b = (int)Utils.GetUInt(stream);
                Color color = Color.FromArgb(b);
                bitmap.SetPixel(x, y, color);
            }

            return bitmap;
        }
        private static Bitmap GTFNormal(Stream stream, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int b = (int)Utils.GetUInt(stream);
                    Color color = Color.FromArgb(b);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }

        private static Bitmap GTF12ColorInterlace(Stream stream, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);

            int size = width;
            int p = -1;
            while (size > 0)
            {
                size >>= 1;
                p++;
            }

            for (int n = 0; n < width * height; n++)
            {
                int b0 = stream.ReadByte();
                int b1 = stream.ReadByte();
                int a = (b0 / 16) * 17;
                int r = (b0 % 16) * 17;
                int g = (b1 / 16) * 17;
                int b = (b1 % 16) * 17;
                Color color = Color.FromArgb(a, r, g, b);
                int x, y;
                (x, y) = GetXY(n, p);
                bitmap.SetPixel(x, y, color);
            }

            return bitmap;
        }

        private static (int, int) GetXY(int n, int p)
        {
            int[] d = new int[2 * p];
            for (int i = 0; i < 2 * p; i++)
            {
                d[i] = n % 2;
                n >>= 1;
            }
            int x = 0, y = 0;
            for (int i = p - 1; i >= 0; i--)
            {
                x <<= 1;
                y <<= 1;
                x += d[2 * i];
                y += d[2 * i + 1];
            }
            return (x, y);
        }

        private static Bitmap GTF4x4(Stream stream, int width, int height)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            Bitmap bitmap = new Bitmap(width, height);

            stream.Position = 128;
            for (int y = 0; y < height / 4; y++)
                for (int x = 0; x < width / 4; x++)
                {
                    int c0 = binaryReader.ReadUInt16();
                    int c1 = binaryReader.ReadUInt16();

                    Color[] color = new Color[4];
                    color[0] = ColorHelp.From16Bit(c0);
                    color[1] = ColorHelp.From16Bit(c1);
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

                    for (int yy = 0; yy < 4; yy++)
                    {
                        byte k = binaryReader.ReadByte();
                        for (int xx = 0; xx < 4; xx++)
                        {
                            int t = k & 3;
                            bitmap.SetPixel(4 * x + xx, 4 * y + yy, color[t]);
                            k >>= 2;
                        }
                    }
                }

            return bitmap;
        }

        private static Bitmap GTF4x4Alpha(Stream stream, int width, int height)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            Bitmap bitmap = new Bitmap(width, height);

            stream.Position = 128;
            for (int y = 0; y < height / 4; y++)
                for (int x = 0; x < width / 4; x++)
                {
                    ulong n = binaryReader.ReadUInt64();
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

                    int[,] alphas = new int[4, 4];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            alphas[j, i] = a[n & 7];
                            n >>= 3;
                        }
                    }

                    int c0 = binaryReader.ReadUInt16();
                    int c1 = binaryReader.ReadUInt16();

                    Color[] color = new Color[4];
                    color[0] = ColorHelp.From16Bit(c0);
                    color[1] = ColorHelp.From16Bit(c1);
                    color[2] = ColorHelp.MixRatio(color[0], color[1], 2, 1);
                    color[3] = ColorHelp.MixRatio(color[0], color[1], 1, 2);

                    for (int yy = 0; yy < 4; yy++)
                    {
                        byte k = binaryReader.ReadByte();
                        for (int xx = 0; xx < 4; xx++)
                        {
                            int t = k & 3;
                            bitmap.SetPixel(4 * x + xx, 4 * y + yy, Color.FromArgb(alphas[xx, yy], color[t]));
                            k >>= 2;
                        }
                    }
                }

            return bitmap;
        }


        private static class ColorHelp
        {
            public static Color From16Bit(int x)
            {
                int r0 = (x >> 11) * 8;
                int g0 = ((x >> 5) & 0x3F) * 4;
                int b0 = (x & 0x1F) * 8;
                return Color.FromArgb(r0, g0, b0);
            }

            public static Color MixRatio(Color c0, Color c1, int m0, int m1)
            {
                int r = (m0 * c0.R + m1 * c1.R) / (m0 + m1);
                int g = (m0 * c0.G + m1 * c1.G) / (m0 + m1);
                int b = (m0 * c0.B + m1 * c1.B) / (m0 + m1);
                return Color.FromArgb(r, g, b);
            }
        }
    }
}

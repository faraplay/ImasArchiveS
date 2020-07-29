using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Imas
{
    public static class GTF
    {

        public static Bitmap ReadGTF(Stream stream)
        {
            long pos = stream.Position;
            Binary binary = new Binary(stream, true);

            binary.ReadUInt32(); // version (1.1.0.0, 2.2.0.-1 or 2.2.0.0)
            binary.ReadUInt32(); // size of file minus header
            int partCount = binary.ReadInt32();

            binary.ReadUInt32(); // 0 : index of first part
            binary.ReadUInt32(); // 0x80 : offset of first part
            binary.ReadUInt32(); // size of first part
            int type = binary.ReadByte();
            binary.ReadByte(); // mipmap count
            binary.ReadUInt16(); // 0x0200
            binary.ReadUInt32(); // part type??
            int width = binary.ReadUInt16();
            int height = binary.ReadUInt16();
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

            Color[] palette = null;
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

            return (type & 15) switch
            {
                1 => ReadGTFIndexed(stream, width, height, palette),
                2 => ReadGTF1555(stream, width, height),
                3 => ReadGTF4444(stream, width, height),
                5 => ReadGTF8888(stream, width, height),
                6 => ReadGTF565Block4Color(stream, width, height),
                7 => ReadGTF565Block8Alpha4Color(stream, width, height),
                8 => ReadGTF565Block8RelAlpha4Color(stream, width, height),

                0xE => ReadGTF8888(stream, width, height),
                _ => throw new NotSupportedException()
            };
        }

        public static async Task WriteGTF(Stream stream, Bitmap bitmap, byte encodingType)
        {
            WriteHeader(stream, encodingType, bitmap.Width, bitmap.Height);
            switch (encodingType)
            {
                case 0x83:
                    await WriteGTF4444(stream, bitmap, true);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #region Read GTF
        private static Bitmap ReadGTFIndexed(Stream stream, int width, int height, Color[] palette)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format8bppIndexed);
            IntPtr bitmapPtr = bitmapData.Scan0;
            int stride = bitmapData.Stride;
            byte[] bitmapArray = new byte[stride * height];

            Order order = new Order(width, height);

            for (int n = 0; n < width * height; n++)
            {
                byte b0 = Binary.ReadByte(stream, true);
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = b0;
            }
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            bitmap.UnlockBits(bitmapData);

            ColorPalette colorPalette = bitmap.Palette;
            for (int i = 0; i < 0x100; i++)
                colorPalette.Entries[i] = palette[i];
            bitmap.Palette = colorPalette;

            return bitmap;
        }
        private static Bitmap ReadGTF1555(Stream stream, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format16bppArgb1555);
            int stride = bitmapData.Stride / 2;
            short[] bitmapArray = new short[stride * height];

            Order order = new Order(width, height);

            for (int n = 0; n < width * height; n++)
            {
                ushort b = Binary.ReadUInt16(stream, true);
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = (short)b;
            }

            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }
        private static Bitmap ReadGTF8888(Stream stream, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            int stride = bitmapData.Stride / 4;
            int[] bitmapArray = new int[stride * height];
            Order order = new Order(width, height);

            for (int n = 0; n < width * height; n++)
            {
                int b = Binary.ReadInt32(stream, true);
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = b;
            }

            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }
        private static Bitmap ReadGTF4444(Stream stream, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            int stride = bitmapData.Stride / 4;
            int[] bitmapArray = new int[stride * height];
            Order order = new Order(width, height);

            for (int n = 0; n < width * height; n++)
            {
                uint b = Binary.ReadUInt16(stream, true); // 0x0000abcd
                b = (b ^ (b << 8)) & 0x00FF00FF;          // 0x00ab00cd
                b = (b ^ (b << 4)) & 0x0F0F0F0F;          // 0x0a0b0c0d
                b ^= b << 4;                              // 0xaabbccdd
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = (int)b;
            }

            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private static Bitmap ReadGTF565Block4Color(Stream stream, int width, int height)
        {
            Binary binary = new Binary(stream, false);
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            int stride = bitmapData.Stride / 4;
            int[] bitmapArray = new int[stride * height];

            for (int y = 0; y < height / 4; y++)
                for (int x = 0; x < width / 4; x++)
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
                    int[] colorVals = new int[4];
                    for (int i = 0; i < 4; i++)
                        colorVals[i] = color[i].ToArgb();

                    for (int yy = 0; yy < 4; yy++)
                    {
                        byte k = binary.ReadByte();
                        for (int xx = 0; xx < 4; xx++)
                        {
                            int t = k & 3;
                            bitmapArray[(4 * y + yy) * stride + 4 * x + xx] = colorVals[t];
                            k >>= 2;
                        }
                    }
                }

            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private static Bitmap ReadGTF565Block8RelAlpha4Color(Stream stream, int width, int height)
        {
            Binary binary = new Binary(stream, false);
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            int stride = bitmapData.Stride / 4;
            int[] bitmapArray = new int[stride * height];

            for (int y = 0; y < height / 4; y++)
                for (int x = 0; x < width / 4; x++)
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

                    int[,] alphas = new int[4, 4];
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
                    int[] colorVals = new int[4];
                    for (int i = 0; i < 4; i++)
                        colorVals[i] = color[i].ToArgb() & 0x00FFFFFF;

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

            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private static Bitmap ReadGTF565Block8Alpha4Color(Stream stream, int width, int height)
        {
            Binary binary = new Binary(stream, false);
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);
            int stride = bitmapData.Stride / 4;
            int[] bitmapArray = new int[stride * height];

            for (int y = 0; y < height / 4; y++)
                for (int x = 0; x < width / 4; x++)
                {
                    ulong n = binary.ReadUInt64();
                    int[,] alphas = new int[4, 4];
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
                    int[] colorVals = new int[4];
                    for (int i = 0; i < 4; i++)
                        colorVals[i] = color[i].ToArgb() & 0x00FFFFFF;

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

            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }
        #endregion

        private static void WriteHeader(Stream stream, byte type, int width, int height)
        {
            Binary binary = new Binary(stream, true);
            bool isIndexed = (type & 15) == 1;
            int pixelCount = width * height;
            int pixelSize = (type & 15) switch
            {
                1 => 8,
                2 => 16,
                3 => 16,
                5 => 32,
                6 => 32 / 16,
                7 => 64 / 16,
                8 => 64 / 16,
                _ => throw new NotSupportedException()
            };
            int size = (pixelCount * pixelSize) / 8;

            binary.WriteUInt32(0x02020000);
            binary.WriteInt32(size);
            binary.WriteInt32(isIndexed ? 2 : 1);

            binary.WriteUInt32(0);
            binary.WriteUInt32(0x80);
            binary.WriteInt32(size);
            binary.WriteByte(type);
            binary.WriteByte(1);
            binary.WriteByte(2);
            binary.WriteByte(0);
            binary.WriteInt32(isIndexed ? 0xA9FF : 0xAAE4);
            binary.WriteInt16((short)width);
            binary.WriteInt16((short)height);
            binary.WriteUInt16(1);
            binary.WriteUInt16(0);
            binary.WriteUInt32(0);
            binary.WriteUInt32(0);

            stream.Write(new byte[0x50]);

        }
        private static async Task WriteGTF4444(Stream stream, Bitmap bitmap, bool zOrder)
        {
            using MemoryStream memStream = new MemoryStream();
            Binary binary = new Binary(memStream, true);
            int pixelCount = bitmap.Width * bitmap.Height;

            Order order = new Order(bitmap.Width, bitmap.Height);
            for (int n = 0; n < pixelCount; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                Color color = bitmap.GetPixel(x, y);
                binary.WriteUInt16(ColorHelp.To4444(color));
            }

            memStream.Position = 0;
            await memStream.CopyToAsync(stream);
        }

        private class Order
        {
            readonly int width;
            readonly int height;
            readonly uint xmax;
            readonly uint ymax;
            int x, y;
            readonly bool zOrder;
            public Order(int width, int height) :
                this(width, height, IsPow2(width) && IsPow2(height))
            { }
            public Order(int width, int height, bool isZOrder)
            {
                this.width = width;
                this.height = height;
                xmax = (uint)(width - 1);
                ymax = (uint)(height - 1);
                x = 0;
                y = 0;
                zOrder = isZOrder;
            }
            private static bool IsPow2(int n) => (n & (n - 1)) == 0;
            public (int, int) GetXY()
            {
                (int, int) result = (x, y);
                if (zOrder)
                {
                    uint xTrail1 = (uint)((x - width) ^ (x - width + 1));
                    uint yTrail1 = (uint)((y - height) ^ (y - height + 1));
                    x ^= (int)(xTrail1 & yTrail1 & xmax);
                    y ^= (int)((xTrail1 >> 1) & yTrail1 & ymax);
                }
                else
                {
                    x++;
                    if (x >= width)
                    {
                        x = 0;
                        y++;
                    }
                }
                return result;
            }
        }

        private static class ColorHelp
        {
            public static Color From565(ushort x)
            {
                int r0 = (x >> 11) * 8;
                int g0 = ((x >> 5) & 0x3F) * 4;
                int b0 = (x & 0x1F) * 8;
                return Color.FromArgb(r0, g0, b0);
            }
            public static Color From1555(ushort x)
            {
                int a0 = (x >> 15) * 255;
                int r0 = ((x >> 10) & 0x1F) * 8;
                int g0 = ((x >> 5) & 0x1F) * 8;
                int b0 = (x & 0x1F) * 8;
                return Color.FromArgb(a0, r0, g0, b0);
            }

            public static Color From4444(int x)
            {
                int b = (x & 15) * 17;
                x >>= 4;
                int g = (x & 15) * 17;
                x >>= 4;
                int r = (x & 15) * 17;
                x >>= 4;
                int a = (x & 15) * 17;
                return Color.FromArgb(a, r, g, b);
            }

            public static ushort To4444(Color color)
            {
                return (ushort)(
                    ((color.A >> 4) << 12)
                    + ((color.R >> 4) << 8)
                    + ((color.G >> 4) << 4)
                    + (color.B >> 4));
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

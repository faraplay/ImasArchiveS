using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Imas
{
    public class GTF : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        private readonly IntPtr bitmapPtr;
        public int Type { get; }

        private GTF(Bitmap bitmap, IntPtr bitmapPtr, int type)
        {
            Bitmap = bitmap;
            this.bitmapPtr = bitmapPtr;
            Type = type;
        }

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Bitmap?.Dispose();
            }
            Marshal.FreeHGlobal(bitmapPtr);
            disposed = true;
        }

        ~GTF()
        {
            Dispose(false);
        }

        #endregion IDisposable

        public static GTF ReadGTF(Stream stream)
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

        #region Read GTF

        private static GTF ReadGTFIndexed(Stream stream, int width, int height, Color[] palette)
        {
            int stride = (width + 3) & -4;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(stride * height);
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
            Bitmap bitmap = new Bitmap(width, height, stride, PixelFormat.Format8bppIndexed, bitmapPtr);

            ColorPalette colorPalette = bitmap.Palette;
            for (int i = 0; i < 0x100; i++)
                colorPalette.Entries[i] = palette[i];
            bitmap.Palette = colorPalette;

            return new GTF(bitmap, bitmapPtr, 1);
        }

        private static GTF ReadGTF1555(Stream stream, int width, int height)
        {
            int stride = (width + 1) & -2;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(2 * stride * height);
            short[] bitmapArray = new short[stride * height];

            Order order = new Order(width, height);

            for (int n = 0; n < width * height; n++)
            {
                ushort b = Binary.ReadUInt16(stream, true);
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = (short)b;
            }

            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 2 * stride, PixelFormat.Format16bppArgb1555, bitmapPtr);

            return new GTF(bitmap, bitmapPtr, 2);
        }

        private static GTF ReadGTF4444(Stream stream, int width, int height)
        {
            int stride = width;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
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

            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapPtr);

            return new GTF(bitmap, bitmapPtr, 3);
        }

        private static GTF ReadGTF8888(Stream stream, int width, int height)
        {
            int stride = width;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
            int[] bitmapArray = new int[stride * height];
            Order order = new Order(width, height);

            for (int n = 0; n < width * height; n++)
            {
                int b = Binary.ReadInt32(stream, true);
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = b;
            }

            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapPtr);

            return new GTF(bitmap, bitmapPtr, 5);
        }

        private static GTF ReadGTF565Block4Color(Stream stream, int width, int height)
        {
            int stride = width;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
            Binary binary = new Binary(stream, false);
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

            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapPtr);

            return new GTF(bitmap, bitmapPtr, 6);
        }

        private static GTF ReadGTF565Block8Alpha4Color(Stream stream, int width, int height)
        {
            int stride = width;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
            Binary binary = new Binary(stream, false);
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

            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapPtr);

            return new GTF(bitmap, bitmapPtr, 7);
        }

        private static GTF ReadGTF565Block8RelAlpha4Color(Stream stream, int width, int height)
        {
            int stride = width;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
            Binary binary = new Binary(stream, false);
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

            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapPtr);

            return new GTF(bitmap, bitmapPtr, 8);
        }

        #endregion Read GTF

        #region Write GTF

        public static async Task WriteGTF(Stream stream, Bitmap bitmap, int encodingType)
        {
            encodingType &= 15;
            WriteHeader(stream, encodingType, bitmap.Width, bitmap.Height);
            switch (encodingType)
            {
                case 1:
                    await WriteGTFIndexed(stream, bitmap);
                    break;

                case 2:
                case 3:
                case 5:
                case 6:
                case 7:
                case 8:
                    await WriteGTF32Bit(stream, bitmap, encodingType);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private static void WriteHeader(Stream stream, int type, int width, int height)
        {
            Binary binary = new Binary(stream, true);
            bool isIndexed = type == 1;
            bool isPow2 = IsPow2(width) && IsPow2(height);
            int pixelCount = width * height;
            int pixelSize = type switch
            {
                1 => 8,
                2 => 16,
                3 => 16,
                5 => 32,
                6 => 64 / 16,
                7 => 128 / 16,
                8 => 128 / 16,
                _ => throw new NotSupportedException()
            };
            int strideSize = type switch
            {
                1 => 1,
                2 => 2,
                3 => 2,
                5 => 4,
                6 => 8 / 4,
                7 => 16 / 4,
                8 => 16 / 4,
                _ => throw new NotSupportedException()
            };
            int size = (pixelCount * pixelSize) / 8;

            binary.WriteUInt32(0x02020000);
            binary.WriteInt32(size + (isIndexed ? 0x400 : 0));
            binary.WriteInt32(isIndexed ? 2 : 1);

            binary.WriteUInt32(0);
            binary.WriteInt32(0x80);
            binary.WriteInt32(size);
            binary.WriteByte((byte)(type ^ (isPow2 ? 0x80 : 0xA0)));
            binary.WriteByte(1);
            binary.WriteByte(2);
            binary.WriteByte(0);
            binary.WriteInt32(isIndexed ? 0xA9FF : 0xAAE4);
            binary.WriteInt16((short)width);
            binary.WriteInt16((short)height);
            binary.WriteUInt16(1);
            binary.WriteUInt16(0);
            binary.WriteInt32(isPow2 ? 0 : width * strideSize);
            binary.WriteUInt32(0);

            if (isIndexed)
            {
                binary.WriteUInt32(1);
                binary.WriteInt32(0x80 + size);
                binary.WriteInt32(0x400);
                binary.WriteByte(0x85);
                binary.WriteByte(1);
                binary.WriteByte(2);
                binary.WriteByte(0);
                binary.WriteInt32(0xAA6C);
                binary.WriteInt16(0x100);
                binary.WriteInt16(1);
                binary.WriteUInt16(1);
                binary.WriteUInt16(0);
                binary.WriteInt32(0);
                binary.WriteUInt32(0);
            }
            else
            {
                stream.Write(new byte[0x24]);
            }

            stream.Write(new byte[0x2C]);
        }

        private static async Task WriteGTFIndexed(Stream stream, Bitmap bitmap)
        {
            nQuant.WuQuantizer wuQuantizer = new nQuant.WuQuantizer();
            using Bitmap qBitmap = (Bitmap)wuQuantizer.QuantizeImage(bitmap);

            if (qBitmap.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new NotSupportedException("Wrong pixel format - expected 8bppIndexed");
            using MemoryStream memStream = new MemoryStream();
            Binary binary = new Binary(memStream, true);
            int pixelCount = qBitmap.Width * qBitmap.Height;

            BitmapData bitmapData = qBitmap.LockBits(
                new Rectangle(0, 0, qBitmap.Width, qBitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format8bppIndexed);
            IntPtr bitmapPtr = bitmapData.Scan0;
            int stride = bitmapData.Stride;
            byte[] bitmapArray = new byte[stride * qBitmap.Height];
            Marshal.Copy(bitmapPtr, bitmapArray, 0, stride * qBitmap.Height);
            qBitmap.UnlockBits(bitmapData);

            Order order = new Order(qBitmap.Width, qBitmap.Height);
            for (int n = 0; n < pixelCount; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                byte b = bitmapArray[y * stride + x];
                binary.WriteByte(b);
            }

            ColorPalette palette = qBitmap.Palette;
            for (int i = 0; i < 0x100; i++)
            {
                binary.WriteByte(palette.Entries[i].A);
                binary.WriteByte(palette.Entries[i].B);
                binary.WriteByte(palette.Entries[i].G);
                binary.WriteByte(palette.Entries[i].R);
            }

            memStream.Position = 0;
            await memStream.CopyToAsync(stream);
        }

        private static async Task WriteGTF32Bit(Stream stream, Bitmap bitmap, int type)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new NotSupportedException("Wrong pixel format - expected 32bppArgb");
            using MemoryStream memStream = new MemoryStream();

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            IntPtr bitmapPtr = bitmapData.Scan0;
            int stride = bitmapData.Stride / 4;
            int[] bitmapArray = new int[stride * bitmap.Height];
            Marshal.Copy(bitmapPtr, bitmapArray, 0, stride * bitmap.Height);
            bitmap.UnlockBits(bitmapData);

            Binary binary = new Binary(memStream, true);
            Order order = new Order(bitmap.Width, bitmap.Height);
            int pixelCount = bitmap.Width * bitmap.Height;
            switch (type)
            {
                case 2:
                    WriteGTFPixels1555(binary, bitmapArray, order, pixelCount, stride);
                    break;

                case 3:
                    WriteGTFPixels4444(binary, bitmapArray, order, pixelCount, stride);
                    break;

                case 5:
                    WriteGTFPixels8888(binary, bitmapArray, order, pixelCount, stride);
                    break;

                case 6:
                    WriteGTFPixels565Block4Color(stream, bitmapArray, bitmap.Width, bitmap.Height, stride);
                    break;

                case 7:
                    WriteGTFPixels565Block8Alpha4Color(stream, bitmapArray, bitmap.Width, bitmap.Height, stride);
                    break;

                case 8:
                    WriteGTFPixels565Block8RelAlpha4Color(stream, bitmapArray, bitmap.Width, bitmap.Height, stride);
                    break;
            }

            memStream.Position = 0;
            await memStream.CopyToAsync(stream);
        }

        #region Write Pixels

        private static void WriteGTFPixels1555(Binary binary, int[] bitmapArray, Order order, int pixelCount, int stride)
        {
            for (int n = 0; n < pixelCount; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                uint b = (uint)bitmapArray[y * stride + x];
                binary.WriteUInt16((ushort)(
                    ((b >> 16) & 0x8000) ^
                    ((b >> 9) & 0x7C00) ^
                    ((b >> 6) & 0x03E0) ^
                    ((b >> 3) & 0x001F)
                    ));
            }
        }

        private static void WriteGTFPixels4444(Binary binary, int[] bitmapArray, Order order, int pixelCount, int stride)
        {
            for (int n = 0; n < pixelCount; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                uint b = (uint)bitmapArray[y * stride + x]; // 0xabcdefgh
                b &= 0xF0F0F0F0;                            // 0xa0c0e0g0
                b >>= 4;                                    // 0x0a0c0e0g
                b = (b ^ (b >> 4)) & 0x00FF00FF;            // 0x00ac00eg
                b = (b ^ (b >> 8)) & 0x0000FFFF;            // 0x0000aceg
                binary.WriteUInt16((ushort)b);
            }
        }

        private static void WriteGTFPixels8888(Binary binary, int[] bitmapArray, Order order, int pixelCount, int stride)
        {
            for (int n = 0; n < pixelCount; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                uint b = (uint)bitmapArray[y * stride + x];
                binary.WriteUInt32(b);
            }
        }

        #endregion Write Pixels

        private static void WriteGTFPixels565Block4Color(Stream stream, int[] bitmapArray, int width, int height, int stride)
        {
            for (int y = 0; y < height / 4; y++)
            {
                for (int x = 0; x < width / 4; x++)
                {
                    Color[] colors = new Color[16];
                    for (int yy = 0; yy < 4; yy++)
                    {
                        for (int xx = 0; xx < 4; xx++)
                        {
                            colors[4 * yy + xx] = Color.FromArgb(bitmapArray[(4 * y + yy) * stride + 4 * x + xx]);
                        }
                    }
                    ushort b0, b1;
                    int[,] colorIndex;
                    (b0, b1, colorIndex) = ReduceColors4(colors);

                    Binary.WriteUInt16(stream, false, b0);
                    Binary.WriteUInt16(stream, false, b1);

                    for (int yy = 0; yy < 4; yy++)
                    {
                        int k = 0;
                        for (int xx = 0; xx < 4; xx++)
                        {
                            k ^= colorIndex[xx, yy] << (2 * xx);
                        }
                        Binary.WriteByte(stream, false, (byte)k);
                    }
                }
            }
        }

        private static void WriteGTFPixels565Block8Alpha4Color(Stream stream, int[] bitmapArray, int width, int height, int stride)
        {
            for (int y = 0; y < height / 4; y++)
            {
                for (int x = 0; x < width / 4; x++)
                {
                    Color[] colors = new Color[16];
                    for (int yy = 0; yy < 4; yy++)
                    {
                        for (int xx = 0; xx < 4; xx++)
                        {
                            colors[4 * yy + xx] = Color.FromArgb(bitmapArray[(4 * y + yy) * stride + 4 * x + xx]);
                        }
                    }

                    ulong alpha = 0;
                    for (int i = 0; i < 16; i++)
                    {
                        alpha ^= (ulong)(colors[i].A >> 4) << (4 * i);
                    }
                    Binary.WriteUInt64(stream, false, alpha);

                    ushort b0, b1;
                    int[,] colorIndex;
                    (b0, b1, colorIndex) = ReduceColors4(colors);

                    Binary.WriteUInt16(stream, false, b0);
                    Binary.WriteUInt16(stream, false, b1);

                    for (int yy = 0; yy < 4; yy++)
                    {
                        int k = 0;
                        for (int xx = 0; xx < 4; xx++)
                        {
                            k ^= colorIndex[xx, yy] << (2 * xx);
                        }
                        Binary.WriteByte(stream, false, (byte)k);
                    }
                }
            }
        }

        private static void WriteGTFPixels565Block8RelAlpha4Color(Stream stream, int[] bitmapArray, int width, int height, int stride)
        {
            for (int y = 0; y < height / 4; y++)
            {
                for (int x = 0; x < width / 4; x++)
                {
                    Color[] colors = new Color[16];
                    for (int yy = 0; yy < 4; yy++)
                    {
                        for (int xx = 0; xx < 4; xx++)
                        {
                            colors[4 * yy + xx] = Color.FromArgb(bitmapArray[(4 * y + yy) * stride + 4 * x + xx]);
                        }
                    }

                    ulong alpha = 0;
                    byte a0, a1;
                    int[] alphaIndex;
                    (a0, a1, alphaIndex) = ReduceAlphas8(colors);
                    alpha ^= ((ulong)a1 << 8) ^ a0;
                    for (int i = 0; i < 16; i++)
                    {
                        alpha ^= (ulong)(alphaIndex[i]) << (3 * i + 16);
                    }
                    Binary.WriteUInt64(stream, false, alpha);

                    ushort b0, b1;
                    int[,] colorIndex;
                    (b0, b1, colorIndex) = ReduceColors4(colors);

                    Binary.WriteUInt16(stream, false, b0);
                    Binary.WriteUInt16(stream, false, b1);

                    for (int yy = 0; yy < 4; yy++)
                    {
                        int k = 0;
                        for (int xx = 0; xx < 4; xx++)
                        {
                            k ^= colorIndex[xx, yy] << (2 * xx);
                        }
                        Binary.WriteByte(stream, false, (byte)k);
                    }
                }
            }
        }

        private static (byte, byte, int[]) ReduceAlphas8(Color[] colors)
        {
            byte min = 0xFF;
            byte max = 0;
            bool hasExtreme = false;
            for (int i = 0; i < 16; i++)
            {
                byte alpha = colors[i].A;
                if (alpha == 0 || alpha == 0xFF)
                {
                    hasExtreme = true;
                }
                else
                {
                    if (alpha < min)
                        min = alpha;
                    if (alpha > max)
                        max = alpha;
                }
            }

            int[] a = new int[8];
            if (hasExtreme)
            {
                if (min > max) // no non-extreme alpha values
                {
                    min = 0;
                    max = 1;
                }
                a[0] = min;
                a[1] = max;
                a[2] = (4 * min + 1 * max) / 5;
                a[3] = (3 * min + 2 * max) / 5;
                a[4] = (2 * min + 3 * max) / 5;
                a[5] = (1 * min + 4 * max) / 5;
                a[6] = 0;
                a[7] = 255;
            }
            else
            {
                a[0] = max;
                a[1] = min;
                a[2] = (6 * max + 1 * min) / 7;
                a[3] = (5 * max + 2 * min) / 7;
                a[4] = (4 * max + 3 * min) / 7;
                a[5] = (3 * max + 4 * min) / 7;
                a[6] = (2 * max + 5 * min) / 7;
                a[7] = (1 * max + 6 * min) / 7;
            }

            int[] alphaIndex = new int[16];
            for (int i = 0; i < 16; i++)
            {
                int mindist = 256;
                int minj = 0;
                for (int j = 0; j < 8; j++)
                {
                    int dist = Math.Abs(colors[i].A - a[j]);
                    if (dist < mindist)
                    {
                        mindist = dist;
                        minj = j;
                    }
                }
                alphaIndex[i] = minj;
            }

            return ((byte)a[0], (byte)a[1], alphaIndex);
        }

        private static (ushort, ushort, int[,]) ReduceColors4(Color[] colors)
        {
            int maxDist = 0;
            int maxi0 = 0, maxi1 = 0;
            for (int i1 = 0; i1 < 16; i1++)
            {
                for (int i2 = 0; i2 < i1; i2++)
                {
                    int dist =
                        (colors[i1].R - colors[i2].R) * (colors[i1].R - colors[i2].R) +
                        (colors[i1].G - colors[i2].G) * (colors[i1].G - colors[i2].G) +
                        (colors[i1].B - colors[i2].B) * (colors[i1].B - colors[i2].B);
                    if (dist >= maxDist)
                    {
                        maxDist = dist;
                        maxi0 = i1;
                        maxi1 = i2;
                    }
                }
            }
            ushort b0 = ColorHelp.To565(colors[maxi0]);
            ushort b1 = ColorHelp.To565(colors[maxi1]);
            ushort b0new, b1new;
            Color[] newColor = new Color[4];
            if (b0 < b1)
            {
                b0new = b1;
                b1new = b0;
            }
            else
            {
                b0new = b0;
                b1new = b1;
            }
            newColor[0] = ColorHelp.From565(b0new);
            newColor[1] = ColorHelp.From565(b1new);
            newColor[2] = ColorHelp.MixRatio(newColor[0], newColor[1], 2, 1);
            newColor[3] = ColorHelp.MixRatio(newColor[0], newColor[1], 1, 2);

            int[,] colorIndex = new int[4, 4];
            for (int i = 0; i < 16; i++)
            {
                int minDist = 3 * 256 * 256;
                int minj = 0;
                for (int j = 0; j < 4; j++)
                {
                    int dist =
                        (colors[i].R - newColor[j].R) * (colors[i].R - newColor[j].R) +
                        (colors[i].G - newColor[j].G) * (colors[i].G - newColor[j].G) +
                        (colors[i].B - newColor[j].B) * (colors[i].B - newColor[j].B);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minj = j;
                    }
                }
                colorIndex[i % 4, i / 4] = minj;
            }
            return (b0new, b1new, colorIndex);
        }

        #endregion Write GTF

        private static bool IsPow2(int n) => (n & (n - 1)) == 0;

        private class Order
        {
            private readonly int width;
            private readonly int height;
            private readonly uint xmax;
            private readonly uint ymax;
            private int x, y;
            private readonly bool zOrder;

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

            public static ushort To565(Color color)
            {
                return (ushort)(
                    ((color.R >> 3) << 11)
                    + ((color.G >> 2) << 5)
                    + (color.B >> 3));
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
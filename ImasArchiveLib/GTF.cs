using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Imas
{
    public partial class GTF : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        private readonly IntPtr pixelDataPtr;
        private readonly int[] pixelDataArray;
        public int Type { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }

        public IntPtr BitmapPtr => pixelDataPtr;

        public int[] BitmapArray => pixelDataArray;

        private GTF(Bitmap bitmap, IntPtr bitmapPtr, int[] bitmapArray, int type, int width, int height, int stride)
        {
            Bitmap = bitmap;
            this.pixelDataPtr = bitmapPtr;
            this.pixelDataArray = bitmapArray;
            Type = type;
            Width = width;
            Height = height;
            Stride = stride;
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
            Marshal.FreeHGlobal(pixelDataPtr);
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

            int stride = width;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
            int[] bitmapArray = new int[stride * height];

            switch (type & 15)
            {
                case 1:
                    ReadPixels(stream, width, height, stream => GetPixelIndexed(stream, palette), stride, bitmapArray);
                    break;
                case 2:
                    ReadPixels(stream, width, height, GetPixel1555, stride, bitmapArray);
                    break;
                case 3:
                    ReadPixels(stream, width, height, GetPixel4444, stride, bitmapArray);
                    break;
                case 5:
                case 0xE:
                    ReadPixels(stream, width, height, GetPixel8888, stride, bitmapArray);
                    break;
                case 6:
                    ReadBlocks(stream, width, height, GetBlockNoAlpha, stride, bitmapArray);
                    break;
                case 7:
                    ReadBlocks(stream, width, height, GetBlock4Alpha, stride, bitmapArray);
                    break;
                case 8:
                    ReadBlocks(stream, width, height, GetBlock8RelAlpha, stride, bitmapArray);
                    break;
                default:
                    throw new NotSupportedException();
            }

            Marshal.Copy(bitmapArray, 0, bitmapPtr, stride * height);
            Bitmap bitmap = new Bitmap(width, height, 4 * stride, PixelFormat.Format32bppArgb, bitmapPtr);
            return new GTF(bitmap, bitmapPtr, bitmapArray, type, width, height, stride);
        }

        #region Read GTF

        private static void ReadPixels(Stream stream, int width, int height, Func<Stream, uint> getPixel, int stride, int[] bitmapArray)
        {
            Order order = new Order(width, height);
            for (int n = 0; n < width * height; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                bitmapArray[y * stride + x] = (int)getPixel(stream);
            }
        }

        private static void ReadBlocks(Stream stream, int width, int height, Action<Binary, int[,], int[]> getBlock, int stride, int[] bitmapArray)
        {
            Binary binary = new Binary(stream, false);
            for (int y = 0; y < height / 4; y++)
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

        #endregion Read GTF

        #region Write GTF

        public static async Task WriteGTF(Stream outStream, Bitmap bitmap, int encodingType)
        {
            GTF gtf = CreateFromBitmap(bitmap, encodingType);
            using Stream gtfStream = gtf.OpenStream();
            await gtfStream.CopyToAsync(outStream);
        }

        public static GTF CreateFromBitmap(Bitmap bitmap, int encodingType)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = width;
            IntPtr bitmapPtr = Marshal.AllocHGlobal(4 * stride * height);
            int[] bitmapArray = new int[stride * height];
            GTF gtf = new GTF(bitmap, bitmapPtr, bitmapArray, encodingType, width, height, stride);
            gtf.LoadBitmapIntoArray(bitmap);
            return gtf;
        }

        private void LoadBitmapIntoArray(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            IntPtr bitmapPtr = bitmapData.Scan0;
            Marshal.Copy(bitmapPtr, pixelDataArray, 0, Stride * Height);
            Marshal.Copy(pixelDataArray, 0, pixelDataPtr, Stride * Height);
            bitmap.UnlockBits(bitmapData);
        }

        public Stream OpenStream()
        {
            MemoryStream memStream = new MemoryStream();
            WriteHeader(memStream);
            WriteBody(memStream);
            memStream.Position = 0;
            return memStream;
        }

        private void WriteHeader(Stream stream)
        {
            int shortType = Type & 15;
            Binary binary = new Binary(stream, true);
            bool isIndexed = shortType == 1;
            bool isPow2 = IsPow2(Width) && IsPow2(Height);
            int pixelCount = Width * Height;
            int pixelSize = shortType switch
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
            int strideSize = shortType switch
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
            binary.WriteByte((byte)(shortType ^ (isPow2 ? 0x80 : 0xA0)));
            binary.WriteByte(1);
            binary.WriteByte(2);
            binary.WriteByte(0);
            binary.WriteInt32(isIndexed ? 0xA9FF : 0xAAE4);
            binary.WriteInt16((short)Width);
            binary.WriteInt16((short)Height);
            binary.WriteUInt16(1);
            binary.WriteUInt16(0);
            binary.WriteInt32(isPow2 ? 0 : Width * strideSize);
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

        private void WriteBody(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            switch (Type & 15)
            {
                case 1:
                    WriteIndexedPixels(binary);
                    break;

                case 2:
                    WritePixels(binary, WritePixel1555);
                    break;

                case 3:
                    WritePixels(binary, WritePixel4444);
                    break;

                case 5:
                    WritePixels(binary, WritePixel8888);
                    break;

                case 6:
                    WriteBlocks(stream, WriteBlockNoAlpha);
                    break;

                case 7:
                    WriteBlocks(stream, WriteBlock4Alpha);
                    break;

                case 8:
                    WriteBlocks(stream, WriteBlock8RelAlpha);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void WriteIndexedPixels(Binary binary)
        {
            (byte[] indexedData, uint[] palette) = WuQuantizer.QuantizeImage(BitmapArray, 0x100);
            Order order = new Order(Width, Height);
            for (int n = 0; n < Width * Height; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                byte b = indexedData[y * Stride + x];
                binary.WriteByte(b);
            }
            for (int i = 0; i < 0x100; i++)
            {
                binary.WriteByte((byte)((palette[i] & 0xFF000000) >> 24));
                binary.WriteByte((byte)((palette[i] & 0x000000FF) >> 0));
                binary.WriteByte((byte)((palette[i] & 0x0000FF00) >> 8));
                binary.WriteByte((byte)((palette[i] & 0x00FF0000) >> 16));
            }
        }

        private void WritePixels(Binary binary, Action<Binary, uint> writePixel)
        {
            Order order = new Order(Width, Height);
            int pixelCount = Width * Height;
            for (int n = 0; n < pixelCount; n++)
            {
                int x, y;
                (x, y) = order.GetXY();
                uint b = (uint)pixelDataArray[y * Stride + x];
                if ((b & 0xFF000000) == 0)
                {
                    b = 0x00FFFFFF;
                }
                writePixel(binary, b);
            }
        }

        private static void WritePixel1555(Binary binary, uint b)
        {
            binary.WriteUInt16((ushort)(
                                ((b >> 16) & 0x8000) ^
                                ((b >> 9) & 0x7C00) ^
                                ((b >> 6) & 0x03E0) ^
                                ((b >> 3) & 0x001F)
                                ));
        }

        private static void WritePixel4444(Binary binary, uint b)
        {
            // 0xabcdefgh
            b &= 0xF0F0F0F0;                            // 0xa0c0e0g0
            b >>= 4;                                    // 0x0a0c0e0g
            b = (b ^ (b >> 4)) & 0x00FF00FF;            // 0x00ac00eg
            b = (b ^ (b >> 8)) & 0x0000FFFF;            // 0x0000aceg
            binary.WriteUInt16((ushort)b);
        }

        private static void WritePixel8888(Binary binary, uint b)
        {
            binary.WriteUInt32(b);
        }

        private void WriteBlocks(Stream stream, Action<Stream, Color[]> writeBlock)
        {
            for (int y = 0; y < Height / 4; y++)
            {
                for (int x = 0; x < Width / 4; x++)
                {
                    Color[] colors = new Color[16];
                    for (int yy = 0; yy < 4; yy++)
                    {
                        for (int xx = 0; xx < 4; xx++)
                        {
                            int b = pixelDataArray[(4 * y + yy) * Stride + 4 * x + xx];
                            if ((b & 0xFF000000) == 0)
                            {
                                b = 0x00FFFFFF;
                            }
                            colors[4 * yy + xx] = Color.FromArgb(b);
                        }
                    }
                    writeBlock(stream, colors);
                }
            }
        }

        private static void WriteBlockNoAlpha(Stream stream, Color[] colors)
        {
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

        private static void WriteBlock4Alpha(Stream stream, Color[] colors)
        {
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

        private static void WriteBlock8RelAlpha(Stream stream, Color[] colors)
        {
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
    }
}
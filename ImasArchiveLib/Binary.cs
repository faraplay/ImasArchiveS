using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    class Binary
    {
        readonly Stream stream;
        readonly bool isBigEndian;
        public Binary(Stream stream, bool isBigEndian)
        {
            this.stream = stream;
            this.isBigEndian = isBigEndian;
        }

        /// <summary>
        /// Returns a 32-bit unsigned integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static uint GetUInt(Stream stream, bool isBigEndian)
        {
            byte[] data = new byte[4];
            int n = stream.Read(data);
            if (n != 4)
            {
                throw new EndOfStreamException();
            }
            return isBigEndian ?
                ((uint)data[0] << 24) |
                ((uint)data[1] << 16) |
                ((uint)data[2] << 8) |
                ((uint)data[3]) :
                ((uint)data[3] << 24) |
                ((uint)data[2] << 16) |
                ((uint)data[1] << 8) |
                ((uint)data[0]);
        }
        public uint GetUInt() => GetUInt(stream, isBigEndian);
        /// <summary>
        /// Returns a 32-bit signed integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static int GetInt32(Stream stream, bool isBigEndian)
        {
            return (int)GetUInt(stream, isBigEndian);
        }
        public int GetInt32() => (int)GetUInt(stream, isBigEndian);
        /// <summary>
        /// Returns a 16-bit unsigned integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static ushort GetUShort(Stream stream, bool isBigEndian)
        {
            byte[] data = new byte[2];
            int n = stream.Read(data);
            if (n != 2)
            {
                throw new EndOfStreamException();
            }
            return isBigEndian ?
                (ushort)((data[0] << 8) | (data[1])):
                (ushort)((data[1] << 8) | (data[0]));
        }
        public ushort GetUShort() => GetUShort(stream, isBigEndian);
        /// <summary>
        /// Returns a 16-bit signed integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static short GetInt16(Stream stream, bool isBigEndian)
        {
            return (short)GetUShort(stream, isBigEndian);
        }
        public short GetInt16() => (short)GetUShort(stream, isBigEndian);


        /// <summary>
        /// Returns a byte from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Match other methods>")]
        public static byte GetByte(Stream stream, bool isBigEndian)
        {
            int n = stream.ReadByte();
            if (n == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte)n;
        }
        public byte GetByte() => GetByte(stream, isBigEndian);
        /// <summary>
        /// Writes a 32-bit unsigned integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutUInt(Stream stream, bool isBigEndian, uint x)
        {
            if (isBigEndian)
            {
                stream.WriteByte((byte)(x >> 24));
                x <<= 8;
                stream.WriteByte((byte)(x >> 24));
                x <<= 8;
                stream.WriteByte((byte)(x >> 24));
                x <<= 8;
                stream.WriteByte((byte)(x >> 24));
            }
            else
            {
                stream.WriteByte((byte)(x & 0xFF));
                x >>= 8;
                stream.WriteByte((byte)(x & 0xFF));
                x >>= 8;
                stream.WriteByte((byte)(x & 0xFF));
                x >>= 8;
                stream.WriteByte((byte)(x & 0xFF));
            }
        }
        public void PutUInt(uint x) => PutUInt(stream, isBigEndian, x);
        /// <summary>
        /// Writes a 32-bit signed integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutInt32(Stream stream, bool isBigEndian, int x)
        {
            PutUInt(stream, isBigEndian, (uint)x);
        }
        public void PutInt32(int x) => PutUInt(stream, isBigEndian, (uint)x);
        /// <summary>
        /// Writes a 16-bit unsigned integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutUShort(Stream stream, bool isBigEndian, ushort x)
        {
            if (isBigEndian)
            {
                stream.WriteByte((byte)(x >> 8));
                x <<= 8;
                stream.WriteByte((byte)(x >> 8));
            }
            else
            {
                stream.WriteByte((byte)(x & 0xFF));
                x >>= 8;
                stream.WriteByte((byte)(x & 0xFF));
            }
        }
        public void PutUShort(ushort x) => PutUShort(stream, isBigEndian, x);
        /// <summary>
        /// Writes a 16-bit signed integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutInt16(Stream stream, bool isBigEndian, short x)
        {
            PutUShort(stream, isBigEndian, (ushort)x);
        }
        public void PutInt16(short x) => PutUShort(stream, isBigEndian, (ushort)x);


        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Match other methods>")]
        public static void PutByte(Stream stream, bool isBigEndian, byte x)
        {
            stream.WriteByte(x);
        }
        public void PutByte(byte x) => PutByte(stream, isBigEndian, x);
    }
}

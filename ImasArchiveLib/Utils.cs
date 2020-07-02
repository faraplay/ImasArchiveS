using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    static class Utils
    {
        /// <summary>
        /// Returns a 32-bit unsigned integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static uint GetUInt(Stream stream)
        {
            byte[] data = new byte[4];
            int n = stream.Read(data);
            if (n != 4)
            {
                throw new EndOfStreamException();
            }
            return ((uint)data[0] << 24) |
                ((uint)data[1] << 16) |
                ((uint)data[2] << 8) |
                ((uint)data[3]);
        }
        /// <summary>
        /// Returns a 32-bit signed integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static int GetInt32(Stream stream)
        {
            return (int)GetUInt(stream);
        }
        /// <summary>
        /// Returns a 16-bit unsigned integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static ushort GetUShort(Stream stream)
        {
            byte[] data = new byte[2];
            int n = stream.Read(data);
            if (n != 2)
            {
                throw new EndOfStreamException();
            }
            return (ushort)((data[0] << 8) | (data[1]));
        }
        /// <summary>
        /// Returns a 16-bit signed integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static short GetInt16(Stream stream)
        {
            return (short)GetUShort(stream);
        }
        /// <summary>
        /// Returns a byte from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static byte GetByte(Stream stream)
        {
            int n = stream.ReadByte();
            if (n == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte)n;
        }
        /// <summary>
        /// Writes a 32-bit unsigned integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutUInt(Stream stream, uint x)
        {
            stream.WriteByte((byte)(x >> 24));
            x <<= 8;
            stream.WriteByte((byte)(x >> 24));
            x <<= 8;
            stream.WriteByte((byte)(x >> 24));
            x <<= 8;
            stream.WriteByte((byte)(x >> 24));
        }
        /// <summary>
        /// Writes a 32-bit signed integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutInt32(Stream stream, int x)
        {
            PutUInt(stream, (uint)x);
        }
        /// <summary>
        /// Writes a 16-bit unsigned integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutUShort(Stream stream, ushort x)
        {
            stream.WriteByte((byte)(x >> 8));
            x <<= 8;
            stream.WriteByte((byte)(x >> 8));
        }
        /// <summary>
        /// Writes a 16-bit signed integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutInt16(Stream stream, short x)
        {
            PutUShort(stream, (ushort)x);
        }
        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void PutByte(Stream stream, byte x)
        {
            stream.WriteByte(x);
        }
    }
}

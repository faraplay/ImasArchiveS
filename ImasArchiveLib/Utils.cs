using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveLib
{
    static class Utils
    {
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
        public static int GetInt32(Stream stream)
        {
            return (int)GetUInt(stream);
        }
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
        public static short GetInt16(Stream stream)
        {
            return (short)GetUShort(stream);
        }
        public static byte GetByte(Stream stream)
        {
            int n = stream.ReadByte();
            if (n == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte)n;
        }
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
        public static void PutInt32(Stream stream, int x)
        {
            PutUInt(stream, (uint)x);
        }
        public static void PutUShort(Stream stream, ushort x)
        {
            stream.WriteByte((byte)(x >> 8));
            x <<= 8;
            stream.WriteByte((byte)(x >> 8));
        }
        public static void PutInt16(Stream stream, short x)
        {
            PutUShort(stream, (ushort)x);
        }
        public static void PutByte(Stream stream, byte x)
        {
            stream.WriteByte(x);
        }
    }
}

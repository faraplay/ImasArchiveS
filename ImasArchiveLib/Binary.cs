using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas
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

        #region Read
        #region 64
        /// <summary>
        /// Returns a 64-bit unsigned integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static ulong ReadUInt64(Stream stream, bool isBigEndian)
        {
            byte[] data = new byte[8];
            int n = stream.Read(data);
            if (n != 8)
            {
                throw new EndOfStreamException();
            }
            return isBigEndian ?
                ((ulong)data[0] << 56) |
                ((ulong)data[1] << 48) |
                ((ulong)data[2] << 40) |
                ((ulong)data[3] << 32) |
                ((ulong)data[4] << 24) |
                ((ulong)data[5] << 16) |
                ((ulong)data[6] << 8) |
                ((ulong)data[7]) :
                ((ulong)data[7] << 56) |
                ((ulong)data[6] << 48) |
                ((ulong)data[5] << 40) |
                ((ulong)data[4] << 32) |
                ((ulong)data[3] << 24) |
                ((ulong)data[2] << 16) |
                ((ulong)data[1] << 8) |
                ((ulong)data[0]);
        }
        public ulong ReadUInt64() => ReadUInt64(stream, isBigEndian);
        /// <summary>
        /// Returns a 64-bit signed integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static long ReadInt64(Stream stream, bool isBigEndian)
        {
            return (long)ReadUInt64(stream, isBigEndian);
        }
        public long ReadInt64() => (long)ReadUInt64(stream, isBigEndian);
        #endregion
        #region 32
        /// <summary>
        /// Returns a 32-bit unsigned integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static uint ReadUInt32(Stream stream, bool isBigEndian)
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
        public uint ReadUInt32() => ReadUInt32(stream, isBigEndian);
        /// <summary>
        /// Returns a 32-bit signed integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static int ReadInt32(Stream stream, bool isBigEndian)
        {
            return (int)ReadUInt32(stream, isBigEndian);
        }
        public int ReadInt32() => (int)ReadUInt32(stream, isBigEndian);
        #endregion
        #region 16
        /// <summary>
        /// Returns a 16-bit unsigned integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static ushort ReadUInt16(Stream stream, bool isBigEndian)
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
        public ushort ReadUInt16() => ReadUInt16(stream, isBigEndian);
        /// <summary>
        /// Returns a 16-bit signed integer from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        public static short ReadInt16(Stream stream, bool isBigEndian)
        {
            return (short)ReadUInt16(stream, isBigEndian);
        }
        public short ReadInt16() => (short)ReadUInt16(stream, isBigEndian);
        #endregion
        #region 8
        /// <summary>
        /// Returns a byte from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="EndOfStreamException"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Match other methods>")]
        public static byte ReadByte(Stream stream, bool isBigEndian)
        {
            int n = stream.ReadByte();
            if (n == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte)n;
        }
        public byte ReadByte() => ReadByte(stream, isBigEndian);
        #endregion
        #endregion
        #region Write
        #region 64
        /// <summary>
        /// Writes a 64-bit unsigned integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void WriteUInt64(Stream stream, bool isBigEndian, ulong x)
        {
            if (isBigEndian)
            {
                stream.Write(new byte[] {
                (byte)((x >> 56) & 0xFF),
                (byte)((x >> 48) & 0xFF),
                (byte)((x >> 40) & 0xFF),
                (byte)((x >> 32) & 0xFF),
                (byte)((x >> 24) & 0xFF),
                (byte)((x >> 16) & 0xFF),
                (byte)((x >> 8) & 0xFF),
                (byte)(x & 0xFF),
                });
            }
            else
            {
                stream.Write(new byte[] {
                (byte)((x >> 0) & 0xFF),
                (byte)((x >> 8) & 0xFF),
                (byte)((x >> 16) & 0xFF),
                (byte)((x >> 24) & 0xFF),
                (byte)((x >> 32) & 0xFF),
                (byte)((x >> 40) & 0xFF),
                (byte)((x >> 48) & 0xFF),
                (byte)((x >> 56) & 0xFF),
                });
            }
        }
        public void WriteUInt64(uint x) => WriteUInt64(stream, isBigEndian, x);
        /// <summary>
        /// Writes a 64-bit signed integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void WriteInt64(Stream stream, bool isBigEndian, int x)
        {
            WriteUInt64(stream, isBigEndian, (uint)x);
        }
        public void WriteInt64(int x) => WriteUInt64(stream, isBigEndian, (uint)x);
        #endregion
        #region 32
        /// <summary>
        /// Writes a 32-bit unsigned integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void WriteUInt32(Stream stream, bool isBigEndian, uint x)
        {
            if (isBigEndian)
            {
                stream.Write(new byte[] {
                (byte)((x >> 24) & 0xFF),
                (byte)((x >> 16) & 0xFF),
                (byte)((x >> 8) & 0xFF),
                (byte)(x & 0xFF),
                });
            }
            else
            {
                stream.Write(new byte[] {
                (byte)((x >> 0) & 0xFF),
                (byte)((x >> 8) & 0xFF),
                (byte)((x >> 16) & 0xFF),
                (byte)((x >> 24) & 0xFF),
                });
            }
        }
        public void WriteUInt32(uint x) => WriteUInt32(stream, isBigEndian, x);
        /// <summary>
        /// Writes a 32-bit signed integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void WriteInt32(Stream stream, bool isBigEndian, int x)
        {
            WriteUInt32(stream, isBigEndian, (uint)x);
        }
        public void WriteInt32(int x) => WriteUInt32(stream, isBigEndian, (uint)x);
        #endregion
        #region 16
        /// <summary>
        /// Writes a 16-bit unsigned integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void WriteUInt16(Stream stream, bool isBigEndian, ushort x)
        {
            if (isBigEndian)
            {
                stream.Write(new byte[] {
                (byte)((x >> 8) & 0xFF),
                (byte)(x & 0xFF),
                });
            }
            else
            {
                stream.Write(new byte[] {
                (byte)((x >> 0) & 0xFF),
                (byte)((x >> 8) & 0xFF),
                });
            }
        }
        public void WriteUInt16(ushort x) => WriteUInt16(stream, isBigEndian, x);
        /// <summary>
        /// Writes a 16-bit signed integer to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void WriteInt16(Stream stream, bool isBigEndian, short x)
        {
            WriteUInt16(stream, isBigEndian, (ushort)x);
        }
        public void WriteInt16(short x) => WriteUInt16(stream, isBigEndian, (ushort)x);
        #endregion
        #region 8
        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="x"></param>
        /// <exception cref="IOException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="ObjectDisposedException"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Match other methods>")]
        public static void WriteByte(Stream stream, bool isBigEndian, byte x)
        {
            stream.WriteByte(x);
        }
        public void WriteByte(byte x) => WriteByte(stream, isBigEndian, x);
        #endregion
        #endregion
    }
}

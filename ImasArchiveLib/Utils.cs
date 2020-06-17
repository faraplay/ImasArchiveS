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
    }
}

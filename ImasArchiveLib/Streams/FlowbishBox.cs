using System;

namespace Imas.Streams
{
    internal class FlowbishBox
    {
        public const int keyUIntLength = 18;
        public UInt32[] PBox = new UInt32[keyUIntLength];
        public UInt32[,] SBox = new UInt32[4, 256];

        public FlowbishBox(in UInt32[] key)
        {
            Array.Copy(FlowbishData.PBox, PBox, 18);
            Array.Copy(FlowbishData.SBox, SBox, 4 * 256);

            for (int i = 0; i < 18; i++)
            {
                PBox[i] ^= Reverse(key[i]);
            }
            UInt32 datal = 0, datar = 0;
            for (int i = 0; i < 18; i += 2)
            {
                (datal, datar) = Encipher(datal, datar);
                PBox[i] = datal;
                PBox[i + 1] = datar;
            }
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 256; i += 2)
                {
                    (datal, datar) = Encipher(datal, datar);
                    SBox[j, i] = datal;
                    SBox[j, i + 1] = datar;
                }
            }
        }

        private UInt32 Reverse(UInt32 x) => (x >> 24) | ((x >> 8) & 0x0000FF00u) | ((x << 8) & 0x00FF0000u) | (x << 24);

        private UInt32 F(UInt32 x)
        {
            Byte a, b, c, d;
            d = (Byte)(x & 0xFF);
            x >>= 8;
            c = (Byte)(x & 0xFF);
            x >>= 8;
            b = (Byte)(x & 0xFF);
            x >>= 8;
            a = (Byte)(x & 0xFF);

            UInt32 y;
            y = SBox[0, d] + SBox[1, c];
            y ^= SBox[2, b];
            y += SBox[3, a];
            return y;
        }

        public (UInt32, UInt32) Encipher(UInt32 xl, UInt32 xr)
        {
            UInt32 temp;
            for (int i = 0; i < 16; i++)
            {
                xl ^= PBox[i];
                xr ^= F(xl);

                temp = xl;
                xl = xr;
                xr = temp;
            }
            temp = xl;
            xl = xr;
            xr = temp;

            xr ^= PBox[16];
            xl ^= PBox[17];
            return (xl, xr);
        }

        public (UInt32, UInt32) Decipher(UInt32 xl, UInt32 xr)
        {
            UInt32 temp;
            for (int i = 17; i > 1; i--)
            {
                xl ^= PBox[i];
                xr ^= F(xl);

                temp = xl;
                xl = xr;
                xr = temp;
            }
            temp = xl;
            xl = xr;
            xr = temp;

            xr ^= PBox[1];
            xl ^= PBox[0];
            return (xl, xr);
        }

        /// <summary>
        ///  Enciphers the buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="ArgumentException"/>
        public void Encipher(Span<byte> buffer)
        {
            if (buffer.Length % 8 != 0)
                throw new ArgumentException(Strings.Argument_LengthNotMultipleOf8);

            uint xl, xr;
            for (int i = 0; i < buffer.Length; i += 8)
            {
                xl = (uint)buffer[i] << 24 | (uint)buffer[i + 1] << 16
                    | (uint)buffer[i + 2] << 8 | (uint)buffer[i + 3];
                xr = (uint)buffer[i + 4] << 24 | (uint)buffer[i + 5] << 16
                    | (uint)buffer[i + 6] << 8 | (uint)buffer[i + 7];
                (xl, xr) = Encipher(xl, xr);
                buffer[i] = (byte)((xl & 0xFF000000u) >> 24);
                buffer[i + 1] = (byte)((xl & 0x00FF0000u) >> 16);
                buffer[i + 2] = (byte)((xl & 0x0000FF00u) >> 8);
                buffer[i + 3] = (byte)((xl & 0x000000FFu) >> 0);
                buffer[i + 4] = (byte)((xr & 0xFF000000u) >> 24);
                buffer[i + 5] = (byte)((xr & 0x00FF0000u) >> 16);
                buffer[i + 6] = (byte)((xr & 0x0000FF00u) >> 8);
                buffer[i + 7] = (byte)((xr & 0x000000FFu) >> 0);
            }
        }

        /// <summary>
        /// Deciphers the buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="ArgumentException"/>
        public void Decipher(Span<byte> buffer)
        {
            if (buffer.Length % 8 != 0)
                throw new ArgumentException(Strings.Argument_LengthNotMultipleOf8);

            uint xl, xr;
            for (int i = 0; i < buffer.Length; i += 8)
            {
                xl = (uint)buffer[i] << 24 | (uint)buffer[i + 1] << 16
                    | (uint)buffer[i + 2] << 8 | (uint)buffer[i + 3];
                xr = (uint)buffer[i + 4] << 24 | (uint)buffer[i + 5] << 16
                    | (uint)buffer[i + 6] << 8 | (uint)buffer[i + 7];
                (xl, xr) = Decipher(xl, xr);
                buffer[i] = (byte)((xl & 0xFF000000u) >> 24);
                buffer[i + 1] = (byte)((xl & 0x00FF0000u) >> 16);
                buffer[i + 2] = (byte)((xl & 0x0000FF00u) >> 8);
                buffer[i + 3] = (byte)((xl & 0x000000FFu) >> 0);
                buffer[i + 4] = (byte)((xr & 0xFF000000u) >> 24);
                buffer[i + 5] = (byte)((xr & 0x00FF0000u) >> 16);
                buffer[i + 6] = (byte)((xr & 0x0000FF00u) >> 8);
                buffer[i + 7] = (byte)((xr & 0x000000FFu) >> 0);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Imas.ImasEncoding
{
    internal static class Ascii
    {
        public static void GetBytes(string s, Span<byte> outBuffer)
        {
            int maxLen = outBuffer.Length - 1;
            int j = 0;
            for (int i = 0; i < s.Length && j < maxLen; i++)
            {
                char c = s[i];
                if (c < 0x7F)
                    outBuffer[j++] = (byte)c;
                else
                    throw new EncoderFallbackException();
            }
        }

        public static string GetString(ReadOnlySpan<byte> inSpan)
        {
            int maxLen = inSpan.Length - 1;
            List<char> chars = new List<char>();
            for (int j = 0; j < maxLen; j++)
            {
                byte code = inSpan[j];
                if (code == 0)
                    break;
                chars.Add((char)code);
            }
            return new string(chars.ToArray());
        }
    }
}
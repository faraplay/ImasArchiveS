using System;
using System.Collections.Generic;
using System.Text;

namespace Imas.ImasEncoding
{
    static class Custom
    {
        public static void FillBufferWithBytes(string s, Span<byte> outBuffer)
        {
            int maxLen = (outBuffer.Length & -2) - 2;
            int j = 0;
            NextByteOptions next = NextByteOptions.None;
            for (int i = 0; i < s.Length && j < maxLen; )
            {
                char c;
                do
                {
                    c = s[i++];
                } while (c == 0xD);
                switch (next)
                {
                    case NextByteOptions.AllASCII:
                        if (c > 0x20 && c < 0x7F)
                        {
                            outBuffer[j++] = (byte)(c + 0x80);
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            outBuffer[j++] = 0;
                        break;
                    case NextByteOptions.Lowercase:
                        if (c == 0x20 || c > 0x60 && c <= 0x7A)
                        {
                            outBuffer[j++] = (byte)(c + 0x80);
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            outBuffer[j++] = 0;
                        break;
                    case NextByteOptions.SpaceOnly:
                        if (c == 0x20)
                        {
                            outBuffer[j++] = (byte)(c + 0x80);
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            outBuffer[j++] = 0;
                        break;
                    case NextByteOptions.None:
                        break;
                }
                if (c == 0x20)
                {
                    outBuffer[j++] = (byte)(c + 0x80);
                    next = NextByteOptions.AllASCII;
                }
                else if (c > 0x60 && c <= 0x7A)
                {
                    outBuffer[j++] = (byte)(c + 0x80);
                    next = NextByteOptions.Lowercase;
                }
                else if (c >= 0x20 && c < 0x7F)
                {
                    outBuffer[j++] = (byte)(c + 0x80);
                    next = NextByteOptions.SpaceOnly;
                }
                else
                {
                    outBuffer[j++] = (byte)((c & 0xFF00) >> 8);
                    outBuffer[j++] = (byte)(c & 0xFF);
                    next = NextByteOptions.None;
                }

            }
            switch (next)
            {
                case NextByteOptions.AllASCII:
                case NextByteOptions.Lowercase:
                case NextByteOptions.SpaceOnly:
                    outBuffer[j++] = 0;
                    break;
                case NextByteOptions.None:
                    break;
            }
        }

        public static int[] GetValues(string s)
        {
            List<int> outList = new List<int>();
            int code = 0;
            NextByteOptions next = NextByteOptions.None;
            for (int i = 0; i < s.Length;)
            {
                char c;
                do
                {
                    c = s[i++];
                } while (c == 0xD);
                switch (next)
                {
                    case NextByteOptions.Backslash:
                        outList.Add(c);
                        next = NextByteOptions.None;
                        continue;
                    case NextByteOptions.AllASCII:
                        if (c > 0x20 && c < 0x7F && c != '\\')
                        {
                            code += c + 0x80;
                            outList.Add(code);
                            next = NextByteOptions.None;
                            continue;
                        }
                        else 
                            outList.Add(code);
                        break;
                    case NextByteOptions.Lowercase:
                        if (c == 0x20 || c > 0x60 && c <= 0x7A)
                        {
                            code += c + 0x80;
                            outList.Add(code);
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            outList.Add(code);
                        break;
                    case NextByteOptions.SpaceOnly:
                        if (c == 0x20)
                        {
                            code += c + 0x80;
                            outList.Add(code);
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            outList.Add(code);
                        break;
                    case NextByteOptions.None:
                        break;
                }
                if (c == 0x20)
                {
                    code = 0x100 * (c + 0x80);
                    next = NextByteOptions.AllASCII;
                }
                else if (c == '\\')
                {
                    next = NextByteOptions.Backslash;
                }
                else if (c > 0x60 && c <= 0x7A)
                {
                    code = 0x100 * (c + 0x80);
                    next = NextByteOptions.Lowercase;
                }
                else if (c >= 0x20 && c < 0x7F)
                {
                    code = 0x100 * (c + 0x80);
                    next = NextByteOptions.SpaceOnly;
                }
                else
                {
                    code = c;
                    outList.Add(code);
                    next = NextByteOptions.None;
                }
            }
            if (next != NextByteOptions.None && next != NextByteOptions.Backslash)
                outList.Add(code);
            return outList.ToArray();
        }

        public static byte[] GetBytes(string s)
        {
            int[] ints = GetValues(s);
            byte[] bytes = new byte[ints.Length * 2];
            for (int i = 0; i < ints.Length; i++)
            {
                bytes[2 * i] = (byte)((ints[i] >> 8) & 0xFF);
                bytes[2 * i + 1] = (byte)(ints[i] & 0xFF);
            }
            return bytes;
        }

        public static string GetString(ReadOnlySpan<byte> inSpan)
        {
            int maxLen = inSpan.Length & -2;
            List<char> chars = new List<char>();
            for (int j = 0; j < maxLen; j += 2)
            {
                int code = inSpan[j] * 256 + inSpan[j + 1];
                if (code == 0)
                    break;
                if (code >= 0xA000 && code < 0xFEFF)
                {
                    chars.Add((char)((code / 256) - 128));
                    if (code % 256 != 0)
                        chars.Add((char)(code % 128));
                }
                else
                    chars.Add((char)code);
            }
            return new string(chars.ToArray());
        }
        private enum NextByteOptions
        {
            None,
            SpaceOnly,
            Lowercase,
            AllASCII,
            Backslash
        }
    }
}

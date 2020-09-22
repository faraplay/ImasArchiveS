using System;
using System.Collections.Generic;
using System.Linq;

namespace Imas.ImasEncoding
{
    internal static class Custom
    {

        public static IEnumerable<ushort> GetValues(string s)
        {
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
                        yield return c;
                        next = NextByteOptions.None;
                        continue;
                    case NextByteOptions.AllASCII:
                        if (c > 0x20 && c < 0x7F && c != '\\')
                        {
                            code += c + 0x80;
                            yield return (ushort)code;
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            yield return (ushort)code;
                        break;

                    case NextByteOptions.Lowercase:
                        if (c == 0x20 || c > 0x60 && c <= 0x7A)
                        {
                            code += c + 0x80;
                            yield return (ushort)code;
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            yield return (ushort)code;
                        break;

                    case NextByteOptions.SpaceOnly:
                        if (c == 0x20)
                        {
                            code += c + 0x80;
                            yield return (ushort)code;
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            yield return (ushort)code;
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
                    yield return (ushort)code;
                    next = NextByteOptions.None;
                }
            }
            if (next != NextByteOptions.None && next != NextByteOptions.Backslash)
                yield return (ushort)code;
        }

        public static IEnumerable<byte> GetBytesEnumerable(string s)
        {
            foreach (int code in GetValues(s))
            {
                yield return (byte)((code >> 8) & 0xFF);
                yield return (byte)(code & 0xFF);
            }
        }

        public static byte[] GetBytes(string s) => GetBytesEnumerable(s).ToArray();
        public static void FillBufferWithBytes(string s, Span<byte> outBuffer)
        {
            int maxLen = (outBuffer.Length & -2) - 2;
            int j = 0;
            foreach (byte b in GetBytesEnumerable(s))
            {
                outBuffer[j] = b;
                if (++j >= maxLen)
                {
                    break;
                }
            }
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
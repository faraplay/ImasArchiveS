using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveLib
{
    class ParCommu
    {
        /// <summary>
        /// Attempts to get the offset of the _m.bin file in the .par.
        /// </summary>
        /// <param name="parStream"></param>
        /// <param name="parName"></param>
        /// <returns>The offset of the _m.bin, or -1 if unsuccessful.</returns>
        public static int TryGetMBin(Stream parStream, string parName)
        {
            try
            {
                parStream.Position = 0;
                if (Binary.GetUInt(parStream, true) != 0x50415202 ||
                    Binary.GetUInt(parStream, true) != 3)
                {
                    return -1;
                }

                int filecount = Binary.GetInt32(parStream, true);

                if (Binary.GetUInt(parStream, true) != 0x03000000)
                {
                    return -1;
                }


                List<uint> fileOffsets = new List<uint>();
                for (int i = 0; i < filecount; i++)
                {
                    fileOffsets.Add(Binary.GetUInt(parStream, true));
                }

                int blank = (-filecount) & 3;
                parStream.Position += 4 * blank;

                int strSize = 128;

                byte[] namebuf = new byte[strSize];
                List<string> filenames = new List<string>();
                for (int i = 0; i < filecount; i++)
                {
                    parStream.Read(namebuf);
                    string name = Encoding.ASCII.GetString(namebuf);
                    name = name.Substring(0, name.IndexOf('\0'));
                    filenames.Add(name);
                }

                List<uint> fileProperty = new List<uint>();
                for (int i = 0; i < filecount; i++)
                {
                    fileProperty.Add(Binary.GetUInt(parStream, true));
                }
                parStream.Position += 4 * blank;

                List<uint> fileSizes = new List<uint>();
                for (int i = 0; i < filecount; i++)
                {
                    fileSizes.Add(Binary.GetUInt(parStream, true));
                }
                parStream.Position += 4 * blank;

                string mbinName = parName[0..^4] + "_m.bin";

                int index = filenames.IndexOf(mbinName);
                if (index == -1)
                    return -1;

                //parStream.Position = fileOffsets[index];
                return (int)fileOffsets[index];
            }
            catch
            {
                return -1;
            }
        }
        public static void GetCommuText(Stream parStream, TextWriter textWriter)
        {
            try
            {
                Binary binary = new Binary(parStream, true);
                if (binary.GetUInt() != 0x004D0053 ||
                    binary.GetUInt() != 0x00470000)
                {
                    throw new InvalidDataException();
                }

                int msgCount = (int)binary.GetUInt();

                binary.GetUInt();

                byte[] namebuf = new byte[32];
                byte[] msgbuf = new byte[128];
                for (int i = 0; i < msgCount; i++)
                {
                    uint lineID = binary.GetUInt();
                    uint flags = binary.GetUInt();
                    if (binary.GetUInt() != 0 ||
                        binary.GetUInt() != 0 ||
                        binary.GetUInt() != 0 ||
                        binary.GetUInt() != 0)
                    {
                        throw new InvalidDataException();
                    }
                    uint nameLen = binary.GetUInt();
                    uint msgLen = binary.GetUInt();

                    parStream.Read(namebuf);
                    parStream.Read(msgbuf);

                    string name = Encoding.BigEndianUnicode.GetString(namebuf);
                    name = name.Substring(0, name.IndexOf('\0'));
                    string msg = Encoding.BigEndianUnicode.GetString(msgbuf);
                    msg = msg.Substring(0, msg.IndexOf('\0'));

                    textWriter.WriteLine(lineID);
                    textWriter.WriteLine(name);
                    textWriter.WriteLine(msg);
                    textWriter.WriteLine("$");
                }
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException();
            }

        }

        public static async Task ReplaceMBin(Stream refParStream, Stream outParStream, TextReader commuBin, string parName)
        {
            string commuParName = GetNonCommentLine(commuBin);
            if (commuParName == null || commuParName.Substring(commuParName.LastIndexOf('/') + 1) != parName)
                throw new InvalidDataException();

            int mbinPos = TryGetMBin(refParStream, parName);
            if (mbinPos == -1)
                throw new InvalidDataException();

            refParStream.Position = 0;
            await refParStream.CopyToAsync(outParStream);

            outParStream.Position = mbinPos + 8;
            int msgCount = Binary.GetInt32(outParStream, true);
            Binary.GetUInt(outParStream, true);

            for (int i = 0; i < msgCount; i++)
            {
                string line = GetNonCommentLine(commuBin);
                if (!int.TryParse(line.Trim(), out int lineID))
                {
                    throw new InvalidDataException();
                }
                int expectedMsgIndex = Binary.GetInt32(outParStream, true);
                if (lineID != expectedMsgIndex)
                {
                    throw new InvalidDataException();
                }
                outParStream.Position += 20;

                string name = GetNonCommentLine(commuBin);
                string msg = GetNonCommentLine(commuBin);
                if (name == null || msg == null || msg == "$")
                    throw new InvalidDataException();
                string msgline;
                while ((msgline = GetNonCommentLine(commuBin)) != "$")
                {
                    if (msgline == null)
                        throw new InvalidDataException();
                    msg += '\n' + msgline;
                }

                byte[] namebytes = ToCustomEncoding(name);
                byte[] msgbytes = ToCustomEncoding(msg);

                int nameLen = Math.Min(namebytes.Length, 30);
                int msgLen = Math.Min(msgbytes.Length, 126);

                Binary.PutInt32(outParStream, true, nameLen / 2);
                Binary.PutInt32(outParStream, true, msgLen / 2);

                outParStream.Write(namebytes, 0, nameLen);
                outParStream.Write(new byte[32 - nameLen]);
                outParStream.Write(msgbytes, 0, msgLen);
                outParStream.Write(new byte[128 - msgLen]);
            }
        }

        private static byte[] ToCustomEncoding(string s)
        {
            List<byte> list = new List<byte>();
            NextByteOptions next = NextByteOptions.None;
            foreach (char c in s)
            {
                switch (next) 
                {
                    case NextByteOptions.AllASCII:
                        if (c > 0x20 && c < 0x7F)
                        {
                            list.Add((byte)(c + 0x80));
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            list.Add(0);
                        break;
                    case NextByteOptions.Lowercase:
                        if (c == 0x20 || c > 0x60 && c <= 0x7A)
                        {
                            list.Add((byte)(c + 0x80));
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            list.Add(0);
                        break;
                    case NextByteOptions.SpaceOnly:
                        if (c == 0x20)
                        {
                            list.Add((byte)(c + 0x80));
                            next = NextByteOptions.None;
                            continue;
                        }
                        else
                            list.Add(0);
                        break;
                    case NextByteOptions.None:
                        break;
                }
                if (c == 0x20)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.AllASCII;
                }
                else if (c > 0x60 && c <= 0x7A)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.Lowercase;
                }
                else if (c >= 0x20  && c < 0x7F)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.SpaceOnly;
                }
                else
                {
                    list.Add((byte)((c & 0xFF00) >> 8));
                    list.Add((byte)(c & 0xFF));
                    next = NextByteOptions.None;
                }
            }
            switch (next)
            {
                case NextByteOptions.AllASCII:
                case NextByteOptions.Lowercase:
                case NextByteOptions.SpaceOnly:
                        list.Add(0);
                    break;
                case NextByteOptions.None:
                    break;
            }
            return list.ToArray();
        }

        private enum NextByteOptions
        {
            None,
            SpaceOnly,
            Lowercase,
            AllASCII
        }

        static string GetNonCommentLine(TextReader textReader)
        {
            string line;
            do
            {
                line = textReader.ReadLine();

            } while (line != null && line.StartsWith('#'));
            return line;
        }
    }
}

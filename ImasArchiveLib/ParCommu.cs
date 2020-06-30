using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveLib
{
    class ParCommu
    {
        public static int TryGetMBin(Stream parStream, string parName)
        {
            parStream.Position = 0;
            if (Utils.GetUInt(parStream) != 0x50415202 ||
                Utils.GetUInt(parStream) != 3)
            {
                return -1;
            }

            int filecount = (int)Utils.GetUInt(parStream);

            if (Utils.GetUInt(parStream) != 0x03000000)
            {
                return -1;
            }


            List<uint> fileOffsets = new List<uint>();
            for (int i = 0; i < filecount; i++)
            {
                fileOffsets.Add(Utils.GetUInt(parStream));
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
                fileProperty.Add(Utils.GetUInt(parStream));
            }
            parStream.Position += 4 * blank;

            List<uint> fileSizes = new List<uint>();
            for (int i = 0; i < filecount; i++)
            {
                fileSizes.Add(Utils.GetUInt(parStream));
            }
            parStream.Position += 4 * blank;

            string mbinName = parName[0..^4] + "_m.bin";

            int index = filenames.IndexOf(mbinName);
            if (index == -1)
                return -1;

            //parStream.Position = fileOffsets[index];
            return (int)fileOffsets[index];
        }
        public static void GetCommuText(Stream parStream, TextWriter textWriter)
        {
            if (Utils.GetUInt(parStream) != 0x004D0053 ||
                Utils.GetUInt(parStream) != 0x00470000)
            {
                throw new InvalidDataException();
            }

            int msgCount = (int)Utils.GetUInt(parStream);

            Utils.GetUInt(parStream);

            byte[] namebuf = new byte[32];
            byte[] msgbuf = new byte[128];
            for (int i = 0; i < msgCount; i++)
            {
                uint lineID = Utils.GetUInt(parStream);
                uint flags = Utils.GetUInt(parStream);
                if (Utils.GetUInt(parStream) != 0 ||
                    Utils.GetUInt(parStream) != 0 ||
                    Utils.GetUInt(parStream) != 0 ||
                    Utils.GetUInt(parStream) != 0)
                {
                    throw new InvalidDataException();
                }
                uint nameLen = Utils.GetUInt(parStream);
                uint msgLen = Utils.GetUInt(parStream);

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
            int msgCount = (int)Utils.GetUInt(outParStream);
            Utils.GetUInt(outParStream);

            for (int i = 0; i < msgCount; i++)
            {
                string line = GetNonCommentLine(commuBin);
                int lineID;
                if (!int.TryParse(line.Trim(), out lineID))
                {
                    throw new InvalidDataException();
                }
                int expectedMsgIndex = (int)Utils.GetUInt(outParStream);
                if (lineID != expectedMsgIndex)
                {
                    throw new InvalidDataException();
                }
                outParStream.Position += 20;

                string name = GetNonCommentLine(commuBin);
                string msg = GetNonCommentLine(commuBin);
                if (name == null || msg == null)
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

                Utils.PutUInt(outParStream, (uint)nameLen);
                Utils.PutUInt(outParStream, (uint)msgLen);

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
            int i = 0;
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
                if (c >= 0x7F)
                {
                    list.Add((byte)((c & 0xFF00) >> 8));
                    list.Add((byte)(c & 0xFF));
                    next = NextByteOptions.None;
                }
                else if (c == 0x20)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.AllASCII;
                }
                else if (c > 0x60 && c <= 0x7A)
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.Lowercase;
                }
                else
                {
                    list.Add((byte)(c + 0x80));
                    next = NextByteOptions.SpaceOnly;
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

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
                if (name.Length > 15)
                    name = name.Substring(0, 15);
                if (msg.Length > 63)
                    msg = msg.Substring(0, 63);

                Utils.PutUInt(outParStream, (uint)name.Length);
                Utils.PutUInt(outParStream, (uint)msg.Length);

                byte[] namebytes = new byte[32];
                Encoding.BigEndianUnicode.GetBytes(name).CopyTo(namebytes, 0);
                byte[] msgbytes = new byte[128];
                Encoding.BigEndianUnicode.GetBytes(msg).CopyTo(msgbytes, 0);

                outParStream.Write(namebytes);
                outParStream.Write(msgbytes);
            }
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

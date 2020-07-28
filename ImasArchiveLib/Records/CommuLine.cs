using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System.IO;

namespace Imas.Records
{
    class CommuLine : IRecordable
    {
        public string file;
        public int messageID;
        public byte flag1;
        public byte flag2;
        public string name_raw;
        public string message_raw;
        public string name;
        public string message;

        public void Deserialise(Stream inStream)
        {
            try
            {
                Binary binary = new Binary(inStream, true);
                messageID = binary.ReadInt32();
                flag1 = binary.ReadByte();
                flag2 = binary.ReadByte();
                if (binary.ReadUInt16() != 0 ||
                    binary.ReadUInt32() != 0 ||
                    binary.ReadUInt32() != 0 ||
                    binary.ReadUInt32() != 0 ||
                    binary.ReadUInt32() != 0)
                {
                    throw new InvalidDataException();
                }
                binary.ReadUInt32();
                binary.ReadUInt32();

                byte[] namebuf = new byte[32];
                byte[] msgbuf = new byte[128];
                inStream.Read(namebuf);
                inStream.Read(msgbuf);
                name_raw = ImasEncoding.Custom.GetString(namebuf);
                message_raw = ImasEncoding.Custom.GetString(msgbuf);
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException();
            }
        }

        public void Serialise(Stream outStream)
        {
            Binary binary = new Binary(outStream, true);
            binary.WriteInt32(messageID);
            binary.WriteByte(flag1);
            binary.WriteByte(flag2);
            binary.WriteUInt16(0);
            binary.WriteUInt32(0);
            binary.WriteUInt32(0);
            binary.WriteUInt32(0);
            binary.WriteUInt32(0);

            string outName = string.IsNullOrWhiteSpace(name) ? name_raw : name;
            string outMessage = string.IsNullOrWhiteSpace(message) ? message_raw : message;

            byte[] namebytes = new byte[32];
            byte[] msgbytes = new byte[128];
            ImasEncoding.Custom.GetBytes(outName, namebytes);
            ImasEncoding.Custom.GetBytes(outMessage, msgbytes);

            int nameLen = 0;
            while (nameLen < 32 / 2 && (namebytes[2 * nameLen] != 0 || namebytes[2 * nameLen + 1] != 0))
                nameLen++;
            int msgLen = 0;
            while (msgLen < 128 / 2 && (msgbytes[2 * msgLen] != 0 || msgbytes[2 * msgLen + 1] != 0))
                msgLen++;

            binary.WriteInt32(nameLen);
            binary.WriteInt32(msgLen);

            outStream.Write(namebytes);
            outStream.Write(msgbytes);
        }

        public void ReadRow(XlsxReader xlsx, Row row)
        {
            file = xlsx.GetString(row, "A");
            messageID = xlsx.GetInt(row, "B");
            flag1 = xlsx.GetBool(row, "C") ? (byte)1 : (byte)0;
            flag2 = xlsx.GetBool(row, "D") ? (byte)1 : (byte)0;
            name_raw = xlsx.GetString(row, "E");
            message_raw = xlsx.GetString(row, "F");
            name = xlsx.GetString(row, "G");
            string message1 = xlsx.GetString(row, "H");
            string message2 = xlsx.GetString(row, "J");
            message = message1;
            if (!string.IsNullOrWhiteSpace(message2))
                message += '\n' + message2;
        }

        public void WriteRow(XlsxWriter xlsx, Row row)
        {
            xlsx.AppendCell(row, "A", file);
            xlsx.AppendCell(row, "B", messageID);
            xlsx.AppendCell(row, "C", flag1 == 1);
            xlsx.AppendCell(row, "D", flag2 == 1);
            xlsx.AppendCell(row, "E", name_raw);
            xlsx.AppendCell(row, "F", message_raw);
        }

        public void WriteFirstRow(XlsxWriter xlsx, Row row)
        {
            xlsx.AppendCell(row, "A",  "File");
            xlsx.AppendCell(row, "B",  "Message ID");
            xlsx.AppendCell(row, "C",  "Flag 1");
            xlsx.AppendCell(row, "D",  "Flag 2");
            xlsx.AppendCell(row, "E",  "Name (raw)");
            xlsx.AppendCell(row, "F",  "Message (raw)");
            xlsx.AppendCell(row, "G",  "Name");
            xlsx.AppendCell(row, "H",  "Message Line 1");
            xlsx.AppendCell(row, "I",  "Width");
            xlsx.AppendCell(row, "J",  "Message Line 2");
            xlsx.AppendCell(row, "K",  "Width");
        }
    }
}

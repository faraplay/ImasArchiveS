using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System.IO;

namespace Imas
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

        public void Serialise(Stream outStream)
        {
            Binary binary = new Binary(outStream, true);
            binary.PutInt32(messageID);
            binary.PutByte(flag1);
            binary.PutByte(flag2);
            binary.PutUShort(0);
            binary.PutUInt(0);
            binary.PutUInt(0);
            binary.PutUInt(0);
            binary.PutUInt(0);

            string outName = string.IsNullOrWhiteSpace(name) ? name_raw : name;
            string outMessage = string.IsNullOrWhiteSpace(message) ? message_raw : message;

            byte[] namebytes = new byte[32];
            byte[] msgbytes = new byte[128];
            CustomEncoding.ToCustomEncoding(outName, namebytes);
            CustomEncoding.ToCustomEncoding(outMessage, msgbytes);

            int nameLen = 0;
            while (nameLen < 32 / 2 && (namebytes[2 * nameLen] != 0 || namebytes[2 * nameLen + 1] != 0))
                nameLen++;
            int msgLen = 0;
            while (msgLen < 128 / 2 && (msgbytes[2 * msgLen] != 0 || msgbytes[2 * msgLen + 1] != 0))
                msgLen++;

            binary.PutInt32(nameLen);
            binary.PutInt32(msgLen);

            outStream.Write(namebytes);
            outStream.Write(msgbytes);
        }

        public void ReadRow(Row row, XlsxReader xlsx)
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
    }
}

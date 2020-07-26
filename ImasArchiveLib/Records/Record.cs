using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.Records
{
    class Record : IRecordable
    {
        readonly List<object> list = new List<object>();
        readonly List<FormatElement> format;

        #region IRecordable
        public Record(string formatString)
        {
            format = new List<FormatElement>();
            int i = 0;
            while (i < formatString.Length)
            {
                switch (formatString[i++])
                {
                    case 'b':
                        format.Add(new FormatElement { type = FormatType.Byte });
                        break;
                    case 's':
                        format.Add(new FormatElement { type = FormatType.Short });
                        break;
                    case 'i':
                        format.Add(new FormatElement { type = FormatType.Int });
                        break;
                    case 'c':
                        int length = int.Parse(formatString.Substring(i, 3), System.Globalization.NumberStyles.HexNumber);
                        i += 3;
                        format.Add(new FormatElement { type = FormatType.CustomString, arrayLen = length });
                        break;
                    case 'a':
                        int asciiLength = int.Parse(formatString.Substring(i, 3), System.Globalization.NumberStyles.HexNumber);
                        i += 3;
                        format.Add(new FormatElement { type = FormatType.AsciiString, arrayLen = asciiLength });
                        break;
                    default:
                        throw new FormatException("Record format string is invalid.");
                }
            }
        }

        public void Deserialise(Stream inStream)
        {
            list.Clear();
            Binary binary = new Binary(inStream, true);
            foreach (FormatElement e in format)
            {
                switch (e.type)
                {
                    case FormatType.Byte:
                        list.Add(binary.GetByte());
                        break;
                    case FormatType.Short:
                        list.Add(binary.GetInt16());
                        break;
                    case FormatType.Int:
                        list.Add(binary.GetInt32());
                        break;
                    case FormatType.AsciiString:
                    case FormatType.CustomString:
                        byte[] array = new byte[e.arrayLen];
                        inStream.Read(array);
                        list.Add(array);
                        break;
                }
            }
        }

        public void ReadRow(XlsxReader xlsx, Row row)
        {
            list.Clear();
            RowReader r = new RowReader(xlsx, row);
            foreach (FormatElement e in format)
            {
                switch (e.type)
                {
                    case FormatType.Byte:
                        list.Add(r.ReadByte());
                        break;
                    case FormatType.Short:
                        list.Add(r.ReadShort());
                        break;
                    case FormatType.Int:
                        list.Add(r.ReadInt());
                        break;
                    case FormatType.AsciiString:
                        byte[] asciiArray = new byte[e.arrayLen];
                        ImasEncoding.Ascii.GetBytes(r.ReadString(), asciiArray);
                        list.Add(asciiArray);
                        break;
                    case FormatType.CustomString:
                        byte[] array = new byte[e.arrayLen];
                        ImasEncoding.Custom.GetBytes(r.ReadString(), array);
                        list.Add(array);
                        break;
                }
            }
        }

        public void Serialise(Stream outStream)
        {
            Binary binary = new Binary(outStream, true);
            int i = 0;
            foreach (FormatElement e in format)
            {
                switch (e.type)
                {
                    case FormatType.Byte:
                        binary.PutByte((byte)list[i]);
                        break;
                    case FormatType.Short:
                        binary.PutInt16((short)list[i]);
                        break;
                    case FormatType.Int:
                        binary.PutInt32((int)list[i]);
                        break;
                    case FormatType.AsciiString:
                    case FormatType.CustomString:
                        outStream.Write((byte[])list[i]);
                        break;
                }
                i++;
            }
        }

        public void WriteFirstRow(XlsxWriter xlsx, Row row)
        {
        }

        public void WriteRow(XlsxWriter xlsx, Row row)
        {
            RowWriter r = new RowWriter(xlsx, row);
            for (int i = 0; i < format.Count; i++)
            {
                switch (format[i].type)
                {
                    case FormatType.Byte:
                        r.Write((byte)list[i]);
                        break;
                    case FormatType.Short:
                        r.Write((short)list[i]);
                        break;
                    case FormatType.Int:
                        r.Write((int)list[i]);
                        break;
                    case FormatType.AsciiString:
                        r.Write(ImasEncoding.Ascii.GetString((byte[])list[i]));
                        break;
                    case FormatType.CustomString:
                        r.Write(ImasEncoding.Custom.GetString((byte[])list[i]));
                        break;
                }
            }
        }
        #endregion

        public static List<Record> GetRecords(Stream stream, string format)
        {
            List<Record> records = new List<Record>();
            while (stream.Position < stream.Length)
            {
                Record record = new Record(format);
                record.Deserialise(stream);
                records.Add(record);
            }
            return records;
        }


        class FormatElement
        {
            public FormatType type;
            public int arrayLen;
        }

        enum FormatType
        {
            Byte,
            Short,
            Int,
            AsciiString,
            CustomString
        }
    }
}

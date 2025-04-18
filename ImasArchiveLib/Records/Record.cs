﻿using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Imas.Records
{
    internal class Record : IRecordable
    {
        private readonly object[] list;
        private readonly string[] headers;
        private readonly List<FormatElement> format;
        private readonly int length;

        public object this[int i]
        {
            get => list[i];
            set => list[i] = value;
        }

        #region IRecordable

        public Record(string formatString) : this(formatString, null) { }
        public Record(string formatString, string[] headers)
        {
            format = new List<FormatElement>();
            int i = 0;
            while (i < formatString.Length)
            {
                switch (formatString[i++])
                {
                    case 'b':
                        format.Add(new FormatElement { type = FormatType.Byte, serialise = true });
                        break;

                    case 's':
                        format.Add(new FormatElement { type = FormatType.Short, serialise = true });
                        break;

                    case 'i':
                        format.Add(new FormatElement { type = FormatType.Int, serialise = true });
                        break;

                    case 'f':
                        format.Add(new FormatElement { type = FormatType.Float, serialise = true });
                        break;

                    case 'c':
                        int length = int.Parse(formatString.Substring(i, 3), System.Globalization.NumberStyles.HexNumber);
                        i += 3;
                        format.Add(new FormatElement { type = FormatType.CustomString, arrayLen = length, serialise = true });
                        break;

                    case 'a':
                        int asciiLength = int.Parse(formatString.Substring(i, 3), System.Globalization.NumberStyles.HexNumber);
                        i += 3;
                        format.Add(new FormatElement { type = FormatType.AsciiString, arrayLen = asciiLength, serialise = true });
                        break;

                    case 'B':
                        format.Add(new FormatElement { type = FormatType.Byte, serialise = false });
                        break;

                    case 'S':
                        format.Add(new FormatElement { type = FormatType.Short, serialise = false });
                        break;

                    case 'I':
                        format.Add(new FormatElement { type = FormatType.Int, serialise = false });
                        break;

                    case 'F':
                        format.Add(new FormatElement { type = FormatType.Float, serialise = false });
                        break;

                    case 'X':
                        format.Add(new FormatElement { type = FormatType.String, serialise = false });
                        break;

                    default:
                        throw new FormatException("Record format string is invalid.");
                }
            }
            length = format.Count;
            list = new object[length];
            if (headers == null)
            {
                this.headers = Enumerable.Repeat(string.Empty, length).ToArray();
            }
            else
            {
                if (headers.Length != length)
                {
                    throw new ArgumentException("Header array and format have differing lengths");
                }
                this.headers = headers;
            }
        }

        public void Deserialise(Stream inStream)
        {
            Binary binary = new Binary(inStream, true);
            for (int i = 0; i < length; i++)
            {
                if (format[i].serialise)
                {
                    switch (format[i].type)
                    {
                        case FormatType.Byte:
                            list[i] = binary.ReadByte();
                            break;

                        case FormatType.Short:
                            list[i] = binary.ReadInt16();
                            break;

                        case FormatType.Int:
                            list[i] = binary.ReadInt32();
                            break;

                        case FormatType.Float:
                            list[i] = binary.ReadFloat();
                            break;

                        case FormatType.AsciiString:
                        case FormatType.CustomString:
                            byte[] array = new byte[format[i].arrayLen];
                            inStream.Read(array);
                            list[i] = array;
                            break;
                    }
                }
            }
        }

        public void ReadRow(XlsxReader xlsx, Row row)
        {
            RowReader r = new RowReader(xlsx, row);
            for (int i = 0; i < length; i++)
            {
                switch (format[i].type)
                {
                    case FormatType.Byte:
                        list[i] = r.ReadByte();
                        break;

                    case FormatType.Short:
                        list[i] = r.ReadShort();
                        break;

                    case FormatType.Int:
                        list[i] = r.ReadInt();
                        break;

                    case FormatType.Float:
                        list[i] = r.ReadFloat();
                        break;

                    case FormatType.AsciiString:
                        byte[] asciiArray = new byte[format[i].arrayLen];
                        ImasEncoding.Ascii.GetBytes(r.ReadString(), asciiArray);
                        list[i] = asciiArray;
                        break;

                    case FormatType.CustomString:
                        byte[] array = new byte[format[i].arrayLen];
                        ImasEncoding.Custom.FillBufferWithBytes(r.ReadString(), array);
                        list[i] = array;
                        break;

                    case FormatType.String:
                        list[i] = r.ReadString();
                        break;
                }
            }
        }

        public void Serialise(Stream outStream)
        {
            Binary binary = new Binary(outStream, true);
            for (int i = 0; i < length; i++)
            {
                if (format[i].serialise)
                {
                    switch (format[i].type)
                    {
                        case FormatType.Byte:
                            binary.WriteByte((byte)list[i]);
                            break;

                        case FormatType.Short:
                            binary.WriteInt16((short)list[i]);
                            break;

                        case FormatType.Int:
                            binary.WriteInt32((int)list[i]);
                            break;

                        case FormatType.Float:
                            binary.WriteFloat((float)list[i]);
                            break;

                        case FormatType.AsciiString:
                        case FormatType.CustomString:
                            outStream.Write((byte[])list[i]);
                            break;
                    }
                }
            }
        }

        public void WriteFirstRow(XlsxWriter xlsx, Row row)
        {
            RowWriter r = new RowWriter(xlsx, row);
            foreach (string header in headers)
            {
                r.Write(header);
            }
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

                    case FormatType.Float:
                        r.Write((float)list[i]);
                        break;

                    case FormatType.AsciiString:
                        r.Write(ImasEncoding.Ascii.GetString((byte[])list[i]));
                        break;

                    case FormatType.CustomString:
                        r.Write(ImasEncoding.Custom.GetString((byte[])list[i]));
                        break;

                    case FormatType.String:
                        r.Write((string)list[i]);
                        break;
                }
            }
        }

        #endregion IRecordable

        public static List<Record> GetRecords(Stream stream, string format, string[] headers)
        {
            List<Record> records = new List<Record>();
            while (stream.Position < stream.Length)
            {
                Record record = new Record(format, headers);
                record.Deserialise(stream);
                records.Add(record);
            }
            return records;
        }

        public static void WriteRecords(Stream stream, IEnumerable<Record> records)
        {
            foreach (Record record in records)
            {
                record.Serialise(stream);
            }
        }

        private class FormatElement
        {
            public FormatType type;
            public int arrayLen;
            public bool serialise;
        }

        private enum FormatType
        {
            Byte,
            Short,
            Int,
            Float,
            AsciiString,
            CustomString,
            String
        }
    }
}
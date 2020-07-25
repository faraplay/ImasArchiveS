using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.Records
{
    class Accessory : IRecordable
    {
        int a1, a2, a3, a4;
        int a5;
        byte c1, c2, c3, c4;
        int a7, a8;
        readonly byte[] name = new byte[0x20];
        readonly byte[] desc = new byte[0x80];
        byte d1, d2, d3, d4;
        byte d5, d6, d7, d8;
        int b3, b4;

        public void Deserialise(Stream inStream)
        {
            Binary binary = new Binary(inStream, true);
            a1 = binary.GetInt32();
            a2 = binary.GetInt32();
            a3 = binary.GetInt32();
            a4 = binary.GetInt32();
            a5 = binary.GetInt32();
            c1 = binary.GetByte();
            c2 = binary.GetByte();
            c3 = binary.GetByte();
            c4 = binary.GetByte();
            a7 = binary.GetInt32();
            a8 = binary.GetInt32();
            inStream.Read(name);
            inStream.Read(desc);
            d1 = binary.GetByte();
            d2 = binary.GetByte();
            d3 = binary.GetByte();
            d4 = binary.GetByte();
            d5 = binary.GetByte();
            d6 = binary.GetByte();
            d7 = binary.GetByte();
            d8 = binary.GetByte();
            b3 = binary.GetInt32();
            b4 = binary.GetInt32();
        }

        public void Serialise(Stream outStream)
        {
            Binary binary = new Binary(outStream, true);
            binary.PutInt32(a1);
            binary.PutInt32(a2);
            binary.PutInt32(a3);
            binary.PutInt32(a4);
            binary.PutInt32(a5);
            binary.PutByte(c1);
            binary.PutByte(c2);
            binary.PutByte(c3);
            binary.PutByte(c4);
            binary.PutInt32(a7);
            binary.PutInt32(a8);
            outStream.Write(name);
            outStream.Write(desc);
            binary.PutByte(d1);
            binary.PutByte(d2);
            binary.PutByte(d3);
            binary.PutByte(d4);
            binary.PutByte(d5);
            binary.PutByte(d6);
            binary.PutByte(d7);
            binary.PutByte(d8);
            binary.PutInt32(b3);
            binary.PutInt32(b4);
        }

        public void ReadRow(XlsxReader xlsx, Row row)
        {
            RowReader r = new RowReader(xlsx, row);
            a1 = r.ReadInt();
            a2 = r.ReadInt();
            a3 = r.ReadInt();
            a4 = r.ReadInt();
            a5 = r.ReadInt();
            c1 = r.ReadByte();
            c2 = r.ReadByte();
            c3 = r.ReadByte();
            c4 = r.ReadByte();
            a7 = r.ReadInt();
            a8 = r.ReadInt();
            ImasEncoding.Custom.GetBytes(r.ReadString(), name);
            ImasEncoding.Custom.GetBytes(r.ReadString(), desc);
            d1 = r.ReadByte();
            d2 = r.ReadByte();
            d3 = r.ReadByte();
            d4 = r.ReadByte();
            d5 = r.ReadByte();
            d6 = r.ReadByte();
            d7 = r.ReadByte();
            d8 = r.ReadByte();
            b3 = r.ReadInt();
            b4 = r.ReadInt();
        }

        public void WriteRow(XlsxWriter xlsx, Row row)
        {
            RowWriter w = new RowWriter(xlsx, row);
            w.Write(a1);
            w.Write(a2);
            w.Write(a3);
            w.Write(a4);
            w.Write(a5);
            w.Write(c1);
            w.Write(c2);
            w.Write(c3);
            w.Write(c4);
            w.Write(a7);
            w.Write(a8);
            w.Write(name);
            w.Write(desc);
            w.Write(d1);
            w.Write(d2);
            w.Write(d3);
            w.Write(d4);
            w.Write(d5);
            w.Write(d6);
            w.Write(d7);
            w.Write(d8);
            w.Write(b3);
            w.Write(b4);
        }

        public void WriteFirstRow(XlsxWriter xlsx, Row row)
        {
            RowWriter w = new RowWriter(xlsx, row);
            w.Write(nameof(a1));
            w.Write(nameof(a2));
            w.Write(nameof(a3));
            w.Write(nameof(a4));
            w.Write(nameof(a5));
            w.Write(nameof(c1));
            w.Write(nameof(c2));
            w.Write(nameof(c3));
            w.Write(nameof(c4));
            w.Write(nameof(a7));
            w.Write(nameof(a8));
            w.Write(nameof(name));
            w.Write(nameof(desc));
            w.Write(nameof(d1));
            w.Write(nameof(d2));
            w.Write(nameof(d3));
            w.Write(nameof(d4));
            w.Write(nameof(d5));
            w.Write(nameof(d6));
            w.Write(nameof(d7));
            w.Write(nameof(d8));
            w.Write(nameof(b3));
            w.Write(nameof(b4));
        }
    }
}

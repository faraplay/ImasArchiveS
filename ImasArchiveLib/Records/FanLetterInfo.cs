using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Records
{
    internal static class FanLetterInfo
    {
        public static readonly string[] sheetNames = new string[]
        {
            "fanLetterInfo1",
            "fanLetterInfo2",
            "fanLetterInfo3",
            "fanLetterInfo4",
            "fanLetterInts",
            "fanLetterInfo6",
            "fanLetterStrings",
        };

        public static void ReadFile(Stream stream, XlsxWriter xlsx)
        {
            int count1 = Binary.ReadInt32(stream, true);
            const string format1 = "iiiss";
            Record[] records1 = new Record[count1];
            for (int i = 0; i < count1; i++)
            {
                records1[i] = new Record(format1);
                records1[i].Deserialise(stream);
            }
            xlsx.AppendRows("fanLetterInfo1", records1);

            int count2 = Binary.ReadInt32(stream, true);
            const string format2 = "iii";
            Record[] records2 = new Record[count2];
            for (int i = 0; i < count2; i++)
            {
                records2[i] = new Record(format2);
                records2[i].Deserialise(stream);
            }
            xlsx.AppendRows("fanLetterInfo2", records2);

            int count3 = Binary.ReadInt32(stream, true);
            const string format3 = "iii";
            Record[] records3 = new Record[count3];
            for (int i = 0; i < count3; i++)
            {
                records3[i] = new Record(format3);
                records3[i].Deserialise(stream);
            }
            xlsx.AppendRows("fanLetterInfo3", records3);

            int count4 = Binary.ReadInt32(stream, true);
            const string format4 = "ss";
            Record[] records4 = new Record[count4];
            for (int i = 0; i < count4; i++)
            {
                records4[i] = new Record(format4);
                records4[i].Deserialise(stream);
            }
            xlsx.AppendRows("fanLetterInfo4", records4);

            int lettCount = Binary.ReadInt32(stream, true);
            const string lettFormat = "iiiiiiII";
            Record[] letts = new Record[lettCount];
            for (int i = 0; i < lettCount; i++)
            {
                letts[i] = new Record(lettFormat);
                letts[i].Deserialise(stream);
            }

            const string format6 = "i";
            Record[] records6 = new Record[6];
            for (int i = 0; i < 6; i++)
            {
                records6[i] = new Record(format6);
                records6[i].Deserialise(stream);
            }
            xlsx.AppendRows("fanLetterInfo6", records6);

            int stringPos = (int)stream.Position;

            int id = 0;
            List<Record> lettStrings = new List<Record>();
            string[] stringSheetHeaders = { "ID", "Position", "Text" };
            do
            {
                Record record = new Record("IIX", stringSheetHeaders);
                record[0] = id;
                record[1] = (int)(stream.Position - stringPos) / 2;
                ushort c;
                StringBuilder stringBuilder = new StringBuilder();
                while ((c = Binary.ReadUInt16(stream, true)) != 0)
                {
                    stringBuilder.Append((char)c);
                }
                record[2] = stringBuilder.ToString();

                lettStrings.Add(record);
                id++;
            } while (stream.Position < stream.Length);

            foreach (Record record in letts)
            {
                record[6] = lettStrings.FindIndex(r => (int)r[1] == (int)record[4]);
                record[7] = lettStrings.FindIndex(r => (int)r[1] == (int)record[5]);
            }
            xlsx.AppendRows("fanLetterInts", letts);
            xlsx.AppendRows("fanLetterStrings", lettStrings);
        }

        public static void WriteFile(Stream stream, XlsxReader xlsx)
        {
            var stringList = xlsx.GetRows("IIX", "fanLetterStrings").ToList();
            List<byte> stringBuffer = new List<byte>();
            foreach (Record record in stringList)
            {
                record[1] = stringBuffer.Count / 2;
                foreach (int n in Imas.ImasEncoding.Custom.GetValues((string)record[2]))
                {
                    stringBuffer.Add((byte)((n >> 8) & 0xFF));
                    stringBuffer.Add((byte)(n & 0xFF));
                }
                stringBuffer.Add(0);
                stringBuffer.Add(0);
            }

            var records1 = xlsx.GetRows("iiiss", "fanLetterInfo1").ToList();
            Binary.WriteInt32(stream, true, records1.Count);
            foreach (Record record in records1)
            {
                record.Serialise(stream);
            }

            var records2 = xlsx.GetRows("iii", "fanLetterInfo2").ToList();
            Binary.WriteInt32(stream, true, records2.Count);
            foreach (Record record in records2)
            {
                record.Serialise(stream);
            }

            var records3 = xlsx.GetRows("iii", "fanLetterInfo3").ToList();
            Binary.WriteInt32(stream, true, records3.Count);
            foreach (Record record in records3)
            {
                record.Serialise(stream);
            }

            var records4 = xlsx.GetRows("ss", "fanLetterInfo4").ToList();
            Binary.WriteInt32(stream, true, records4.Count);
            foreach (Record record in records4)
            {
                record.Serialise(stream);
            }

            var letts = xlsx.GetRows("iiiiiiII", "fanLetterInts").ToList();
            Binary.WriteInt32(stream, true, letts.Count);
            foreach (Record record in letts)
            {
                record[4] = stringList[(int)record[6]][1];
                record[5] = stringList[(int)record[7]][1];
                record.Serialise(stream);
            }

            var records6 = xlsx.GetRows("i", "fanLetterInfo6").ToList();
            foreach (Record record in records6)
            {
                record.Serialise(stream);
            }

            stream.Write(stringBuffer.ToArray());
        }

    }
}

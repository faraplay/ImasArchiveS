using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Records
{
    static class JaJpText
    {
        public static void ReadFile(Stream stream, string xlsxName)
        {
            List<Record> list = new List<Record>();
            List<Record> stringList = new List<Record>();
            Binary binary = new Binary(stream, false);
            binary.ReadInt32();
            int stringOffset = binary.ReadInt32();
            int recordCount = binary.ReadInt32();
            int data2Offset = binary.ReadInt32();

            for (int i = 0; i < recordCount; i++)
            {
                Record record = new Record("IIIII");
                list.Add(record);
            }
            for (int i = 0; i < recordCount; i++)
            {
                list[i][0] = binary.ReadInt32();
                list[i][1] = binary.ReadInt32();
            }
            stream.Position = data2Offset;
            for (int i = 0; i < recordCount; i++)
            {
                list[i][2] = binary.ReadInt32();
                list[i][3] = binary.ReadInt32();
            }


            stream.Position = stringOffset;
            int id = 0;
            do
            {
                Record record = new Record("IIXI");
                record[0] = id;
                record[1] = (int)(stream.Position - stringOffset);
                ushort c;
                StringBuilder stringBuilder = new StringBuilder();
                while ((c = binary.ReadUInt16()) != 0)
                {
                    stringBuilder.Append((char)c);
                }
                record[2] = stringBuilder.ToString();
                record[3] = 0;

                stringList.Add(record);
                id++;
            } while (stream.Position < stream.Length);

            foreach (Record record in list)
            {
                record[4] = stringList.FindIndex(r => (int)r[1] == (int)record[1]);
            }
            using XlsxWriter xlsx = new XlsxWriter(xlsxName);
            xlsx.AppendRows("ints", list);
            xlsx.AppendRows("strings", stringList);
        }

        public static void WriteFile(Stream stream, string xlsxName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            List<Record> list = xlsx.GetRows("IIIII", "ints").ToList();
            List<Record> stringList = xlsx.GetRows("IIXI", "strings").ToList();
            Binary binary = new Binary(stream, false);
            binary.WriteInt32(0);
            int stringOffset = 16 + 16 * list.Count;
            binary.WriteInt32(stringOffset);
            int recordCount = list.Count;
            binary.WriteInt32(recordCount);
            int data2Offset = 16 + 8 * list.Count;
            binary.WriteInt32(data2Offset);

            for (int i = 0; i < recordCount; i++)
            {
                binary.WriteInt32((int)list[i][0]);
                binary.WriteInt32((int)stringList[(int)list[i][4]][1]);
            }
            for (int i = 0; i < recordCount; i++)
            {
                binary.WriteInt32((int)list[i][2]);
                binary.WriteInt32((int)list[i][3]);
            }

            for (int i = 0; i < stringList.Count; i++)
            {
                stringList[i][1] = (int)(stream.Position - stringOffset);
                string outString = (string)stringList[i][2];
                if (outString.Length == 0)
                {
                    binary.WriteUInt16(0);
                    continue;
                }
                if (outString[0] != (char)0xFEFF)
                {
                    binary.WriteUInt16(0xFEFF);
                }
                if ((int)stringList[i][3] == 1)
                {
                    foreach (char c in outString)
                    {
                        binary.WriteUInt16((ushort)c);
                    }
                }
                else
                {
                    foreach (int n in Imas.ImasEncoding.Custom.GetValues(outString))
                    {
                        binary.WriteUInt16((ushort)n);
                    }
                }
                binary.WriteUInt16(0);
            }

            stream.Position = 16;
            for (int i = 0; i < recordCount; i++)
            {
                binary.WriteInt32((int)list[i][0]);
                binary.WriteInt32((int)stringList[(int)list[i][4]][1]);
            }

        }
    }
}

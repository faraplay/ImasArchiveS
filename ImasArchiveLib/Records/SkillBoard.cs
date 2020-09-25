using Imas.Spreadsheet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Records
{
    internal static class SkillBoard
    {
        public static void ReadFile(Stream stream, XlsxWriter xlsx)
        {
            List<Record> list = new List<Record>();
            List<Record> stringList = new List<Record>();
            Binary binary = new Binary(stream, true);
            int recordCount = binary.ReadInt32();
            int strBufLen = binary.ReadInt32();

            byte[] strBuf = new byte[strBufLen * 2];
            stream.Read(strBuf);

            for (int j = 0; j < recordCount; j++)
            {
                Record record = new Record("iiiiiIII");
                record.Deserialise(stream);

                list.Add(record);
            }

            int stringIndex = 0;
            int i = 0;
            char getChar()
            {
                char cc = (char)(256 * strBuf[2 * i] + strBuf[2 * i + 1]);
                i++;
                return cc;
            }
            string[] stringSheetHeaders = { "Position", "ID", "Text" };
            while (i < strBufLen)
            {
                Record record = new Record("IIX", stringSheetHeaders);
                record[0] = i;
                record[1] = stringIndex++;
                StringBuilder sb = new StringBuilder();
                char c;
                while ((c = getChar()) != 0)
                {
                    sb.Append(c);
                }
                record[2] = sb.ToString();
                stringList.Add(record);
            }

            for (int j = 0; j < recordCount; j++)
            {
                Record strRecord;
                strRecord = stringList.Find(r => (int)r[0] == (int)list[j][2]);
                list[j][5] = strRecord[1];
                strRecord = stringList.Find(r => (int)r[0] == (int)list[j][3]);
                list[j][6] = strRecord[1];
                strRecord = stringList.Find(r => (int)r[0] == (int)list[j][4]);
                list[j][7] = strRecord[1];
            }

            xlsx.AppendRows("skillBoard", list);
            xlsx.AppendRows("skillBoardStrings", stringList);
        }

        public static void WriteFile(Stream stream, XlsxReader xlsx)
        {
            List<Record> list = xlsx.GetRows("iiiiiIII", "skillBoard").ToList();
            List<Record> stringList = xlsx.GetRows("IIX", "skillBoardStrings").ToList();
            Binary binary = new Binary(stream, true);

            List<byte> strBuf = new List<byte>();
            foreach (Record record in stringList)
            {
                record[0] = strBuf.Count / 2;
                var buf = ImasEncoding.Custom.GetBytes((string)record[2]);
                strBuf.AddRange(buf);
                strBuf.Add(0);
                strBuf.Add(0);
            }

            int recordCount = list.Count;
            binary.WriteInt32(recordCount);
            int strBufLen = strBuf.Count / 2;
            binary.WriteInt32(strBufLen);
            stream.Write(strBuf.ToArray());

            for (int i = 0; i < recordCount; i++)
            {
                list[i][2] = stringList[(int)list[i][5]][0];
                list[i][3] = stringList[(int)list[i][6]][0];
                list[i][4] = stringList[(int)list[i][7]][0];
                list[i].Serialise(stream);
            }
        }
    }
}
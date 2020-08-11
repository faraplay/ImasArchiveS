using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Records
{
    class SongInfo
    {
        static readonly string format = "iiiia020issiiiiiiiiiiiiiiic040c100c040c040iiiiiiiia020ii";
        public static IEnumerable<Record> ReadFile(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            if (binary.ReadUInt64() != 0x534F4E47494E464F) //"SONGINFO"
            {
                return Enumerable.Empty<Record>();
            }
            int recCount = binary.ReadInt32();

            Record[] records = new Record[recCount];
            for (int i = 0; i < recCount; i++)
            {
                records[i] = new Record(format);
                records[i].Deserialise(stream);
            }

            return records;
        }

        public static void WriteFile(Stream stream, XlsxReader xlsxReader)
        {
            IEnumerable<Record> records = xlsxReader.GetRows(format, "songInfo");
            int recCount = records.Count();

            Binary binary = new Binary(stream, true);
            binary.WriteUInt64(0x534F4E47494E464F);
            binary.WriteInt32(recCount);
            foreach (Record record in records)
            {
                record.Serialise(stream);
            }
        }
    }
}

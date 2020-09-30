using Imas.Spreadsheet;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Imas.Records
{
    internal class SongInfo
    {
        private static readonly string format = "iiiia020issiiiiiiiiiiiiiiic040c100c040c040iiiiiiiia020ii";
        private static readonly string[] headers =
        {
            "",
            "",
            "Order On Song Screen",
            "",
            "String ID",
            "BPM",
            "",
            "",
            "",
            "",
            "Dance",
            "Visual",
            "Vocal",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "Name",
            "Description",
            "Lyricist",
            "Composer",
            "", "", "", "", "", "",
            "", "", "", "", "",
        };

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
                records[i] = new Record(format, headers);
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
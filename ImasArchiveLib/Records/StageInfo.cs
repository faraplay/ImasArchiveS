using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Records
{
    internal static class StageInfo
    {
        public static readonly string parFilePath = "parameter/stage_info_par/";
        public static readonly string sheetName = "stageInfo";
        public static readonly string format = "Xiibbbbbbsa020a020a020a020a020c020c080iissbbs";
        public static readonly string[] headers = { 
            "Filename",
            "", "", "", "", "", "", "", "", "",
            "bg3d", "parName", "path", "gtfName", "gtfMName",
            "Name", "Description",
            "", "", "", "", "", "", ""
        };

        public static IEnumerable<Record> ReadFile(Stream stream, string fileName)
        {
            List<Record> records = Record.GetRecords(stream, format, headers);
            records[0][0] = fileName;
            return records;
        }

        public static void WriteFile(Stream stream, List<string> strings)
        {

        }
    }
}
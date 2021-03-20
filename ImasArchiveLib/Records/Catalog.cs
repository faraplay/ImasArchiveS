using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imas.Records
{
    internal static class Catalog
    {
        public static readonly string sheetName = "catalog";
        public static readonly string format = "Xbbbbsssa020a020c080c040c100";
        public static readonly string[] headers = {
            "Filename",
            "", "", "", "", "", "", "",
            "Code", "GTF Name",
            "Banner", "Name", "Description"
        };
        public static readonly string[] fileNames =
        {
            "catalog01.bin",
            "catalog02.bin",
            "catalog03.bin",
            "catalog04.bin",
            "catalog05.bin",
            "catalog06.bin",
            "catalog07.bin",
            "catalog08.bin",
            "catalog09.bin",
            "catalog10.bin",
            "catalog11.bin",
            "catalog12.bin",
            "catalog13.bin",
            "catalog14.bin",
        };

        public static IEnumerable<Record> ReadFile(Stream stream, string fileName)
        {
            List<Record> records = Record.GetRecords(stream, format, headers);
            foreach (Record record in records)
            {
                record[0] = fileName;
            }
            return records;
        }

        public static void WriteFile(Stream stream, List<string> strings)
        {

        }
    }
}

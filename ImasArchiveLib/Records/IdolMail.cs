using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Imas.Records
{
    class IdolMail
    {
        private static readonly string[] idols =
        {
            ""   , "har", "mik", "chi",
            "yay", "yuk", "mak", "mam",
            "tak", "hib", ""   , ""   ,
            ""   , "ior", "ami", "azu",
            "rit",
        };
        private static readonly string[] mailSheetNameArray = idols.Select(name => "mail_" + name).ToArray();
        public static readonly IEnumerable<string> allMailSheetNames = mailSheetNameArray.Where(name => name != "mail_");

        private static readonly string mailFormat = "ssic020c800";
        private static readonly string[] mailHeadings = new string[] { "Mail ID", "Image", "", "Subject", "Message" };
        public static void ReadFile(Stream mailStream, Stream infoStream, XlsxWriter xlsx)
        {
            if (allMailSheetNames.Any(sheetName => xlsx.HasWorksheet(sheetName)))
            {
                return;
            }
            var mailInfos = Record.GetRecords(infoStream, "ssss",
                new string[] { "Mail ID", "Order in Idol Folder", "Idol ID", "" })
                .ToDictionary(record => (short)record[0]);
            var mails = Record.GetRecords(mailStream, mailFormat, mailHeadings);
            foreach (var record in mails)
            {
                short mailID = (short)record[0];
                short idolID = (short)mailInfos[mailID][2];
                xlsx.AppendRow(mailSheetNameArray[idolID], record);
            }
        }

        public static void WriteFile(Stream stream, XlsxReader xlsx)
        {
            List<Record> mails = new List<Record>();
            foreach (string sheetName in allMailSheetNames)
            {
                mails.AddRange(xlsx.GetRows(mailFormat, sheetName));
            }
            mails.Sort((record1, record2) => ((short)record1[0]).CompareTo((short)record2[0]));
            Record.WriteRecords(stream, mails);
        }
    }
}

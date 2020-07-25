using Imas.Records;
using Imas.Spreadsheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class RecordXlsxTest
    {
        [DataTestMethod]
        [DataRow("disc/parameter/accessory.bin", "parameter/accessory.xlsx", "accessory", "iiiiibbbbiic020c080bbbbbbbbii")]
        [DataRow("disc/parameter/item.bin", "parameter/item.xlsx", "item", "isbbiic020c080c080ssssssiiiibbbb")]
        [DataRow("disc/parameter/profile.bin", "parameter/profile.xlsx", "profile", "sbbbbbbbbbbbbbba010c020c020c020c020c020c020c020c040c080c080")]
        [DataRow("disc/parameter/album.bin", "parameter/album.xlsx", "album", "ssbbsbbsssa010c020c020c020")]
        [DataRow("disc/parameter/season/seasonText.bin", "parameter/seasonText.xlsx", "seasonText", "bbsc040c020c020")]
        public void ReadRecordTest(string binName, string xlsxName, string sheetName, string format)
        {
            List<Record> list = new List<Record>();
            using (FileStream fileStream = new FileStream(binName, FileMode.Open, FileAccess.Read))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    Record record = new Record(format);
                    record.Deserialise(fileStream);
                    list.Add(record);
                }
            }
            using XlsxWriter xlsx = new XlsxWriter(xlsxName);
            xlsx.AppendRows(sheetName, list);
        }

        [DataTestMethod]
        [DataRow("parameter/accessory.xlsx", "accessory", "iiiiibbbbiic020c080bbbbbbbbii")]
        public void WriteRecordTest(string xlsxName, string sheetName, string format)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            var records = xlsx.GetRows(format, sheetName);
            using (FileStream fileStream = new FileStream("temp.bin", FileMode.Create, FileAccess.Write))
            {
                foreach (Record record in records)
                {
                    record.Serialise(fileStream);
                }
            }
            File.Delete("temp.bin");
        }
    }
}

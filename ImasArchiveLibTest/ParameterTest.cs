using Imas.Records;
using Imas.Spreadsheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class ParameterTest
    {
        [DataTestMethod]
        [DataRow("disc/parameter/profile.bin", "other/profile.xlsx")]
        public void ReadProfileTest(string profileFileName, string xlsxName)
        {
            List<Profile> list = new List<Profile>();
            using (FileStream fileStream = new FileStream(profileFileName, FileMode.Open, FileAccess.Read))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    Profile record = new Profile();
                    record.Deserialise(fileStream);
                    list.Add(record);
                }
            }
            using XlsxWriter xlsx = new XlsxWriter(xlsxName);
            xlsx.AppendRows("profile", list);
        }

        [DataTestMethod]
        [DataRow("other/profile.xlsx")]
        public void WriteProfileTest(string xlsxName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            var records = xlsx.GetRows<Profile>("profile");
            using (FileStream fileStream = new FileStream("profile.bin", FileMode.Create, FileAccess.Write))
            {
                foreach (Profile record in records)
                {
                    record.Serialise(fileStream);
                }
            }
            File.Delete("profile.bin");
        }
    }
}

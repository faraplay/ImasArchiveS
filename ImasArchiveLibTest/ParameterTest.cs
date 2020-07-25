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

        [DataTestMethod]
        [DataRow("disc/parameter/accessory.bin", "other/accessory.xlsx")]
        public void ReadAccessoryTest(string accessoryFileName, string xlsxName)
        {
            List<Accessory> list = new List<Accessory>();
            using (FileStream fileStream = new FileStream(accessoryFileName, FileMode.Open, FileAccess.Read))
            {
                while (fileStream.Position < fileStream.Length)
                {
                    Accessory record = new Accessory();
                    record.Deserialise(fileStream);
                    list.Add(record);
                }
            }
            using XlsxWriter xlsx = new XlsxWriter(xlsxName);
            xlsx.AppendRows("accessory", list);
        }

        [DataTestMethod]
        [DataRow("other/accessory.xlsx")]
        public void WriteAccessoryTest(string xlsxName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            var records = xlsx.GetRows<Accessory>("accessory");
            using (FileStream fileStream = new FileStream("accessory.bin", FileMode.Create, FileAccess.Write))
            {
                foreach (Accessory record in records)
                {
                    record.Serialise(fileStream);
                }
            }
            File.Delete("accessory.bin");
        }
    }
}

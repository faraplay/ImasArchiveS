using Imas.Records;
using Imas.Spreadsheet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class RecordXlsxTest
    {
        //[DataTestMethod]
        //[DataRow("disc/parameter/accessory.bin", "parameter/accessory.xlsx", "accessory", "iiiiibbbbiic020c080bbbbbbbbii")]
        //[DataRow("disc/parameter/item.bin", "parameter/item.xlsx", "item", "isbbiic020c080c080ssssssiiiibbbb")]
        //[DataRow("disc/parameter/profile.bin", "parameter/profile.xlsx", "profile", "sbbbbbbbbbbbbbba010c020c020c020c020c020c020c020c040c080c080")]
        //[DataRow("disc/parameter/album.bin", "parameter/album.xlsx", "album", "ssbbsbbsssa010c020c020c020")]
        //[DataRow("disc/parameter/season/seasonText.bin", "parameter/seasonText.xlsx", "seasonText", "bbsc040c020c020")]
        //public void ReadRecordTest(string binName, string xlsxName, string sheetName, string format)
        //{
        //    List<Record> list = new List<Record>();
        //    using (FileStream stream = new FileStream(binName, FileMode.Open, FileAccess.Read))
        //    {
        //        list = Record.GetRecords(stream, format);
        //    }
        //    using XlsxWriter xlsx = new XlsxWriter(xlsxName);
        //    xlsx.AppendRows(sheetName, list);
        //}

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

        [DataTestMethod]
        [DataRow("disc/text/im2nx_text.ja_jp", "other/text_ja_jp.xlsx")]
        //[DataRow("new.ja_jp", "text_ja_jp_reread.xlsx")]
        public void JaJpReadTest(string binName, string xlsxName)
        {
            using FileStream stream = new FileStream(binName, FileMode.Open, FileAccess.Read);
            using XlsxWriter xlsxWriter = new XlsxWriter(xlsxName, true);
            JaJpText.ReadFile(stream, xlsxWriter);
        }

        [DataTestMethod]
        [DataRow("other/text_ja_jp.xlsx", "other/new.ja_jp")]
        public void JaJpWriteTest(string xlsxName, string binName)
        {
            using FileStream stream = new FileStream(binName, FileMode.Create, FileAccess.Write);
            using XlsxReader xlsxReader = new XlsxReader(xlsxName);
            JaJpText.WriteFile(stream, xlsxReader);
        }

        [DataTestMethod]
        [DataRow("other/auditionDanText.pastbl", "other/audition_pastbl.xlsx")]
        public void PastblReadTest(string binName, string xlsxName)
        {
            using XlsxWriter xlsx = new XlsxWriter(xlsxName, true);
            using FileStream stream = new FileStream(binName, FileMode.Open, FileAccess.Read);
            IEnumerable<Record> records = Pastbl.ReadFile(stream);
            xlsx.AppendRows("pastbl", records);
        }

        [DataTestMethod]
        [DataRow("other/auditionDanText_out.pastbl", "other/parameter_hdd.xlsx", "alf/auditionText_par/auditionDanText.pastbl")]
        public void PastblWriteTest(string binName, string xlsxName, string alfFileName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            List<string> strings = xlsx.GetRows("XX", "pastbl")
                .Where(record => (string)record[0] == alfFileName)
                .Select(record => (string)record[1])
                .ToList();
            using FileStream stream = new FileStream(binName, FileMode.Create, FileAccess.Write);
            Pastbl.WriteFile(stream, strings);
        }

        [DataTestMethod]
        [DataRow("hdd/songinfo/songResource.bin", "other/songinfo.xlsx")]
        public void SongInfoReadTest(string binName, string xlsxName)
        {
            using XlsxWriter xlsx = new XlsxWriter(xlsxName, true);
            using FileStream stream = new FileStream(binName, FileMode.Open, FileAccess.Read);
            IEnumerable<Record> records = SongInfo.ReadFile(stream);
            xlsx.AppendRows("songInfo", records);
        }

        [DataTestMethod]
        [DataRow("hdd/ui/menu/skillBoard/skillBoard.info", "other/skillBoard.xlsx")]
        public void SkillBoardReadTest(string binName, string xlsxName)
        {
            using XlsxWriter xlsx = new XlsxWriter(xlsxName, true);
            using FileStream stream = new FileStream(binName, FileMode.Open, FileAccess.Read);
            SkillBoard.ReadFile(stream, xlsx);
        }

        [DataTestMethod]
        [DataRow("other/newSkillBoard.info", "other/skillBoard.xlsx")]
        public void SkillBoardWriteTest(string binName, string xlsxName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            using FileStream stream = new FileStream(binName, FileMode.Create, FileAccess.Write);
            SkillBoard.WriteFile(stream, xlsx);
        }

        [DataTestMethod]
        [DataRow("disc/parameter/fanLetterInfo.bin", "other/fanLetterInfo.xlsx")]
        public void FanLetterInfoReadTest(string binName, string xlsxName)
        {
            using XlsxWriter xlsx = new XlsxWriter(xlsxName, true);
            using FileStream stream = new FileStream(binName, FileMode.Open, FileAccess.Read);
            FanLetterInfo.ReadFile(stream, xlsx);
        }

        [DataTestMethod]
        [DataRow("other/newFanLetterInfo.bin", "other/fanLetterInfo.xlsx")]
        public void FanLetterInfoWriteTest(string binName, string xlsxName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            using FileStream stream = new FileStream(binName, FileMode.Create, FileAccess.Write);
            FanLetterInfo.WriteFile(stream, xlsx);
        }

        [DataTestMethod]
        [DataRow("parameter/_dlc01_mail_idol.bin", "parameter/_ps3_info_idol.bin", "parameter/mailIdol.xlsx")]
        public void IdolMailReadTest(string mailName, string infoName, string xlsxName)
        {
            using XlsxWriter xlsx = new XlsxWriter(xlsxName, true);
            using FileStream mailStream = new FileStream(mailName, FileMode.Open, FileAccess.Read);
            using FileStream infoStream = new FileStream(infoName, FileMode.Open, FileAccess.Read);
            IdolMail.ReadFile(mailStream, infoStream, xlsx);
        }

        [DataTestMethod]
        [DataRow("parameter/new_dlc01_mail_idol.bin", "parameter/mailIdol.xlsx")]
        public void IdolMailWriteTest(string mailName, string xlsxName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            using FileStream stream = new FileStream(mailName, FileMode.Create, FileAccess.Write);
            IdolMail.WriteFile(stream, xlsx);
        }
    }
}
using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Records;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public class PatchZipFile : ContainerFile<PatchZipEntry>
    {
        private readonly ZipArchive zipArchive;

        public PatchZipFile(string fileName, PatchZipMode mode)
        {
            switch (mode)
            {
                case PatchZipMode.Read:
                    _stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    zipArchive = new ZipArchive(_stream, ZipArchiveMode.Read);
                    break;

                case PatchZipMode.Create:
                    _stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
                    zipArchive = new ZipArchive(_stream, ZipArchiveMode.Update);
                    break;

                case PatchZipMode.Update:
                    _stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    zipArchive = new ZipArchive(_stream, ZipArchiveMode.Update);
                    break;
            }
            _entries = new List<PatchZipEntry>();
            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        public bool HasFile(string entryName) => _entries.Any(entry => entry.FileName == entryName);

        public async Task AddFile(Stream stream, string entryName)
        {
            if (!HasFile(entryName))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
                using Stream entryStream = entry.Open();
                await stream.CopyToAsync(entryStream);

                _entries.Add(new PatchZipEntry(entry));
            }
        }

        internal void AddCommu(string commuName, IEnumerable<CommuLine> lines)
        {
            if (!HasFile(commuName) && lines.Any(line => !string.IsNullOrWhiteSpace(line.message)))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry(commuName);
                using Stream stream = entry.Open();
                CommuFromXlsx.WriteBin(stream, lines);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        #region Add Files

        public void AddFile(string inputFilename, string entryName)
        {
            if (!HasFile(entryName))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntryFromFile(inputFilename, entryName);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        public async Task AddCommus(string xlsxName, IProgress<ProgressData> progress1 = null, IProgress<ProgressData> progress2 = null)
        {
            using CommuFromXlsx commuFromXlsx = new CommuFromXlsx(xlsxName);
            await commuFromXlsx.GetAndWriteAllCommus(this, progress1, progress2);
        }

        public async Task AddGtfs(string dirName, IProgress<ProgressData> progress = null)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dirName);
            HashSet<string> filenames =
                new HashSet<string>(dInfo.GetFiles("*.png")
                .Select(fInfo => fInfo.Name));

            string filenameXlsxName = dirName + "/filenames.xlsx";
            XlsxReader xlsx = new XlsxReader(filenameXlsxName);
            IEnumerable<Record> records = (await Task.Run(() => xlsx.GetRows("XXI", "filenames", progress)))
                .Where(record => filenames.Contains((string)record[1]));
            int total = records.Count();
            int count = 0;
            foreach (Record record in records)
            {
                string entryName = (string)record[0];
                string pngName = dInfo.FullName + '\\' + (string)record[1];
                count++;
                progress?.Report(new ProgressData { count = count, total = total, filename = entryName });
                if (!HasFile(entryName))
                {
                    ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
                    using Stream entryStream = entry.Open();
                    await GTF.WriteGTF(entryStream, new System.Drawing.Bitmap(pngName), (int)record[2]);
                    _entries.Add(new PatchZipEntry(entry));
                }
            }
        }

        public async Task AddLyrics(string dirName, IProgress<ProgressData> progress = null)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dirName);
            HashSet<string> filenames =
                new HashSet<string>(dInfo.GetFiles("*.xml")
                .Select(fInfo => fInfo.Name));

            string filenameXlsxName = dirName + "/filenames.xlsx";
            XlsxReader xlsx = new XlsxReader(filenameXlsxName);
            IEnumerable<Record> records = xlsx.GetRows("XX", "filenames")
                .Where(record => filenames.Contains((string)record[1]));
            int total = records.Count();
            int count = 0;
            foreach (Record record in records)
            {
                string entryName = (string)record[0];
                string xmlName = dInfo.FullName + '\\' + (string)record[1];
                count++;
                progress?.Report(new ProgressData { count = count, total = total, filename = entryName });
                if (!HasFile(entryName))
                {
                    Xmb xmb = new Xmb();
                    using (FileStream xmlStream = new FileStream(xmlName, FileMode.Open, FileAccess.Read))
                    {
                        xmb.ReadXml(xmlStream);
                    }
                    ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
                    using Stream entryStream = entry.Open();
                    await xmb.WriteXmb(entryStream);
                    _entries.Add(new PatchZipEntry(entry));
                }
            }
        }

        public void AddParameterFiles(string xlsxName, IProgress<string> progress = null)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            AddRecordFormatFiles(xlsx, progress);
            AddStageInfoFiles(xlsx, progress);
            AddFanLetter(xlsx, progress);
            AddPastblFiles(xlsx, progress);
            AddSongInfo(xlsx, progress);
            AddSkillBoard(xlsx, progress);
            AddJaJp(xlsx, progress);
            AddIdolMail(xlsx, progress);
        }

        private void AddIdolMail(XlsxReader xlsx, IProgress<string> progress)
        {
            const string idolMailName = "parameter/mail_idol_par/_dlc01_mail_idol.bin";
            if (!HasFile(idolMailName) &&
                IdolMail.allMailSheetNames.Any(sheetName =>
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == sheetName)))
            {
                progress?.Report(string.Format("Adding {0}", idolMailName));
                ZipArchiveEntry entry = zipArchive.CreateEntry(idolMailName);
                using Stream entryStream = entry.Open();
                IdolMail.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        private void AddJaJp(XlsxReader xlsx, IProgress<string> progress)
        {
            const string jaJpName = "text/im2nx_text.ja_jp";
            if (!HasFile(jaJpName) &&
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "jaJp") &&
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "jaJpStrings"))
            {
                progress?.Report(string.Format("Adding {0}", jaJpName));
                ZipArchiveEntry entry = zipArchive.CreateEntry(jaJpName);
                using Stream entryStream = entry.Open();
                JaJpText.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        private void AddSkillBoard(XlsxReader xlsx, IProgress<string> progress)
        {
            const string skillBoardName = "ui/menu/skillBoard/skillBoard.info";
            if (!HasFile(skillBoardName) &&
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "skillBoard") &&
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "skillBoardStrings"))
            {
                progress?.Report(string.Format("Adding {0}", skillBoardName));
                ZipArchiveEntry entry = zipArchive.CreateEntry(skillBoardName);
                using Stream entryStream = entry.Open();
                SkillBoard.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        private void AddSongInfo(XlsxReader xlsx, IProgress<string> progress)
        {
            const string songInfoName = "songinfo/songResource.bin";
            if (!HasFile(songInfoName) && xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "songInfo"))
            {
                progress?.Report(string.Format("Adding {0}", songInfoName));
                ZipArchiveEntry entry = zipArchive.CreateEntry(songInfoName);
                using Stream entryStream = entry.Open();
                SongInfo.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        private void AddPastblFiles(XlsxReader xlsx, IProgress<string> progress)
        {
            foreach (string fileName in Pastbl.fileNames)
            {
                if (!HasFile(fileName))
                {
                    progress?.Report(string.Format("Adding {0}", fileName));
                    List<string> strings = xlsx.GetRows("XX", "pastbl")
                        .Where(record => (string)record[0] == fileName)
                        .Select(record => (string)record[1])
                        .ToList();
                    if (strings.Any())
                    {
                        ZipArchiveEntry entry = zipArchive.CreateEntry(fileName);
                        using Stream entryStream = entry.Open();
                        Pastbl.WriteFile(entryStream, strings);
                        _entries.Add(new PatchZipEntry(entry));
                    }
                }
            }
        }

        private void AddFanLetter(XlsxReader xlsx, IProgress<string> progress)
        {
            const string fanLetterFilename = "parameter/fanLetterInfo.bin";
            if (!HasFile(fanLetterFilename) && FanLetterInfo.sheetNames.All(sheetName =>
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == sheetName)))
            {
                progress?.Report(string.Format("Adding {0}", fanLetterFilename));
                ZipArchiveEntry entry = zipArchive.CreateEntry(fanLetterFilename);
                using Stream entryStream = entry.Open();
                FanLetterInfo.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        private void AddRecordFormatFiles(XlsxReader xlsx, IProgress<string> progress)
        {
            foreach (RecordFormat format in RecordFormat.formats)
            {
                if (!HasFile(format.fileName))
                {
                    progress?.Report(string.Format("Adding {0}", format.fileName));
                    IEnumerable<Record> records = xlsx.GetRows(format.format, format.sheetName);
                    if (records.Any())
                    {
                        ZipArchiveEntry entry = zipArchive.CreateEntry(format.fileName);
                        using Stream entryStream = entry.Open();
                        Record.WriteRecords(entryStream, records);
                        _entries.Add(new PatchZipEntry(entry));
                    }
                }
            }
        }

        private void AddStageInfoFiles(XlsxReader xlsx, IProgress<string> progress)
        {
            progress?.Report(string.Format("Adding stageInfo"));
            IEnumerable<Record> records = xlsx.GetRows(StageInfo.format, StageInfo.sheetName);
            foreach (Record record in records)
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry(StageInfo.parFilePath + (string)record[0]);
                using Stream entryStream = entry.Open();
                record.Serialise(entryStream);
                _entries.Add(new PatchZipEntry(entry));
            }
        }

        #endregion Add Files

        #region IDisposable

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                zipArchive?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable
    }

    public enum PatchZipMode
    {
        Read,
        Create,
        Update
    }
}
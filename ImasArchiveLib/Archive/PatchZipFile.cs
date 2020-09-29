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

        public async Task AddFile(Stream stream, string entryName)
        {
            ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
            using Stream entryStream = entry.Open();
            await stream.CopyToAsync(entryStream);

            _entries.Add(new PatchZipEntry(entry));
        }

        internal void AddCommu(string commuName, IEnumerable<CommuLine> lines)
        {
            if (lines.Any(line => !string.IsNullOrWhiteSpace(line.message)))
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
            ZipArchiveEntry entry = zipArchive.CreateEntryFromFile(inputFilename, entryName);
            _entries.Add(new PatchZipEntry(entry));
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

                ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
                using Stream entryStream = entry.Open();
                await GTF.WriteGTF(entryStream, new System.Drawing.Bitmap(pngName), (int)record[2]);
                _entries.Add(new PatchZipEntry(entry));
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

        public void AddParameterFiles(string xlsxName)
        {
            using XlsxReader xlsx = new XlsxReader(xlsxName);
            foreach (RecordFormat format in RecordFormat.formats)
            {
                IEnumerable<Record> records = xlsx.GetRows(format.format, format.sheetName);
                if (records.Any())
                {
                    ZipArchiveEntry entry = zipArchive.CreateEntry(format.fileName);
                    using Stream entryStream = entry.Open();
                    Record.WriteRecords(entryStream, records);
                    _entries.Add(new PatchZipEntry(entry));
                }
            }
            if (FanLetterInfo.sheetNames.All(sheetName =>
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == sheetName)))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry("parameter/fanLetterInfo.bin");
                using Stream entryStream = entry.Open();
                FanLetterInfo.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
            foreach (string fileName in Pastbl.fileNames)
            {
                List<string> strings = xlsx.GetRows("XXX", "pastbl")
                    .Where(record => (string)record[0] == fileName)
                    .Select(record => (string)record[2])
                    .ToList();
                if (strings.Any())
                {
                    ZipArchiveEntry entry = zipArchive.CreateEntry(fileName);
                    using Stream entryStream = entry.Open();
                    Pastbl.WriteFile(entryStream, strings);
                    _entries.Add(new PatchZipEntry(entry));
                }
            }
            if (xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "songInfo"))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry("songinfo/songResource.bin");
                using Stream entryStream = entry.Open();
                SongInfo.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
            if (xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "skillBoard") &&
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "skillBoardStrings"))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry("ui/menu/skillBoard/skillBoard.info");
                using Stream entryStream = entry.Open();
                SkillBoard.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
            if (xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "jaJp") &&
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "jaJpStrings"))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry("text/im2nx_text.ja_jp");
                using Stream entryStream = entry.Open();
                JaJpText.WriteFile(entryStream, xlsx);
                _entries.Add(new PatchZipEntry(entry));
            }
            if (IdolMail.allMailSheetNames.Any(sheetName => 
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == sheetName)))
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry("parameter/mail_idol_par/_dlc01_mail_idol.bin");
                using Stream entryStream = entry.Open();
                IdolMail.WriteFile(entryStream, xlsx);
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
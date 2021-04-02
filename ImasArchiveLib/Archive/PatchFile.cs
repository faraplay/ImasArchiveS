using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Records;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public abstract class PatchFile
    {
        protected abstract IEnumerable<IPatchEntry> Entries { get; }
        public bool HasFile(string entryName) => Entries.Any(entry => entry.FileName == entryName);

        protected abstract void AddAndWriteToFile(string entryName, Action<Stream> writeAction);
        protected abstract Task AddAndWriteToFileAsync(string entryName, Func<Stream, Task> writeAction);
        public abstract void AddFile(string inputFilename, string entryName);

        #region Adding stuff


        public async Task AddFile(Stream stream, string entryName)
        {
            if (!HasFile(entryName))
            {
                await AddAndWriteToFileAsync(entryName, entryStream => stream.CopyToAsync(entryStream));
            }
        }
        internal void AddCommu(string commuName, IEnumerable<CommuLine> lines)
        {
            if (!HasFile(commuName) && lines.Any(line => !string.IsNullOrWhiteSpace(line.message)))
            {
                AddAndWriteToFile(commuName, stream => CommuFromXlsx.WriteBin(stream, lines));
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
            IEnumerable<Record> records = (await Task.Run(() => xlsx.GetRows("XXI", "filenames")))
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
                    await AddAndWriteToFileAsync(entryName, stream => GTF.WriteGTF(stream, new System.Drawing.Bitmap(pngName), (int)record[2]));
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
                    await AddAndWriteToFileAsync(entryName, stream => xmb.WriteXmb(stream));
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
            AddCatalogFiles(xlsx, progress);
        }

        private void AddIdolMail(XlsxReader xlsx, IProgress<string> progress)
        {
            const string idolMailName = "parameter/mail_idol_par/_dlc01_mail_idol.bin";
            if (!HasFile(idolMailName) &&
                IdolMail.allMailSheetNames.Any(sheetName =>
                xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == sheetName)))
            {
                progress?.Report(string.Format("Adding {0}", idolMailName));
                AddAndWriteToFile(idolMailName, stream => IdolMail.WriteFile(stream, xlsx));
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
                AddAndWriteToFile(jaJpName, stream => JaJpText.WriteFile(stream, xlsx));
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
                AddAndWriteToFile(skillBoardName, stream => SkillBoard.WriteFile(stream, xlsx));
            }
        }

        private void AddSongInfo(XlsxReader xlsx, IProgress<string> progress)
        {
            const string songInfoName = "songinfo/songResource.bin";
            if (!HasFile(songInfoName) && xlsx.Sheets.Descendants<Sheet>().Any(sheet => sheet.Name == "songInfo"))
            {
                progress?.Report(string.Format("Adding {0}", songInfoName));
                AddAndWriteToFile(songInfoName, stream => SongInfo.WriteFile(stream, xlsx));
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
                        AddAndWriteToFile(fileName, stream => Pastbl.WriteFile(stream, strings));
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
                AddAndWriteToFile(fanLetterFilename, stream => FanLetterInfo.WriteFile(stream, xlsx));
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
                        AddAndWriteToFile(format.fileName, stream => Record.WriteRecords(stream, records));
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
                AddAndWriteToFile(StageInfo.parFilePath + (string)record[0], stream => record.Serialise(stream));
            }
        }

        private void AddCatalogFiles(XlsxReader xlsx, IProgress<string> progress)
        {
            progress?.Report(string.Format("Adding catalogs"));
            IEnumerable<Record> records = xlsx.GetRows(Catalog.format, Catalog.sheetName);
            foreach (string filename in Catalog.fileNames)
            {
                var fileRecords = records.Where(record => (string)record[0] == filename).ToList();
                if (fileRecords.Any())
                {
                    AddAndWriteToFile(filename, stream =>
                    {
                        foreach (Record record in fileRecords)
                        {
                            record.Serialise(stream);
                        }
                    }
                    );
                }
            }
        }

        #endregion Adding stuff
    }
}

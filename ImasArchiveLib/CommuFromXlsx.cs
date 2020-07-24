using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Records;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Imas
{
    public class CommuFromXlsx : IDisposable
    {
        readonly XlsxReader xlsx;
        readonly ZipArchive zipArchive;

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                xlsx?.Dispose();
                zipArchive?.Dispose();
            }
            disposed = true;
        }
        #endregion

        public CommuFromXlsx(string xlsxName, string zipName)
        {
            xlsx = new XlsxReader(xlsxName);
            zipArchive = new ZipArchive(new FileStream(zipName, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Create);
        }
        #region Write Commus
        public async Task GetAndWriteAllCommus(IProgress<ProgressData> progress1 = null, IProgress<ProgressData> progress2 = null)
        {
            int total1 = xlsx.Sheets.Descendants<Sheet>().Count();
            int count1 = 0;
            foreach (Sheet sheet in xlsx.Sheets.Descendants<Sheet>())
            {
                count1++;
                progress1?.Report(new ProgressData { count = count1, total = total1, filename = sheet.Name });
                IEnumerable<CommuLine> lines = await Task.Run(() => xlsx.GetRows<CommuLine>(sheet, progress2));
                IEnumerable<string> commuFilenames = lines.Select(line => line.file).Distinct();
                int total2 = commuFilenames.Count();
                int count2 = 0;
                foreach (string filename in commuFilenames)
                {
                    count2++;
                    progress2?.Report(new ProgressData { count = count2, total = total2, filename = filename });
                    await Task.Run(() => WriteBin(filename, lines.Where(line => line.file == filename)));
                }
            }
        }
        private void WriteBin(string commuName, IEnumerable<CommuLine> lines)
        {
            if (lines.Any(line =>
                !string.IsNullOrWhiteSpace(line.name) || !string.IsNullOrWhiteSpace(line.message))
                )
            {
                ZipArchiveEntry entry = zipArchive.CreateEntry(commuName);
                using Stream stream = entry.Open();
                WriteBin(stream, lines);
            }
        }

        private void WriteBin(Stream stream, IEnumerable<CommuLine> lines)
        {
            Binary binary = new Binary(stream, true);
            binary.PutUInt(0x004D0053);
            binary.PutUInt(0x00470000);
            binary.PutInt32(lines.Count());
            binary.PutUInt(0);
            foreach (CommuLine line in lines)
            {
                line.Serialise(stream);
            }
        }
        #endregion
    }
}

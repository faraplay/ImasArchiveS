using DocumentFormat.OpenXml.Spreadsheet;
using Imas.Archive;
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
            }
            disposed = true;
        }
        #endregion

        public CommuFromXlsx(string xlsxName)
        {
            xlsx = new XlsxReader(xlsxName);
        }
        #region Write Commus
        public async Task GetAndWriteAllCommus(PatchZipFile patchZipFile, IProgress<ProgressData> progress1 = null, IProgress<ProgressData> progress2 = null)
        {
            var commuSheets = xlsx.Sheets.Descendants<Sheet>().Where(sheet => CommuLine.commuSheetNames.Contains(sheet.Name));
            int total1 = commuSheets.Count();
            int count1 = 0;
            foreach (Sheet sheet in commuSheets)
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
                    await Task.Run(() => patchZipFile.AddCommu(filename, lines.Where(line => line.file == filename)));
                }
            }
        }

        internal static void WriteBin(Stream stream, IEnumerable<CommuLine> lines)
        {
            Binary binary = new Binary(stream, true);
            binary.WriteUInt32(0x004D0053);
            binary.WriteUInt32(0x00470000);
            binary.WriteInt32(lines.Count());
            binary.WriteUInt32(0);
            foreach (CommuLine line in lines)
            {
                line.Serialise(stream);
            }
        }
        #endregion
    }
}

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
    public class PatchZipContainer : ContainerFile<PatchZipEntry>
    {
        private readonly ZipArchive zipArchive;
        public ZipArchive ZipArchive => zipArchive;
        public new List<PatchZipEntry> Entries => _entries;
        public PatchZipContainer(string fileName, PatchZipMode mode)
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

    public class PatchZipFile : PatchFile, IDisposable
    {
        public readonly PatchZipContainer patchZipContainer;

        public PatchZipFile(string fileName, PatchZipMode mode)
        {
            patchZipContainer = new PatchZipContainer(fileName, mode);
        }

        protected override IEnumerable<IPatchEntry> Entries => patchZipContainer.Entries;
        protected override void AddAndWriteToFile(string entryName, Action<Stream> writeAction)
        {
            ZipArchiveEntry entry = patchZipContainer.ZipArchive.CreateEntry(entryName);
            using Stream stream = entry.Open();
            writeAction(stream);
            patchZipContainer.Entries.Add(new PatchZipEntry(entry));
        }
        protected override async Task AddAndWriteToFileAsync(string entryName, Func<Stream, Task> writeAction)
        {
            ZipArchiveEntry entry = patchZipContainer.ZipArchive.CreateEntry(entryName);
            using Stream stream = entry.Open();
            await writeAction(stream);
            patchZipContainer.Entries.Add(new PatchZipEntry(entry));
        }
        public override void AddFile(string inputFilename, string entryName)
        {
            if (!HasFile(entryName))
            {
                ZipArchiveEntry entry = patchZipContainer.ZipArchive.CreateEntryFromFile(inputFilename, entryName);
                patchZipContainer.Entries.Add(new PatchZipEntry(entry));
            }
        }


        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                patchZipContainer?.Dispose();
            }
            disposed = true;
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
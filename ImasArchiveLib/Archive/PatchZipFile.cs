using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imas.Archive
{
    class PatchZipFile : ContainerFile<PatchZipEntry>
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
        #endregion
    }

    public enum PatchZipMode
    {
        Read,
        Create,
        Update
    }
}

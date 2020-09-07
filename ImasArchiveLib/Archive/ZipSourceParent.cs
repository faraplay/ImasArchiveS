using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Imas.Archive
{
    public class ZipSourceParent : IDisposable
    {
        private readonly ZipArchive zipArchive;
        internal IReadOnlyCollection<string> Filenames { get; }

        public ZipSourceParent(string zipFileName)
        {
            zipArchive = new ZipArchive(new FileStream(zipFileName, FileMode.Open, FileAccess.Read), ZipArchiveMode.Read);
            Filenames = zipArchive.Entries.Select(entry => '/' + entry.FullName).ToList();
        }

        internal Stream GetFile(string filename) => zipArchive.GetEntry(filename[1..]).Open();

        public ZipSource GetZipSource() => GetZipSource("");

        public ZipSource GetZipSource(string dirName) => new ZipSource(this, dirName);

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
                zipArchive?.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable
    }

    public class ZipSource : IFileSource
    {
        private readonly string myDir;
        private IReadOnlyCollection<string> Filenames { get; }
        private readonly ZipSourceParent parent;

        internal ZipSource(ZipSourceParent parent, string dirName)
        {
            this.parent = parent;
            myDir = dirName;
            Filenames = parent.Filenames.Where(s => s.StartsWith(dirName)).ToList();
        }

        public bool FileExists(string fileName)
        {
            return Filenames.Contains(myDir + '/' + fileName);
        }

        public Stream OpenFile(string fileName)
        {
            return parent.GetFile(myDir + '/' + fileName);
        }

        public bool DirectoryExists(string dirName)
        {
            return Filenames.Any(s => s.StartsWith(myDir + '/' + dirName + '/'));
        }

        public IFileSource OpenDirectory(string dirName)
        {
            return parent.GetZipSource(myDir + '/' + dirName);
        }
    }
}
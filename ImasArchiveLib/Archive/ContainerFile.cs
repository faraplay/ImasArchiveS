using Imas.Streams;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public abstract class ContainerFile<T> : IDisposable where T : ContainerEntry 
    {
        protected Stream _stream;
        protected List<T> _entries;

        #region Properties
        public ReadOnlyCollection<T> Entries
        {
            get => new ReadOnlyCollection<T>(_entries);
        }
        #endregion

        public Task ForAll(Action<ContainerEntry, string> action) => ForAll(action, "");

        private async Task ForAll(Action<ContainerEntry, string> action, string prefix)
        {
            foreach (ContainerEntry entry in Entries)
            {
                if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                {
                    string dirName = entry.FileName[0..^4] + '_' + entry.FileName[^3..];
                    using Stream stream = await entry.GetData();
                    using ParFile parFile = new ParFile(stream);
                    await parFile.ForAll(action, prefix + dirName + '/');
                }
                else
                {
                    action(entry, prefix + entry.FileName);
                }
            }
        }

        public Task ForAllTask(Func<ContainerEntry, string, Task> action) => ForAllTask(action, "");
        private async Task ForAllTask(Func<ContainerEntry, string, Task> action, string prefix)
        {
            foreach (ContainerEntry entry in Entries)
            {
                if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                {
                    string dirName = entry.FileName[0..^4] + '_' + entry.FileName[^3..];
                    using Stream stream = await entry.GetData();
                    using ParFile parFile = new ParFile(stream);
                    await parFile.ForAllTask(action, prefix + dirName + '/');
                }
                else
                {
                    await action(entry, prefix + entry.FileName);
                }
            }
        }

        internal Substream GetSubstream(long offset, long length)
        {
            return new Substream(_stream, offset, length);
        }

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
                _stream?.Dispose();
            }
            disposed = true;
        }
        #endregion
    }

    public abstract class ContainerEntry : IDisposable
    {
        protected MemoryStream _newData;
        protected readonly long _originalLength;
        protected readonly long _originalOffset;


        public string FileName { get; }
        internal bool UsesMemoryStream => _newData != null;
        internal long Offset { get; set; }
        public long Length { get => UsesMemoryStream ? _newData.Length : _originalLength; }

        #region Constructors
        protected ContainerEntry(string fileName, long originalLength, long originalOffset)
        {
            FileName = fileName;
            _originalLength = originalLength;
            _originalOffset = originalOffset;
            Offset = originalOffset;
        }
        #endregion

        public abstract Task<Stream> GetData();
        public abstract Task SetData(Stream stream);

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
                _newData?.Dispose();
            }
            disposed = true;
        }
        #endregion
    }
}

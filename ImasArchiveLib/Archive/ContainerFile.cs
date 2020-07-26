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

        public T GetEntry(string fileName)
        {
            return _entries.Find(e => e.FileName == fileName);
        }

        /// <summary>
        /// Recursively searches the ContainerFile for the filename string.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>An EntryStack containing the entry with that filename, or null if no entry is found.</returns>
        public async Task<EntryStack> GetEntryRecursive(string fileName)
        {
            return await GetEntryRecursive(fileName, new EntryStack());
        }
        private async Task<EntryStack> GetEntryRecursive(string fileName, EntryStack stack)
        {
            int parIndex = fileName.IndexOf("_par/");
            int ptaIndex = fileName.IndexOf("_pta/");
            int index;
            if (parIndex == -1)
                index = ptaIndex;
            else if (ptaIndex == -1)
                index = parIndex;
            else
                index = Math.Min(parIndex, ptaIndex);

            if (index == -1)
            {
                ContainerEntry entry = GetEntry(fileName);
                if (entry == null)
                {
                    stack.Dispose();
                    return null;
                }
                stack.Entry = entry;
                return stack;
            }
            else
            {
                ContainerEntry entry = GetEntry(fileName.Substring(0, index) + '.' + fileName.Substring(index + 1, 3));
                if (entry == null)
                {
                    stack.Dispose();
                    return null;
                }
                Stream stream = await entry.GetData();
                stack.Push(stream);
                ParFile parFile = new ParFile(stream);
                stack.Push(parFile);
                return await parFile.GetEntryRecursive(fileName.Substring(index + 5));
            }
        }

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

    /// <summary>
    /// Stack of parFiles and streams.
    /// To access the data in an entry in a nested entry, the parFiles in 
    /// the chain leading up to the entry
    /// need to all be loaded. This class holds all the parFiles in this
    /// chain and will automatically dispose them when the stack is disposed.
    /// </summary>
    public class EntryStack : IDisposable
    {
        private readonly Stack<ParFile> pars = new Stack<ParFile>();
        private readonly Stack<Stream> streams = new Stack<Stream>();

        public ContainerEntry Entry { get; set; }

        internal void Push(ParFile par) => pars.Push(par);
        internal void Push(Stream stream) => streams.Push(stream);

        public ParFile Peek() => pars.Peek();

        private void Clear()
        {
            foreach (ParFile par in pars)
            {
                par?.Dispose();
            }
            pars.Clear();
            foreach (Stream stream in streams)
            {
                stream?.Dispose();
            }
            streams.Clear();
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
                Entry?.Dispose();
                Clear();
            }
            disposed = true;
        }
        #endregion

    }
}

using Imas.Streams;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public interface IContainerFile
    {
        public IReadOnlyList<ContainerEntry> Entries { get; }

        public ContainerEntry GetEntry(string fileName);
    }

    public abstract class ContainerFile<T> : IContainerFile, IDisposable where T : ContainerEntry
    {
        protected Stream _stream;
        protected List<T> _entries;

        #region Properties

        public IReadOnlyList<ContainerEntry> Entries => _entries;

        #endregion Properties

        public ContainerEntry GetEntry(string fileName) => _entries.Find(e => e.FileName == fileName);

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

        public Task ForAllTask(Func<ContainerEntry, string, Task> action, IProgress<ProgressData> progress = null) => ForAllTask(action, "", progress);

        private async Task ForAllTask(Func<ContainerEntry, string, Task> action, string prefix, IProgress<ProgressData> progress = null)
        {
            int total = Entries.Count;
            int count = 0;
            foreach (ContainerEntry entry in Entries)
            {
                count++;
                progress?.Report(new ProgressData { count = count, total = total, filename = entry.FileName });
                if (entry.FileName.EndsWith(".par") || entry.FileName.EndsWith(".pta"))
                {
                    string dirName = entry.FileName[0..^4] + '_' + entry.FileName[^3..];
                    using Stream stream = await entry.GetData();
                    using ParFile parFile = new ParFile(stream);
                    await parFile.ForAllTask(action, prefix + dirName + '/', null);
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

        #endregion IDisposable
    }
}
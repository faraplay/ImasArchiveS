using System;
using System.Collections.Generic;
using System.IO;

namespace Imas.Archive
{
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

        #endregion IDisposable
    }
}
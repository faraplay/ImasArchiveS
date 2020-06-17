using System;
using System.IO;

namespace ImasArchiveLib
{
    public class ArcEntry : IDisposable
    {
        private ArcFile _parent_file;
        private string _filepath;
        private string _name;
        private long _base_offset;
        private long _length;
        private bool _openedForRead;
        private MemoryStream _memory_stream;
        private BufferedStream _buffered_stream;
        private bool disposed = false;

        internal ArcEntry(ArcFile parent, string filepath, long baseOffset, long length)
        {
            _parent_file = parent;
            _filepath = filepath;
            _base_offset = baseOffset;
            _length = length;
            _name = _filepath.Substring(_filepath.LastIndexOf('/') + 1);
            _openedForRead = false;
            _memory_stream = null;
        }

        public string Filepath { get => _filepath; }
        public string Name { get => _name; }
        public long Length { get => _length; }

        /// <summary>
        /// Opens a stream containing the file data of the entry for read access.
        /// </summary>
        /// <returns></returns>
        public Stream Open()
        {
            if (_openedForRead)
            {
                throw new IOException(Strings.IO_EntryAlreadyOpen);
            }
            _openedForRead = true;
            if (_memory_stream == null)
            {
                _buffered_stream = new BufferedStream(new Substream(_parent_file.ArcStream, _base_offset, _length));
                return _buffered_stream;
            }
            else
            {
                return _memory_stream;
            }
        }

        public void Close()
        {
            _buffered_stream?.Dispose();
            _buffered_stream = null;
            _openedForRead = false;
        }

        /// <summary>
        /// Writes the contents of a stream into the entry, overwriting any previous data.
        /// </summary>
        /// <param name="stream">The stream to copy from</param>
        public void Replace(Stream stream)
        {
            if (_openedForRead)
            {
                throw new IOException(Strings.IO_EntryAlreadyOpen);
            }
            _memory_stream = new MemoryStream();
            stream.CopyTo(_memory_stream);
        }

        public void Delete()
        {
            if (_openedForRead)
            {
                throw new IOException(Strings.IO_EntryAlreadyOpen);
            }
            _parent_file.RemoveEntry(this);
            _parent_file = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ArcEntry() => Dispose(false);

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _buffered_stream?.Dispose();
                _memory_stream?.Dispose();
            }

            disposed = true;
        }
    }
}
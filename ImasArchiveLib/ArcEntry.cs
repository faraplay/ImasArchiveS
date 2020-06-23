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
        private MemoryStream _memory_stream;
        private bool disposed = false;

        internal ArcEntry(ArcFile parent, string filepath, long baseOffset, long length)
        {
            _parent_file = parent;
            _filepath = filepath;
            _base_offset = baseOffset;
            _length = length;
            _name = _filepath.Substring(_filepath.LastIndexOf('/') + 1);
            _memory_stream = null;
        }

        public string Filepath { get => _filepath; }
        public string Name { get => _name; }
        public long Length { get => _length; }
        internal long Base_offset { get => _base_offset; set => _base_offset = value; }

        /// <summary>
        /// Opens a stream containing the file data of the entry for read access.
        /// </summary>
        /// <returns></returns>
        internal Stream OpenRaw()
        {
            if (_memory_stream == null)
            {
                return new BufferedStream(new Substream(_parent_file.ArcStream, _base_offset, _length));
            }
            else
            {
                return _memory_stream;
            }
        }

        public Stream Open()
        {
            FlowbishStream flowbishStream = new FlowbishStream(OpenRaw(), FlowbishStreamMode.Decipher, _name);
            SegsStream segsStream = new SegsStream(flowbishStream, SegsStreamMode.Decompress);
            return segsStream;
        }

        /// <summary>
        /// Writes the contents of a stream into the entry, overwriting any previous data.
        /// </summary>
        /// <param name="stream">The stream to copy from</param>
        public void Replace(Stream stream)
        {
            _memory_stream?.Dispose();
            _memory_stream = new MemoryStream();
            using (FlowbishStream flowbishStream = new FlowbishStream(_memory_stream, FlowbishStreamMode.Encipher, Name, true))
            {
                SegsStream.CompressStream(stream, flowbishStream);
            }
            _length = _memory_stream.Length;

        }

        public void Delete()
        {
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
                _memory_stream?.Dispose();
            }

            disposed = true;
        }
    }
}
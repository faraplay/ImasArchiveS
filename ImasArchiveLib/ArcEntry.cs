using System;
using System.IO;
using System.Threading.Tasks;

namespace ImasArchiveLib
{
    public class ArcEntry : IDisposable
    {
        #region Fields
        private ArcFile _parent_file;
        private readonly long _base_offset;
        private long _originalLength;
        private MemoryStream _memory_stream;
        private bool disposed = false;
        #endregion
        #region Properties
        public string Filepath { get; }
        public string Name { get; }
        public long Length { get => Edited ? _memory_stream.Length : _originalLength; }
        internal long CreatedLength { get; private set; }
        internal long SaveAsOffset { get; set; }
        internal bool Edited { get => _memory_stream != null; }
        #endregion
        #region Constructors & Factory Methods
        /// <summary>
        /// Create an ArcEntry based on an ArcFile.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="filepath"></param>
        /// <param name="baseOffset"></param>
        /// <param name="length"></param>
        internal ArcEntry(ArcFile parent, string filepath, long baseOffset, long length)
        {
            _parent_file = parent;
            Filepath = filepath;
            Name = Filepath.Substring(Filepath.LastIndexOf('/') + 1);
            _base_offset = baseOffset;
            SaveAsOffset = baseOffset;
            _originalLength = length;
            _memory_stream = null;
        }

        private ArcEntry(ArcFile parent, string filepath)
        {
            _parent_file = parent;
            Filepath = filepath;
            _base_offset = -1;
            Name = Filepath.Substring(Filepath.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// Asynchronously create a new ArcEntry without any backing data in its parent ArcFile.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="filepath"></param>
        /// <param name="stream">The stream to copy from.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <exception cref="IOException"/>
        internal static async Task<ArcEntry> NewEntry(ArcFile parent, string filepath, Stream stream)
        {
            ArcEntry arcEntry = new ArcEntry(parent, filepath);
            await arcEntry.Replace(stream);
            arcEntry._originalLength = arcEntry._memory_stream.Length;
            return arcEntry;
        }
        #endregion


        /// <summary>
        /// Opens a stream containing the raw file data of the entry for read access.
        /// If an exception occurs, returns null.
        /// Do not dispose if there is a memory stream (Edited is true)
        /// </summary>
        /// <returns></returns>
        internal Stream OpenRaw()
        {
            try
            {
                if (_memory_stream != null)
                {
                    _memory_stream.Seek(0, SeekOrigin.Begin);
                    return _memory_stream;
                }
                else if (_base_offset >= 0)
                {
                    return new BufferedStream(_parent_file.GetSubstream(_base_offset, _originalLength));
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Opens a stream containing the raw file data of the entry for read access.
        /// If an exception occurs, returns null.
        /// </summary>
        /// <returns></returns>
        public Stream Open()
        {
            try
            {
                FlowbishStream flowbishStream = new FlowbishStream(OpenRaw(), FlowbishStreamMode.Decipher, Name);
                SegsStream segsStream = new SegsStream(flowbishStream, SegsStreamMode.Decompress);
                return segsStream;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Writes the contents of a stream into the entry, overwriting any previous data.
        /// </summary>
        /// <param name="stream">The stream to copy from</param>
        /// <exception cref="IOException"/>
        public async Task Replace(Stream stream)
        {
            _memory_stream?.Dispose();
            _memory_stream = new MemoryStream();
            using FlowbishStream flowbishStream = new FlowbishStream(_memory_stream, FlowbishStreamMode.Encipher, Name, true);
            await SegsStream.CompressStream(stream, flowbishStream);
        }

        public void Delete()
        {
            _parent_file.RemoveEntry(this);
            _parent_file = null;
        }

        public void RevertToOriginal()
        {
            ClearMemoryStream();
            if (_base_offset == -1)
                _originalLength = 0;
        }
        internal void ClearMemoryStream()
        {
            _memory_stream?.Dispose();
            _memory_stream = null;
        }

        #region IDisposable

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
        #endregion
        #region Commu
        /// <summary>
        /// Tries to output commu text from the entry to a new file in the specified directory.
        /// </summary>
        /// <param name="destDir"></param>
        /// <returns></returns>
        public bool TryGetCommuText(string destDir)
        {
            try
            {
                using Stream parStream = this.Open();
                if (!Name.EndsWith(".par.gz"))
                    return false;
                int mbinPos = ParCommu.TryGetMBin(parStream, Name[0..^3]);
                if (mbinPos != -1)
                {
                    using StreamWriter streamWriter = new StreamWriter(destDir + '/' + Name[0..^7] + "_m.txt");
                    streamWriter.WriteLine(Filepath[0..^3]);
                    parStream.Position = mbinPos;
                    ParCommu.GetCommuText(parStream, streamWriter);
                }
                return (mbinPos != -1);
            }
            catch
            {
                return false;
            }
        }

        public async Task TryReplaceCommuText(string commuFileName)
        {
            using Stream parStream = this.Open();
            MemoryStream memoryStream = new MemoryStream();
            using StreamReader commuReader = new StreamReader(commuFileName);
            try
            {
                await ParCommu.ReplaceMBin(parStream, memoryStream, commuReader, Name[0..^3]);
            }
            catch (InvalidDataException)
            {
                return;
            }
            memoryStream.Position = 0;
            await Replace(memoryStream);
        }
        #endregion
    }
}
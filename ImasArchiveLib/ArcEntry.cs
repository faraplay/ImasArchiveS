using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveLib
{
    public class ArcEntry : IDisposable
    {
        #region Fields
        private ArcFile _parent_file;
        private readonly long _base_offset;
        private long _originalLength;
        private MemoryStream _newData;
        private bool disposed = false;
        #endregion
        #region Properties
        public string Filepath { get; }
        public string Name { get; }
        public long Length { get => UsesMemoryStream ? _newData.Length : _originalLength; }
        internal long CreatedLength { get; private set; }
        internal long SaveAsOffset { get; set; }
        internal bool UsesMemoryStream { get => _newData != null; }
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
            _newData = null;
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
            await arcEntry.SetData(stream);
            arcEntry._originalLength = arcEntry._newData.Length;
            return arcEntry;
        }
        #endregion
        #region Stream Methods

        /// <summary>
        /// Opens a stream containing the raw file data of the entry for read access.
        /// If an exception occurs, returns null.
        /// Do not dispose if there is a memory stream (UsesMemoryStream is true)
        /// </summary>
        /// <returns></returns>
        internal Stream OpenRaw()
        {
            try
            {
                if (_newData != null)
                {
                    _newData.Seek(0, SeekOrigin.Begin);
                    return _newData;
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
        /// Creates a new stream containing the raw file data of the entry for read access.
        /// Changes in this new stream will not affect the ArcEntry.
        /// If an exception occurs, returns null.
        /// </summary>
        /// <returns></returns>
        public async Task<Stream> GetData()
        {
            try
            {
                using FlowbishStream flowbishStream = new FlowbishStream(OpenRaw(), FlowbishStreamMode.Decipher, Name + ".gz", UsesMemoryStream);
                using SegsStream segsStream = new SegsStream(flowbishStream, SegsStreamMode.Decompress);
                MemoryStream memoryStream = new MemoryStream();
                await segsStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0;
                return memoryStream;
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
        public async Task SetData(Stream stream)
        {
            _newData?.Dispose();
            _newData = new MemoryStream();
            using FlowbishStream flowbishStream = new FlowbishStream(_newData, FlowbishStreamMode.Encipher, Name + ".gz", true);
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
            _newData?.Dispose();
            _newData = null;
        }
        #endregion
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
                _newData?.Dispose();
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
        public async Task<bool> TryGetCommuText(string destDir)
        {
            try
            {
                using Stream parStream = await GetData();
                if (!Name.EndsWith(".par"))
                    return false;
                int mbinPos = ParCommu.TryGetMBin(parStream, Name);
                if (mbinPos != -1)
                {
                    using MemoryStream memStream = new MemoryStream();
                    using (StreamWriter streamWriter = new StreamWriter(memStream, Encoding.Default, 65536, true))
                    {
                        streamWriter.WriteLine(Filepath);
                        parStream.Position = mbinPos;
                        ParCommu.GetCommuText(parStream, streamWriter);
                    }
                    memStream.Position = 0;
                    using FileStream fileStream = new FileStream(destDir + '/' + Name[0..^4] + "_m.txt", FileMode.Create, FileAccess.Write);
                    await memStream.CopyToAsync(fileStream);
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
            using Stream parStream = await GetData();
            MemoryStream memoryStream = new MemoryStream();
            using StreamReader commuReader = new StreamReader(commuFileName);
            try
            {
                await ParCommu.ReplaceMBin(parStream, memoryStream, commuReader, Name);
            }
            catch (InvalidDataException)
            {
                return;
            }
            memoryStream.Position = 0;
            await SetData(memoryStream);
        }
        #endregion
    }
}
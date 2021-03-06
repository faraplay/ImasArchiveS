﻿using Imas.Streams;
using System.IO;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public class ArcEntry : ContainerEntry
    {
        #region Fields

        private readonly ArcFile _parent_file;

        #endregion Fields

        #region Properties

        private string ShortName { get; }
        private bool rememberPastLength = false;
        private long _pastLength;
        internal long PastLength => rememberPastLength ? _pastLength : Length;

        #endregion Properties

        #region Constructors & Factory Methods

        /// <summary>
        /// Create an ArcEntry based on an ArcFile.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="fileName"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal ArcEntry(ArcFile parent, string fileName, long offset, long length) :
            base(fileName, length, offset)
        {
            _parent_file = parent;
            ShortName = FileName.Substring(FileName.LastIndexOf('/') + 1);
            _newData = null;
        }

        private ArcEntry(ArcFile parent, string fileName) :
            base(fileName, 0, -1)
        {
            _parent_file = parent;
            ShortName = FileName.Substring(FileName.LastIndexOf('/') + 1);
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
            return arcEntry;
        }

        #endregion Constructors & Factory Methods

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
                else if (_originalOffset >= 0)
                {
                    return new BufferedStream(_parent_file.GetSubstream(_originalOffset, _originalLength));
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
        public override async Task<Stream> GetData()
        {
            using FlowbishStream flowbishStream = new FlowbishStream(OpenRaw(), FlowbishStreamMode.Decipher, ShortName + ".gz", UsesMemoryStream);
            using SegsStream segsStream = new SegsStream(flowbishStream, SegsStreamMode.Decompress);
            MemoryStream memoryStream = new MemoryStream();
            await segsStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Writes the contents of a stream into the entry, overwriting any previous data.
        /// </summary>
        /// <param name="stream">The stream to copy from</param>
        /// <exception cref="IOException"/>
        public override async Task SetData(Stream stream)
        {
            _newData?.Dispose();
            _newData = new MemoryStream();
            using FlowbishStream flowbishStream = new FlowbishStream(_newData, FlowbishStreamMode.Encipher, ShortName + ".gz", true);
            if (stream.CanSeek)
            {
                await SegsStream.CompressStream(stream, flowbishStream);
            }
            else
            {
                using MemoryStream memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;
                await SegsStream.CompressStream(memStream, flowbishStream);
            }
        }

        public void RevertToOriginal()
        {
            ClearMemoryStream();
            rememberPastLength = false;
        }

        internal void ClearMemoryStream()
        {
            if (UsesMemoryStream)
            {
                rememberPastLength = true;
                _pastLength = _newData.Length;
                _newData?.Dispose();
                _newData = null;
            }
        }

        #endregion Stream Methods
    }
}
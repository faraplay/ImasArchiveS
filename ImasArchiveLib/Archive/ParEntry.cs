using System.IO;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public class ParEntry : ContainerEntry
    {
        public ParFile Parent { get; set; }
        public int Property { get; set; }

        internal MemoryStream NewData
        {
            get
            {
                if (_newData == null)
                {
                    _newData = new MemoryStream();
                }
                return _newData;
            }
        }

        #region Constructors

        internal ParEntry(ParFile parent, string fileName, int offset, int length, int property) :
            base(fileName, length, offset)
        {
            Parent = parent;
            Property = property;
        }

        #endregion Constructors

        /// <summary>
        /// Creates a new stream containing the raw file data of the entry for read access.
        /// Changes in this new stream will not affect the ParEntry.
        /// </summary>
        public override async Task<Stream> GetData()
        {
            MemoryStream memoryStream = new MemoryStream();
            if (UsesMemoryStream)
            {
                _newData.Position = 0;
                await _newData.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
            else
            {
                using Stream stream = Parent.GetSubstream(_originalOffset, _originalLength);
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Writes the contents of a stream into the entry, overwriting any previous data.
        /// </summary>
        /// <param name="stream">The stream to copy from</param>
        public override async Task SetData(Stream stream)
        {
            _newData?.Dispose();
            _newData = new MemoryStream();
            await stream.CopyToAsync(_newData).ConfigureAwait(false);
        }
    }
}
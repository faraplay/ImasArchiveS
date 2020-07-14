using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImasArchiveLib
{
    public class ParEntry
    {
        private MemoryStream _newData;
        private readonly int _originalLength;
        private readonly int _originalOffset;

        public string Name { get; set; }
        internal int Offset { get; set; }
        internal int Length => UsesMemoryStream ? (int)_newData.Length : _originalLength;
        public ParFile Parent { get; set; }
        public int Property { get; set; }
        internal bool UsesMemoryStream => _newData != null;

        internal ParEntry(ParFile parent, string name, int offset, int length, int property)
        {
            Parent = parent;
            Name = name;
            _originalOffset = offset;
            Offset = offset;
            _originalLength = length;
            Property = property;
        }

        /// <summary>
        /// Creates a new stream containing the raw file data of the entry for read access.
        /// Changes in this new stream will not affect the ParEntry.
        /// </summary>
        public async Task<Stream> GetData()
        {
            MemoryStream memoryStream = new MemoryStream();
            using Stream stream = Parent.GetSubstream(_originalOffset, _originalLength);
            await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Writes the contents of a stream into the entry, overwriting any previous data.
        /// </summary>
        /// <param name="stream">The stream to copy from</param>
        public async Task SetData(Stream stream)
        {
            _newData?.Dispose();
            _newData = new MemoryStream();
            await stream.CopyToAsync(_newData).ConfigureAwait(false);
        }
    }
}

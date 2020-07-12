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

        public string Name { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public ParFile Parent { get; set; }
        public int Property { get; set; }

        internal ParEntry(ParFile parent, string name, int offset, int length, int property)
        {
            Parent = parent;
            Name = name;
            Offset = offset;
            Length = length;
            Property = property;
        }

        /// <summary>
        /// Creates a new stream containing the raw file data of the entry for read access.
        /// Changes in this new stream will not affect the ParEntry.
        /// </summary>
        public async Task<Stream> GetData()
        {
            MemoryStream memoryStream = new MemoryStream();
            using Stream stream = Parent.GetSubstream(Offset, Length);
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

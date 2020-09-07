using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Imas.Archive
{
    public class PatchZipEntry : ContainerEntry
    {
        private readonly ZipArchiveEntry _entry;

        internal PatchZipEntry(ZipArchiveEntry entry) : base(entry.FullName, 0, 0)
        {
            _entry = entry;
        }

        public async override Task<Stream> GetData()
        {
            MemoryStream stream = new MemoryStream();
            using Stream entryStream = _entry.Open();
            await entryStream.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async override Task SetData(Stream stream)
        {
            using Stream entryStream = _entry.Open();
            await stream.CopyToAsync(entryStream);
        }
    }
}
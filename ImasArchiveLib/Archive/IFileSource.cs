using System.IO;

namespace Imas.Archive
{
    public interface IFileSource
    {
        public bool FileExists(string fileName);

        public Stream OpenFile(string fileName);

        public bool DirectoryExists(string dirName);

        public IFileSource OpenDirectory(string dirName);
    }
}
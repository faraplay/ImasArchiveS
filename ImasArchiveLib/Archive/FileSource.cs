using System.IO;

namespace Imas.Archive
{
    public class FileSource : IFileSource
    {
        private readonly DirectoryInfo dInfo;

        public FileSource(string dirName)
        {
            dInfo = new DirectoryInfo(dirName);
            if (!dInfo.Exists)
                throw new DirectoryNotFoundException();
        }

        public bool FileExists(string fileName)
        {
            return File.Exists(dInfo.FullName + '\\' + fileName);
        }

        public Stream OpenFile(string fileName)
        {
            return new FileStream(dInfo.FullName + '\\' + fileName, FileMode.Open, FileAccess.Read);
        }

        public bool DirectoryExists(string dirName)
        {
            return Directory.Exists(dInfo.FullName + '\\' + dirName);
        }

        public IFileSource OpenDirectory(string dirName)
        {
            return new FileSource(dInfo.FullName + '\\' + dirName);
        }
    }
}
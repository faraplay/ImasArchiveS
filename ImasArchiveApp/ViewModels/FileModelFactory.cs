using Imas.Archive;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImasArchiveApp
{
    public static class FileModelFactory
    {
        public static IReport report;
        public static IGetFileName getFileName;

        public static async Task<IFileModel> CreateFileModel(Stream stream, string fileName)
        {
            string extension = fileName[(fileName.LastIndexOf('.') + 1)..];
            switch (extension)
            {
                case "par":
                    ParFile parFile = new ParFile(stream);
                    if (parFile.Entries.Any(entry => entry.FileName.EndsWith(".pau")))
                        return await UIComponentModel.CreateComponentModel(report, stream, fileName, getFileName);
                    else
                        return new ParModel(report, parFile, fileName, getFileName);
                case "pta":
                    return new ParModel(report, stream, fileName, getFileName);

                case "gtf":
                case "tex":
                case "dds":
                    return new GTFModel(report, fileName, getFileName, stream);

                default:
                    return new HexViewModel(report, fileName, stream);
            }
        }

        public delegate IFileModel FileModelBuilder(IReport report, string filename, IGetFileName getFilename, Stream stream);

        public static IFileModel CreateFileModel(string fileName, FileModelBuilder fileModelBuilder) =>
            CreateFileModel(new FileStream(fileName, FileMode.Open, FileAccess.Read), fileName.Substring(fileName.LastIndexOf('\\')), fileModelBuilder);

        public static IFileModel CreateFileModel(Stream stream, string fileName, FileModelBuilder fileModelBuilder)
        {
            return fileModelBuilder(report, fileName, getFileName, stream);
        }

        public static async Task<IFileModel> CreateFileModel(string fileName)
        {
            if (fileName.EndsWith(".arc") || fileName.EndsWith(".arc.dat"))
            {
                return new ArcModel(report, fileName, new Dialogs());
            }
            else if (fileName.EndsWith(".zip"))
            {
                return new PatchZipModel(report, fileName, new Dialogs());
            }
            else
            {
                return await CreateFileModel(new FileStream(fileName, FileMode.Open, FileAccess.Read), fileName.Substring(fileName.LastIndexOf('\\') + 1));
            }
        }
    }
}
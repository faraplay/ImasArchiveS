using System.IO;

namespace ImasArchiveApp
{
    static class FileModelFactory
    {
        public static IReport report;
        public static IGetFileName getFileName;
        public static IFileModel CreateFileModel(Stream stream, string fileName)
        {
            string extension = fileName.Substring(fileName.LastIndexOf('.') + 1);
            switch (extension)
            {
                case "par":
                case "pta":
                    return new ParModel(report, stream, fileName);
                case "gtf":
                case "tex":
                case "dds":
                    return new GTFModel(report, fileName, getFileName, stream);
                default:
                    return new HexViewModel(report, fileName, stream);
            }
        }

        public static IFileModel CreateFileModel(string fileName)
        {
            if (fileName.EndsWith(".arc") || fileName.EndsWith(".arc.dat"))
            {
                return new ArcModel(report, fileName, new Dialogs());
            }
            else
            {
                return CreateFileModel(new FileStream(fileName, FileMode.Open, FileAccess.Read), fileName.Substring(fileName.LastIndexOf('\\')));
            }
        }
    }
}

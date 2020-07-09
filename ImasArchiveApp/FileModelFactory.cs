using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveApp
{
    static class FileModelFactory
    {
        public static ModelWithReport parent;
        public static IFileModel CreateFileModel(Stream stream, string fileName)
        {
            string extension = fileName.Substring(fileName.LastIndexOf('.') + 1);
            switch (extension)
            {
                case "par":
                case "pta":
                    return new ParModel(stream);
                case "gtf":
                case "tex":
                case "dds":
                    return new GTFModel(stream);
                default:
                    return new HexViewModel(stream);
            }
        }

        public static IFileModel CreateFileModel(string fileName)
        {
            string extension = fileName.Substring(fileName.LastIndexOf('.') + 1);
            if (fileName.EndsWith(".arc") || fileName.EndsWith(".arc.dat"))
            {
                return new ArcModel(parent, fileName);
            }
            else
            {
                return CreateFileModel(new FileStream(fileName, FileMode.Open, FileAccess.Read), fileName);
            }
        }
    }
}

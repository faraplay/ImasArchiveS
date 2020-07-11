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
                    return new ParModel(parent, stream, fileName);
                case "gtf":
                case "tex":
                case "dds":
                    return new GTFModel(stream, fileName);
                default:
                    return new HexViewModel(stream, fileName);
            }
        }

        public static IFileModel CreateFileModel(string fileName)
        {
            if (fileName.EndsWith(".arc") || fileName.EndsWith(".arc.dat"))
            {
                return new ArcModel(parent, fileName, new Dialogs());
            }
            else
            {
                return CreateFileModel(new FileStream(fileName, FileMode.Open, FileAccess.Read), fileName.Substring(fileName.LastIndexOf('\\')));
            }
        }
    }
}

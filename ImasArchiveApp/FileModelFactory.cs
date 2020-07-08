using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveApp
{
    static class FileModelFactory
    {
        public static IFileModel CreateFileModel(Stream stream, string fileName)
        {
            string extension = fileName.Substring(fileName.LastIndexOf('.') + 1);
            switch (extension)
            {
                case "gtf":
                case "tex":
                case "dds":
                    return new GTFModel(stream);
                default:
                    return new HexViewModel(stream);
            }
        }
    }
}

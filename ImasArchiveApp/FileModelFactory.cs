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
                    try
                    {
                        return new GTFModel(stream);
                    }
                    catch
                    {
                        stream.Position = 0;
                        return new HexViewModel(stream);
                    }
                default:
                    return new HexViewModel(stream);
            }
        }
    }
}

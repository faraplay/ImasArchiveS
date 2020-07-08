using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveApp
{
    static class FileModelFactory
    {
        public static IFileModel CreateFileModel(Stream stream)
        {
            return new HexViewModel(stream);
        }
    }
}

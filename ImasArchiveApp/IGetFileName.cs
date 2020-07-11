using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    interface IGetFileName
    {
        public string OpenGetFileName(string title, string filter);
        public string SaveGetFileName(string title, string defaultPath, string filter);
        public string SaveGetFileName(string title, string defaultDir, string defaultName, string filter);
        public string OpenGetFolderName(string title);
    }
}

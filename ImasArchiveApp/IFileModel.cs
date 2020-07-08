using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    interface IFileModel : IDisposable
    {
    }

    interface IContainerFileModel : IFileModel
    {
        public void LoadChildFileModel(string filename);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ImasArchiveApp
{
    interface IFileModel : INotifyPropertyChanged, IDisposable
    {
    }

    interface IContainerFileModel : IFileModel
    {
        public void LoadChildFileModel(string filename);
    }
}

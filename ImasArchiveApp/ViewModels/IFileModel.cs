using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ImasArchiveApp
{
    interface IFileModel : INotifyPropertyChanged, IDisposable
    {
        public string FileName { get; }
    }

    interface IContainerFileModel : IFileModel
    {
        public void LoadChildFileModel(string filename);
    }
}

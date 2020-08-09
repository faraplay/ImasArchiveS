using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ImasArchiveApp
{
    public interface IFileModel : INotifyPropertyChanged, IDisposable
    {
        public string FileName { get; }
    }
}

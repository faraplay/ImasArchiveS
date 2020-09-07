using System;
using System.ComponentModel;

namespace ImasArchiveApp
{
    public interface IFileModel : INotifyPropertyChanged, IDisposable
    {
        public string FileName { get; }
    }
}
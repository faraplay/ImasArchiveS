using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImasArchiveApp
{
    class ParModel : IContainerFileModel
    {
        #region Fields
        private ParFile _parFile;
        private bool disposed;

        private BrowserModel _browserModel;
        private IFileModel _fileModel;
        #endregion
        #region Properties
        public BrowserModel BrowserModel
        {
            get => _browserModel;
            set
            {
                _browserModel?.Dispose();
                _browserModel = value;
                OnPropertyChanged();
            }
        }
        public IFileModel CurrentFileModel
        {
            get => _fileModel;
            set
            {
                _fileModel?.Dispose();
                _fileModel = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Constructors
        public ParModel(Stream stream)
        {
            _parFile = new ParFile(stream);
            List<string> browserEntries = new List<string>();
            foreach (ParEntry entry in _parFile.Entries)
            {
                browserEntries.Add(entry._name);
            }
            BrowserModel = new BrowserModel(this, new BrowserTree("", browserEntries));
        }
        #endregion
        #region IDisposable
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _parFile?.Dispose();
                _fileModel?.Dispose();
            }
            disposed = true;
        }
        ~ParModel() => Dispose(false);
        #endregion

        public void LoadChildFileModel(string fileName)
        {
            if (fileName != null)
            {
                CurrentFileModel = FileModelFactory.CreateFileModel(_parFile.GetEntry(fileName).Open(), fileName);
            }
            else
            {
                CurrentFileModel = null;
            }
        }
    }
}

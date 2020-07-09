using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImasArchiveApp
{
    class ParModel : ModelWithReport, IContainerFileModel
    {
        #region Fields
        private readonly ParFile _parFile;

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
        public IFileModel FileModel
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
        public ParModel(ModelWithReport parent, Stream stream)
        {
            ClearStatus = parent.ClearStatus;
            ReportException = parent.ReportException;
            ReportMessage = parent.ReportMessage;
            ReportProgress = parent.ReportProgress;
            try
            {
                _parFile = new ParFile(stream);
                List<string> browserEntries = new List<string>();
                foreach (ParEntry entry in _parFile.Entries)
                {
                    browserEntries.Add(entry._name);
                }
                BrowserModel = new BrowserModel(this, new BrowserTree("", browserEntries));
            }
            catch
            {
                Dispose();
                throw;
            }
        }
        #endregion
        #region IDisposable
        private bool disposed;
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
            ClearStatus();
            if (fileName != null)
            {
                try
                {
                    FileModel = FileModelFactory.CreateFileModel(_parFile.GetEntry(fileName).Open(), fileName);
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    FileModel = null;
                }
            }
            else
            {
                FileModel = null;
            }
        }
    }
}

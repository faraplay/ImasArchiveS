using Imas;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImasArchiveApp
{
    public abstract class FileModel : IFileModel, IReport
    {
        #region Properties
        public string FileName { get; }
        #endregion

        #region Constructors
        protected FileModel(IReport parent, string fileName)
        {
            ClearStatus = parent.ClearStatus;
            ReportProgress = parent.ReportProgress;
            ReportMessage = parent.ReportMessage;
            ReportException = parent.ReportException;
            FileName = fileName;
        }
        #endregion
        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
        }

        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IReport
        public Action ClearStatus { get; }
        public Action<ProgressData> ReportProgress { get; }
        public Action<string> ReportMessage { get; }
        public Action<Exception> ReportException { get; }
        #endregion
    }
}

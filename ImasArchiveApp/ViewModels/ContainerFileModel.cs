using Imas;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ImasArchiveApp
{
    abstract class ContainerFileModel : IFileModel, IReport
    {
        #region Properties
        public string FileName { get; }
        public BrowserModel BrowserModel { get; set; }
        public abstract IFileModel FileModel { get; set; }
        #endregion
        #region Constructors
        protected ContainerFileModel(IReport parent, string fileName)
        {
            ClearStatus = parent.ClearStatus;
            ReportProgress = parent.ReportProgress;
            ReportMessage = parent.ReportMessage;
            ReportException = parent.ReportException;
            FileName = fileName;
        }
        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public abstract Task LoadChildFileModel(string filename);

        #region IDisposable
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                FileModel?.Dispose();
            }
            disposed = true;
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

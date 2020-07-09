using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class MainWindowModel : ModelWithReport, INotifyPropertyChanged
    {
        #region Fields
        private IFileModel _fileModel;

        private string _statusMessage;
        private bool _statusIsException;

        public string InPath;
        #endregion
        #region Properties
        public IFileModel FileModel
        {
            get => _fileModel;
            set
            {
                _fileModel = value;
                OnPropertyChanged();
            }
        }
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
        public bool StatusIsException
        {
            get => _statusIsException;
            set
            {
                _statusIsException = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Constructors
        public MainWindowModel()
        {
            ClearStatus = MyClearStatus;
            ReportProgress = MyReportProgress;
            ReportMessage = MyReportMessage;
            ReportException = MyReportException;
        }
        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Commands
        private RelayCommand _openCommand;
        public ICommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                {
                    _openCommand = new RelayCommand(_ => Open());
                }
                return _openCommand;
            }
        }
        private RelayCommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(_ => Close(), _ => CanClose());
                }
                return _closeCommand;
            }
        }
        public bool CanClose() => FileModel != null;
        #endregion
        #region Command Methods
        public void Open()
        {
            OpenArc(InPath);
        }
        public void OpenArc(string inPath)
        {
            try
            {
                FileModel = FileModelFactory.CreateFileModel(inPath);
            }
            catch (Exception ex)
            {
                ReportException(ex);
                FileModel.Dispose();
                FileModel = null;
            }
        }   
        public void Close()
        {
            FileModel.Dispose();
            FileModel = null;
        }
        #endregion
        #region Progress
        public void MyClearStatus()
        {
            StatusMessage = "";
            StatusIsException = false;
        }

        public void MyReportProgress(ProgressData data)
        {
            StatusMessage = string.Format("{0} of {1}: {2}", data.count, data.total, data.filename);
            StatusIsException = false;
        }

        public void MyReportMessage(string message)
        {
            StatusMessage = message;
        }

        public void MyReportException(Exception ex)
        {
            StatusMessage = ex.GetType().ToString() + ": " + ex.Message;
            StatusIsException = true;
        }
        #endregion
    }
}

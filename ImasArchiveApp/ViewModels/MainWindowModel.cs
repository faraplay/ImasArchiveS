using Imas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class MainWindowModel : IReport, INotifyPropertyChanged
    {
        #region Fields
        private IFileModel _fileModel;

        private string _statusMessage;
        private bool _statusIsException;
        private readonly IGetFileName _getFileName;
        #endregion
        #region Properties
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
        public MainWindowModel(IGetFileName getFileName)
        {
            _getFileName = getFileName;
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
        #region IReport
        public Action ClearStatus { get; }
        public Action<ProgressData> ReportProgress { get; }
        public Action<string> ReportMessage { get; }
        public Action<Exception> ReportException { get; }
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
        AsyncCommand _newFromFolderCommand;
        public ICommand NewFromFolderCommand
        {
            get
            {
                if (_newFromFolderCommand == null)
                {
                    _newFromFolderCommand = new AsyncCommand(() => CreateNewFromFolder(), () => CanNewFromFolder());
                }
                return _newFromFolderCommand;
            }
        }
        public bool CanNewFromFolder() => FileModel == null;
        AsyncCommand _replaceSaveCommand;
        public ICommand ReplaceSaveCommand
        {
            get
            {
                if (_replaceSaveCommand == null)
                {
                    _replaceSaveCommand = new AsyncCommand(() => OpenReplaceAndSave(), () => CanReplaceSave());
                }
                return _replaceSaveCommand;
            }
        }
        public bool CanReplaceSave() => FileModel == null;
        #endregion
        #region Command Methods
        public void Open()
        {
            if (CanClose())
                Close();
            string fileName = _getFileName.OpenGetFileName("Open archive",
                "Arc files (*.arc;*.arc.dat)|*.arc;*.arc.dat|All files (*.*)|*.*");
            if (fileName != null)
            {
                Open(fileName);
            }
        }
        public void Open(string inPath)
        {
            try
            {
                FileModel = FileModelFactory.CreateFileModel(inPath);
            }
            catch (Exception ex)
            {
                ReportException(ex);
                FileModel = null;
            }
        }   
        public void Close()
        {
            FileModel = null;
        }
        public async Task CreateNewFromFolder()
        {
            try
            {
                ClearStatus();
                string inFileName = _getFileName.OpenGetFolderName("Choose folder");
                if (inFileName != null)
                {
                    string outFileName = _getFileName.SaveGetFileName("Save As", inFileName, "Arc file (*.arc)|*.arc");
                    if (outFileName != null)
                    {
                        await Imas.Archive.ArcFile.BuildFromDirectory(inFileName, outFileName[0..^4], new Progress<ProgressData>(ReportProgress));
                        ReportMessage("Done.");
                        Open(outFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
        }

        public async Task OpenReplaceAndSave()
        {
            try
            {
                ClearStatus();
                string inFileName = _getFileName.OpenGetFileName("Choose folder", "Arc file (*.arc;*.arc.dat)|*.arc;*.arc.dat");
                if (inFileName != null)
                {
                    string repFolderName = _getFileName.OpenGetFolderName("Choose patch folder");
                    if (repFolderName != null)
                    {
                        string outFileName = _getFileName.SaveGetFileName("Save As", inFileName, "Arc file (*.arc)|*.arc");
                        if (outFileName != null)
                        {
                            string extension;
                            if (inFileName.EndsWith(".arc"))
                            {
                                inFileName = inFileName[0..^4];
                                extension = "";
                            }
                            else if (inFileName.EndsWith(".arc.dat"))
                            {
                                inFileName = inFileName[0..^8];
                                extension = ".dat";
                            }
                            else
                            {
                                throw new ArgumentException("Selected file does not have .arc or .arc.dat extension.");
                            }
                            await Imas.Archive.ArcFile.OpenReplaceAndSave(inFileName, extension, repFolderName, outFileName[0..^4], "", new Progress<ProgressData>(ReportProgress));
                            ReportMessage("Done.");
                            Open(outFileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
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
            StatusMessage = ex.ToString();
            StatusIsException = true;
        }
        #endregion
    }
}

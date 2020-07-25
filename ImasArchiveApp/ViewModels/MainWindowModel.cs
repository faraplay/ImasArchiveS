using Imas;
using Imas.Archive;
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
            progress = new Progress<ProgressData>(ReportProgress);
            duoProgress1 = new Progress<ProgressData>(ReportLine1);
            duoProgress2 = new Progress<ProgressData>(ReportLine2);
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
        private readonly Progress<ProgressData> progress;
        private readonly Progress<ProgressData> duoProgress1;
        private readonly Progress<ProgressData> duoProgress2;
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
        AsyncCommand _replaceSaveFolderCommand;
        public ICommand ReplaceSaveFolderCommand
        {
            get
            {
                if (_replaceSaveFolderCommand == null)
                {
                    _replaceSaveFolderCommand = new AsyncCommand(() => OpenReplaceAndSave(false), () => CanReplaceSave());
                }
                return _replaceSaveFolderCommand;
            }
        }
        AsyncCommand _replaceSaveZipCommand;
        public ICommand ReplaceSaveZipCommand
        {
            get
            {
                if (_replaceSaveZipCommand == null)
                {
                    _replaceSaveZipCommand = new AsyncCommand(() => OpenReplaceAndSave(true), () => CanReplaceSave());
                }
                return _replaceSaveZipCommand;
            }
        }
        public bool CanReplaceSave() => FileModel == null || FileModel is ArcModel;
        AsyncCommand _createCommuPatchCommand;
        public ICommand CreateCommuPatchCommand
        {
            get
            {
                if (_createCommuPatchCommand == null)
                {
                    _createCommuPatchCommand = new AsyncCommand(() => CreateCommuPatch(), () => CanCreateCommuPatch());
                }
                return _createCommuPatchCommand;
            }
        }
        public bool CanCreateCommuPatch() => FileModel == null;
        #endregion
        #region Command Methods
        public void Open()
        {
            string fileName = _getFileName.OpenGetFileName("Open archive",
                "Arc files (*.arc;*.arc.dat)|*.arc;*.arc.dat|All files (*.*)|*.*");
            if (fileName != null)
            {
                if (FileModel != null)
                    Close();
                Open(fileName);
            }
        }
        private void Open(string inPath)
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
                        await Imas.Archive.ArcFile.BuildFromDirectory(inFileName, outFileName[0..^4], progress);
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

        public async Task OpenReplaceAndSave(bool fromZip)
        {
            try
            {
                ClearStatus();
                string inFileName;
                bool fileOpened = FileModel != null;
                if (fileOpened)
                {
                    if (FileModel is ArcModel arcModel)
                    {
                        inFileName = arcModel.ArcPath;
                    }
                    else
                        throw new InvalidOperationException("Cannot patch non-arc file.");
                }
                else
                {
                    inFileName = _getFileName.OpenGetFileName("Choose arc", "Arc file (*.arc;*.arc.dat)|*.arc;*.arc.dat");
                    if (inFileName == null) return;
                }
                string replacementName = fromZip ? _getFileName.OpenGetFileName("Choose patch zip", "Zip file (*.zip)|*.zip") : _getFileName.OpenGetFolderName("Choose patch folder");
                if (replacementName == null) return;
                string outFileName = _getFileName.SaveGetFileName("Save As", inFileName, "Arc file (*.arc)|*.arc");
                if (outFileName == null) return;

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
                    throw new ArgumentException("Selected file does not have .arc or .arc.dat extension.");
                if (fileOpened)
                    Close();
                if (fromZip)
                {
                    using ZipSourceParent zipSourceParent = new ZipSourceParent(replacementName);
                    await ArcFile.OpenReplaceAndSave(
                        inFileName,
                        extension,
                        zipSourceParent.GetZipSource(),
                        outFileName[0..^4],
                        "",
                        progress);
                }
                else
                {
                    await ArcFile.OpenReplaceAndSave(
                        inFileName,
                        extension,
                        new FileSource(replacementName),
                        outFileName[0..^4],
                        "",
                        progress);
                }
                ReportMessage("Done.");
                Open(outFileName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
        }

        public async Task CreateCommuPatch()
        {
            try
            {
                ClearStatus();
                string inFileName = _getFileName.OpenGetFileName("Choose spreadsheet", "Excel spreadsheet (*.xlsx)|*.xlsx");
                if (inFileName != null)
                {
                    string outFileName = _getFileName.SaveGetFileName("Save As", inFileName[0..^5], "Zip file (*.zip)|*.zip");
                    if (outFileName != null)
                    {
                        using CommuFromXlsx commuFromXlsx = new CommuFromXlsx(inFileName, outFileName);
                        await commuFromXlsx.GetAndWriteAllCommus(duoProgress1, duoProgress2);
                        ReportMessage("Done.");
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

        string duoProgressLine1 = "";
        string duoProgressLine2 = "";
        public void ReportLine1(ProgressData data)
        {
            duoProgressLine1 = string.Format("{0} of {1}: {2}", data.count, data.total, data.filename);
            duoProgressLine2 = "";
            StatusMessage = duoProgressLine1 + "\n" + duoProgressLine2;
            StatusIsException = false;
        }
        public void ReportLine2(ProgressData data)
        {
            duoProgressLine2 = string.Format("{0} of {1}: {2}", data.count, data.total, data.filename);
            StatusMessage = duoProgressLine1 + "\n" + duoProgressLine2;
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

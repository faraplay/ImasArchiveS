using Imas;
using Imas.Archive;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class MainWindowModel : IReport, INotifyPropertyChanged
    {
        #region Fields

        private IFileModel _fileModel;

        private string _statusMessage;
        private bool _statusIsException;
        private readonly IGetFileName _getFileName;

        #endregion Fields

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

        #endregion Properties

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

        #endregion Constructors

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        #region IReport

        public Action ClearStatus { get; }
        public Action<ProgressData> ReportProgress { get; }
        public Action<string> ReportMessage { get; }
        public Action<Exception> ReportException { get; }
        private readonly Progress<ProgressData> progress;
        private readonly Progress<ProgressData> duoProgress1;
        private readonly Progress<ProgressData> duoProgress2;

        #endregion IReport

        #region Commands

        private AsyncCommand _openArcCommand;

        public ICommand OpenArcCommand
        {
            get
            {
                if (_openArcCommand == null)
                {
                    _openArcCommand = new AsyncCommand(() => OpenWithFilter("Arc files (*.arc;*.arc.dat)|*.arc;*.arc.dat"));
                }
                return _openArcCommand;
            }
        }

        private AsyncCommand _openPatchZipCommand;

        public ICommand OpenPatchZipCommand
        {
            get
            {
                if (_openPatchZipCommand == null)
                {
                    _openPatchZipCommand = new AsyncCommand(() => OpenWithFilter("Zip files (*.zip)|*zip"));
                }
                return _openPatchZipCommand;
            }
        }

        private AsyncCommand _openParCommand;

        public ICommand OpenParCommand
        {
            get
            {
                if (_openParCommand == null)
                {
                    _openParCommand = new AsyncCommand(() =>
                        OpenWithFilter("Par files (*.par)|*.par|All files (*.*)|*.*", ParModel.Builder));
                }
                return _openParCommand;
            }
        }

        private AsyncCommand _openGtfCommand;

        public ICommand OpenGtfCommand
        {
            get
            {
                if (_openGtfCommand == null)
                {
                    _openGtfCommand = new AsyncCommand(() =>
                        OpenWithFilter("GTF files (*.gtf;*.dds;*.tex)|*.gtf;*.dds;*.tex|All files (*.*)|*.*", GTFModel.Builder));
                }
                return _openGtfCommand;
            }
        }

        private AsyncCommand _openHexCommand;

        public ICommand OpenHexCommand
        {
            get
            {
                if (_openHexCommand == null)
                {
                    _openHexCommand = new AsyncCommand(() =>
                        OpenWithFilter("All files (*.*)|*.*", HexViewModel.Builder));
                }
                return _openHexCommand;
            }
        }

        private AsyncCommand _openComponentCommand;

        public ICommand OpenComponentCommand
        {
            get
            {
                if (_openComponentCommand == null)
                {
                    _openComponentCommand = new AsyncCommand(() =>
                        OpenUIComponent("Par files (*.par)|*.par|All files (*.*)|*.*"));
                }
                return _openComponentCommand;
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

        private AsyncCommand _newFromFolderCommand;

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

        private AsyncCommand _newPatchCommand;

        public ICommand NewPatchCommand
        {
            get
            {
                if (_newPatchCommand == null)
                {
                    _newPatchCommand = new AsyncCommand(() => CreateNewPatch(), () => CanNewPatch());
                }
                return _newPatchCommand;
            }
        }

        public bool CanNewPatch() => FileModel == null;

        private AsyncCommand _replaceSaveFolderCommand;

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

        private AsyncCommand _replaceSaveZipCommand;

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

        private AsyncCommand _createCommuPatchCommand;

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

        private AsyncCommand _convertToGtfCommand;

        public ICommand ConvertToGtfCommand
        {
            get
            {
                if (_convertToGtfCommand == null)
                {
                    _convertToGtfCommand = new AsyncCommand(() => ConvertToGtf());
                }
                return _convertToGtfCommand;
            }
        }

        #endregion Commands

        #region Command Methods

        public async Task OpenWithFilter(string filter, FileModelFactory.FileModelBuilder fileModelBuilder)
        {
            string fileName = _getFileName.OpenGetFileName("Open", filter);
            if (fileName != null)
            {
                if (FileModel != null)
                    Close();
                await Open(fileName, fileModelBuilder);
            }
        }

        private async Task Open(string inPath, FileModelFactory.FileModelBuilder fileModelBuilder)
        {
            try
            {
                ClearStatus();
                FileModel = await Task.Run(() => FileModelFactory.CreateFileModel(inPath, fileModelBuilder));
            }
            catch (Exception ex)
            {
                ReportException(ex);
                FileModel = null;
            }
        }

        public async Task OpenWithFilter(string filter)
        {
            string fileName = _getFileName.OpenGetFileName("Open", filter);
            if (fileName != null)
            {
                if (FileModel != null)
                    Close();
                await Open(fileName);
            }
        }

        public async Task Open(string inPath)
        {
            try
            {
                ClearStatus();
                FileModel = await Task.Run(() => FileModelFactory.CreateFileModel(inPath));
            }
            catch (Exception ex)
            {
                ReportException(ex);
                FileModel = null;
            }
        }

        public async Task OpenUIComponent(string filter)
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.OpenGetFileName("Open", filter);
                if (fileName != null)
                {
                    if (FileModel != null)
                        Close();
                    ReportMessage($"Opening {fileName}...");
                    FileModel = await UIComponentModel.CreateComponentModel(FileModelFactory.report, new FileStream(fileName, FileMode.Open), fileName, _getFileName);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
                FileModel = null;
            }
        }

        public void Close()
        {
            ClearStatus();
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
                    string outFileName = _getFileName.SaveGetFileName("Save As", inFileName, "Arc file (*.arc)|*.arc|Arc dat file (*.arc.dat)|*.arc.dat");
                    if (outFileName != null)
                    {
                        await ArcFile.BuildFromDirectory(inFileName, outFileName, progress);
                        ReportMessage("Done.");
                        await Open(outFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
        }

        public async Task CreateNewPatch()
        {
            try
            {
                ClearStatus();
                string outFileName = _getFileName.SaveGetFileName("Save As", "patch", "Zip file (*.zip)|*.zip");
                if (outFileName != null)
                {
                    await Open(outFileName);
                    ReportMessage("Done.");
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
                string outFileName = _getFileName.SaveGetFileName("Save As", inFileName, "Arc file (*.arc)|*.arc|Arc dat file (*.arc.dat)|*.arc.dat");
                if (outFileName == null) return;
                if (fileOpened)
                    Close();
                if (fromZip)
                {
                    await ArcFile.PatchArcFromZip(
                        inFileName,
                        replacementName,
                        outFileName,
                        progress);
                }
                else
                {
                    await ArcFile.PatchArcFromFolder(
                        inFileName,
                        replacementName,
                        outFileName,
                        progress);
                }
                ReportMessage("Done.");
                await Open(outFileName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
        }

        public async Task CreateCommuPatch()
        {
        }

        public async Task ConvertToGtf()
        {
            try
            {
                ClearStatus();
                string imagePath, gtfPath;
                int type;
                var names = Dialogs.GetConvertToGTFData();
                if (names.HasValue)
                {
                    Close();
                    (imagePath, gtfPath, type) = names.Value;
                    using (FileStream gtfStream = new FileStream(gtfPath, FileMode.Create, FileAccess.Write))
                    {
                        using Bitmap bitmap = new Bitmap(imagePath);
                        await GTF.WriteGTF(gtfStream, bitmap, type);
                    }
                    await Open(gtfPath);
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion Command Methods

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

        private string duoProgressLine1 = "";
        private string duoProgressLine2 = "";

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

        #endregion Progress
    }
}
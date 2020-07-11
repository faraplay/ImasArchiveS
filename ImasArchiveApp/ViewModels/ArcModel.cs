using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class ArcModel : ModelWithReport, INotifyPropertyChanged, IContainerFileModel
    {
        #region Fields
        private string _arcPath;
        private ArcFile _arcFile;
        private BrowserModel _browserModel;
        private IFileModel _fileModel;
        private readonly IGetFileName _getFileName;
        private bool disposed = false;
        #endregion
        #region Properties
        public string FileName { get; }
        public string ArcPath
        {
            get => _arcPath;
            set
            {
                _arcPath = value;
                OnPropertyChanged();
            }
        }
        public string CurrentFile
        {
            get => FileModel.FileName;
        }
        public ArcFile ArcFile
        {
            get => _arcFile;
            set
            {
                _arcFile = value;
                OnPropertyChanged();
            }
        }
        public BrowserModel BrowserModel
        {
            get => _browserModel;
            set
            {
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
        public ArcModel(ModelWithReport parent, string inPath, IGetFileName getFileName)
        {
            _getFileName = getFileName;
            FileName = inPath.Substring(inPath.LastIndexOf('\\') + 1);
            _arcPath = inPath;
            ClearStatus = parent.ClearStatus;
            ReportProgress = parent.ReportProgress;
            ReportMessage = parent.ReportMessage;
            ReportException = parent.ReportException;
            _fileModel = null;
            OpenArc();
            List<string> browserEntries = new List<string>();
            foreach (ArcEntry entry in ArcFile.Entries)
            {
                browserEntries.Add(entry.Filepath);
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
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileModel?.Dispose();
                BrowserModel?.Dispose();
                _arcFile?.Dispose();
            }
            disposed = true;
        }
        ~ArcModel() => Dispose(false);
        #endregion
        #region Commands
        AsyncCommand _importCommand;
        public ICommand ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new AsyncCommand(() => Import(), () => CanImport());
                }
                return _importCommand;
            }
        }
        public bool CanImport() => _arcFile != null && FileModel != null;
        AsyncCommand _exportCommand;
        public ICommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new AsyncCommand(() => Export(), () => CanExport());
                }
                return _exportCommand;
            }
        }
        public bool CanExport() => _arcFile != null && FileModel != null;
        AsyncCommand _saveAsCommand;
        public ICommand SaveAsCommand
        {
            get
            {
                if (_saveAsCommand == null)
                {
                    _saveAsCommand = new AsyncCommand(() => SaveAs(), () => CanSaveAs());
                }
                return _saveAsCommand;
            }
        }
        public bool CanSaveAs() => _arcFile != null;
        AsyncCommand _extractAllCommand;
        public ICommand ExtractAllCommand
        {
            get
            {
                if (_extractAllCommand == null)
                {
                    _extractAllCommand = new AsyncCommand(() => ExtractAll(), () => CanExtractAll());
                }
                return _extractAllCommand;
            }
        }
        public bool CanExtractAll() => _arcFile != null;
        AsyncCommand _extractCommusCommand;
        public ICommand ExtractCommusCommand
        {
            get
            {
                if (_extractCommusCommand == null)
                {
                    _extractCommusCommand = new AsyncCommand(() => ExtractCommus(), () => CanExtractCommus());
                }
                return _extractCommusCommand;
            }
        }
        public bool CanExtractCommus() => _arcFile != null;
        AsyncCommand _replaceCommusCommand;
        public ICommand ReplaceCommusCommand
        {
            get
            {
                if (_replaceCommusCommand == null)
                {
                    _replaceCommusCommand = new AsyncCommand(() => ReplaceCommus(), () => CanReplaceCommus());
                }
                return _replaceCommusCommand;
            }
        }
        public bool CanReplaceCommus() => _arcFile != null;
        AsyncCommand _patchFontCommand;
        public ICommand PatchFontCommand
        {
            get
            {
                if (_patchFontCommand == null)
                {
                    _patchFontCommand = new AsyncCommand(() => PatchFont(), () => CanPatchFont());
                }
                return _patchFontCommand;
            }
        }
        public bool CanPatchFont() => _arcFile != null;
        #endregion
        #region Command Methods
        public async Task Import()
        {
            string fileName;
            ArcEntry arcEntry;
            try
            {
                ClearStatus();
                fileName = _getFileName.OpenGetFileName("Import", "All files (*.*)|*.*");
                if (fileName != null)
                {
                    if (!File.Exists(fileName))
                    {
                        ReportMessage("Selected file not found.");
                        return;
                    }
                    arcEntry = ArcFile.GetEntry(CurrentFile);
                    if (arcEntry == null)
                        throw new ArgumentNullException("Could not find current file in archive.");
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
            try
            {
                using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                ReportMessage("Importing...");
                await arcEntry.Replace(fileStream);
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
                arcEntry.RevertToOriginal();
            }
            finally
            {
                LoadChildFileModel(CurrentFile);
            }
        }
        public async Task Export()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Export",
                    ArcPath.Substring(0, ArcPath.LastIndexOf('\\')),
                    CurrentFile.Substring(CurrentFile.LastIndexOf('/') + 1),
                    "");
                if (fileName != null)
                {
                    ArcEntry arcEntry = ArcFile.GetEntry(CurrentFile);
                    if (arcEntry == null)
                        throw new ArgumentNullException("Could not find current file in archive.");
                    using Stream stream = arcEntry.Open();
                    using FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    ReportMessage("Exporting...");
                    await stream.CopyToAsync(fileStream);
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        public async Task SaveAs()
        {
            try
            {
                ClearStatus(); 
                string fileName = _getFileName.SaveGetFileName("Save As", "", "Arc file (*.arc)|*.arc");
                if (fileName != null)
                {
                    await ArcFile.SaveAs(fileName[0..^4], new Progress<ProgressData>(ReportProgress));
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        public async Task ExtractAll()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Extract to...", RemoveArcExtension(ArcPath), "");
                if (fileName != null)
                {
                    await ArcFile.ExtractAllAsync(fileName, new Progress<ProgressData>(ReportProgress));
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        public async Task ExtractCommus()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.SaveGetFileName("Choose folder", RemoveArcExtension(ArcPath) + "commu", "");
                if (fileName != null)
                {
                    await ArcFile.ExtractCommusDir(fileName, new Progress<ProgressData>(ReportProgress));
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        public async Task ReplaceCommus()
        {
            try
            {
                ClearStatus();
                string fileName = _getFileName.OpenGetFolderName("Choose folder");
                if (fileName != null)
                {
                    await ArcFile.ReplaceCommusDir(fileName, new Progress<ProgressData>(ReportProgress));
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        public async Task PatchFont()
        {
            try
            {
                ClearStatus();
                ArcEntry fontEntry = ArcFile.GetEntry("im2nx_font.par");
                if (fontEntry == null)
                {
                    throw new FileNotFoundException("File im2nx_font.par not found. Make sure you are opening disc.arc");
                }
                using (Font font = new Font())
                {
                    using (Stream parStream = fontEntry.Open())
                    {
                        ReportMessage("Reading im2nx_font.par...");
                        await Task.Run(() => font.ReadFontPar(parStream));
                    }
                    ReportMessage("Patching font...");
                    await Task.Run(() => font.AddDigraphs());

                    ReportMessage("Writing font as par...");
                    using MemoryStream memStream = new MemoryStream();
                    await font.WriteFontPar(memStream, false);
                    memStream.Position = 0;
                    await fontEntry.Replace(memStream);
                }
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        #endregion
        #region Other Methods

        private void OpenArc()
        {
            try
            {
                ClearStatus();
                string truncFilename = _arcPath;
                string extension;
                if (truncFilename.EndsWith(".arc"))
                {
                    truncFilename = truncFilename.Remove(truncFilename.Length - 4);
                    extension = "";
                }
                else if (truncFilename.EndsWith(".arc.dat"))
                {
                    truncFilename = truncFilename.Remove(truncFilename.Length - 8);
                    extension = ".dat";
                }
                else
                {
                    throw new ArgumentException("Selected file does not have .arc or .arc.dat extension.");
                }
                ArcFile = new ArcFile(truncFilename, extension);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void LoadChildFileModel(string path)
        {
            if (ArcFile != null && path != null)
            {
                ArcEntry arcEntry = ArcFile.GetEntry(path);
                if (arcEntry != null)
                {
                    ClearStatus();
                    try
                    {
                        FileModel = FileModelFactory.CreateFileModel(arcEntry.Open(), path);
                    }
                    catch (Exception ex)
                    {
                        ReportException(ex);
                        FileModel = null;
                    }
                }
            } 
            else
            {
                FileModel = null;
            }

        }

        public static string RemoveArcExtension(string name)
        {
            if (name.EndsWith(".arc"))
            {
                return name[0..^4];
            }
            else if (name.EndsWith(".arc.dat"))
            {
                return name[0..^8];
            }
            else
            {
                return name;
            }
        }
        #endregion
    }
}

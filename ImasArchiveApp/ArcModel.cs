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
        private string _current_file;
        private ArcFile _arc_file;
        private BrowserModel _browser_model;
        private IFileModel _currentFileModel;
        private string _inPath;
        private string _outPath;
        private bool disposed = false;
        #endregion
        #region Properties
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
            get => _current_file;
            set
            {
                _current_file = value;
                LoadChildFileModel(_current_file);
                OnPropertyChanged();
            }
        }
        public ArcFile ArcFile
        {
            get => _arc_file;
            set
            {
                _arc_file = value;
                OnPropertyChanged();
            }
        }
        public BrowserModel BrowserModel
        {
            get => _browser_model;
            set
            {
                _browser_model = value;
                OnPropertyChanged();
            }
        }
        public IFileModel CurrentFileModel
        {
            get => _currentFileModel;
            set
            {
                _currentFileModel?.Dispose();
                _currentFileModel = value;
                OnPropertyChanged();
            }
        }
        public string InPath
        {
            get => _inPath;
            set
            {
                _inPath = value;
                OnPropertyChanged();
            }
        }
        public string OutPath
        {
            get => _outPath;
            set
            {
                _outPath = value;
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
        public ArcModel(ModelWithReport parent, string inPath)
        {
            _arcPath = inPath;
            ClearStatus = parent.ClearStatus;
            ReportProgress = parent.ReportProgress;
            ReportMessage = parent.ReportMessage;
            ReportException = parent.ReportException;
            _currentFileModel = null;
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
                _currentFileModel?.Dispose();
                BrowserModel?.Dispose();
                _arc_file?.Dispose();
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
        public bool CanImport() => _arc_file != null && !string.IsNullOrEmpty(_current_file);
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
        public bool CanExport() => _arc_file != null && !string.IsNullOrEmpty(_current_file);
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
        public bool CanSaveAs() => _arc_file != null;
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
        public bool CanExtractAll() => _arc_file != null;
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
        public bool CanNewFromFolder() => _arc_file == null;
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
        public bool CanExtractCommus() => _arc_file != null;
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
        public bool CanReplaceCommus() => _arc_file != null;
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
        public bool CanPatchFont() => _arc_file != null;
        #endregion
        #region Command Methods
        public async Task Import()
        {
            ArcEntry arcEntry;
            try
            {
                ClearStatus();
                if (_inPath == null)
                    return;
                if (!File.Exists(_inPath))
                {
                    ReportMessage("Selected file not found.");
                    return;
                }
                arcEntry = _arc_file.GetEntry(_current_file);
                if (arcEntry == null)
                    throw new ArgumentNullException("Could not find current file in archive.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
            try
            {
                using FileStream fileStream = new FileStream(_inPath, FileMode.Open, FileAccess.Read);
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
                LoadChildFileModel(_current_file);
            }
        }
        public async Task Export()
        {
            try
            {
                ClearStatus();
                ArcEntry arcEntry = _arc_file.GetEntry(_current_file);
                if (arcEntry == null)
                    throw new ArgumentNullException("Could not find current file in archive.");
                using Stream stream = arcEntry.Open();
                using FileStream fileStream = new FileStream(_outPath, FileMode.Create, FileAccess.Write);
                ReportMessage("Exporting...");
                await stream.CopyToAsync(fileStream);
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
                try
                {
                    if (File.Exists(_outPath))
                        File.Delete(_outPath);
                }
                catch { }
            }
        }
        public async Task SaveAs()
        {
            try
            {
                ClearStatus();
                await ArcFile.SaveAs(OutPath[0..^4], new Progress<ProgressData>(ReportProgress));
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
                try
                {
                    if (File.Exists(OutPath[0..^4] + ".arc"))
                        File.Delete(OutPath[0..^4] + ".arc");
                }
                catch { }
                try
                {
                    if (File.Exists(OutPath[0..^4] + ".bin"))
                        File.Delete(OutPath[0..^4] + ".bin");
                }
                catch { }
            }
        }
        public async Task ExtractAll()
        {
            try
            {
                ClearStatus();
                await ArcFile.ExtractAllAsync(OutPath, new Progress<ProgressData>(ReportProgress));
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        public async Task CreateNewFromFolder()
        {
            try
            {
                ClearStatus();
                await ArcFile.BuildFromDirectory(InPath, OutPath[0..^4], new Progress<ProgressData>(ReportProgress));
                ReportMessage("Done.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
                return;
            }
            _inPath = _outPath;
            OpenArc();
        }
        public async Task ExtractCommus()
        {
            try
            {
                ClearStatus();
                await ArcFile.ExtractCommusDir(_outPath, new Progress<ProgressData>(ReportProgress));
                ReportMessage("Done.");
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
                await ArcFile.ReplaceCommusDir(_inPath, new Progress<ProgressData>(ReportProgress));
                ReportMessage("Done.");
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
                    CurrentFileModel = FileModelFactory.CreateFileModel(arcEntry.Open(), path);
                }
            } 
            else
            {
                CurrentFileModel = null;
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

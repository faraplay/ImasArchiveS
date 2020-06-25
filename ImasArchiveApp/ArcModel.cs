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
    class ArcModel : INotifyPropertyChanged
    {
        #region Fields
        private string _arcPath;
        private string _current_file;
        private ArcFile _arc_file;
        private BrowserTree _root;
        private FileBrowserModel _browser_model;
        private HexViewModel _hexViewModel;
        private string _progressMessage;
        private string _inPath;
        private string _outPath;
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
                LoadToHex(_current_file);
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
        public BrowserTree Root 
        { 
            get => _root;
            set
            {
                _root = value;
                OnPropertyChanged();
            }
        }
        public FileBrowserModel BrowserModel
        {
            get => _browser_model;
            set
            {
                _browser_model = value;
                OnPropertyChanged();
            }
        }
        public HexViewModel HexViewModel
        {
            get => _hexViewModel;
            set
            {
                _hexViewModel = value;
                OnPropertyChanged();
            }
        }
        public string ProgressMessage
        {
            get => _progressMessage;
            set
            {
                _progressMessage = value;
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
        public ArcModel()
        {
            BrowserModel = new FileBrowserModel(this);
            HexViewModel = new HexViewModel();
        }
        #endregion
        #region Commands
        RelayCommand _openArcCommand;
        public ICommand OpenArcCommand
        {
            get
            {
                if (_openArcCommand == null)
                {
                    _openArcCommand = new RelayCommand(
                        param => OpenArc());
                }
                return _openArcCommand;
            }
        }
        public void OpenArc()
        {
            _arcPath = _inPath;
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
                throw new Exception();
            }
            ArcFile = new ArcFile(truncFilename, extension);
            List<string> browserEntries = new List<string>();
            foreach (ArcEntry entry in ArcFile.Entries)
            {
                browserEntries.Add(entry.Filepath);
            }
            Root = new BrowserTree("", browserEntries);
            _browser_model.HomeDir = Root;
        }
        RelayCommand _closeArcCommand;
        public ICommand CloseArcCommand
        {
            get
            {
                if (_closeArcCommand == null)
                {
                    _closeArcCommand = new RelayCommand(
                        param => CloseArc(),
                        param => CanCloseArc
                        );
                }
                return _closeArcCommand;
            }
        }
        public bool CanCloseArc => _arc_file != null;
        public void CloseArc()
        {
            CurrentFile = null;
            Root = null;
            BrowserModel.UseTree(null);
            _arc_file?.Dispose();
            ArcFile = null;
            ArcPath = null;
        }
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
        public async Task Import()
        {
            ArcEntry arcEntry = _arc_file.GetEntry(_current_file);
            using FileStream fileStream = new FileStream(_inPath, FileMode.Open, FileAccess.Read);
            ProgressMessage = "Importing...";
            await arcEntry.Replace(fileStream);
            ProgressMessage = "Done.";
            LoadToHex(_current_file);
        }
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
        public async Task Export()
        {
            ArcEntry arcEntry = _arc_file.GetEntry(_current_file);
            using Stream stream = arcEntry.Open();
            using FileStream fileStream = new FileStream(_outPath, FileMode.Create, FileAccess.Write);
            ProgressMessage = "Exporting...";
            await stream.CopyToAsync(fileStream);
            ProgressMessage = "Done.";
        }
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
        public async Task SaveAs()
        {
            await ArcFile.SaveAs(OutPath[0..^4], new Progress<ProgressData>(ReportProgress));
            ProgressMessage = "Done.";
        }
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
        public async Task ExtractAll()
        {
            await ArcFile.ExtractAllAsync(OutPath, new Progress<ProgressData>(ReportProgress));
            ProgressMessage = "Done.";
        }
        AsyncCommand _newFromFolderCommand;
        public ICommand NewFromFolderCommand
        {
            get
            {
                if (_newFromFolderCommand == null)
                {
                    _newFromFolderCommand = new AsyncCommand(() => NewFromFolder(), () => CanNewFromFolder());
                }
                return _newFromFolderCommand;
            }
        }
        public bool CanNewFromFolder() => _arc_file == null;
        public async Task NewFromFolder()
        {
            await ArcFile.BuildFromDirectory(InPath, OutPath[0..^4], new Progress<ProgressData>(ReportProgress));
            ProgressMessage = "Done.";
            _inPath = _outPath;
            OpenArc();
        }
        #endregion
        #region Methods
        private void LoadToHex(string path)
        {
            if (ArcFile != null && path != null)
            {
                ArcEntry arcEntry = ArcFile.GetEntry(path);
                if (arcEntry != null)
                {
                    HexViewModel.Stream = arcEntry.Open();
                }
            } 
            else
            {
                HexViewModel.Stream = null;
            }

        }

        private void ReportProgress(ProgressData data)
        {
            ProgressMessage = string.Format("{0} of {1}: {2}", data.count, data.total, data.filename);
        }
        #endregion
    }
}

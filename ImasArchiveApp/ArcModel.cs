using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
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
        #endregion
        #region Methods
        private void LoadToHex(string path)
        {
            if (ArcFile != null && path != null)
            {
                string entryPath = path.Substring(1);
                ArcEntry arcEntry = ArcFile.GetEntry(entryPath);
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
        #endregion
    }

    public class RelayCommand : ICommand
    {
        #region Fields 
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;
        #endregion // Fields 
        #region Constructors 
        public RelayCommand(Action<object> execute) : this(execute, null) { }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute; _canExecute = canExecute;
        }
        #endregion // Constructors 
        #region ICommand Members 
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter) { _execute(parameter); }
        #endregion // ICommand Members 
    }
}

using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class ArcModel : INotifyPropertyChanged
    {
        private string _current_file;
        private ArcFile _arc_file;
        private BrowserTree _root;
        private List<string> _browser_entries = new List<string>();

        private readonly FileBrowserModel _browser_model = new FileBrowserModel();

        public string CurrentFile
        {
            get => _current_file;
            set
            {
                _current_file = value;
                OnPropertyChanged(nameof(CurrentFile));
            }
        }

        public ArcFile ArcFile
        {
            get => _arc_file;
            set
            {
                _arc_file = value;
                OnPropertyChanged(nameof(ArcFile));
            }
        }

        public BrowserTree Root 
        { 
            get => _root;
            set
            {
                _root = value;
                OnPropertyChanged(nameof(Root));
            }
        }

        public List<string> BrowserEntries
        {
            get => _browser_entries;
            set
            {
                _browser_entries = value;
                OnPropertyChanged(nameof(BrowserEntries));
            }
        }

        internal FileBrowserModel BrowserModel => _browser_model;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OpenArc(string arcPath)
        {

            string truncFilename = arcPath;
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
            BrowserEntries.Clear();
            foreach (ArcEntry entry in ArcFile.Entries)
            {
                BrowserEntries.Add(entry.Filepath);
            }
            Root = new BrowserTree("", BrowserEntries);
        }
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

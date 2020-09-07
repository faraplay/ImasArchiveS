using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class BrowserModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly ContainerFileModel _parentModel;
        private BrowserTree _currentDir;
        private readonly List<BrowserTree> _history;
        private int _history_index = 0;
        private string _selectedFile;

        #endregion Fields

        #region Properties

        public BrowserTree CurrentDir
        {
            get => _currentDir;
            set
            {
                _currentDir = value;
                UpdateItems();
                OnPropertyChanged();
            }
        }

        public int HistoryIndex
        {
            get => _history_index;
            set
            {
                if (value >= 0 && value < _history.Count)
                {
                    _history_index = value;
                    CurrentDir = _history[_history_index];
                }
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BrowserItemModel> Items { get; }

        public string SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        #region Constructors

        public BrowserModel(ContainerFileModel parentModel, BrowserTree browserTree)
        {
            if (browserTree == null)
                throw new ArgumentNullException(nameof(browserTree));
            _parentModel = parentModel;
            _history = new List<BrowserTree>();
            _history_index = 0;
            _history.Add(browserTree);
            _currentDir = browserTree;
            Items = new ObservableCollection<BrowserItemModel>();
            UpdateItems();
        }

        #endregion Constructors

        #region Commands

        private RelayCommand _browseBackCommand;

        public ICommand BrowseBackCommand
        {
            get
            {
                if (_browseBackCommand == null)
                {
                    _browseBackCommand = new RelayCommand(
                        param => this.BrowseBack(),
                        param => this.CanBrowseBack);
                }
                return _browseBackCommand;
            }
        }

        bool CanBrowseBack => _history_index - 1 >= 0 && _history_index - 1 < _history.Count;
        private RelayCommand _browseForwardCommand;

        public ICommand BrowseForwardCommand
        {
            get
            {
                if (_browseForwardCommand == null)
                {
                    _browseForwardCommand = new RelayCommand(
                        param => this.BrowseForward(),
                        param => this.CanBrowseForward);
                }
                return _browseForwardCommand;
            }
        }

        bool CanBrowseForward => _history_index + 1 >= 0 && _history_index + 1 < _history.Count;
        private RelayCommand _goUpCommand;

        public ICommand GoUpCommand
        {
            get
            {
                if (_goUpCommand == null)
                {
                    _goUpCommand = new RelayCommand(
                        param => GoUp(),
                        param => CanGoUp);
                }
                return _goUpCommand;
            }
        }

        bool CanGoUp => _currentDir?.Parent != null;

        #endregion Commands

        #region Command Methods

        private void BrowseBack()
        {
            HistoryIndex--;
        }

        private void BrowseForward()
        {
            HistoryIndex++;
        }

        private void GoUp()
        {
            if (_currentDir?.Parent != null)
                MoveToTree(_currentDir.Parent);
        }

        #endregion Command Methods

        #region Other Methods

        internal void UpdateItems()
        {
            Items.Clear();
            if (_currentDir != null)
            {
                foreach (BrowserTree tree in _currentDir.Entries)
                {
                    Items.Add(BrowserItemModel.CreateBrowserItemModel(this, tree));
                }
            }
        }

        internal void MoveToTree(BrowserTree tree)
        {
            if (_history_index + 1 < _history.Count)
            {
                _history.RemoveRange(_history_index + 1, _history.Count - (_history_index + 1));
            }
            _history_index++;
            _history.Add(tree);
            CurrentDir = tree;
        }

        internal async Task LoadSelectedFile(string selectedFile)
        {
            await _parentModel.LoadChildFileModel(selectedFile);
        }

        internal Task Import(string fileName) => _parentModel.Import(fileName);

        internal Task Export(string fileName) => _parentModel.Export(fileName);

        #endregion Other Methods
    }
}
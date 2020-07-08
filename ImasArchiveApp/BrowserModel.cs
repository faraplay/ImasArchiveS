using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class BrowserModel : INotifyPropertyChanged, IFileModel
    {
        #region Fields
        private readonly ArcModel _parentModel;
        private BrowserTree _home_dir;
        private BrowserTree _current_dir;
        private readonly List<BrowserTree> _history = new List<BrowserTree>();
        private int _history_index = 0;
        private ObservableCollection<BrowserItemModel> _items;
        private string _selectedFile;
        private bool disposed = false;
        #endregion
        #region Properties
        public BrowserTree HomeDir
        {
            get => _home_dir;
            set
            {
                _home_dir = value;
                _history.Clear();
                _history_index = 0;
                _history.Add(value);
                OnPropertyChanged();
                CurrentDir = value;
            }
        }

        public BrowserTree CurrentDir
        {
            get => _current_dir; 
            set
            {
                _current_dir = value;
                UpdateItems();
                OnPropertyChanged();
            }
        }

        public List<BrowserTree> History => _history;

        public int HistoryIndex
        {
            get => _history_index;
            set
            {
                if (value >= 0 && value < _history.Count)
                {
                    _history_index = value;
                    CurrentDir = History[_history_index];
                }
                OnPropertyChanged();
            }
        }
        public ObservableCollection<BrowserItemModel> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<BrowserItemModel>();
                }
                return _items;
            }
        }
        public string SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                _parentModel.CurrentFile = value;
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
        public BrowserModel(ArcModel parentModel)
        {
            _parentModel = parentModel;
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

            }
            disposed = true;
        }
        ~BrowserModel() => Dispose(false);
        #endregion
        #region Commands
        RelayCommand _browseBackCommand;
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
        RelayCommand _browseForwardCommand;
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
        RelayCommand _goUpCommand;
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
        bool CanGoUp => _current_dir?.Parent != null;
        #endregion
        #region Command Methods
        void BrowseBack()
        {
            HistoryIndex--;
        }

        void BrowseForward()
        {
            HistoryIndex++;
        }

        void GoUp()
        {
            if (_current_dir?.Parent != null)
                MoveToTree(_current_dir.Parent);
        }
        #endregion
        #region Other Methods
        public void UseTree(BrowserTree tree)
        {
            _history.Clear();
            _history_index = 0;
            _history.Add(tree);
            CurrentDir = tree;
        }

        internal void UpdateItems()
        {
            Items.Clear();
            if (_current_dir != null)
            {
                foreach (BrowserTree tree in _current_dir.Entries)
                {
                    Items.Add(new BrowserItemModel(this, tree));
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
        #endregion
    }
}

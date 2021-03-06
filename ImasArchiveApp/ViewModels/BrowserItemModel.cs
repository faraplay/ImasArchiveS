﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class BrowserItemModel : INotifyPropertyChanged
    {
        protected readonly BrowserModel _parent;
        protected readonly BrowserTree _tree;
        private string _name;

        #region Constructors & Factory Methods

        protected BrowserItemModel(BrowserModel parent, BrowserTree tree)
        {
            _parent = parent;
            _tree = tree;
            _name = tree.Name;
        }

        internal static BrowserItemModel CreateBrowserItemModel(BrowserModel parent, BrowserTree tree)
        {
            return tree.Type switch
            {
                BrowserEntryType.Directory => new BrowserFolderItemModel(parent, tree),
                BrowserEntryType.RegularFile => new BrowserFileItemModel(parent, tree),
                _ => throw new IndexOutOfRangeException("Type not in enum."),
            };
        }

        #endregion Constructors & Factory Methods

        #region Properties

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string FullName => _tree.ToString().Substring(1);

        #endregion Properties

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }

    internal class BrowserFileItemModel : BrowserItemModel
    {
        internal BrowserFileItemModel(BrowserModel parent, BrowserTree tree) : base(parent, tree)
        {
        }

        #region Commands

        private AsyncCommand _selectCommand;

        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new AsyncCommand(
                        () => Select());
                }
                return _selectCommand;
            }
        }

        private AsyncCommand _importCommand;

        public ICommand ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new AsyncCommand(
                        () => Import());
                }
                return _importCommand;
            }
        }

        private AsyncCommand _exportCommand;

        public ICommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new AsyncCommand(
                        () => Export());
                }
                return _exportCommand;
            }
        }

        #endregion Commands

        #region Command Methods

        private async Task Select()
        {
            _parent.SelectedFile = FullName;
            await _parent.LoadSelectedFile(FullName);
        }

        private Task Import() => _parent.Import(FullName);

        private Task Export() => _parent.Export(FullName);

        #endregion Command Methods
    }

    internal class BrowserFolderItemModel : BrowserItemModel
    {
        internal BrowserFolderItemModel(BrowserModel parent, BrowserTree tree) : base(parent, tree)
        {
        }

        #region Commands

        private RelayCommand _selectCommand;

        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(
                        _ => Select());
                }
                return _selectCommand;
            }
        }

        #endregion Commands

        #region Command Methods

        private void Select()
        {
            _parent.MoveToTree(_tree);
        }

        #endregion Command Methods
    }
}
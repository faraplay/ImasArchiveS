using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class BrowserItemModel : INotifyPropertyChanged
    {
        private readonly BrowserModel _parent;
        private readonly BrowserTree _tree;
        private string _name;
        private BrowserEntryType _type;

        internal BrowserItemModel(BrowserModel parent, BrowserTree tree)
        {
            _parent = parent;
            _tree = tree;
            _name = tree.Name;
            _type = tree.Type;
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public BrowserEntryType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        RelayCommand _selectCommand;
        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(
                        param => Select());
                }
                return _selectCommand;
            }
        }
        void Select()
        {
            switch (_type)
            {
                case BrowserEntryType.Directory:
                    _parent.MoveToTree(_tree);
                    break;
                case BrowserEntryType.RegularFile:
                    _parent.SelectedFile = _tree.ToString().Substring(1);
                    break;
            }
        }
    }
}

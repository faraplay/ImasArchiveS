using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImasArchiveApp
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : UserControl
    {
        private FileBrowserModel model = new FileBrowserModel();
        private BrowserTree _current_dir;
        private readonly List<BrowserTree> _history = new List<BrowserTree>();
        private int _history_index = 0;

        static FileBrowser()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(FileBrowser), 
                new CommandBinding(NavigationCommands.BrowseBack, Back_Executed, Back_CanExecute)
                );
            CommandManager.RegisterClassCommandBinding(
                typeof(FileBrowser),
                new CommandBinding(NavigationCommands.BrowseForward, Forward_Executed, Forward_CanExecute)
                );
            CommandManager.RegisterClassCommandBinding(
                typeof(FileBrowser),
                new CommandBinding(CustomFileBrowserCommands.Up, Up_Executed, Up_CanExecute)
                );
        }

        public FileBrowser()
        {
            InitializeComponent();
            Binding myBinding = new Binding("CurrentDir");
            myBinding.Source = model;
            SetBinding(DataContextProperty, myBinding);
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BrowserTree tempTree = (BrowserTree)((ListViewItem)sender).DataContext;
            switch (tempTree.Type)
            {
                case BrowserEntryType.Directory:
                    model.MoveToTree(tempTree);
                    break;
                case BrowserEntryType.RegularFile:
                    RaiseFileSelectedEvent(tempTree);
                    break;
            }
        }

        public void UseTree(BrowserTree tree)
        {
            model.HomeDir = tree;
        }

        private static void Back_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
                e.CanExecute = (fb._history_index > 0 && fb._history_index - 1 < fb._history.Count);
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private static void Back_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
            {
                fb.model.HistoryIndex--;
            }
        }

        private static void Forward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
                e.CanExecute = (fb._history_index + 1 < fb._history.Count);
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private static void Forward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
            {
                fb.model.HistoryIndex++;
            }
        }

        private static void Up_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
                e.CanExecute = (fb._current_dir?.IsRoot == false);
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private static void Up_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
                fb.model.MoveUp();
        }


        public static RoutedEvent FileSelectedEvent = EventManager.RegisterRoutedEvent(
            "FileSelected",
            RoutingStrategy.Bubble,
            typeof(FileSelectedEventHandler),
            typeof(FileBrowser)
            );

        public event EventHandler<FileSelectedRoutedEventArgs> FileSelected
        {
            add { AddHandler(FileSelectedEvent, value); }
            remove { RemoveHandler(FileSelectedEvent, value); }
        }

        void RaiseFileSelectedEvent(BrowserTree file)
        {
            FileSelectedRoutedEventArgs routedEventArgs = new FileSelectedRoutedEventArgs(FileSelectedEvent);
            routedEventArgs.File = file;
            RaiseEvent(routedEventArgs);
        }
    }

    class FileBrowserModel : INotifyPropertyChanged
    {
        private BrowserTree _home_dir;
        private BrowserTree _current_dir;
        private readonly List<BrowserTree> _history = new List<BrowserTree>();
        private int _history_index = 0;

        public BrowserTree HomeDir
        {
            get => _home_dir;
            set
            {
                _home_dir = value;
                _history.Clear();
                _history_index = 0;
                _history.Add(value);
                _current_dir = value;
                OnPropertyChanged(nameof(HomeDir));
                OnPropertyChanged(nameof(CurrentDir));
            }
        }

        public BrowserTree CurrentDir
        {
            get => _current_dir; 
            set
            {
                _current_dir = value;
                OnPropertyChanged(nameof(CurrentDir));
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
                OnPropertyChanged(nameof(HistoryIndex));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UseTree(BrowserTree tree)
        {
            _history.Clear();
            _history_index = 0;
            _history.Add(tree);
            CurrentDir = tree;
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

        internal void MoveUp()
        {
            if (_current_dir.Parent != null)
                MoveToTree(_current_dir.Parent);
        }
    }

    public static class CustomFileBrowserCommands
    {
        public static readonly RoutedUICommand Up = new RoutedUICommand(
            "Up",
            "Up",
            typeof(FileBrowser),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Up, ModifierKeys.Alt)
            }
            );
    }

    public delegate void FileSelectedEventHandler(object sender, FileSelectedRoutedEventArgs e);

    public class FileSelectedRoutedEventArgs : RoutedEventArgs
    {
        public BrowserTree File { get; set; }
        public FileSelectedRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        private BrowserTree _tree;
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
        }

        public void UpdateFileBrowser()
        {
            DataContext = _tree;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BrowserTree tempTree = (BrowserTree)((ListViewItem)sender).DataContext;
            if (tempTree.Type == BrowserEntryType.Directory)
                MoveToTree(tempTree);
        }

        public void UseTree(BrowserTree tree)
        {
            _history.Clear();
            _history_index = 0;
            _tree = tree;
            _history.Add(_tree);
            UpdateFileBrowser();
        }

        private void MoveToTree(BrowserTree tree)
        {
            _tree = tree;
            if (_history_index + 1 < _history.Count)
            {
                _history.RemoveRange(_history_index + 1, _history.Count - (_history_index + 1));
            }
            _history.Add(_tree);
            _history_index++;
            UpdateFileBrowser();
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
                fb._history_index--;
                fb._tree = fb._history[fb._history_index];
                fb.UpdateFileBrowser();
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
                fb._history_index++;
                fb._tree = fb._history[fb._history_index];
                fb.UpdateFileBrowser();
            }
        }

        private static void Up_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
                e.CanExecute = (fb._tree?.IsRoot == false);
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private static void Up_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is FileBrowser fb)
                fb.MoveToTree(fb._tree.Parent);
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
}

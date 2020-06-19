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
        private FileBrowserModel model = new FileBrowserModel();

        public FileBrowser()
        {
            InitializeComponent();
            //Binding myBinding = new Binding("CurrentDir");
            //myBinding.Source = model;
            //SetBinding(DataContextProperty, myBinding);
            DataContext = model;
        }

        public void UseTree(BrowserTree tree)
        {
            model.HomeDir = tree;
        }
    }

    public delegate void FileSelectedEventHandler(object sender, FileSelectedRoutedEventArgs e);

    public class FileSelectedRoutedEventArgs : RoutedEventArgs
    {
        public BrowserTree File { get; set; }
        public FileSelectedRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    }
}

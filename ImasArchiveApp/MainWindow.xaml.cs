using ImasArchiveLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ArcFile arcFile;
        BrowserTree root;
        List<string> browserEntryStrings = new List<string>();

        FileStream fileStream;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (fileStream != null)
            {
                CloseStream();
            }
            OpenStream();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (fileStream != null);
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseStream();
        }

        private void OpenStream()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arc files (*.arc;*.arc.dat)|*.arc;*.arc.dat";
            if (openFileDialog.ShowDialog() == true)
            {
                string truncFilename = openFileDialog.FileName;
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
                using (ArcFile tempArcFile = new ArcFile(truncFilename, extension))
                {
                    browserEntryStrings.Clear();
                    foreach (ArcEntry entry in tempArcFile.Entries)
                    {
                        browserEntryStrings.Add(entry.Filepath);
                    }
                    root = new BrowserTree("", browserEntryStrings);
                    fbBrowser.UseTree(root);
                }
                fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                hxViewer.Stream = fileStream;
            }
        }

        private void CloseStream()
        {
            hxViewer.Stream = null;
            fileStream.Dispose();
            fileStream = null;
        }

    }
}

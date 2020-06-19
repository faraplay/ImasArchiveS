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
        ArcModel arcModel = new ArcModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = arcModel;
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseHexViewerStream();
            OpenArcDialog();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (hxViewer.Stream != null);
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseHexViewerStream();
        }

        private void OpenArcDialog()
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
                arcModel.ArcFile = new ArcFile(truncFilename, extension);
                arcModel.BrowserEntries.Clear();
                foreach (ArcEntry entry in arcModel.ArcFile.Entries)
                {
                    arcModel.BrowserEntries.Add(entry.Filepath);
                }
                arcModel.Root = new BrowserTree("", arcModel.BrowserEntries);
                fbBrowser.UseTree(arcModel.Root);
            }
        }

        private void OpenHexViewerStream(Stream stream)
        {
            if (stream != null)
            {
                CloseHexViewerStream();
                hxViewer.Stream = stream;
            }
        }

        private void CloseHexViewerStream()
        {
            hxViewer.Stream?.Dispose();
            hxViewer.Stream = null;
        }

        private void fbBrowser_FileSelected(object sender, FileSelectedRoutedEventArgs e)
        {
            if (arcModel.ArcFile != null)
            {
                string entryPath = e.File.ToString();
                // remove starting slash
                entryPath = entryPath.Substring(1);
                ArcEntry arcEntry = arcModel.ArcFile.GetEntry(entryPath);
                if (arcEntry != null)
                {
                    OpenHexViewerStream(arcEntry.Open());
                }
            }
        }
    }
}

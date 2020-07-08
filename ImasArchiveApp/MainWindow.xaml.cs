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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImasArchiveApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((DataContext as MainWindowModel).CloseCommand.CanExecute(null))
                (DataContext as MainWindowModel).CloseCommand.Execute(null);
            if (OpenDialog("Open archive", "Arc files (*.arc;*.arc.dat)|*.arc;*.arc.dat"))
                (DataContext as MainWindowModel).OpenCommand.Execute(null);
        }


        //private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (DataContext as ArcModel).SaveAsCommand.CanExecute(null);
        //}

        //private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (SaveDialog("Save As", "", "Arc file (*.arc)|*.arc"))
        //        (DataContext as ArcModel).SaveAsCommand.Execute(null);
        //}
        //private void Import_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (DataContext as ArcModel).ImportCommand.CanExecute(null);
        //}

        //private void Import_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (OpenDialog("Import", "All files (*.*)|*.*"))
        //        (DataContext as ArcModel).ImportCommand.Execute(null);
        //}
        //private void Export_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (DataContext as ArcModel).ExportCommand.CanExecute(null);
        //}

        //private void Export_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    string arcpath = (DataContext as ArcModel).ArcPath;
        //    string currentFile = (DataContext as ArcModel).CurrentFile;
        //    if (SaveDialog("Export", 
        //        arcpath.Substring(0, arcpath.LastIndexOf('\\')), 
        //        currentFile.Substring(currentFile.LastIndexOf('/') + 1), 
        //        ""))
        //        (DataContext as ArcModel).ExportCommand.Execute(null);
        //}
        //private void ExtractAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (DataContext as ArcModel).ExtractAllCommand.CanExecute(null);
        //}

        //private void ExtractAll_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    string arcpath = (DataContext as ArcModel).ArcPath;
        //    if (SaveDialog("Extract to...", ArcModel.RemoveArcExtension(arcpath), ""))
        //        (DataContext as ArcModel).ExtractAllCommand.Execute(null);
        //}
        //private void NewFromFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (DataContext as ArcModel).NewFromFolderCommand.CanExecute(null);
        //}

        //private void NewFromFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (OpenFolderDialog("Choose folder")) {
        //        string chosenFolder = (DataContext as ArcModel).InPath;
        //        if (SaveDialog("Save As", chosenFolder, "Arc file (*.arc)|*.arc"))
        //            (DataContext as ArcModel).NewFromFolderCommand.Execute(null);
        //    }
        //}

        //private void ExtractCommus_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (DataContext as ArcModel).ExtractCommusCommand.CanExecute(null);
        //}

        //private void ExtractCommus_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    string arcpath = (DataContext as ArcModel).ArcPath;
        //    if (SaveDialog("Choose folder", ArcModel.RemoveArcExtension(arcpath) + "commu", ""))
        //        (DataContext as ArcModel).ExtractCommusCommand.Execute(null);
        //}

        //private void ReplaceCommus_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (DataContext as ArcModel).ReplaceCommusCommand.CanExecute(null);
        //}

        //private void ReplaceCommus_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (OpenFolderDialog("Choose folder"))
        //        (DataContext as ArcModel).ReplaceCommusCommand.Execute(null);
        //}



        private bool OpenDialog(string title, string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };
            bool fileSelected = (openFileDialog.ShowDialog() == true);
            if (fileSelected)
            {
                (DataContext as MainWindowModel).InPath = openFileDialog.FileName;
            }
            return fileSelected;
        }
        private bool SaveDialog(string title, string defaultPath, string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter
            };
            if (defaultPath != null && defaultPath.Contains('\\'))
            {
                saveFileDialog.FileName = defaultPath.Substring(defaultPath.LastIndexOf('\\') + 1);
                saveFileDialog.InitialDirectory = defaultPath.Substring(0, defaultPath.LastIndexOf('\\'));
            }
            bool fileSelected = (saveFileDialog.ShowDialog() == true);
            if (fileSelected)
            {
                (DataContext as ArcModel).OutPath = saveFileDialog.FileName;
            }
            return fileSelected;
        }
        private bool SaveDialog(string title, string defaultDir, string defaultName, string filter)
        {
            return SaveDialog(title, defaultDir + '\\' + defaultName, filter);
        }
        private bool OpenFolderDialog(string title)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = title,
                IsFolderPicker = true
            };
            bool fileSelected = dialog.ShowDialog() == CommonFileDialogResult.Ok;
            if (fileSelected)
            {
                (DataContext as ArcModel).InPath = dialog.FileName;
            }
            return fileSelected;
        }
    }
}

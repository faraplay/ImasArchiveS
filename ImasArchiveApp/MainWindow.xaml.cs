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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((DataContext as ArcModel).CloseArcCommand.CanExecute(null))
                (DataContext as ArcModel).CloseArcCommand.Execute(null);
            if (OpenArcDialog())
                (DataContext as ArcModel).OpenArcCommand.Execute(null);
        }

        private bool OpenArcDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arc files (*.arc;*.arc.dat)|*.arc;*.arc.dat";
            bool fileSelected = (openFileDialog.ShowDialog() == true);
            if (fileSelected)
            {
                (DataContext as ArcModel).ArcPath = openFileDialog.FileName;
            }
            return fileSelected;
        }
    }
}

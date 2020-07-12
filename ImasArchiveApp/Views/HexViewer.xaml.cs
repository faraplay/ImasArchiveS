using System;
using System.Collections.Generic;
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
    /// Interaction logic for HexViewer.xaml
    /// </summary>
    public partial class HexViewer : UserControl
    {
        public HexViewer()
        {
            InitializeComponent();
        }

        private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetLineCount();
        }

        private void DataTextBlock_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is HexViewModel)
                SetLineCount();
        }
        private void SetLineCount()
        {
            double lineHeight = tbData.LineHeight;
            if (double.IsNaN(lineHeight))
                lineHeight = tbData.FontSize * tbData.FontFamily.LineSpacing;
            int lineCount = (int)(ActualHeight / lineHeight);
            (DataContext as HexViewModel).LineCount = lineCount;
        }
    }
}

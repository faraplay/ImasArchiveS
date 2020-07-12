using System.Windows;
using System.Windows.Controls;

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

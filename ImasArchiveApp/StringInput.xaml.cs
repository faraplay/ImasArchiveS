using System.Windows;

namespace ImasArchiveApp
{
    /// <summary>
    /// Interaction logic for StringInput.xaml
    /// </summary>
    public partial class StringInput : Window
    {
        public string Filename => txtFileName.Text;

        public StringInput()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
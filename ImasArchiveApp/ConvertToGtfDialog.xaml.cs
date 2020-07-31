using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImasArchiveApp
{
    /// <summary>
    /// Interaction logic for ConvertToGtfDialog.xaml
    /// </summary>
    public partial class ConvertToGtfDialog : Window
    {
        public string ImagePath => txtImageName.Text;
        public string GtfPath => txtGtfName.Text;
        public int Type => types[cmbType.SelectedIndex];

        private readonly string[] descs =
        {
            "1: 8-bit indexed color. Used for tutorial panels, icons, and backgrounds with transparency.",
            "2: 16-bit color (1555). Only used in the baton hand accessory.",
            "3: 16-bit color (4444). Only used in some textures in the 13th live stage, and the sakura petals in The World Is All One.",
            "5: 32-bit color (ARGB). Used in lots of textures in 3D models.",
            "6: 4x4 blocks, no transparency. Used in 2D backgrounds, effect and 3D model textures.",
            "7: 4x4 blocks, 4-bit transparency support. Used in 3D model textures.",
            "8: 4x4 blocks, variable-range transparency support. Used in effect and 3D model textures."
        };
        private readonly int[] types = { 1, 2, 3, 5, 6, 7, 8 };
        private void UpdateDesc()
        {
            txtTypeDesc.Text = descs[cmbType.SelectedIndex];
        }
        private void UpdateOKEnabled()
        {
            btnOK.IsEnabled =
                !string.IsNullOrWhiteSpace(txtImageName.Text) &&
                !string.IsNullOrWhiteSpace(txtGtfName.Text) &&
                cmbType.SelectedIndex >= 0;
        }
        public ConvertToGtfDialog()
        {
            InitializeComponent();
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() 
                { Filter = "Image files(*.png;*.jpg;*.bmp)|*.png;*.jpg;*.png" };
            if (openFileDialog.ShowDialog() == true)
            {
                txtImageName.Text = openFileDialog.FileName;
            }
            UpdateOKEnabled();
        }
        private void BtnSelectGtf_Click(object sender, RoutedEventArgs e)
        {
            string imageNameNoExtend = "";
            if (ImagePath != null && ImagePath.Contains('.'))
            {
                imageNameNoExtend = ImagePath.Substring(ImagePath.LastIndexOf('\\') + 1);
                imageNameNoExtend = imageNameNoExtend.Remove(imageNameNoExtend.LastIndexOf('.'));
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            { Filter = "GTF file(*.gtf)|*.gtf", FileName = imageNameNoExtend };
            if (saveFileDialog.ShowDialog() == true)
            {
                txtGtfName.Text = saveFileDialog.FileName;
            }
            UpdateOKEnabled();
        }
        private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDesc();
            UpdateOKEnabled();
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}

using ImasArchiveLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    class GTFModel : IFileModel
    {
        private ImageSource _imageSource;
        private MemoryStream ms;

        private bool disposed = false;

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Constructors
        public GTFModel(Stream stream)
        {
            try
            {
                ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                using Bitmap bitmap = GTF.ReadGTF(ms);
                ms.SetLength(0);
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();

                ImageSource = image;
            }
            catch
            {
                Dispose();
                throw;
            }
        }
        #endregion
        #region IDisposable
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                ms?.Dispose();
            }
            disposed = true;
        }
        ~GTFModel() => Dispose(false);
        #endregion
    }
}

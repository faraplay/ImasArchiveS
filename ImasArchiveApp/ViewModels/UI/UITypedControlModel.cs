using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imas.UI;

namespace ImasArchiveApp
{
    class UITypedControlModel<T> : UIControlModel where T : Control
    {
        protected readonly T _control;
        private readonly MemoryStream ms;
        private ImageSource _imageSource;
        public override Control Control => _control;
        public override ImageSource ImageSource 
        { 
            get => _imageSource; 
            set 
            { 
                _imageSource = value; 
                OnPropertyChanged(); 
            } 
        }

        public UITypedControlModel(UISubcomponentModel parent, T control) : base(parent, control.ToString())
        {
            _control = control;
            ms = new MemoryStream();
        }

        protected override void LoadImage()
        {
            ms.SetLength(0);
            using (Bitmap bitmap = _control.GetBitmap())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            }
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            ImageSource = image;
        }

        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                ms?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable
    }
}

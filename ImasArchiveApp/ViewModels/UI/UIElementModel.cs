using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public abstract class UIElementModel : FileModel
    {
        protected readonly UISubcomponentModel parent;
        private readonly MemoryStream ms;
        private ImageSource _imageSource;

        public ObservableCollection<UIElementModel> Children { get; set; }
        protected abstract UIElement UIElement { get; }
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }
        public abstract string ModelName { get; }

        protected UIElementModel(UISubcomponentModel parent, string name) : base(parent, name)
        {
            this.parent = parent;
            Children = new ObservableCollection<UIElementModel>();
            ms = new MemoryStream();
        }

        private RelayCommand _selectCommand;

        public virtual ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(
                        _ => {
                            LoadImage();
                            parent.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }

        protected void LoadImage()
        {
            ms.SetLength(0);
            using (Bitmap bitmap = UIElement.GetBitmap())
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

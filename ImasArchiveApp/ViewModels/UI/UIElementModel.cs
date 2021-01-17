using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public abstract class UIElementModel : FileModel
    {
        protected readonly UISubcomponentModel subcomponent;
        protected readonly UIElementModel parent;
        private readonly MemoryStream ms;
        private ImageSource _imageSource;

        private bool? cacheVisible;
        public virtual bool? Visible
        {
            get => cacheVisible;
            set
            {
                if (value != cacheVisible)
                {
                    cacheVisible = value;
                    if (value == null) // being set by a child
                    {
                        cacheVisible = null;
                        if (parent != null) 
                            parent.Visible = null;
                    }
                    else
                    {
                        foreach (var child in Children)
                        {
                            child.Visible = value;
                        }
                        if (parent != null)
                        {
                            if (parent.Children.All(model => model.Visible == value))
                            {
                                parent.Visible = value;
                            }
                            else
                            {
                                parent.Visible = null;
                            }
                        }
                    }
                    OnPropertyChanged(nameof(BindVisible));
                }
            }
        }
        public bool VisibleIsNull => Visible == null;
        public bool? BindVisible
        {
            get => Visible;
            set
            {
                Visible = value;
                LoadActiveImage();
                OnPropertyChanged();
            }
        }

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

        protected UIElementModel(UISubcomponentModel subcomponent, UIElementModel parent, string name) : base(subcomponent, name)
        {
            this.subcomponent = subcomponent;
            this.parent = parent;
            cacheVisible = true;
            Children = new ObservableCollection<UIElementModel>();
            ms = new MemoryStream();
        }

        private RelayCommand _selectCommand;

        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(
                        _ => {
                            LoadImage();
                            subcomponent.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }

        protected void LoadActiveImage() => subcomponent.SelectedModel?.LoadImage();

        public virtual void LoadImage()
        {
            try
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
            catch (Exception ex)
            {
                ReportException(ex);
            }
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

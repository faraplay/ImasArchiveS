﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public abstract class UIModel : FileModel
    {
        protected readonly UISubcomponentModel subcomponent;
        private readonly MemoryStream ms;
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        protected UIModel(UISubcomponentModel subcomponent, string name) : base(subcomponent, name)
        {
            this.subcomponent = subcomponent;
            ms = new MemoryStream();
        }

        private RelayCommand _displayCommand;

        public ICommand DisplayCommand
        {
            get
            {
                if (_displayCommand == null)
                {
                    _displayCommand = new RelayCommand(
                        _ => {
                            subcomponent.DisplayedModel = this;
                        });
                }
                return _displayCommand;
            }
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
                            subcomponent.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }

        private AsyncCommand _saveImageCommand;

        public ICommand SaveImageCommand
        {
            get
            {
                if (_saveImageCommand == null)
                {
                    _saveImageCommand = new AsyncCommand(
                        () => SaveImage());
                }
                return _saveImageCommand;
            }
        }

        protected abstract Bitmap GetBitmap();

        protected void LoadActiveImages()
        {
            subcomponent.DisplayedModel?.LoadImage();
            subcomponent.SelectedModel?.LoadImage();
        }

        public virtual void LoadImage()
        {
            try
            {
                ms.SetLength(0);
                using (Bitmap bitmap = GetBitmap())
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

        private async Task SaveImage()
        {
            try
            {
                ClearStatus();
                string imgName = subcomponent.GetFileName.SaveGetFileName("Save Image", "", "Portable Network Graphic (*.png)|*.png");
                if (imgName != null)
                {
                    ReportMessage("Saving image");
                    ms.Seek(0, SeekOrigin.Begin);
                    using (FileStream outStream = new FileStream(imgName, FileMode.Create, FileAccess.Write))
                    {
                        await ms.CopyToAsync(outStream);
                    }
                    ReportMessage("Done.");
                }
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
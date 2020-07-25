using Imas;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    class GTFModel : FileModel
    {
        private ImageSource _imageSource;
        private readonly MemoryStream ms;
        private readonly IGetFileName getFileName;

        #region Properties
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructors
        public GTFModel(IReport report, string fileName, IGetFileName getFileName, Stream stream) : base(report, fileName)
        {
            try
            {
                this.getFileName = getFileName;
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
            catch (Exception ex)
            {
                Dispose();
                throw new InvalidDataException("Could not read file as GTF. Original exception:\n" + ex.ToString());
            }
        }
        #endregion
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
        #endregion
        #region Commands
        private AsyncCommand _savePngCommand;
        public IAsyncCommand SavePngCommand
        {
            get
            {
                if (_savePngCommand == null)
                {
                    _savePngCommand = new AsyncCommand(() => SavePng());
                }
                return _savePngCommand;
            }
        }
        #endregion
        #region Command Methods
        public async Task SavePng()
        {
            try
            {
                ClearStatus();
                string name = FileName.Substring(FileName.LastIndexOf('/') + 1);
                string nameNoExtension = name.Contains('.') ? name.Substring(0, name.LastIndexOf('.')) : name;
                string pngName = getFileName.SaveGetFileName("Save As Png", "", nameNoExtension, "PNG file (*.png)|*.png");

                using FileStream fileStream = new FileStream(pngName, FileMode.Create, FileAccess.Write);
                ms.Position = 0;
                await ms.CopyToAsync(fileStream);
                ReportMessage("Saved to " + pngName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }
        #endregion
    }
}

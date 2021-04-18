using Imas;
using Imas.Gtf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public class GTFModel : FileModel
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

        #endregion Properties

        #region Constructors

        public GTFModel(IReport report, string fileName, IGetFileName getFileName, Stream stream) : base(report, fileName)
        {
            try
            {
                this.getFileName = getFileName;
                ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                using GTF gtf = GTF.CreateFromGtfStream(ms);
                ms.SetLength(0);
                gtf.SavePngTo(ms);
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

        internal static FileModelFactory.FileModelBuilder Builder { get; set; } =
            (report, filename, getFilename, stream) => new GTFModel(report, filename, getFilename, stream);

        #endregion Constructors

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

        #endregion Commands

        #region Command Methods

        public async Task SavePng()
        {
            try
            {
                ClearStatus();
                string name = FileName.Substring(FileName.LastIndexOf('/') + 1);
                string nameNoExtension = name.Contains('.') ? name.Substring(0, name.LastIndexOf('.')) : name;
                string pngName = getFileName.SaveGetFileName("Save As Png", "", nameNoExtension, "PNG file (*.png)|*.png");

                if (pngName != null)
                {
                    using FileStream fileStream = new FileStream(pngName, FileMode.Create, FileAccess.Write);
                    ms.Position = 0;
                    await ms.CopyToAsync(fileStream);
                    ReportMessage("Saved to " + pngName);
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion Command Methods
    }
}
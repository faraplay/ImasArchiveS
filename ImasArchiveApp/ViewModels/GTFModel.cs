using Imas;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public class GTFModel : RenderableModel
    {
        private readonly GTF gtf;
        private ImageSource _imageSource;
        private readonly IGetFileName getFileName;


        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }


        public GTFModel(IReport report, string fileName, IGetFileName getFileName, Stream stream) : base(report, fileName, getFileName)
        {
            try
            {
                this.getFileName = getFileName;
                gtf = GTF.ReadGTF(stream);
                _imageSource = BitmapSource.Create(
                    gtf.Width,
                    gtf.Height,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null,
                    gtf.BitmapPtr,
                    4 * gtf.Stride * gtf.Height,
                    4 * gtf.Stride);
                _imageSource.Freeze();
            }
            catch (Exception ex)
            {
                Dispose();
                throw new InvalidDataException("Could not read file as GTF. Original exception:\n" + ex.ToString());
            }
        }

        internal static FileModelFactory.FileModelBuilder Builder { get; set; } =
            (report, filename, getFilename, stream) => new GTFModel(report, filename, getFilename, stream);


        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                gtf?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable

        public override int BoundingPixelWidth => gtf.Width;
        public override int BoundingPixelHeight => gtf.Height;
        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            drawingContext.DrawImage(
                ImageSource,
                new Rect(new Size(ImageSource.Width, ImageSource.Height))
                );
        }
    }
}
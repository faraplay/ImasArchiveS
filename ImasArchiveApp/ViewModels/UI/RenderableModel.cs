using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public abstract class RenderableModel : FileModel
    {
        private readonly IGetFileName getFileName;
        protected RenderableModel(IReport parent, string fileName, IGetFileName getFileName) : base(parent, fileName)
        {
            this.getFileName = getFileName;
        }

        private RelayCommand _saveImageCommand;

        public ICommand SaveImageCommand
        {
            get
            {
                if (_saveImageCommand == null)
                {
                    _saveImageCommand = new RelayCommand(
                        _ => SaveImage());
                }
                return _saveImageCommand;
            }
        }

        public abstract int BoundingPixelWidth { get; }
        public abstract int BoundingPixelHeight { get; }
        public void RenderElement(DrawingContext drawingContext) => RenderElement(drawingContext, ColorMultiplier.One(), true);
        internal abstract void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop);

        private void SaveImage()
        {
            try
            {
                ClearStatus();
                string imgName = getFileName.SaveGetFileName("Save Image", "", "Portable Network Graphic (*.png)|*.png");
                if (imgName != null)
                {
                    ReportMessage("Saving image");
                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();
                    RenderElement(drawingContext);
                    drawingContext.Close();
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(BoundingPixelWidth, BoundingPixelHeight, 96, 96, PixelFormats.Default);
                    renderTargetBitmap.Render(drawingVisual);
                    BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
                    bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    using (FileStream outStream = new FileStream(imgName, FileMode.Create, FileAccess.Write))
                    {
                        bitmapEncoder.Save(outStream);
                    }
                    ReportMessage("Done.");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

        }
    }
}
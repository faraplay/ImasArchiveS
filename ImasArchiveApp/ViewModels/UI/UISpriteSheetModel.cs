using Imas;
using Imas.Gtf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public class UISpriteSheetModel : UIModel
    {
        private readonly int index;
        private readonly IGetFileName _getfileName;
        //public Bitmap bitmap;

        public List<UISpriteModel> Sprites { get; }

        private Dictionary<Rect, UISpriteSheetRectangleModel> rectLookup;

        public ObservableCollection<UISpriteSheetRectangleModel> Rectangles { get; }

        public UISpriteSheetModel(UISubcomponentModel subcomponent, string name, int index, IGetFileName getFileName) : base(subcomponent, name)
        {
            this.index = index;
            GTF gtf = subcomponent.GetSpritesheet(index);
            colorGtfs = new ColorChannelGTF(gtf);
            _bitmapSource = BitmapSource.Create(
                gtf.Width,
                gtf.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                gtf.BitmapDataPtr,
                4 * gtf.Stride * gtf.Height,
                4 * gtf.Stride);
            _bitmapSource.Freeze();
            _getfileName = getFileName;
            Sprites = new List<UISpriteModel>();
            Rectangles = new ObservableCollection<UISpriteSheetRectangleModel>();
        }


        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                colorGtfs.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable

        public void UpdateRectangles()
        {
            rectLookup = new Dictionary<Rect, UISpriteSheetRectangleModel>();
            Rectangles.Clear();
            foreach (UISpriteModel sprite in Sprites)
            {
                Rect rectangle = new Rect(
                    new System.Windows.Point(sprite.SourceX, sprite.SourceY),
                    new Vector(sprite.SourceWidth, sprite.SourceHeight)
                    );
                if (rectLookup.TryGetValue(rectangle, out var model))
                {
                    model.Sprites.Add(sprite);
                }
                else
                {
                    model = new UISpriteSheetRectangleModel(subcomponent, this, rectangle);
                    model.Sprites.Add(sprite);
                    Rectangles.Add(model);
                    rectLookup.Add(rectangle, model);
                }
                sprite.SpriteSheetRectangleModel = model;
            }
        }

        private AsyncCommand _replaceImageCommand;

        public ICommand ReplaceImageCommand
        {
            get
            {
                if (_replaceImageCommand == null)
                {
                    _replaceImageCommand = new AsyncCommand(
                        () => ReplaceImage());
                }
                return _replaceImageCommand;
            }
        }

        private async Task ReplaceImage()
        {
            await subcomponent.ReplaceImage(index);
            GTF gtf = subcomponent.GetSpritesheet(index);
            colorGtfs = new ColorChannelGTF(gtf);
            _bitmapSource = BitmapSource.Create(
                gtf.Width,
                gtf.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                gtf.BitmapDataPtr,
                4 * gtf.Stride * gtf.Height,
                4 * gtf.Stride);
        }

        internal ColorChannelGTF colorGtfs;

        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource => _bitmapSource;

        private BitmapSource whiteBitmap;
        public BitmapSource WhiteBitmap
        {
            get
            {
                if (whiteBitmap == null)
                {
                    whiteBitmap = BitmapSource.Create(
                        colorGtfs.Gtf.Width,
                        colorGtfs.Gtf.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        colorGtfs.PtrWhite,
                        4 * colorGtfs.Gtf.Stride * colorGtfs.Gtf.Height,
                        4 * colorGtfs.Gtf.Stride);
                    whiteBitmap.Freeze();
                }
                return whiteBitmap;
            }
        }
        private BitmapSource yellowBitmap;
        public BitmapSource YellowBitmap
        {
            get
            {
                if (yellowBitmap == null)
                {
                    yellowBitmap = BitmapSource.Create(
                        colorGtfs.Gtf.Width,
                        colorGtfs.Gtf.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        colorGtfs.PtrYellow,
                        4 * colorGtfs.Gtf.Stride * colorGtfs.Gtf.Height,
                        4 * colorGtfs.Gtf.Stride);
                    yellowBitmap.Freeze();
                }
                return yellowBitmap;
            }
        }
        private BitmapSource magentaBitmap;
        public BitmapSource MagentaBitmap
        {
            get
            {
                if (magentaBitmap == null)
                {
                    magentaBitmap = BitmapSource.Create(
                        colorGtfs.Gtf.Width,
                        colorGtfs.Gtf.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        colorGtfs.PtrMagenta,
                        4 * colorGtfs.Gtf.Stride * colorGtfs.Gtf.Height,
                        4 * colorGtfs.Gtf.Stride);
                    magentaBitmap.Freeze();
                }
                return magentaBitmap;
            }
        }
        private BitmapSource cyanBitmap;
        public BitmapSource CyanBitmap
        {
            get
            {
                if (cyanBitmap == null)
                {
                    cyanBitmap = BitmapSource.Create(
                        colorGtfs.Gtf.Width,
                        colorGtfs.Gtf.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        colorGtfs.PtrCyan,
                        4 * colorGtfs.Gtf.Stride * colorGtfs.Gtf.Height,
                        4 * colorGtfs.Gtf.Stride);
                    cyanBitmap.Freeze();
                }
                return cyanBitmap;
            }
        }
        private BitmapSource redBitmap;
        public BitmapSource RedBitmap
        {
            get
            {
                if (redBitmap == null)
                {
                    redBitmap = BitmapSource.Create(
                        colorGtfs.Gtf.Width,
                        colorGtfs.Gtf.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        colorGtfs.PtrRed,
                        4 * colorGtfs.Gtf.Stride * colorGtfs.Gtf.Height,
                        4 * colorGtfs.Gtf.Stride);
                    redBitmap.Freeze();
                }
                return redBitmap;
            }
        }
        private BitmapSource greenBitmap;
        public BitmapSource GreenBitmap
        {
            get
            {
                if (greenBitmap == null)
                {
                    greenBitmap = BitmapSource.Create(
                        colorGtfs.Gtf.Width,
                        colorGtfs.Gtf.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        colorGtfs.PtrGreen,
                        4 * colorGtfs.Gtf.Stride * colorGtfs.Gtf.Height,
                        4 * colorGtfs.Gtf.Stride);
                    greenBitmap.Freeze();
                }
                return greenBitmap;
            }
        }
        private BitmapSource blueBitmap;
        public BitmapSource BlueBitmap
        {
            get
            {
                if (blueBitmap == null)
                {
                    blueBitmap = BitmapSource.Create(
                        colorGtfs.Gtf.Width,
                        colorGtfs.Gtf.Height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        colorGtfs.PtrBlue,
                        4 * colorGtfs.Gtf.Stride * colorGtfs.Gtf.Height,
                        4 * colorGtfs.Gtf.Stride);
                    blueBitmap.Freeze();
                }
                return blueBitmap;
            }
        }

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            drawingContext.DrawImage(
                BitmapSource,
                new Rect(new System.Windows.Size(BitmapSource.Width, BitmapSource.Height))
                );
        }

        public override int BoundingPixelWidth => BitmapSource.PixelWidth;
        public override int BoundingPixelHeight => BitmapSource.PixelHeight;
    }

}

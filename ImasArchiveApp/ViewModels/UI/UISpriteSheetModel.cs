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
        public Bitmap bitmap;

        public List<UISpriteModel> Sprites { get; }

        private Dictionary<Rect, UISpriteSheetRectangleModel> rectLookup;

        public ObservableCollection<UISpriteSheetRectangleModel> Rectangles { get; }

        public UISpriteSheetModel(UISubcomponentModel subcomponent, string name, int index, IGetFileName getFileName) : base(subcomponent, name)
        {
            this.index = index;
            bitmap = subcomponent.GetSpritesheet(index);
            _getfileName = getFileName;
            Sprites = new List<UISpriteModel>();
            Rectangles = new ObservableCollection<UISpriteSheetRectangleModel>();
        }

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
            bitmap = subcomponent.GetSpritesheet(index);
        }

        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get {
                if (_bitmapSource == null)
                {
                    _bitmapSource = BmpImageFromBmp(bitmap);
                }
                return _bitmapSource;
            }
        }
        private BitmapImage BmpImageFromBmp(Bitmap bmp)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
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

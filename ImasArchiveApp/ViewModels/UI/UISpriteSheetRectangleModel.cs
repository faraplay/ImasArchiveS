using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UISpriteSheetRectangleModel : UIModel
    {
        private readonly UISpriteSheetModel parent;
        private RectangleF rectangle;
        public RectangleF Rectangle => rectangle;
        public ObservableCollection<UISpriteModel> Sprites { get; }

        public string Description
        {
            get => rectangle.ToString();
        }

        #region Properties

        public float X
        {
            get => rectangle.X;
            set
            {
                rectangle.X = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceXQuiet = X;
                }
                LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }
        public float Y
        {
            get => rectangle.Y;
            set
            {
                rectangle.Y = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceYQuiet = Y;
                }
                LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }
        public float Width
        {
            get => rectangle.Width;
            set
            {
                rectangle.Width = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceWidthQuiet = Width;
                }
                LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }
        public float Height
        {
            get => rectangle.Height;
            set
            {
                rectangle.Height = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceHeightQuiet = Height;
                }
                LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }

        #endregion

        public UISpriteSheetRectangleModel(UISubcomponentModel subcomponent, UISpriteSheetModel parent, RectangleF rectangle) : base(subcomponent, rectangle.ToString())
        {
            this.parent = parent;
            this.rectangle = rectangle;
            Sprites = new ObservableCollection<UISpriteModel>();
        }

        protected override Bitmap GetBitmap()
        {
            Bitmap newBitmap = new Bitmap(parent.bitmap.Width + 1, parent.bitmap.Height + 1);
            using Graphics g = Graphics.FromImage(newBitmap);
            g.DrawImage(parent.bitmap, new Point());
            g.DrawRectangle(Pens.Yellow, System.Drawing.Rectangle.Round(rectangle));
            return newBitmap;
        }

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            parent.RenderElement(drawingContext, multiplier, isTop);
            drawingContext.DrawRectangle(null, new System.Windows.Media.Pen(System.Windows.Media.Brushes.Yellow, 1), new System.Windows.Rect(X, Y, Width, Height));
        }
    }
}

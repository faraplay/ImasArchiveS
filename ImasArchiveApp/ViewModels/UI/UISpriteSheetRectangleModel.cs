using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UISpriteSheetRectangleModel : UIModel
    {
        private readonly UISpriteSheetModel parent;
        private Rect rectangle;
        public Rect Rectangle => rectangle;
        public ObservableCollection<UISpriteModel> Sprites { get; }

        public string Description
        {
            get => rectangle.ToString();
        }

        #region Properties

        public float X
        {
            get => (float)rectangle.X;
            set
            {
                rectangle.X = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceXQuiet = X;
                }
                //LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }
        public float Y
        {
            get => (float)rectangle.Y;
            set
            {
                rectangle.Y = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceYQuiet = Y;
                }
                //LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }
        public float Width
        {
            get => (float)rectangle.Width;
            set
            {
                rectangle.Width = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceWidthQuiet = Width;
                }
                //LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }
        public float Height
        {
            get => (float)rectangle.Height;
            set
            {
                rectangle.Height = value;
                foreach (UISpriteModel sprite in Sprites)
                {
                    sprite.SourceHeightQuiet = Height;
                }
                //LoadActiveImages();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Description));
            }
        }

        #endregion

        public UISpriteSheetRectangleModel(UISubcomponentModel subcomponent, UISpriteSheetModel parent, Rect rectangle) 
            : base(subcomponent, rectangle.ToString())
        {
            this.parent = parent;
            this.rectangle = rectangle;
            Sprites = new ObservableCollection<UISpriteModel>();
        }

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            parent.RenderElement(drawingContext, multiplier, isTop);
            drawingContext.DrawRectangle(null, new Pen(Brushes.Yellow, 1), rectangle);
        }

        public override int BoundingPixelWidth => parent.BoundingPixelWidth;
        public override int BoundingPixelHeight => parent.BoundingPixelHeight;
    }
}

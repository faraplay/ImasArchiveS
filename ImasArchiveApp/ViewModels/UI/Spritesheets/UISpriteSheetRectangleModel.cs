using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UISpriteSheetRectangleModel : PtaElementModel
    {
        private readonly UISpriteSheetModel parent;
        private Rect rectangle;
        public Rect Rectangle => rectangle;
        public override object Element => rectangle;
        public ObservableCollection<UISpriteModel> Sprites { get; }

        public override string ElementName => rectangle.ToString();

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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ElementName));
                ForceRender();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ElementName));
                ForceRender();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ElementName));
                ForceRender();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ElementName));
                ForceRender();
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
            parent.RenderElement(drawingContext, multiplier, false);
            drawingContext.DrawRectangle(null, new Pen(Brushes.Yellow, 1), rectangle);
        }

        public override int BoundingPixelWidth => parent.BoundingPixelWidth;
        public override int BoundingPixelHeight => parent.BoundingPixelHeight;
    }
}

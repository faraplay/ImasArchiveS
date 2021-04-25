using Imas.UI;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UISpriteModel : UIElementModel
    {
        private readonly Sprite sprite;
        public override UIElement UIElement => sprite;
        private readonly UISpriteGroupModel parentGroupModel;
        protected override UIElementModel Parent => parentGroupModel;

        public UISpriteSheetModel ParentSheet => (sprite.SrcImageID == -1) ? null : subcomponent.PtaModel.SpriteSheets[sprite.SrcImageID];
        private UISpriteSheetRectangleModel spriteSheetRectangleModel;
        public UISpriteSheetRectangleModel SpriteSheetRectangleModel 
        { 
            get => spriteSheetRectangleModel; 
            set
            {
                spriteSheetRectangleModel = value;
                OnPropertyChanged();
            }
        }
        public override string ModelName => $"({sprite.Width}x{sprite.Height})";
        private int SrcImgWidth => ParentSheet?.BoundingPixelWidth ?? 1;
        private int SrcImgHeight => ParentSheet?.BoundingPixelHeight ?? 1;
        internal float SourceXQuiet
        {
            get => sprite.SrcFracLeft * SrcImgWidth;
            set
            {
                float srcFracWidth = sprite.SrcFracRight - sprite.SrcFracLeft;
                sprite.SrcFracLeft = value / SrcImgWidth;
                sprite.SrcFracRight = sprite.SrcFracLeft + srcFracWidth;
            }
        }
        internal float SourceYQuiet
        {
            get => sprite.SrcFracTop * SrcImgHeight;
            set
            {
                float srcFracHeight = sprite.SrcFracBottom - sprite.SrcFracTop;
                sprite.SrcFracTop = value / SrcImgHeight;
                sprite.SrcFracBottom = sprite.SrcFracTop + srcFracHeight;
            }
        }
        internal float SourceWidthQuiet
        {
            get => (sprite.SrcFracRight - sprite.SrcFracLeft) * SrcImgWidth;
            set
            {
                sprite.SrcFracRight = sprite.SrcFracLeft + (value / SrcImgWidth);
            }
        }
        internal float SourceHeightQuiet
        {
            get => (sprite.SrcFracBottom - sprite.SrcFracTop) * SrcImgHeight;
            set
            {
                sprite.SrcFracBottom = sprite.SrcFracTop + (value / SrcImgHeight);
            }
        }
        //#endregion Quiet
        public float SourceX
        {
            get => SourceXQuiet;
            set
            {
                SourceXQuiet = value;
                ParentSheet.UpdateRectangles();
                InvalidateBrushes();
                OnPropertyChanged();
                PropertyChangedHandler(this, new PropertyChangedEventArgs(nameof(SourceX)));
            }
        }
        public float SourceY
        {
            get => SourceYQuiet;
            set
            {
                SourceYQuiet = value;
                ParentSheet.UpdateRectangles();
                InvalidateBrushes();
                OnPropertyChanged();
                PropertyChangedHandler(this, new PropertyChangedEventArgs(nameof(SourceY)));
            }
        }
        public float SourceWidth
        {
            get => SourceWidthQuiet;
            set
            {
                SourceWidthQuiet = value;
                ParentSheet.UpdateRectangles();
                InvalidateBrushes();
                OnPropertyChanged();
                PropertyChangedHandler(this, new PropertyChangedEventArgs(nameof(SourceWidth)));
            }
        }
        public float SourceHeight
        {
            get => SourceHeightQuiet;
            set
            {
                SourceHeightQuiet = value;
                ParentSheet.UpdateRectangles();
                InvalidateBrushes();
                OnPropertyChanged();
                PropertyChangedHandler(this, new PropertyChangedEventArgs(nameof(SourceHeight)));
            }
        }

        public UISpriteModel(UISubcomponentModel subcomponent, UISpriteGroupModel parent, Sprite sprite) : base(subcomponent, null)
        {
            this.sprite = sprite;
            parentGroupModel = parent;
            if (this.sprite.SrcImageID >= 0)
                ParentSheet.Sprites.Add(this);
        }

        public override void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Sprite.SrcImageID))
            {
                InvalidateBrushes();
            }
            base.PropertyChangedHandler(sender, e);
        }

        private ImageBrush alphaImageBrush;
        public ImageBrush AlphaImageBrush
        {
            get
            {
                if (alphaImageBrush == null)
                    alphaImageBrush = CreateImageBrush(ParentSheet.BitmapSource);
                return alphaImageBrush;
            }
        }
        private ImageBrush whiteImageBrush;
        public ImageBrush WhiteImageBrush
        {
            get
            {
                if (whiteImageBrush == null)
                    whiteImageBrush = CreateImageBrush(ParentSheet.WhiteBitmap);
                return whiteImageBrush;
            }
        }
        private ImageBrush yellowImageBrush;
        public ImageBrush YellowImageBrush
        {
            get
            {
                if (yellowImageBrush == null)
                    yellowImageBrush = CreateImageBrush(ParentSheet.YellowBitmap);
                return yellowImageBrush;
            }
        }
        private ImageBrush magentaImageBrush;
        public ImageBrush MagentaImageBrush
        {
            get
            {
                if (magentaImageBrush == null)
                    magentaImageBrush = CreateImageBrush(ParentSheet.MagentaBitmap);
                return magentaImageBrush;
            }
        }
        private ImageBrush cyanImageBrush;
        public ImageBrush CyanImageBrush
        {
            get
            {
                if (cyanImageBrush == null)
                    cyanImageBrush = CreateImageBrush(ParentSheet.CyanBitmap);
                return cyanImageBrush;
            }
        }
        private ImageBrush redImageBrush;
        public ImageBrush RedImageBrush
        {
            get
            {
                if (redImageBrush == null)
                    redImageBrush = CreateImageBrush(ParentSheet.RedBitmap);
                return redImageBrush;
            }
        }
        private ImageBrush greenImageBrush;
        public ImageBrush GreenImageBrush
        {
            get
            {
                if (greenImageBrush == null)
                    greenImageBrush = CreateImageBrush(ParentSheet.GreenBitmap);
                return greenImageBrush;
            }
        }
        private ImageBrush blueImageBrush;
        public ImageBrush BlueImageBrush
        {
            get
            {
                if (blueImageBrush == null)
                    blueImageBrush = CreateImageBrush(ParentSheet.BlueBitmap);
                return blueImageBrush;
            }
        }

        private bool ImageXIsFlipped => SourceWidthQuiet < 0;
        private bool ImageYIsFlipped => SourceHeightQuiet < 0;
        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            double red = sprite.Red / 255.0 * multiplier.r;
            double green = sprite.Green / 255.0 * multiplier.g;
            double blue = sprite.Blue / 255.0 * multiplier.b;
            drawingContext.PushTransform(new TranslateTransform(sprite.Xpos, sprite.Ypos));
            drawingContext.PushTransform(new ScaleTransform(
                ImageXIsFlipped ? -1 : 1,
                ImageYIsFlipped ? -1 : 1,
                0.5 * sprite.Width,
                0.5 * sprite.Height
                ));
            drawingContext.PushOpacity(sprite.Alpha / 255.0);
            if (sprite.SrcImageID == -1)
            {
                DrawRectangle(drawingContext, CreateColorBrush(red, green, blue));
            }
            else
            {
                DrawImageRectangle(drawingContext, red, green, blue);
            }
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.Pop();
        }

        private void DrawImageRectangle(DrawingContext drawingContext, double red, double green, double blue)
        {
            drawingContext.PushOpacityMask(AlphaImageBrush);
            DrawRectangle(drawingContext, WhiteImageBrush);
            double middle;
            if (red < green && red < blue)
            {
                middle = Math.Min(green, blue);
                drawingContext.PushOpacity((middle - red) / middle);
                DrawRectangle(drawingContext, CyanImageBrush);
                drawingContext.Pop();
            }
            if (green < blue && green < red)
            {
                middle = Math.Min(blue, red);
                drawingContext.PushOpacity((middle - green) / middle);
                DrawRectangle(drawingContext, MagentaImageBrush);
                drawingContext.Pop();
            }
            if (blue < red && blue < green)
            {
                middle = Math.Min(red, green);
                drawingContext.PushOpacity((middle - blue) / middle);
                DrawRectangle(drawingContext, YellowImageBrush);
                drawingContext.Pop();
            }

            if (red > green && red > blue)
            {
                middle = Math.Max(green, blue);
                drawingContext.PushOpacity((red - middle) / red);
                DrawRectangle(drawingContext, RedImageBrush);
                drawingContext.Pop();
            }
            if (green > blue && green > red)
            {
                middle = Math.Max(blue, red);
                drawingContext.PushOpacity((green - middle) / green);
                DrawRectangle(drawingContext, GreenImageBrush);
                drawingContext.Pop();
            }
            if (blue > red && blue > green)
            {
                middle = Math.Max(red, green);
                drawingContext.PushOpacity((blue - middle) / blue);
                DrawRectangle(drawingContext, BlueImageBrush);
                drawingContext.Pop();
            }

            double max = Math.Max(red, Math.Max(green, blue));
            if (max < 1)
            {
                drawingContext.PushOpacity(1 - max);
                DrawRectangle(drawingContext, Brushes.Black);
                drawingContext.Pop();
            }
            drawingContext.Pop();
        }

        private void DrawRectangle(DrawingContext drawingContext, Brush brush)
        {
            drawingContext.DrawRectangle(
                            brush,
                            null,
                            new System.Windows.Rect(
                                new System.Windows.Size(sprite.Width, sprite.Height))
                            );
        }

        private SolidColorBrush CreateColorBrush(double red, double green, double blue)
        {
            return new SolidColorBrush(
                    Color.FromRgb(
                        (byte)(red * 255),
                        (byte)(green * 255),
                        (byte)(blue * 255)
                        ));
        }

        private ImageBrush CreateImageBrush(ImageSource imageSource)
        {
            ImageBrush imageBrush = new ImageBrush(imageSource);
            imageBrush.Viewbox = new System.Windows.Rect(
                new System.Windows.Point(sprite.SrcFracLeft, sprite.SrcFracTop),
                new System.Windows.Point(sprite.SrcFracRight, sprite.SrcFracBottom)
                );
            if (sprite.Start[0] == 1)
            {
                imageBrush.TileMode = TileMode.Tile;
                imageBrush.Viewport = new System.Windows.Rect(
                    new System.Windows.Size(Math.Abs(SourceWidth), Math.Abs(SourceHeight))
                    );
                imageBrush.ViewportUnits = BrushMappingMode.Absolute;
            }
            return imageBrush;
        }

        private void InvalidateBrushes()
        {
            alphaImageBrush = null;
            whiteImageBrush = null;
            yellowImageBrush = null;
            magentaImageBrush = null;
            cyanImageBrush = null;
            redImageBrush = null;
            greenImageBrush = null;
            blueImageBrush = null;
        }

        public void InsertSprite()
        {
            parentGroupModel.InsertSprite(parentGroupModel.Children.IndexOf(this), new Sprite());
        }

        private RelayCommand _insertCommand;
        public ICommand InsertCommand
        {
            get
            {
                if (_insertCommand == null)
                    _insertCommand = new RelayCommand(
                        _ => InsertSprite());
                return _insertCommand;
            }
        }

        public void DeleteSprite()
        {
            parentGroupModel.RemoveSprite(this);
        }
        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(
                        _ => DeleteSprite());
                return _deleteCommand;
            }
        }
    }
}

using Imas.UI;
using System;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UISpriteModel : UIElementModel
    {
        private readonly Sprite _sprite;
        public override UIElement UIElement => _sprite;

        public UISpriteSheetModel ParentSheet => (_sprite.srcImageID == -1) ? null : subcomponent.SpriteSheets[_sprite.srcImageID];
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
        public override string ModelName => $"({_sprite.width}x{_sprite.height})";
        private int SrcImgWidth => ParentSheet?.BoundingPixelWidth ?? 1;
        private int SrcImgHeight => ParentSheet?.BoundingPixelHeight ?? 1;
        internal float SourceXQuiet
        {
            get => _sprite.srcFracLeft * SrcImgWidth;
            set
            {
                float srcFracWidth = _sprite.srcFracRight - _sprite.srcFracLeft;
                _sprite.srcFracLeft = value / SrcImgWidth;
                _sprite.srcFracRight = _sprite.srcFracLeft + srcFracWidth;
            }
        }
        internal float SourceYQuiet
        {
            get => _sprite.srcFracTop * SrcImgHeight;
            set
            {
                float srcFracHeight = _sprite.srcFracBottom - _sprite.srcFracTop;
                _sprite.srcFracTop = value / SrcImgHeight;
                _sprite.srcFracBottom = _sprite.srcFracTop + srcFracHeight;
            }
        }
        internal float SourceWidthQuiet
        {
            get => (_sprite.srcFracRight - _sprite.srcFracLeft) * SrcImgWidth;
            set
            {
                _sprite.srcFracRight = _sprite.srcFracLeft + (value / SrcImgWidth);
            }
        }
        internal float SourceHeightQuiet
        {
            get => (_sprite.srcFracBottom - _sprite.srcFracTop) * SrcImgHeight;
            set
            {
                _sprite.srcFracBottom = _sprite.srcFracTop + (value / SrcImgHeight);
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
            }
        }

        public UISpriteModel(UISubcomponentModel subcomponent, UIElementModel parent, Sprite sprite) : base(subcomponent, parent, "sprite")
        {
            _sprite = sprite;
            if (_sprite.srcImageID >= 0)
                ParentSheet.Sprites.Add(this);
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
            double red = _sprite.red / 255.0 * multiplier.r;
            double green = _sprite.green / 255.0 * multiplier.g;
            double blue = _sprite.blue / 255.0 * multiplier.b;
            drawingContext.PushTransform(new TranslateTransform(_sprite.xpos, _sprite.ypos));
            drawingContext.PushTransform(new ScaleTransform(
                ImageXIsFlipped ? -1 : 1,
                ImageYIsFlipped ? -1 : 1,
                0.5 * _sprite.width,
                0.5 * _sprite.height
                ));
            drawingContext.PushOpacity(_sprite.alpha / 255.0);
            if (_sprite.srcImageID == -1)
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
                                new System.Windows.Size(_sprite.width, _sprite.height))
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
                new System.Windows.Point(_sprite.srcFracLeft, _sprite.srcFracTop),
                new System.Windows.Point(_sprite.srcFracRight, _sprite.srcFracBottom)
                );
            if (_sprite.start[0] == 1)
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
    }
}

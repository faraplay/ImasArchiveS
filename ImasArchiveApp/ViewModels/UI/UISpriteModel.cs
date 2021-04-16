using Imas.UI;
using System;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UISpriteModel : UIElementModel
    {
        private readonly Sprite _sprite;
        protected override UIElement UIElement => _sprite;

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
        private float SrcFracWidth => _sprite.srcFracRight - _sprite.srcFracLeft;
        private float SrcFracHeight => _sprite.srcFracBottom - _sprite.srcFracTop;
        internal float SourceXQuiet
        {
            get => _sprite.srcFracLeft * SrcImgWidth;
            set
            {
                _sprite.srcFracLeft = value / SrcImgWidth;
                _sprite.srcFracRight = _sprite.srcFracLeft + SrcFracWidth;
            }
        }
        internal float SourceYQuiet
        {
            get => _sprite.srcFracTop * SrcImgHeight;
            set
            {
                _sprite.srcFracTop = value / SrcImgHeight;
                _sprite.srcFracBottom = _sprite.srcFracTop + SrcFracHeight;
                OnPropertyChanged();
            }
        }
        internal float SourceWidthQuiet
        {
            get => SrcFracWidth * SrcImgWidth;
            set
            {
                _sprite.srcFracRight = _sprite.srcFracLeft + (value / SrcImgWidth);
                OnPropertyChanged();
            }
        }
        internal float SourceHeightQuiet
        {
            get => SrcFracHeight * SrcImgHeight;
            set
            {
                _sprite.srcFracBottom = _sprite.srcFracTop + (value / SrcImgHeight);
                OnPropertyChanged();
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
                //LoadActiveImages();
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
                //LoadActiveImages();
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
                //LoadActiveImages();
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
                //LoadActiveImages();
                OnPropertyChanged();
            }
        }

        public UISpriteModel(UISubcomponentModel subcomponent, UIElementModel parent, Sprite sprite) : base(subcomponent, parent, "sprite", sprite.myVisible)
        {
            _sprite = sprite;
            if (_sprite.srcImageID >= 0)
                ParentSheet.Sprites.Add(this);
        }

        private SolidColorBrush colorBrush;
        private ImageBrush imageBrush;
        private bool imageXIsFlipped, imageYIsFlipped;
        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            double red = _sprite.red / 255.0 * multiplier.r;
            double green = _sprite.green / 255.0 * multiplier.g;
            double blue = _sprite.blue / 255.0 * multiplier.b;
            if (colorBrush == null)
            {
                InitialiseColorBrush(red, green, blue);
            }
            if (imageBrush == null && _sprite.srcImageID != -1)
            {
                InitialiseImageBrush();
            }
            drawingContext.PushTransform(new TranslateTransform(_sprite.xpos, _sprite.ypos));
            drawingContext.PushTransform(new ScaleTransform(
                imageXIsFlipped ? -1 : 1,
                imageYIsFlipped ? -1 : 1,
                -0.5 * SourceWidth,
                -0.5 * SourceHeight
                ));
            drawingContext.PushOpacity(_sprite.alpha / 255.0);
            if (_sprite.srcImageID == -1)
            {
                DrawRectangle(drawingContext, colorBrush);
            }
            else
            {
                double brightness = (red + green + blue) / 3;
                if (Math.Abs(brightness - 1) < 1 / 255.0)
                {
                    DrawRectangle(drawingContext, imageBrush);
                }
                else
                {
                    drawingContext.PushOpacity(brightness);
                    DrawRectangle(drawingContext, imageBrush);
                    drawingContext.Pop();

                    drawingContext.PushOpacity(1.0 - brightness);
                    drawingContext.PushOpacityMask(imageBrush);
                    DrawRectangle(drawingContext, colorBrush);
                    drawingContext.Pop();
                    drawingContext.Pop();
                }
            }
            drawingContext.Pop();
            drawingContext.Pop();
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

        private void InitialiseColorBrush(double red, double green, double blue)
        {
            colorBrush = new SolidColorBrush(
                    Color.FromRgb(
                        (byte)(red * 255),
                        (byte)(green * 255),
                        (byte)(blue * 255)
                        ));
        }

        private void InitialiseImageBrush()
        {
            imageBrush = new ImageBrush(subcomponent.SpriteSheets[_sprite.srcImageID].BitmapSource);
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
            imageXIsFlipped = (SourceWidth < 0);
            imageYIsFlipped = (SourceHeight < 0);
        }
    }
}

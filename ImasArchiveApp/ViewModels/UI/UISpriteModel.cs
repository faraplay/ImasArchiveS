using Imas.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public class UISpriteModel : UIElementModel
    {
        private readonly Sprite _sprite;
        protected override UIElement UIElement => _sprite;

        private UISpriteSheetModel ParentSheet => subcomponent.SpriteSheets[_sprite.srcImageID];
        public override string ModelName => $"({_sprite.width}x{_sprite.height})";

        #region Properties

        public int Int0
        {
            get => _sprite.start[0];
            set
            {
                _sprite.start[0] = value;
                LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int1
        {
            get => _sprite.start[1];
            set
            {
                _sprite.start[1] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int2
        {
            get => _sprite.start[2];
            set
            {
                _sprite.start[2] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int3
        {
            get => _sprite.start[3];
            set
            {
                _sprite.start[3] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int4
        {
            get => _sprite.start[4];
            set
            {
                _sprite.start[4] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int5
        {
            get => _sprite.start[5];
            set
            {
                _sprite.start[5] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int6
        {
            get => _sprite.start[6];
            set
            {
                _sprite.start[6] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int7
        {
            get => _sprite.start[7];
            set
            {
                _sprite.start[7] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int Int8
        {
            get => _sprite.start[8];
            set
            {
                _sprite.start[8] = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Xpos
        {
            get => _sprite.xpos;
            set
            {
                _sprite.xpos = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Ypos
        {
            get => _sprite.ypos;
            set
            {
                _sprite.ypos = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Width
        {
            get => _sprite.width;
            set
            {
                _sprite.width = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Height
        {
            get => _sprite.height;
            set
            {
                _sprite.height = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int A1
        {
            get => _sprite.a1;
            set
            {
                _sprite.a1 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int A2
        {
            get => _sprite.a2;
            set
            {
                _sprite.a2 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B1
        {
            get => _sprite.b1;
            set
            {
                _sprite.b1 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B2
        {
            get => _sprite.b2;
            set
            {
                _sprite.b2 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B3
        {
            get => _sprite.b3;
            set
            {
                _sprite.b3 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B4
        {
            get => _sprite.b4;
            set
            {
                _sprite.b4 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int SourceImageID
        {
            get => _sprite.srcImageID;
            set
            {
                _sprite.srcImageID = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Alpha
        {
            get => _sprite.alpha;
            set
            {
                _sprite.alpha = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Red
        {
            get => _sprite.red;
            set
            {
                _sprite.red = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Green
        {
            get => _sprite.green;
            set
            {
                _sprite.green = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Blue
        {
            get => _sprite.blue;
            set
            {
                _sprite.blue = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        #region Quiet
        // These update without re-rendering the active image
        internal float SourceXQuiet
        {
            get => _sprite.SourceX;
            set
            {
                _sprite.SourceX = value;
                OnPropertyChanged();
            }
        }
        internal float SourceYQuiet
        {
            get => _sprite.SourceY;
            set
            {
                _sprite.SourceY = value;
                OnPropertyChanged();
            }
        }
        internal float SourceWidthQuiet
        {
            get => _sprite.SourceWidth;
            set
            {
                _sprite.SourceWidth = value;
                OnPropertyChanged();
            }
        }
        internal float SourceHeightQuiet
        {
            get => _sprite.SourceHeight;
            set
            {
                _sprite.SourceHeight = value;
                OnPropertyChanged();
            }
        }
        #endregion Quiet
        public float SourceX
        {
            get => _sprite.SourceX;
            set
            {
                _sprite.SourceX = value;
                ParentSheet.UpdateRectangles();
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float SourceY
        {
            get => _sprite.SourceY;
            set
            {
                _sprite.SourceY = value;
                ParentSheet.UpdateRectangles();
                LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float SourceWidth
        {
            get => _sprite.SourceWidth;
            set
            {
                _sprite.SourceWidth = value;
                ParentSheet.UpdateRectangles();
                LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float SourceHeight
        {
            get => _sprite.SourceHeight;
            set
            {
                _sprite.SourceHeight = value;
                ParentSheet.UpdateRectangles();
                LoadActiveImages();
                OnPropertyChanged();
            }
        }

        #endregion Properties

        public override bool? Visible
        {
            get => base.Visible;
            set
            {
                if (value.HasValue)
                    UIElement.myVisible = value.Value;
                cacheVisible = value;
            }
        }

        #region ImageSource
        private readonly MemoryStream ms;
        private ImageSource _imageSource;
        public ImageSource SpriteSheetImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public UISpriteModel(UISubcomponentModel subcomponent, UIElementModel parent, Sprite sprite) : base(subcomponent, parent, "sprite", sprite.myVisible)
        {
            _sprite = sprite;
            centerRender = _sprite.xpos < 0 || _sprite.ypos < 0;
            ms = new MemoryStream();
            if (_sprite.srcImageID >= 0)
                ParentSheet.Sprites.Add(this);
        }

        public override void LoadImage()
        {
            base.LoadImage();
            LoadSpriteSheetImage();
        }
        protected void LoadSpriteSheetImage()
        {
            ms.SetLength(0);
            using (Bitmap bitmap = _sprite.ShowInSpriteSheet())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            }
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            SpriteSheetImageSource = image;
        }

        private ImageBrush imageBrush;
        public void RenderSprite(DrawingContext drawingContext)
        {
            if (imageBrush == null)
            {
                imageBrush = new ImageBrush(subcomponent.SpriteSheets[_sprite.srcImageID].BitmapSource);
                imageBrush.Viewbox = new System.Windows.Rect(
                    new System.Windows.Point(_sprite.srcFracLeft, _sprite.srcFracTop),
                    new System.Windows.Point(_sprite.srcFracRight, _sprite.srcFracBottom)
                    );
                imageBrush.Stretch = Stretch.Fill;
            }
            drawingContext.DrawRectangle(
                imageBrush, 
                null, 
                new System.Windows.Rect(new System.Windows.Size(_sprite.width, _sprite.height))
                );
        }

        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                ms?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion IDisposable

        #endregion
    }
}

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
    class UISpriteModel : UIElementModel
    {
        private readonly Sprite _sprite;
        protected override UIElement UIElement => _sprite;
        public override string ModelName => $"({_sprite.width}x{_sprite.height})";

        #region Properties

        public int Int0
        {
            get => _sprite.start[0];
            set
            {
                _sprite.start[0] = value;
                LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int1
        {
            get => _sprite.start[1];
            set
            {
                _sprite.start[1] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int2
        {
            get => _sprite.start[2];
            set
            {
                _sprite.start[2] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int3
        {
            get => _sprite.start[3];
            set
            {
                _sprite.start[3] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int4
        {
            get => _sprite.start[4];
            set
            {
                _sprite.start[4] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int5
        {
            get => _sprite.start[5];
            set
            {
                _sprite.start[5] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int6
        {
            get => _sprite.start[6];
            set
            {
                _sprite.start[6] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int7
        {
            get => _sprite.start[7];
            set
            {
                _sprite.start[7] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int Int8
        {
            get => _sprite.start[8];
            set
            {
                _sprite.start[8] = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Xpos
        {
            get => _sprite.xpos;
            set
            {
                _sprite.xpos = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Ypos
        {
            get => _sprite.ypos;
            set
            {
                _sprite.ypos = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Width
        {
            get => _sprite.width;
            set
            {
                _sprite.width = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Height
        {
            get => _sprite.height;
            set
            {
                _sprite.height = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int A1
        {
            get => _sprite.a1;
            set
            {
                _sprite.a1 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int A2
        {
            get => _sprite.a2;
            set
            {
                _sprite.a2 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B1
        {
            get => _sprite.b1;
            set
            {
                _sprite.b1 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B2
        {
            get => _sprite.b2;
            set
            {
                _sprite.b2 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B3
        {
            get => _sprite.b3;
            set
            {
                _sprite.b3 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B4
        {
            get => _sprite.b4;
            set
            {
                _sprite.b4 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int SourceImageID
        {
            get => _sprite.srcImageID;
            set
            {
                _sprite.srcImageID = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Alpha
        {
            get => _sprite.alpha;
            set
            {
                _sprite.alpha = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Red
        {
            get => _sprite.red;
            set
            {
                _sprite.red = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Green
        {
            get => _sprite.green;
            set
            {
                _sprite.green = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Blue
        {
            get => _sprite.blue;
            set
            {
                _sprite.blue = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float SourceX
        {
            get => _sprite.SourceX;
            set
            {
                _sprite.SourceX = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float SourceY
        {
            get => _sprite.SourceY;
            set
            {
                _sprite.SourceY = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float SourceWidth
        {
            get => _sprite.SourceWidth;
            set
            {
                _sprite.SourceWidth = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float SourceHeight
        {
            get => _sprite.SourceHeight;
            set
            {
                _sprite.SourceHeight = value;
				LoadImage();
                OnPropertyChanged();
            }
        }

        #endregion Properties

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

        public UISpriteModel(UISubcomponentModel parent, Sprite sprite) : base(parent, "sprite")
        {
            _sprite = sprite;
            ms = new MemoryStream();
        }

        protected override void LoadImage()
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

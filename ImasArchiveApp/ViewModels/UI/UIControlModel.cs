using Imas.UI;
using System.Windows.Input;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public abstract class UIControlModel : UIElementModel
    {
        protected abstract Control Control { get; }
        protected override UIElement UIElement => Control;

        #region Properties
        public override string ModelName => string.IsNullOrWhiteSpace(Control.Name) ? "(no name)" : Control.Name;
        public int Type
        {
            get => Control.type;
            set
            {
                Control.type = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get => Control.Name;
            set
            {
                Control.Name = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Xpos
        {
            get => Control.xpos;
            set
            {
                Control.xpos = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Ypos
        {
            get => Control.ypos;
            set
            {
                Control.ypos = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Width
        {
            get => Control.width;
            set
            {
                Control.width = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float Height
        {
            get => Control.height;
            set
            {
                Control.height = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int A1
        {
            get => Control.a1;
            set
            {
                Control.a1 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int A2
        {
            get => Control.a2;
            set
            {
                Control.a2 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int A3
        {
            get => Control.a3;
            set
            {
                Control.a3 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int A4
        {
            get => Control.a4;
            set
            {
                Control.a4 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B1
        {
            get => Control.b1;
            set
            {
                Control.b1 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B2
        {
            get => Control.b2;
            set
            {
                Control.b2 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B3
        {
            get => Control.b3;
            set
            {
                Control.b3 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float B4
        {
            get => Control.b4;
            set
            {
                Control.b4 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int C1
        {
            get => Control.c1;
            set
            {
                Control.c1 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int C2
        {
            get => Control.c2;
            set
            {
                Control.c2 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int C3
        {
            get => Control.c3;
            set
            {
                Control.c3 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int C4
        {
            get => Control.c4;
            set
            {
                Control.c4 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Alpha
        {
            get => Control.alpha;
            set
            {
                Control.alpha = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Red
        {
            get => Control.red;
            set
            {
                Control.red = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Green
        {
            get => Control.green;
            set
            {
                Control.green = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte Blue
        {
            get => Control.blue;
            set
            {
                Control.blue = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float ScaleX
        {
            get => Control.scaleX;
            set
            {
                Control.scaleX = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float ScaleY
        {
            get => Control.scaleY;
            set
            {
                Control.scaleY = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float SourceRight
        {
            get => Control.sourceRight;
            set
            {
                Control.sourceRight = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public float SourceBottom
        {
            get => Control.sourceBottom;
            set
            {
                Control.sourceBottom = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int D1
        {
            get => Control.d1;
            set
            {
                Control.d1 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public bool HasExtData => Control.HasExtData;
        public int ExtInt0
        {
            get => Control.extData[0];
            set
            {
                Control.extData[0] = value;
                LoadImage();
                OnPropertyChanged();
            }
        }
        public int ExtInt1
        {
            get => Control.extData[1];
            set
            {
                Control.extData[1] = value;
                LoadImage();
                OnPropertyChanged();
            }
        }
        public int ExtInt2
        {
            get => Control.extData[2];
            set
            {
                Control.extData[2] = value;
                LoadImage();
                OnPropertyChanged();
            }
        }
        public int ExtInt3
        {
            get => Control.extData[3];
            set
            {
                Control.extData[3] = value;
                LoadImage();
                OnPropertyChanged();
            }
        }

        #endregion Properties

        protected UIControlModel(UISubcomponentModel parent, string name) : base(parent, name) { }

        public static UIControlModel CreateModel(UISubcomponentModel parent, Control control)
        {
            return control switch
            {
                GroupControl gc => new UIGroupControlModel(parent, gc),
                SpriteCollection sc => new UISpriteCollectionModel(parent, sc),
                TextBox tb => new UITextBoxModel(parent, tb),
                _ => new UITypedControlModel<Control>(parent, control),
            };
        }
    }
}

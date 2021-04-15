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
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get => Control.Name;
            set
            {
                Control.Name = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Xpos
        {
            get => Control.xpos;
            set
            {
                Control.xpos = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Ypos
        {
            get => Control.ypos;
            set
            {
                Control.ypos = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Width
        {
            get => Control.width;
            set
            {
                Control.width = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float Height
        {
            get => Control.height;
            set
            {
                Control.height = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int A1
        {
            get => Control.a1;
            set
            {
                Control.a1 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int A2
        {
            get => Control.a2;
            set
            {
                Control.a2 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int A3
        {
            get => Control.a3;
            set
            {
                Control.a3 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int A4
        {
            get => Control.a4;
            set
            {
                Control.a4 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B1
        {
            get => Control.b1;
            set
            {
                Control.b1 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B2
        {
            get => Control.b2;
            set
            {
                Control.b2 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B3
        {
            get => Control.b3;
            set
            {
                Control.b3 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float B4
        {
            get => Control.b4;
            set
            {
                Control.b4 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int C1
        {
            get => Control.c1;
            set
            {
                Control.c1 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int C2
        {
            get => Control.c2;
            set
            {
                Control.c2 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int C3
        {
            get => Control.c3;
            set
            {
                Control.c3 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int C4
        {
            get => Control.c4;
            set
            {
                Control.c4 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Alpha
        {
            get => Control.alpha;
            set
            {
                Control.alpha = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Red
        {
            get => Control.red;
            set
            {
                Control.red = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Green
        {
            get => Control.green;
            set
            {
                Control.green = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte Blue
        {
            get => Control.blue;
            set
            {
                Control.blue = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float ScaleX
        {
            get => Control.scaleX;
            set
            {
                Control.scaleX = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float ScaleY
        {
            get => Control.scaleY;
            set
            {
                Control.scaleY = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float SourceRight
        {
            get => Control.sourceRight;
            set
            {
                Control.sourceRight = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public float SourceBottom
        {
            get => Control.sourceBottom;
            set
            {
                Control.sourceBottom = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int D1
        {
            get => Control.d1;
            set
            {
                Control.d1 = value;
				LoadActiveImages();
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
                LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int ExtInt1
        {
            get => Control.extData[1];
            set
            {
                Control.extData[1] = value;
                LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int ExtInt2
        {
            get => Control.extData[2];
            set
            {
                Control.extData[2] = value;
                LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int ExtInt3
        {
            get => Control.extData[3];
            set
            {
                Control.extData[3] = value;
                LoadActiveImages();
                OnPropertyChanged();
            }
        }

        #endregion Properties

        protected UIControlModel(UISubcomponentModel subcomponent, UIElementModel parent, string name) : base(subcomponent, parent, name, true) { }

        public static UIControlModel CreateModel(UISubcomponentModel subcomponent, UIElementModel parent, Control control)
        {
            UIControlModel newControlModel = control switch
            {
                GroupControl gc => new UIGroupControlModel(subcomponent, parent, gc),
                SpriteCollection sc => new UISpriteCollectionModel(subcomponent, parent, sc),
                TextBox tb => new UITextBoxModel(subcomponent, parent, tb),
                _ => new UITypedControlModel<Control>(subcomponent, parent, control),
            };
            newControlModel.currentVisibility = control.DefaultVisibility;
            return newControlModel;
        }

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            drawingContext.PushTransform(new TranslateTransform(Xpos, Ypos));
            drawingContext.PushTransform(new ScaleTransform(ScaleX == 0 ? 1 : ScaleX, ScaleY));
            drawingContext.PushOpacity(Alpha / 255.0);
            multiplier.Scale(Red / 255.0, Green / 255.0, Blue / 255.0);
            base.RenderElement(drawingContext, multiplier, isTop);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.Pop();
        }
    }
}

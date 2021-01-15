using Imas.UI;
using System.Windows.Input;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public abstract class UIControlModel : UIElementModel
    {
        protected readonly UISubcomponentModel parent;
        public abstract Control Control { get; }
        public abstract ImageSource ImageSource { get; set; }

        #region Properties
        public int Type
        {
            get => Control.type;
            set
            {
                Control.type = value;
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get => string.IsNullOrWhiteSpace(Control.name) ? "(no name)" : Control.name;
            set
            {
                Control.name = value;
                OnPropertyChanged();
            }
        }
        public float Xpos
        {
            get => Control.xpos;
            set
            {
                Control.xpos = value;
                OnPropertyChanged();
            }
        }
        public float Ypos
        {
            get => Control.ypos;
            set
            {
                Control.ypos = value;
                OnPropertyChanged();
            }
        }
        public float Width
        {
            get => Control.width;
            set
            {
                Control.width = value;
                OnPropertyChanged();
            }
        }
        public float Height
        {
            get => Control.height;
            set
            {
                Control.height = value;
                OnPropertyChanged();
            }
        }
        public int A1
        {
            get => Control.a1;
            set
            {
                Control.a1 = value;
                OnPropertyChanged();
            }
        }
        public int A2
        {
            get => Control.a2;
            set
            {
                Control.a2 = value;
                OnPropertyChanged();
            }
        }
        public int A3
        {
            get => Control.a3;
            set
            {
                Control.a3 = value;
                OnPropertyChanged();
            }
        }
        public int A4
        {
            get => Control.a4;
            set
            {
                Control.a4 = value;
                OnPropertyChanged();
            }
        }
        public float B1
        {
            get => Control.b1;
            set
            {
                Control.b1 = value;
                OnPropertyChanged();
            }
        }
        public float B2
        {
            get => Control.b2;
            set
            {
                Control.b2 = value;
                OnPropertyChanged();
            }
        }
        public float B3
        {
            get => Control.b3;
            set
            {
                Control.b3 = value;
                OnPropertyChanged();
            }
        }
        public float B4
        {
            get => Control.b4;
            set
            {
                Control.b4 = value;
                OnPropertyChanged();
            }
        }
        public int C1
        {
            get => Control.c1;
            set
            {
                Control.c1 = value;
                OnPropertyChanged();
            }
        }
        public int C2
        {
            get => Control.c2;
            set
            {
                Control.c2 = value;
                OnPropertyChanged();
            }
        }
        public int C3
        {
            get => Control.c3;
            set
            {
                Control.c3 = value;
                OnPropertyChanged();
            }
        }
        public int C4
        {
            get => Control.c4;
            set
            {
                Control.c4 = value;
                OnPropertyChanged();
            }
        }
        public byte Alpha
        {
            get => Control.alpha;
            set
            {
                Control.alpha = value;
                OnPropertyChanged();
            }
        }
        public byte Red
        {
            get => Control.red;
            set
            {
                Control.red = value;
                OnPropertyChanged();
            }
        }
        public byte Green
        {
            get => Control.green;
            set
            {
                Control.green = value;
                OnPropertyChanged();
            }
        }
        public byte Blue
        {
            get => Control.blue;
            set
            {
                Control.blue = value;
                OnPropertyChanged();
            }
        }
        public float ScaleX
        {
            get => Control.scaleX;
            set
            {
                Control.scaleX = value;
                OnPropertyChanged();
            }
        }
        public float ScaleY
        {
            get => Control.scaleY;
            set
            {
                Control.scaleY = value;
                OnPropertyChanged();
            }
        }
        public float SourceRight
        {
            get => Control.sourceRight;
            set
            {
                Control.sourceRight = value;
                OnPropertyChanged();
            }
        }
        public float SourceBottom
        {
            get => Control.sourceBottom;
            set
            {
                Control.sourceBottom = value;
                OnPropertyChanged();
            }
        }
        public int D1
        {
            get => Control.d1;
            set
            {
                Control.d1 = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties

        protected UIControlModel(UISubcomponentModel parent, string name) : base(parent, name)
        {
            this.parent = parent;
        }

        public static UIControlModel CreateModel(UISubcomponentModel parent, Control control)
        {
            return control switch
            {
                GroupControl gc => new UIGroupControlModel(parent, gc),
                _ => new UITypedControlModel<Control>(parent, control),
            };
        }

        private RelayCommand _selectCommand;

        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new RelayCommand(
                        _ => {
                            LoadImage();
                            parent.SelectedModel = this;
                        });
                }
                return _selectCommand;
            }
        }

        protected abstract void LoadImage();
    }
}

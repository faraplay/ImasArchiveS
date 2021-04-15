using Imas.UI;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public abstract class UIElementModel : UIModel
    {
        protected readonly UIElementModel parent;

        protected bool currentVisibility = true;
        public bool CurrentVisibility
        {
            get => currentVisibility;
            set
            {
                currentVisibility = value;
                OnPropertyChanged();
            }
        }

        protected bool centerRender = false;
        public bool CenterRender
        {
            get => centerRender;
            set
            {
                centerRender = value;
                LoadActiveImages();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UIElementModel> Children { get; set; }
        protected abstract UIElement UIElement { get; }
        public UIElement MyUIElement => UIElement;
        public abstract string ModelName { get; }

        public string GetUniqueString() => parent == null ? "0" : $"{parent.GetUniqueString()},{parent.Children.IndexOf(this)}";
        public string ParentUniqueID => parent.GetUniqueString();
        public bool IsInCollection => parent is UISpriteCollectionModel;

        protected UIElementModel(UISubcomponentModel subcomponent, UIElementModel parent, string name, bool visible) : base(subcomponent, name)
        {
            this.parent = parent;
            Children = new ObservableCollection<UIElementModel>();
        }

        protected override Bitmap GetBitmap() => UIElement.GetBitmap(CenterRender ? new PointF(640, 360) : new PointF());

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            if (!CurrentVisibility && !isTop)
                return;
            foreach (UIElementModel child in Children)
            {
                child.RenderElement(drawingContext, multiplier, false);
            }
        }
    }
}

using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImasArchiveApp
{
    public abstract class UIElementModel : UIModel
    {
        protected readonly UIElementModel parent;

        protected bool? cacheVisible;
        public virtual bool? Visible
        {
            get => cacheVisible;
            set
            {
                if (value != cacheVisible)
                {
                    cacheVisible = value;
                    if (value == null) // being set by a child
                    {
                        cacheVisible = null;
                        if (parent != null) 
                            parent.Visible = null;
                    }
                    else
                    {
                        foreach (var child in Children)
                        {
                            child.Visible = value;
                        }
                        if (parent != null)
                        {
                            if (parent.Children.All(model => model.Visible == value))
                            {
                                parent.Visible = value;
                            }
                            else
                            {
                                parent.Visible = null;
                            }
                        }
                    }
                    OnPropertyChanged(nameof(BindVisible));
                }
            }
        }
        public bool VisibleIsNull => Visible == null;
        public bool? BindVisible
        {
            get => Visible;
            set
            {
                Visible = value;
                LoadActiveImages();
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
            cacheVisible = visible;
            Children = new ObservableCollection<UIElementModel>();
        }

        protected override Bitmap GetBitmap() => UIElement.GetBitmap(CenterRender ? new PointF(640, 360) : new PointF());
    }
}

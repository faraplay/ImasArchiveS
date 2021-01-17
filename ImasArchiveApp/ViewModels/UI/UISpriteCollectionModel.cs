using Imas.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImasArchiveApp
{
    class UISpriteCollectionModel : UITypedControlModel<SpriteCollection>
    {
        #region Properties

        public int E1
        {
            get => _control.e1;
            set
            {
                _control.e1 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int E2
        {
            get => _control.e2;
            set
            {
                _control.e2 = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }

        #endregion

        public override bool? Visible
        {
            get => cacheVisible;
            set
            {
                if (value != cacheVisible)
                {
                    cacheVisible = value;
                    if (value != null)
                    {
                        if (value == false)
                        {
                            foreach (var child in Children)
                            {
                                child.Visible = false;
                            }
                        }
                        else if (Children.All(model => model.Visible == false))
                        {
                            Children[0].Visible = true;
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

        public UISpriteCollectionModel(UISubcomponentModel subcomponent, UIElementModel parent, SpriteCollection control) : base(subcomponent, parent, control)
        {
            foreach (SpriteGroup child in control.childSpriteGroups)
            {
                Children.Add(new UISpriteGroupModel(subcomponent, this, child));
            }
        }
    }
}

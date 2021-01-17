using Imas.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImasArchiveApp
{
    class UISpriteGroupModel : UIElementModel
    {
        private readonly SpriteGroup _spriteGroup;
        protected override UIElement UIElement => _spriteGroup;
        public override string ModelName => $"({_spriteGroup.sprites.Count} sprites)";
        public override bool? Visible
        {
            get => base.Visible;
            set
            {
                if (value != cacheVisible)
                {
                    cacheVisible = value;
                    if (value != null)
                    {
                        UIElement.myVisible = value.Value;
                        if (parent != null)
                        {
                            if (IsInCollection)
                            {
                                if (value == true)
                                {
                                    foreach (var model in parent.Children)
                                    {
                                        if (model != this)
                                            model.Visible = false;
                                    }
                                    parent.Visible = true;
                                }
                            }
                            else
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
                    }
                    OnPropertyChanged(nameof(BindVisible));
                }
            }
        }
        public UISpriteGroupModel(UISubcomponentModel subcomponent, UIElementModel parent, SpriteGroup spriteGroup) : base(subcomponent, parent, "spriteGroup", spriteGroup.myVisible)
        {
            _spriteGroup = spriteGroup;
            foreach (Sprite sprite in spriteGroup.sprites)
            {
                Children.Add(new UISpriteModel(subcomponent, this, sprite));
            }
        }

    }
}

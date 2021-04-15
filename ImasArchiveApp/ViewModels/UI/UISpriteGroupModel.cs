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

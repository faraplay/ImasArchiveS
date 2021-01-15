using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class UISpriteGroupModel : UIElementModel
    {
        private readonly SpriteGroup _spriteGroup;
        protected override UIElement UIElement => _spriteGroup;
        public override string ModelName => $"({_spriteGroup.spriteCount} sprites)";
        public UISpriteGroupModel(UISubcomponentModel parent, SpriteGroup spriteGroup) : base(parent, "spriteGroup")
        {
            _spriteGroup = spriteGroup;
            foreach (Sprite sprite in spriteGroup.sprites)
            {
                Children.Add(new UISpriteModel(parent, sprite));
            }
        }

    }
}

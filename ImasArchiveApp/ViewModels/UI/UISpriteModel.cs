using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class UISpriteModel : UIElementModel
    {
        private readonly Sprite _sprite;
        protected override UIElement UIElement => _sprite;
        public override string ModelName => $"({_sprite.width}x{_sprite.height})";
        public UISpriteModel(UISubcomponentModel parent, Sprite sprite) : base(parent, "sprite")
        {
            _sprite = sprite;
        }
    }
}

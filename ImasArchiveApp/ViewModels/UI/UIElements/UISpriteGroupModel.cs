using Imas.UI;

namespace ImasArchiveApp
{
    class UISpriteGroupModel : UIElementModel
    {
        private readonly SpriteGroup _spriteGroup;
        public override UIElement UIElement => _spriteGroup;
        public override string ModelName => $"({_spriteGroup.Sprites.Count} sprites)";
        public bool IsSpriteCollectionChild { get; }

        public UISpriteGroupModel(
            UISubcomponentModel subcomponent,
            UIElementModel parent,
            SpriteGroup spriteGroup,
            bool isSpriteCollectionChild) : base(subcomponent, parent, "spriteGroup")
        {
            _spriteGroup = spriteGroup;
            IsSpriteCollectionChild = isSpriteCollectionChild;
            foreach (Sprite sprite in spriteGroup.Sprites)
            {
                Children.Add(new UISpriteModel(subcomponent, this, sprite));
            }
        }

    }
}

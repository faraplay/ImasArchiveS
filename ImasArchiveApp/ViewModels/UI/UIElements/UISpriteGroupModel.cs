using Imas.UI;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class UISpriteGroupModel : UIElementModel
    {
        private readonly SpriteGroup spriteGroup;
        public override UIElement UIElement => spriteGroup;
        public override string ModelName => $"({spriteGroup.Sprites.Count} sprites)";
        public bool IsSpriteCollectionChild { get; }

        public UISpriteGroupModel(
            UISubcomponentModel subcomponent,
            UIElementModel parent,
            SpriteGroup spriteGroup,
            bool isSpriteCollectionChild) : base(subcomponent, parent, "spriteGroup")
        {
            this.spriteGroup = spriteGroup;
            IsSpriteCollectionChild = isSpriteCollectionChild;
            foreach (Sprite sprite in spriteGroup.Sprites)
            {
                Children.Add(new UISpriteModel(subcomponent, this, sprite));
            }
        }

        public void InsertSprite(int index, Sprite sprite)
        {
            spriteGroup.Sprites.Insert(index, sprite);
            spriteGroup.SpriteCount++;
            Children.Insert(index, new UISpriteModel(subcomponent, this, sprite));

        }

        public void RemoveSprite(UISpriteModel spriteModel)
        {
            int index = Children.IndexOf(spriteModel);
            if (index == -1)
                return;
            if (!IsSpriteCollectionChild && Children.Count == 1)
            {
                Delete();
                return;
            }
            spriteGroup.Sprites.RemoveAt(index);
            spriteGroup.SpriteCount--;
            Children.RemoveAt(index);
        }

        private RelayCommand _addSpriteCommand;
        public ICommand AddSpriteCommand
        {
            get
            {
                if (_addSpriteCommand == null)
                    _addSpriteCommand = new RelayCommand(
                        _ => InsertSprite(Children.Count, new Sprite()));
                return _addSpriteCommand;
            }
        }

        public void Delete()
        {
            if (IsSpriteCollectionChild)
            {
                if (!(parent is UISpriteCollectionModel spriteCollection))
                    return;
                spriteCollection.RemoveSpriteGroup(this);
            }
            else
            {
                if (!(parent is UIControlModel controlModel))
                    return;
                controlModel.RemoveSpecialSprite();
            }
        }

        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(
                        _ => Delete());
                return _deleteCommand;
            }
        }
    }
}

using Imas.UI;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class UISpriteGroupModel : UIElementModel
    {
        private readonly SpriteGroup spriteGroup;
        public override UIElement UIElement => spriteGroup;
        private readonly UIControlModel parentControlModel;
        protected override UIElementModel Parent => parentControlModel;
        public override string ElementName => $"({spriteGroup.Sprites.Count} sprites)";
        public bool IsSpriteCollectionChild { get; }

        public UISpriteGroupModel(
            UISubcomponentModel subcomponent,
            UIControlModel parent,
            SpriteGroup spriteGroup,
            bool isSpriteCollectionChild,
            bool visibility) : base(subcomponent, null)
        {
            this.spriteGroup = spriteGroup;
            parentControlModel = parent;
            IsSpriteCollectionChild = isSpriteCollectionChild;
            currentVisibility = visibility;
            foreach (Sprite sprite in spriteGroup.Sprites)
            {
                Children.Add(new UISpriteModel(subcomponent, this, sprite));
            }
        }

        public void InsertSprite(int index, Sprite sprite)
        {
            spriteGroup.Sprites.Insert(index, sprite);
            Children.Insert(index, new UISpriteModel(subcomponent, this, sprite));
            subcomponent.PauModel.ForceRender();
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
            Children.RemoveAt(index);
            subcomponent.PauModel.ForceRender();
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
                if (!(parentControlModel is UISpriteCollectionModel spriteCollection))
                    return;
                spriteCollection.RemoveSpriteGroup(this);
            }
            else
            {
                parentControlModel.RemoveSpecialSprite();
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

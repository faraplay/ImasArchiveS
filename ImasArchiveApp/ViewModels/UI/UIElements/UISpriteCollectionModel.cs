using Imas.UI;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    class UISpriteCollectionModel : UIControlModel
    {
        private readonly SpriteCollection spriteCollection;
        protected override Control Control => spriteCollection;
        public UISpriteCollectionModel(SpriteCollection control, UISubcomponentModel subcomponent, UIControlModel parent) : base(control, subcomponent, parent)
        {
            spriteCollection = control;
            int index = 0;
            foreach (SpriteGroup child in spriteCollection.ChildSpriteGroups)
            {
                Children.Add(new UISpriteGroupModel(subcomponent, this, child, true)
                { CurrentVisibility = index == spriteCollection.DefaultSpriteIndex });
                index++;
            }
        }

        protected override void RenderElementUntransformed(DrawingContext drawingContext, ColorMultiplier multiplier, bool forceVisible)
        {
            if (!CurrentVisibility && !forceVisible && VisibilityClock == null)
                return;
            if (SpriteClocks == null)
            {
                foreach (UIElementModel child in Children)
                {
                    child.RenderElement(drawingContext, multiplier, false);
                }
            }
            else
            {
                int index = 0;
                foreach (UIElementModel child in Children)
                {
                    if (!(child is UISpriteGroupModel groupModel))
                        continue;
                    if (!groupModel.IsSpriteCollectionChild)
                        continue;
                    drawingContext.PushOpacity(child.CurrentVisibility ? 1 : 0, SpriteClocks[index]);
                    child.RenderElement(drawingContext, multiplier, true);
                    drawingContext.Pop();
                    index++;
                }
            }
        }

        public AnimationClock[] SpriteClocks { get; set; }
        public void ApplySpriteClocks(ClockGroup clockGroup)
        {
            if (clockGroup == null)
            {
                SpriteClocks = null;
                return;
            }
            SpriteClocks = new AnimationClock[clockGroup.Children.Count];
            int index = 0;
            foreach (Clock clock in clockGroup.Children)
            {
                SpriteClocks[index] = (AnimationClock)clock;
                index++;
            }
        }

        public int GetSpriteGroupIndex(UISpriteGroupModel groupModel)
        {
            int index = Children.IndexOf(groupModel);
            if (index == -1)
                return -1;
            if (HasSpecialSprite)
            {
                index--;
            }
            return index;
        }

        public void InsertSpriteGroup(int index, SpriteGroup spriteGroup)
        {
            spriteCollection.ChildSpriteGroups.Insert(index, spriteGroup);
            spriteCollection.ChildSpriteGroupCount++;
            if (HasSpecialSprite)
            {
                index++;
            }
            Children.Insert(index, new UISpriteGroupModel(subcomponent, this, spriteGroup, true) { CurrentVisibility = false });
            if (index <= spriteCollection.DefaultSpriteIndex)
                spriteCollection.DefaultSpriteIndex++;
        }
        public void RemoveSpriteGroup(UISpriteGroupModel groupModel)
        {
            int index = GetSpriteGroupIndex(groupModel);
            if (index == -1)
                return;
            spriteCollection.ChildSpriteGroups.RemoveAt(index);
            spriteCollection.ChildSpriteGroupCount--;
            if (HasSpecialSprite)
            {
                index++;
            }
            Children.RemoveAt(index);
            if (index <= spriteCollection.DefaultSpriteIndex)
                spriteCollection.DefaultSpriteIndex--;
        }

        public void AddSpriteGroup(SpriteGroup spriteGroup)
        {
            int groupCount = Children.Count;
            if (HasSpecialSprite)
            {
                groupCount--;
            }
            InsertSpriteGroup(groupCount, spriteGroup);
        }

        private RelayCommand _addSpriteGroupCommand;
        public ICommand AddSpriteGroupCommand
        {
            get
            {
                if (_addSpriteGroupCommand == null)
                    _addSpriteGroupCommand = new RelayCommand(
                        _ => AddSpriteGroup(new SpriteGroup()));
                return _addSpriteGroupCommand;
            }
        }
    }
}

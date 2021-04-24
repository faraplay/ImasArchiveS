using Imas.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    class UISpriteCollectionModel : UIControlModel
    {
        public UISpriteCollectionModel(SpriteCollection control, UISubcomponentModel subcomponent, UIElementModel parent) : base(control, subcomponent, parent)
        {
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
            if (Control.SpecialSprite.Sprites.Count != 0)
            {
                index--;
            }
            return index;
        }

        public void InsertSpriteGroup(int index, SpriteGroup spriteGroup)
        {
            if (!(Control is SpriteCollection spriteCollection))
                return;
            spriteCollection.ChildSpriteGroups.Insert(index, spriteGroup);
            spriteCollection.ChildSpriteGroupCount++;
            if (Control.SpecialSprite.Sprites.Count != 0)
            {
                index++;
            }
            Children.Insert(index, new UISpriteGroupModel(subcomponent, this, spriteGroup, true) { CurrentVisibility = false });
            if (index <= spriteCollection.DefaultSpriteIndex)
                spriteCollection.DefaultSpriteIndex++;
        }
        public void RemoveSpriteGroup(UISpriteGroupModel groupModel)
        {
            if (!(Control is SpriteCollection spriteCollection))
                return;
            int index = GetSpriteGroupIndex(groupModel);
            if (index == -1)
                return;
            spriteCollection.ChildSpriteGroups.RemoveAt(index);
            spriteCollection.ChildSpriteGroupCount--;
            if (Control.SpecialSprite.Sprites.Count != 0)
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
            if (Control.SpecialSprite.Sprites.Count != 0)
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

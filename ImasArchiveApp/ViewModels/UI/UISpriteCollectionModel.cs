using Imas.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}

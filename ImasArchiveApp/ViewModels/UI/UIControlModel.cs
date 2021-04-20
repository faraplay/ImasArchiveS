using Imas.UI;
using System;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public class UIControlModel : UIElementModel
    {
        protected virtual Control Control { get; }
        public override UIElement UIElement => Control;

        public override string ModelName => string.IsNullOrWhiteSpace(Control.Name) ? "(no name)" : Control.Name;
        public int ControlTypeID => Control switch
        {
            SpriteCollection _ => 10,
            Control9 _ => 9,
            ScrollControl _ => 6,
            RotatableGroupControl _ => 5,
            GroupControl _ => 4,
            Control3 _ => 3,
            TextBox _ => 2,
            _ => 0,
        };

        public UIControlModel(Control control, UISubcomponentModel subcomponent, UIElementModel parent) : base(subcomponent, parent, control.Name)
        {
            Control = control;
            if (control.specialSprite.sprites.Count != 0)
            {
                Children.Add(new UISpriteGroupModel(subcomponent, this, control.specialSprite, false) 
                    { CurrentVisibility = !(control is SpriteCollection) });
            }
            if (control is GroupControl groupControl)
            {
                foreach (Control child in groupControl.childControls)
                {
                    Children.Add(new UIControlModel(child, subcomponent, this));
                }
            }
            if (control is SpriteCollection spriteCollection)
            {
                int index = 0;
                foreach (SpriteGroup child in spriteCollection.childSpriteGroups)
                {
                    Children.Add(new UISpriteGroupModel(subcomponent, this, child, true) 
                        { CurrentVisibility = index == spriteCollection.defaultSpriteIndex });
                    index++;
                }
            }
            CurrentVisibility = control.DefaultVisibility;
            PositionTransform = new TranslateTransform(control.Xpos, control.Ypos);
            ScaleTransform = new ScaleTransform(control.ScaleX, control.ScaleY);
            if (Control is RotatableGroupControl icon)
            {
                AngleTransform = new RotateTransform(-icon.angle * 180 / Math.PI);
            }
        }

        public void ForAll(Action<UIControlModel> action)
        {
            action(this);
            foreach (UIElementModel child in Children)
            {
                if (child is UIControlModel childControl)
                {
                    childControl.ForAll(action);
                }
            }
        }

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            drawingContext.PushOpacity(CurrentVisibility ? 1 : 0, VisibilityClock);
            drawingContext.PushTransform(PositionTransform);
            if (Control is RotatableGroupControl)
            {
                drawingContext.PushTransform(AngleTransform);
            }
            drawingContext.PushTransform(ScaleTransform);
            drawingContext.PushOpacity(Control.Alpha / 255.0, OpacityClock);
            multiplier.Scale(Control.Red / 255.0f, Control.Green / 255.0f, Control.Blue / 255.0f);

            RenderElementUntransformed(drawingContext, multiplier, isTop);

            drawingContext.Pop();
            drawingContext.Pop();
            if (Control is RotatableGroupControl)
            {
                drawingContext.Pop();
            }
            drawingContext.Pop();
            drawingContext.Pop();
        }

        protected virtual void RenderElementUntransformed(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
        {
            if (!CurrentVisibility && !isTop && VisibilityClock == null)
                return;
            foreach (UIElementModel child in Children)
            {
                child.RenderElement(drawingContext, multiplier, false);
            }
        }

        public AnimationClock VisibilityClock { get; set; }
        public TranslateTransform PositionTransform { get; }
        public AnimationClock OpacityClock { get; set; }
        public ScaleTransform ScaleTransform { get; }
        public RotateTransform AngleTransform { get; }
    }
}

using Imas.UI;
using System;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public abstract class UIControlModel : UIElementModel
    {
        protected abstract Control Control { get; }
        protected override UIElement UIElement => Control;

        public override string ModelName => string.IsNullOrWhiteSpace(Control.Name) ? "(no name)" : Control.Name;


        protected UIControlModel(Control control, UISubcomponentModel subcomponent, UIElementModel parent, string name) : base(subcomponent, parent, name, true)
        {
            PositionTransform = new TranslateTransform(control.Xpos, control.Ypos);
            //ScaleTransform = new ScaleTransform(Control.ScaleX == 0 ? 1 : Control.ScaleX, Control.ScaleY);
            ScaleTransform = new ScaleTransform(control.ScaleX, control.ScaleY);
        }

        public static UIControlModel CreateModel(UISubcomponentModel subcomponent, UIElementModel parent, Control control)
        {
            UIControlModel newControlModel = control switch
            {
                GroupControl gc => new UIGroupControlModel(subcomponent, parent, gc),
                SpriteCollection sc => new UISpriteCollectionModel(subcomponent, parent, sc),
                TextBox tb => new UITextBoxModel(subcomponent, parent, tb),
                _ => new UITypedControlModel<Control>(subcomponent, parent, control),
            };
            newControlModel.currentVisibility = control.DefaultVisibility;
            return newControlModel;
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
            drawingContext.PushTransform(ScaleTransform);
            drawingContext.PushOpacity(Control.Alpha / 255.0, OpacityClock);
            multiplier.Scale(Control.Red / 255.0f, Control.Green / 255.0f, Control.Blue / 255.0f);
            RenderElementUntransformed(drawingContext, multiplier, isTop);
            drawingContext.Pop();
            drawingContext.Pop();
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
    }
}

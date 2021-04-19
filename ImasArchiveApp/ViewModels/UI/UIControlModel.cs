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


        protected UIControlModel(UISubcomponentModel subcomponent, UIElementModel parent, string name) : base(subcomponent, parent, name, true) { }

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
            drawingContext.PushTransform(new TranslateTransform(Control.Xpos, Control.Ypos));
            drawingContext.PushTransform(new ScaleTransform(Control.ScaleX == 0 ? 1 : Control.ScaleX, Control.ScaleY));
            drawingContext.PushOpacity(Control.Alpha / 255.0, OpacityClock);
            multiplier.Scale(Control.Red / 255.0f, Control.Green / 255.0f, Control.Blue / 255.0f);
            RenderElementUntransformed(drawingContext, multiplier, isTop);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.Pop();
        }

        protected virtual void RenderElementUntransformed(DrawingContext drawingContext, ColorMultiplier multiplier, bool isTop)
            => base.RenderElement(drawingContext, multiplier, isTop);

        //public TranslateTransform TranslateTransform { get; set; }
        //public ScaleTransform ScaleTransform { get; set; }
        public AnimationClock OpacityClock { get; set; }
        public void SetTransforms()
        {
            //TranslateTransform = new TranslateTransform(Control.Xpos, Control.Ypos);
            //ScaleTransform = new ScaleTransform(Control.ScaleX == 0 ? 1 : Control.ScaleX, Control.ScaleY);
        }
    }
}

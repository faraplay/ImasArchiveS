using Imas.UI;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public class UIControlModel : UIElementModel
    {
        private readonly Control _control;
        protected virtual Control Control { get => _control; }
        public override UIElement UIElement => Control;
        protected readonly UIControlModel parentControlModel;
        protected override UIElementModel Parent => parentControlModel;

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

        protected UIControlModel(Control control, UISubcomponentModel subcomponent, UIControlModel parent) : base(subcomponent, control.Name)
        {
            _control = control;
            parentControlModel = parent;
        }

        private void Initialise()
        {
            if (HasSpecialSprite)
            {
                Children.Add(new UISpriteGroupModel(subcomponent, this, Control.SpecialSprite, false)
                { CurrentVisibility = !(Control is SpriteCollection) });
            }
            InitialiseRenderVars();
        }

        public static UIControlModel CreateControlModel(Control control, UISubcomponentModel subcomponent, UIControlModel parent)
        {
            UIControlModel controlModel = control switch
            {
                TextBox textBox => new UITextBoxModel(textBox, subcomponent, parent),
                SpriteCollection spriteChild => new UISpriteCollectionModel(spriteChild, subcomponent, parent),
                GroupControl groupControl => new UIGroupControlModel(groupControl, subcomponent, parent),
                _ => new UIControlModel(control, subcomponent, parent),
            };
            controlModel.Initialise();
            return controlModel;
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

        private void InitialiseRenderVars()
        {
            CurrentVisibility = Control.DefaultVisibility;
            PositionTransform = new TranslateTransform(Control.Xpos, Control.Ypos);
            ScaleTransform = new ScaleTransform(Control.ScaleX, Control.ScaleY);
            if (Control is RotatableGroupControl icon)
            {
                AngleTransform = new RotateTransform(-icon.Angle * 180 / Math.PI);
            }
            PositionTransform.Freeze();
            ScaleTransform.Freeze();
            AngleTransform?.Freeze();
        }

        internal override void RenderElement(DrawingContext drawingContext, ColorMultiplier multiplier, bool forceVisible)
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

            RenderElementUntransformed(drawingContext, multiplier, forceVisible);

            drawingContext.Pop();
            drawingContext.Pop();
            if (Control is RotatableGroupControl)
            {
                drawingContext.Pop();
            }
            drawingContext.Pop();
            drawingContext.Pop();
        }

        protected virtual void RenderElementUntransformed(DrawingContext drawingContext, ColorMultiplier multiplier, bool forceVisible)
        {
            if (!CurrentVisibility && !forceVisible && VisibilityClock == null)
                return;
            foreach (UIElementModel child in Children)
            {
                child.RenderElement(drawingContext, multiplier, false);
            }
        }

        public AnimationClock VisibilityClock { get; set; }
        public TranslateTransform PositionTransform { get; set; }
        public void ApplyPositionAnimation(AnimationClock x, AnimationClock y)
        {
            PositionTransform = new TranslateTransform(Control.Xpos, Control.Ypos);
            PositionTransform.ApplyAnimationClock(TranslateTransform.XProperty, x);
            PositionTransform.ApplyAnimationClock(TranslateTransform.YProperty, y);
        }
        public AnimationClock OpacityClock { get; set; }
        public ScaleTransform ScaleTransform { get; set; }
        public void ApplyScaleAnimation(AnimationClock scaleX, AnimationClock scaleY)
        {
            ScaleTransform = new ScaleTransform(Control.ScaleX, Control.ScaleY);
            ScaleTransform.ApplyAnimationClock(ScaleTransform.ScaleXProperty, scaleX);
            ScaleTransform.ApplyAnimationClock(ScaleTransform.ScaleYProperty, scaleY);
        }
        public RotateTransform AngleTransform { get; set; }
        public void ApplyAngleAnimation(AnimationClock angle)
        {
            if (!(Control is RotatableGroupControl rotatable))
                return;
            AngleTransform = new RotateTransform(-rotatable.Angle);
            AngleTransform.ApplyAnimationClock(RotateTransform.AngleProperty, angle);
        }

        public void ResetAnimatedValues()
        {
            CurrentVisibility = Control.DefaultVisibility;
        }


        public void AddSpecialSprite()
        {
            if (HasSpecialSprite)
                return;
            SpriteGroup spriteGroup = new SpriteGroup();
            var groupModel = new UISpriteGroupModel(subcomponent, this, spriteGroup, false);
            groupModel.InsertSprite(0, new Sprite());
            Control.SpecialSprite = spriteGroup;
            Children.Insert(0, groupModel);
        }
        public void RemoveSpecialSprite()
        {
            if (!HasSpecialSprite)
                return;
            Control.SpecialSprite = new SpriteGroup();
            Children.RemoveAt(0);
        }

        public bool HasSpecialSprite => Control.SpecialSprite.Sprites.Count != 0;
        private RelayCommand _addSpecialSpriteCommand;
        public ICommand AddSpecialSpriteCommand
        {
            get
            {
                if (_addSpecialSpriteCommand == null)
                    _addSpecialSpriteCommand = new RelayCommand(
                        _ => AddSpecialSprite(), _ => !HasSpecialSprite);
                return _addSpecialSpriteCommand;
            }
        }
    }
}

using Imas.UI;
using System;
using System.ComponentModel;
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
        protected readonly UIGroupControlModel parentGroupModel;
        protected override UIElementModel Parent => parentGroupModel;

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

        protected UIControlModel(Control control, UISubcomponentModel subcomponent, UIGroupControlModel parent) : base(subcomponent, control.Name)
        {
            _control = control;
            parentGroupModel = parent;
        }

        private void Initialise()
        {
            if (HasSpecialSprite)
            {
                Children.Insert(0, new UISpriteGroupModel(subcomponent, this, Control.SpecialSprite, false,
                    !(Control is SpriteCollection)));
            }
            currentVisibility = Control.DefaultVisibility;
            SetRenderTransforms();
        }

        public static UIControlModel CreateControlModel(Control control, UISubcomponentModel subcomponent, UIGroupControlModel parent)
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

        private void SetRenderTransforms()
        {
            PositionTransform = new TranslateTransform(Control.Xpos, Control.Ypos);
            ScaleTransform = new ScaleTransform(Control.ScaleX, Control.ScaleY);
            if (Control is RotatableGroupControl icon)
            {
                AngleTransform = new RotateTransform(-icon.Angle);
            }
            PositionTransform.Freeze();
            ScaleTransform.Freeze();
            AngleTransform?.Freeze();
        }

        public override void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            SetRenderTransforms();
            base.PropertyChangedHandler(sender, e);
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
            var groupModel = new UISpriteGroupModel(subcomponent, this, spriteGroup, false, true);
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

        public bool HasParent => parentGroupModel != null;

        public void InsertControl<T>() where T : Control, new()
        {
            parentGroupModel.InsertNewControl<T>(parentGroupModel.IndexOf(this));
        }

        private RelayCommand _insertTextBoxCommand;
        public ICommand InsertTextBoxCommand
        {
            get
            {
                if (_insertTextBoxCommand == null)
                    _insertTextBoxCommand = new RelayCommand(
                        _ => InsertControl<TextBox>(), _ => HasParent);
                return _insertTextBoxCommand;
            }
        }
        private RelayCommand _insertGroupControlCommand;
        public ICommand InsertGroupControlCommand
        {
            get
            {
                if (_insertGroupControlCommand == null)
                    _insertGroupControlCommand = new RelayCommand(
                        _ => InsertControl<GroupControl>(), _ => HasParent);
                return _insertGroupControlCommand;
            }
        }
        private RelayCommand _insertRotatableGroupControlCommand;
        public ICommand InsertRotatableGroupControlCommand
        {
            get
            {
                if (_insertRotatableGroupControlCommand == null)
                    _insertRotatableGroupControlCommand = new RelayCommand(
                        _ => InsertControl<RotatableGroupControl>(), _ => HasParent);
                return _insertRotatableGroupControlCommand;
            }
        }
        private RelayCommand _insertSpriteCollectionCommand;
        public ICommand InsertSpriteCollectionCommand
        {
            get
            {
                if (_insertSpriteCollectionCommand == null)
                    _insertSpriteCollectionCommand = new RelayCommand(
                        _ => InsertControl<SpriteCollection>(), _ => HasParent);
                return _insertSpriteCollectionCommand;
            }
        }

        public void Delete()
        {
            parentGroupModel.RemoveControl(this);
        }
        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(
                        _ => Delete(), _ => HasParent);
                return _deleteCommand;
            }
        }
    }
}

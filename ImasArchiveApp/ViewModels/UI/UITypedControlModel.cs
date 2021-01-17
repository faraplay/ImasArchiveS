using Imas.UI;

namespace ImasArchiveApp
{
    class UITypedControlModel<T> : UIControlModel where T : Control
    {
        protected readonly T _control;
        protected override Control Control => _control;

        public UITypedControlModel(UISubcomponentModel subcomponent, UIElementModel parent, T control) : base(subcomponent, parent, control.ToString())
        {
            _control = control;
            centerRender = control.xpos < 0 || control.ypos < 0;
            if (control.specialSprite.sprites.Count != 0)
            {
                Children.Add(new UISpriteGroupModel(subcomponent, this, control.specialSprite));
            }
        }
    }
}

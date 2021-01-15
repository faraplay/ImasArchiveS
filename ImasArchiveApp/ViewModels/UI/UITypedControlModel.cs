using Imas.UI;

namespace ImasArchiveApp
{
    class UITypedControlModel<T> : UIControlModel where T : Control
    {
        protected readonly T _control;
        protected override Control Control => _control;

        public UITypedControlModel(UISubcomponentModel parent, T control) : base(parent, control.ToString())
        {
            _control = control;
            Children.Add(new UISpriteGroupModel(parent, control.specialSprite));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Imas.UI;

namespace ImasArchiveApp
{
    abstract class UIControlModel : UIElementModel
    {
        public abstract Control Control { get; }
        public string Name => string.IsNullOrWhiteSpace(Control.name) ? "(no name)" : Control.name;

        public static UIControlModel CreateModel(Control control)
        {
            return control switch
            {
                GroupControl gc => new UIGroupControlModel(gc),
                _ => new UITypedControlModel<Control>(control),
            };
        }
    }
    class UITypedControlModel<T> : UIControlModel where T : Control
    {
        protected readonly T _control;
        public override Control Control => _control;

        public UITypedControlModel(T control)
        {
            _control = control;
        }

    }
}

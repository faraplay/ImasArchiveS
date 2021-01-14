using System;
using System.Collections.Generic;
using System.Text;
using Imas.UI;

namespace ImasArchiveApp
{
    class UITypedControlModel<T> : UIControlModel where T : Control
    {
        protected readonly T _control;
        public override Control Control => _control;

        public UITypedControlModel(UISubcomponentModel parent, T control) : base(parent)
        {
            _control = control;
        }

    }
}

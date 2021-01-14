using System;
using System.Collections.Generic;
using System.Text;
using Imas.UI;

namespace ImasArchiveApp
{
    class UIGroupControlModel : UITypedControlModel<GroupControl>
    {
        public UIGroupControlModel(UISubcomponentModel parent, GroupControl control) : base(parent, control)
        {
            foreach (Control child in control.childControls)
            {
                Children.Add(UIControlModel.CreateModel(parent, child));
            }
        }
    }
}

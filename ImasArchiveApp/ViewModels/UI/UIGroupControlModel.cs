using System;
using System.Collections.Generic;
using System.Text;
using Imas.UI;

namespace ImasArchiveApp
{
    class UIGroupControlModel : UITypedControlModel<GroupControl>
    {
        public UIGroupControlModel(UISubcomponentModel subcomponent, UIElementModel parent, GroupControl control) : base(subcomponent, parent, control)
        {
            foreach (Control child in control.childControls)
            {
                Children.Add(UIControlModel.CreateModel(subcomponent, this, child));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Imas.UI;

namespace ImasArchiveApp
{
    class UIGroupControlModel : UITypedControlModel<GroupControl>
    {
        public UIGroupControlModel(GroupControl control) : base(control)
        {
            foreach (Control child in control.childControls)
            {
                Children.Add(UIControlModel.CreateModel(child));
            }
        }
    }
}

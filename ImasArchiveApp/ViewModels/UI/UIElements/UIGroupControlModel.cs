using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class UIGroupControlModel : UIControlModel
    {
        private readonly GroupControl groupControl;
        protected override Control Control => groupControl;
        public UIGroupControlModel(GroupControl control, UISubcomponentModel subcomponent, UIControlModel parent) : base(control, subcomponent, parent)
        {
            groupControl = control;
            foreach (Control child in groupControl.ChildControls)
            {
                Children.Add(CreateControlModel(child, subcomponent, this));
            }
        }
    }
}

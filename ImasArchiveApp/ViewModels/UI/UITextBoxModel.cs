using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class UITextBoxModel : UITypedControlModel<TextBox>
    {
        public UITextBoxModel(UISubcomponentModel parent, TextBox control) : base(parent, control)
        {
        }
    }
}

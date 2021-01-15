using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class UISpriteCollectionModel : UITypedControlModel<SpriteCollection>
    {
        public UISpriteCollectionModel(UISubcomponentModel parent, SpriteCollection control) : base(parent, control)
        {
            foreach (SpriteGroup child in control.childSpriteGroups)
            {
                Children.Add(new UISpriteGroupModel(parent, child));
            }
        }
    }
}

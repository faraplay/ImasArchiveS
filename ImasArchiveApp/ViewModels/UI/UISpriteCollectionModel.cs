using Imas.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImasArchiveApp
{
    class UISpriteCollectionModel : UITypedControlModel<SpriteCollection>
    {
    //    #region Properties

    //    public int E1
    //    {
    //        get => _control.e1;
    //        set
    //        {
    //            _control.e1 = value;
				//LoadActiveImages();
    //            OnPropertyChanged();
    //        }
    //    }
    //    public int E2
    //    {
    //        get => _control.e2;
    //        set
    //        {
    //            _control.e2 = value;
				//LoadActiveImages();
    //            OnPropertyChanged();
    //        }
    //    }

    //    #endregion

        public UISpriteCollectionModel(UISubcomponentModel subcomponent, UIElementModel parent, SpriteCollection control) : base(subcomponent, parent, control)
        {
            foreach (SpriteGroup child in control.childSpriteGroups)
            {
                Children.Add(new UISpriteGroupModel(subcomponent, this, child) { CurrentVisibility = false });
            }
        }
    }
}

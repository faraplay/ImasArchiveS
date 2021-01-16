using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class UISpriteCollectionModel : UITypedControlModel<SpriteCollection>
    {
        #region Properties

        public int E1
        {
            get => _control.e1;
            set
            {
                _control.e1 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int E2
        {
            get => _control.e2;
            set
            {
                _control.e2 = value;
				LoadImage();
                OnPropertyChanged();
            }
        }

        #endregion

        public UISpriteCollectionModel(UISubcomponentModel parent, SpriteCollection control) : base(parent, control)
        {
            foreach (SpriteGroup child in control.childSpriteGroups)
            {
                Children.Add(new UISpriteGroupModel(parent, child));
            }
        }
    }
}

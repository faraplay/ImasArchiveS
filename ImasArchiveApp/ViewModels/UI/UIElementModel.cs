using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImasArchiveApp
{
    public class UIElementModel : FileModel
    {
        public ObservableCollection<UIElementModel> Children { get; set; }

        public UIElementModel(IReport parent, string name) : base(parent, name)
        {
            Children = new ObservableCollection<UIElementModel>();
        }
    }
}

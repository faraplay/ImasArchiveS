using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImasArchiveApp
{
    public abstract class UIElementModel : FileModel
    {
        protected readonly UISubcomponentModel parent;
        public ObservableCollection<UIElementModel> Children { get; set; }
        public abstract string ModelName { get; }

        protected UIElementModel(UISubcomponentModel parent, string name) : base(parent, name)
        {
            this.parent = parent;
            Children = new ObservableCollection<UIElementModel>();
        }
    }
}

using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

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

        public void InsertControl(int index, Control control)
        {
            groupControl.ChildControls.Insert(index, control);
            groupControl.ChildCount++;
            if (HasSpecialSprite)
                index++;
            Children.Insert(index, CreateControlModel(control, subcomponent, this));
        }

        public void InsertNewControl<T>(int index) where T : Control, new() => InsertControl(index, new T());
        public void AddNewControl<T>() where T : Control, new() => InsertNewControl<T>(groupControl.ChildControls.Count);

        private RelayCommand _addGroupControlCommand;
        public ICommand AddGroupControlCommand
        {
            get
            {
                if (_addGroupControlCommand == null)
                    _addGroupControlCommand = new RelayCommand(
                        _ => AddNewControl<GroupControl>());
                return _addGroupControlCommand;
            }
        }
    }
}

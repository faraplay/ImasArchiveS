using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class UIGroupControlModel : UIControlModel
    {
        private readonly GroupControl groupControl;
        protected override Control Control => groupControl;
        public UIGroupControlModel(GroupControl control, UISubcomponentModel subcomponent, UIGroupControlModel parent) : base(control, subcomponent, parent)
        {
            groupControl = control;
            foreach (Control child in groupControl.ChildControls)
            {
                Children.Add(CreateControlModel(child, subcomponent, this));
            }
        }

        public int IndexOf(UIControlModel controlModel)
        {
            int index = Children.IndexOf(controlModel);
            if (index == -1)
                return -1;
            if (HasSpecialSprite)
                index--;
            return index;
        }

        public void InsertControl(int index, Control control)
        {
            groupControl.ChildControls.Insert(index, control);
            if (HasSpecialSprite)
                index++;
            Children.Insert(index, CreateControlModel(control, subcomponent, this));
        }

        public void RemoveControl(UIControlModel controlModel)
        {
            int index = Children.IndexOf(controlModel);
            if (index == -1)
                return;
            Children.RemoveAt(index);
            if (HasSpecialSprite)
                index--;
            groupControl.ChildControls.RemoveAt(index);
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

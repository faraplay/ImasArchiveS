using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
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

        private RelayCommand _addTextBoxCommand;
        public ICommand AddTextBoxCommand
        {
            get
            {
                if (_addTextBoxCommand == null)
                    _addTextBoxCommand = new RelayCommand(
                        _ => AddNewControl<TextBox>());
                return _addTextBoxCommand;
            }
        }
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
        private RelayCommand _addRotatableGroupControlCommand;
        public ICommand AddRotatableGroupControlCommand
        {
            get
            {
                if (_addRotatableGroupControlCommand == null)
                    _addRotatableGroupControlCommand = new RelayCommand(
                        _ => AddNewControl<RotatableGroupControl>());
                return _addRotatableGroupControlCommand;
            }
        }
        private RelayCommand _addSpriteCollectionCommand;
        public ICommand AddSpriteCollectionCommand
        {
            get
            {
                if (_addSpriteCollectionCommand == null)
                    _addSpriteCollectionCommand = new RelayCommand(
                        _ => AddNewControl<SpriteCollection>());
                return _addSpriteCollectionCommand;
            }
        }

        public void Paste()
        {
            try
            {
                Control control = (Control)Base64.FromBase64(Clipboard.GetText(), typeof(Control));
                InsertControl(groupControl.ChildControls.Count, control);
            }
            catch (System.FormatException)
            {
            }
            catch (System.IO.EndOfStreamException)
            { }
        }
        private RelayCommand _pasteCommand;
        public ICommand PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                    _pasteCommand = new RelayCommand(
                        _ => Paste());
                return _pasteCommand;
            }
        }
    }
}

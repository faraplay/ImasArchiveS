using System;

namespace ImasArchiveApp
{
    public class PropertyListItemModel : NotifyPropertyChanged
    {
        public PropertyListModel ListModel { get; }
        private int index;

        public PropertyListItemModel(PropertyListModel listModel, int index)
        {
            ListModel = listModel;
            this.index = index;
        }

        public object PropertyValue
        {
            get => ListModel.PropertyList[index];
            set
            {
                ListModel.PropertyList[index] = value;
                OnPropertyChanged();
            }
        }
        public Type PropertyType => PropertyValue.GetType();
    }
}

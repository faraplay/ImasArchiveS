using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace ImasArchiveApp
{
    public class PropertyListModel : PropertyModel
    {
        public ObservableCollection<PropertyListItemModel> ListItemModels { get; }
        public IList PropertyList => PropertyValue as IList;
        public PropertyListModel(PropertyInfo propertyInfo, object element) : base(propertyInfo, element)
        {
            ListItemModels = new ObservableCollection<PropertyListItemModel>();
            for (int i = 0; i < PropertyList.Count; ++i)
            {
                PropertyListItemModel listItem = new PropertyListItemModel(this, i);
                listItem.PropertyChanged += FireListPropertyChanged;
                ListItemModels.Add(listItem);
            }
        }
        public event PropertyChangedEventHandler ListPropertyChanged;
        private void FireListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ListPropertyChanged.Invoke(sender, e);
        }
    }
}

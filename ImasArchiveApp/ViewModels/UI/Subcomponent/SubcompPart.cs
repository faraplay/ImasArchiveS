using Imas.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ImasArchiveApp
{
    public abstract class SubcompPart<T> : NotifyPropertyChanged where T : IElementModel
    {
        public UISubcomponentModel subcomponentModel;
        public ObservableCollection<PropertyModel> UIProperties { get; }
        private T _selectedModel;
        public T SelectedModel
        {
            get => _selectedModel;
            set
            {
                UnsubscribePropertyEvents();
                _selectedModel = value;
                RefreshPropertiesList();
                OnPropertyChanged();
            }
        }
        protected abstract int VisibilityIndex { get; }
        public Visibility Visibility => subcomponentModel.SelectedIndex == VisibilityIndex ? Visibility.Visible : Visibility.Collapsed;
        private void ChangeVisibility(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UISubcomponentModel.SelectedIndex))
                OnPropertyChanged(nameof(Visibility));
        }

        public SubcompPart(UISubcomponentModel subcomponentModel)
        {
            this.subcomponentModel = subcomponentModel;
            UIProperties = new ObservableCollection<PropertyModel>();
            subcomponentModel.PropertyChanged += ChangeVisibility;
        }

        private void RefreshPropertiesList()
        {
            UIProperties.Clear();
            object element = SelectedModel.Element;
            if (element == null)
                return;
            foreach ((var property, var attr) in element.GetType()
                .GetProperties()
                .Select(p => (p, p.GetCustomAttributes(typeof(ListedAttribute), false)))
                .Where(tuple => tuple.Item2.Length > 0)
                .Select(tuple => (tuple.p, (ListedAttribute)tuple.Item2[0]))
                .OrderBy(tuple => tuple.Item2.Order))
            {
                PropertyModel propertyModel = PropertyModel.CreatePropertyModel(property, element);
                if (propertyModel is PropertyListModel listModel)
                {
                    listModel.ListPropertyChanged += SelectedModel.PropertyChangedHandler;
                }
                else
                {
                    propertyModel.PropertyChanged += SelectedModel.PropertyChangedHandler;
                }
                UIProperties.Add(propertyModel);
            }
        }

        private void UnsubscribePropertyEvents()
        {
            foreach (PropertyModel propertyModel in UIProperties)
            {
                if (propertyModel is PropertyListModel listModel)
                {
                    listModel.ListPropertyChanged -= SelectedModel.PropertyChangedHandler;
                }
                else
                {
                    propertyModel.PropertyChanged -= SelectedModel.PropertyChangedHandler;
                }
            }
        }
    }
}

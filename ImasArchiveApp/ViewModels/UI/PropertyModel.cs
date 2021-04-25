using Imas.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ImasArchiveApp
{
    public class PropertyModel : INotifyPropertyChanged
    {
        public PropertyInfo PropertyInfo { get; }
        private ListedAttribute Attribute { get; }
        public override string ToString()
        {
            return $"{ PropertyName }: { PropertyValue }";
        }
        public string PropertyName => PropertyInfo.Name;
        public object Element { get; }
        public Type PropertyType => PropertyInfo.PropertyType;
        public object PropertyValue
        {
            get => PropertyInfo.GetValue(Element);
            set
            {
                PropertyInfo.SetValue(Element, value);
                PropertyChanged?.Invoke(Element, new PropertyChangedEventArgs(PropertyName));
            }
        }
        public bool PropertyStringMultiline => Attribute.StringMultiline;
        public bool PropertyCondition
        {
            get
            {
                if (Attribute.ConditionProperty == null)
                    return true;
                PropertyInfo conditionPropertyInfo = PropertyInfo.DeclaringType.GetProperty(Attribute.ConditionProperty);
                if (conditionPropertyInfo == null)
                    return false;
                if (!(conditionPropertyInfo.GetValue(Element) is bool value))
                    return false;
                return value;
            }
        }


        public PropertyModel(PropertyInfo propertyInfo, object element)
        {
            PropertyInfo = propertyInfo;
            Attribute = (ListedAttribute)propertyInfo.GetCustomAttributes(typeof(ListedAttribute), false)[0];
            Element = element;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static PropertyModel CreatePropertyModel(PropertyInfo propertyInfo, object element)
        {
            if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return new PropertyListModel(propertyInfo, element);
            }
            return new PropertyModel(propertyInfo, element);
        }
    }
}

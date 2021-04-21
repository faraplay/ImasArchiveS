using Imas.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace ImasArchiveApp
{
    public class PropertyModel : INotifyPropertyChanged
    {
        private PropertyInfo PropertyInfo { get; }
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
                OnPropertyChanged();
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
    }
}

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
    public class UIElementPropertyModel : INotifyPropertyChanged
    {
        private PropertyInfo PropertyInfo { get; }
        private ListedAttribute Attribute { get; }
        public override string ToString()
        {
            return $"{ PropertyName }: { PropertyValue }";
        }
        public string PropertyName => PropertyInfo.Name;
        public Imas.UI.UIElement Element { get; }
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


        public UIElementPropertyModel(PropertyInfo propertyInfo, Imas.UI.UIElement element)
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

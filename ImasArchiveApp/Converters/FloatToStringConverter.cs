using System;
using System.Globalization;
using System.Windows.Data;

namespace ImasArchiveApp
{
    class FloatToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (float.TryParse((string)value, out float result))
                return result;
            else
                return null;
        }
    }
}

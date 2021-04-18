using System;
using System.Globalization;
using System.Windows.Data;

namespace ImasArchiveApp
{
    class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse((string)value, out int result))
                return result;
            else
                return null;
        }
    }
}

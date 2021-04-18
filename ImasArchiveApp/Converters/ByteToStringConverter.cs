using System;
using System.Globalization;
using System.Windows.Data;

namespace ImasArchiveApp
{
    class ByteToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (byte.TryParse((string)value, out byte result))
                return result;
            else
                return null;
        }
    }
}

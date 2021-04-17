using System;
using System.Globalization;
using System.Windows.Data;

namespace ImasArchiveApp
{
    class UIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is uint uvalue))
                return value.ToString();
            return $"0x{uvalue.ToString("X8")}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string svalue))
                return null;
            if (svalue.StartsWith("0x"))
                svalue = svalue[2..];
            if (uint.TryParse(svalue, NumberStyles.HexNumber, culture, out uint result))
                return result;
            else
                return null;
        }
    }
}

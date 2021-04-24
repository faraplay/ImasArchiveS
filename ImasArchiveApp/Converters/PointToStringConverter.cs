using Imas.UI;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ImasArchiveApp
{
    class PointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Point p))
                return null;
            return $"{p.X}, {p.Y}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string s))
                return null;
            int pos = s.IndexOf(',');
            if (pos < 0)
                return null;
            if (!float.TryParse(s[..pos], out float x))
                return null;
            if (!float.TryParse(s[(pos + 1)..], out float y))
                return null;
            return new Point() { X = x, Y = y };
        }
    }
}

using Imas.Gtf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ImasArchiveApp
{
    class VerticalAlignmentToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is VerticalAlignment alignment))
                return null;
            return (int)alignment / 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int i))
                return null;
            if (i < 0 || i > 3)
                return null;
            return (VerticalAlignment)(i * 4);
        }
    }
}

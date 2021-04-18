using Imas.Gtf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ImasArchiveApp
{
    class HorizontalAlignmentToInt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is HorizontalAlignment alignment))
                return null;
            return (int)alignment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int i))
                return null;
            if (i < 0 || i > 3)
                return null;
            return (HorizontalAlignment)i;
        }
    }
}

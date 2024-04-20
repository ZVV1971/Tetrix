using System;
using System.Globalization;
using System.Windows.Data;

namespace TetrisFigures.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value != null && !((string)value).Equals(string.Empty))
            {
                return System.Windows.Visibility.Visible;
            }

            return System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
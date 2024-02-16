using System;
using System.Globalization;
using System.Windows.Data;

namespace TetrisFigures.Converters
{
    public class MultiBoolToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type sourceType, object parameter, CultureInfo culture)
        {
            foreach(object o in values)
            {
                if ((bool)o)
                {
                    return System.Windows.Visibility.Visible;
                }
            }
            return System.Windows.Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

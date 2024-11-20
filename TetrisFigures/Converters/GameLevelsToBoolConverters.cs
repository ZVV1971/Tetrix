using System;
using System.Globalization;
using System.Windows.Data;
using TetrisFigures.Auxiliary;

namespace TetrisFigures.Converters
{
    public class GameLevelEasyToBoolConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (GameComplexity)value == GameComplexity.Easy;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GameComplexity.Easy;
        }
    }

    public class GameLevelAverageToBoolConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (GameComplexity)value == GameComplexity.Average;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GameComplexity.Average;
        }
    }

    public class GameLevelHardToBoolConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (GameComplexity)value == GameComplexity.Hard;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GameComplexity.Hard;
        }
    }
}
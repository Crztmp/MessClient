using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ClientApp01.Converters
{
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value
                ? new SolidColorBrush(Color.FromRgb(0, 132, 255))
                : new SolidColorBrush(Color.FromRgb(234, 234, 234));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
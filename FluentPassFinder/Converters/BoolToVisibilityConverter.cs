using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FluentPassFinder.Converters
{
    internal class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = false;
            if (value is bool b)
            {
                boolValue = b;
            }

            var invertValue = false;
            if (parameter is string stringParameter)
            {
                if (stringParameter.Equals("invert"))
                {
                    invertValue = true;
                }
            }

            if (invertValue)
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

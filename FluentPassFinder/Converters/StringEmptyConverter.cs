using Avalonia.Data.Converters;
using System.Globalization;

namespace FluentPassFinder.Converters
{
    internal class StringEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = string.IsNullOrEmpty(value as string);
            return parameter as string == "Invert" ? !result : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace FluentPassFinder.Converters
{
    public class BoolToCaretBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is true)
                return Brushes.Transparent;

            return AvaloniaProperty.UnsetValue; // fallback to default theme brush
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

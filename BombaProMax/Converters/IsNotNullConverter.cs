using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a value to true if it's not null, false otherwise.
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

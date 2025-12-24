using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a boolean to a subtle selection background color.
/// True = Light blue-gray (selected), False = Transparent (not selected)
/// </summary>
public class BoolToSelectionColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            // Selected: subtle light blue-gray matching the neumorphic design
            return Color.FromArgb("#E8ECF1");
        }
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

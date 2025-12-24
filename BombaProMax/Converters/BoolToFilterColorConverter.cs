using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a boolean to a filter button background color.
/// True = Primary blue (active), False = Light gray (inactive)
/// </summary>
public class BoolToFilterColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            // Active: Primary blue
            return Color.FromArgb("#4A8FBF");
        }
        // Inactive: Light gray neumorphic
        return Color.FromArgb("#E8ECF1");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean to a filter button text color.
/// True = White (active), False = Secondary gray (inactive)
/// </summary>
public class BoolToFilterTextColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            // Active: White text
            return Colors.White;
        }
        // Inactive: Secondary text color
        return Color.FromArgb("#5A6068");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

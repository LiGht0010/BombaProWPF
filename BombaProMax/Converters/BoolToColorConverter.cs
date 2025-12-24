using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a boolean to a background color.
/// Used for EstFacture status: True = Green (Facturé), False = Orange (Non Facturé)
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // For EstFacture: true = green (facturé), false = orange (non facturé)
            return boolValue ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF9800");
        }
        return Color.FromArgb("#FF9800"); // Default to orange
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

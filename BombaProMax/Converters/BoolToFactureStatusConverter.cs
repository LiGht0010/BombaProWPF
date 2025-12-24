using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a boolean (EstFacture) to status text for display.
/// True = "Facturé", False = "Non Facturé"
/// </summary>
public class BoolToFactureStatusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isFacture)
        {
            return isFacture ? "Facturé" : "Non Facturé";
        }
        return "Non Facturé";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

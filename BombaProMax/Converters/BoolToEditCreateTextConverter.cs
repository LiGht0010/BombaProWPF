using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a boolean to "Modifier" or "Configurer" text for edit/create buttons.
/// True = "?? Modifier", False = "? Configurer"
/// </summary>
public class BoolToEditCreateTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasData && hasData)
        {
            return "Modifier";
        }
        return "Configurer";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

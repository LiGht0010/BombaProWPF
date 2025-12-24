using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Returns true if the status is not "Payée" (for showing pay button and enabling checkboxes).
/// </summary>
public class IsNotPaidConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            // Check for various forms of "Payée" to handle encoding issues
            return !status.Equals("Payée", StringComparison.OrdinalIgnoreCase) 
                && !status.Equals("Payee", StringComparison.OrdinalIgnoreCase);
        }
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

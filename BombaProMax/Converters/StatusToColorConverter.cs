using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a status string to a background color
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            var normalized = status.ToLower().Trim();
            
            // Handle "Payée" status (green)
            if (normalized == "payée" || normalized == "payee")
                return Color.FromArgb("#27AE60");
            
            // Handle "Non Payée" status (orange)
            if (normalized.Contains("non") && (normalized.Contains("payée") || normalized.Contains("payee")))
                return Color.FromArgb("#E67E22");
            
            // Handle "Annulée" status (red)
            if (normalized == "annulée" || normalized == "annulee")
                return Color.FromArgb("#E74C3C");
            
            // Handle "En attente" status (yellow)
            if (normalized.Contains("attente"))
                return Color.FromArgb("#F39C12");
        }
        
        return Color.FromArgb("#95A5A6"); // Gray default
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

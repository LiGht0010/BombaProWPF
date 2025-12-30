using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a balance value to a color.
/// Balance = TotalPaye - TotalCredit
/// Positive balance (client overpaid/has credit) = Green
/// Negative balance (client owes money) = Red
/// </summary>
public class BalanceToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal balance)
        {
            // Positive balance means client overpaid or has credit (green)
            // Negative balance means client owes money (red)
            return balance >= 0 
                ? Color.FromArgb("#2E7D32")  // Green for overpaid/balanced
                : Color.FromArgb("#C62828"); // Red for owing money
        }

        return Color.FromArgb("#666666"); // Default gray
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

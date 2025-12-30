using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Converts a balance value to formatted text with +/- sign.
/// Positive balance displays with "+" prefix.
/// Negative balance displays with "-" prefix (default behavior).
/// </summary>
public class BalanceToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal balance)
        {
            if (balance > 0)
            {
                return $"+{balance:N2} MAD";
            }
            else if (balance < 0)
            {
                return $"{balance:N2} MAD";
            }
            else
            {
                return "0.00 MAD";
            }
        }

        return "0.00 MAD";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

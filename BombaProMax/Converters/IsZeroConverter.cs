using System.Globalization;

namespace BombaProMax.Converters;

public class IsZeroConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return true;

        if (int.TryParse(value.ToString(), out int intValue))
        {
            return intValue == 0;
        }

        if (decimal.TryParse(value.ToString(), out decimal decimalValue))
        {
            return decimalValue == 0;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

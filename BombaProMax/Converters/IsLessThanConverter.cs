using System.Globalization;

namespace BombaProMax.Converters;

public class IsLessThanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        if (decimal.TryParse(value.ToString(), out decimal numericValue) &&
            decimal.TryParse(parameter.ToString(), out decimal threshold))
        {
            return numericValue < threshold;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

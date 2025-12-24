using System.Globalization;

namespace BombaProMax.Converters;

public class FirstLetterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
        {
            return text.Trim()[0].ToString().ToUpper();
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

using Microsoft.Maui.Graphics;
using System.Globalization;

namespace BombaProMax.Converters
{
    /// <summary>
    /// Multi-value converter for citerne status color
    /// </summary>
    public class CiterneStatusColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 0)
            {
                if (values[0] is int citerneId && citerneId > 0)
                {
                    return Color.FromArgb("#27AE60");  // Green - Assigned
                }
                else if (values[0] is null)
                {
                    return Color.FromArgb("#9E9E9E");  // Gray - Not assigned
                }
            }
            return Color.FromArgb("#9E9E9E");  // Default gray
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Simple single-value converter for citerne status color (for use with regular Binding)
    /// </summary>
    public class CiterneIdToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int citerneId && citerneId > 0)
            {
                return Color.FromArgb("#5EAA8D");  // Green - Has citerne
            }
            return Color.FromArgb("#E8A84C");  // Orange/Warning - No citerne
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

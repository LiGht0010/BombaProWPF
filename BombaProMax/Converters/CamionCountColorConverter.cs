using Microsoft.Maui.Graphics;
using System.Globalization;

namespace BombaProMax.Converters
{
    public class CamionCountColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 0)
            {
                if (values[0] is int count)
                {
                    if (count > 0)
                    {
                        return Color.FromArgb("#27AE60");  // Green - Has camions
                    }
                    else
                    {
                        return Color.FromArgb("#9E9E9E");  // Gray - No camions
                    }
                }
            }
            return Color.FromArgb("#9E9E9E");  // Default gray
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.Maui.Graphics;
using System.Globalization;

namespace BombaProMax.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "actif" => Color.FromArgb("#27AE60"),  // Green
                    "inactif" => Color.FromArgb("#C62828"),  // Red
                    "en attente" => Color.FromArgb("#FF9800"),  // Orange
                    "suspendu" => Color.FromArgb("#9E9E9E"),  // Gray
                    _ => Color.FromArgb("#336860")  // Default brand color
                };
            }
            return Color.FromArgb("#336860");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

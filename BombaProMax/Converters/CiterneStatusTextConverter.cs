using System.Globalization;

namespace BombaProMax.Converters
{
    public class CiterneStatusTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length >= 2)
            {
                if (values[0] is int citerneId && citerneId > 0)
                {
                    if (values[1] is decimal capacite && capacite > 0)
                    {
                        return $"{capacite}L";
                    }
                    return "Assignée";
                }
            }
            return "Non assignée";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Globalization;

namespace BombaProMax.Converters;

/// <summary>
/// Compares the bound value with the converter parameter.
/// Returns a selection color if they are equal (by ID comparison), transparent otherwise.
/// Used for highlighting selected items in lists.
/// </summary>
public class EqualityToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Colors.Transparent;

        // Compare by reference or by ID if available
        bool isEqual = ReferenceEquals(value, parameter);
        
        if (!isEqual && value is BombaProMax.Models.PeriodeDto currentPeriode && 
            parameter is BombaProMax.Models.PeriodeDto selectedPeriode)
        {
            isEqual = currentPeriode.PeriodeID == selectedPeriode.PeriodeID;
        }

        return isEqual ? Color.FromArgb("#E3F2FD") : Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

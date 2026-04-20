using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BombaProMaxWPF.Models;

/// <summary>
/// Model for capturing pump meter readings during a period.
/// Used in the PeriodeCreatePopup to allow users to enter final counter values.
/// </summary>
public class PompeReadingModel : INotifyPropertyChanged
{
    public int PompeID { get; set; }
    public string PompeNumero { get; set; } = string.Empty;
    public int? ReservoirID { get; set; }
    public string? ReservoirNumero { get; set; }
    public int? ProduitID { get; set; }
    public string? ProduitNom { get; set; }
    public decimal PrixCarburant { get; set; }

    // Starting values (read-only, from pump's current state)
    public decimal CompteurElecDebut { get; set; }
    public decimal CompteurMecaDebut { get; set; }

    // Final values (user input)
    private string _compteurElecFin = string.Empty;
    public string CompteurElecFin
    {
        get => _compteurElecFin;
        set
        {
            if (_compteurElecFin != value)
            {
                _compteurElecFin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(QuantiteVendue));
                OnPropertyChanged(nameof(PrixTotal));
            }
        }
    }

    private string _compteurMecaFin = string.Empty;
    public string CompteurMecaFin
    {
        get => _compteurMecaFin;
        set
        {
            if (_compteurMecaFin != value)
            {
                _compteurMecaFin = value;
                OnPropertyChanged();
            }
        }
    }

    // Calculated properties
    public decimal QuantiteVendue
    {
        get
        {
            if (decimal.TryParse(CompteurElecFin, out decimal elecFin))
            {
                return Math.Max(0, elecFin - CompteurElecDebut);
            }
            return 0;
        }
    }

    public decimal PrixTotal => QuantiteVendue * PrixCarburant;

    // For validation - check if this pump has valid readings
    public bool HasValidReadings
    {
        get
        {
            if (!decimal.TryParse(CompteurElecFin, out decimal elecFin))
                return false;

            // Final must be >= start
            return elecFin >= CompteurElecDebut;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

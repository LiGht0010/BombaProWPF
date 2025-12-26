using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMax.ViewModels;

public partial class RapportViewModel : ObservableObject
{
    private readonly RapportService _rapportService;

    #region Observable Properties - Loading State

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _periodeLabel = "Selectionnez une periode";

    #endregion

    #region Observable Properties - Filters

    [ObservableProperty]
    private DateTime? _dateSpecifique;

    [ObservableProperty]
    private int _selectedMois;

    [ObservableProperty]
    private int _selectedAnnee;

    #endregion

    #region Observable Properties - Ventes Data

    [ObservableProperty]
    private decimal _totalVentesCarburant;

    [ObservableProperty]
    private decimal _totalQuantiteCarburant;

    [ObservableProperty]
    private decimal _totalVentesLubArticles;

    [ObservableProperty]
    private int _totalQuantiteLubArticles;

    [ObservableProperty]
    private decimal _totalVentes;

    #endregion

    #region Observable Properties - Depenses Data

    [ObservableProperty]
    private decimal _totalDepenses;

    [ObservableProperty]
    private int _nombreDepenses;

    #endregion

    #region Observable Properties - Stock Data

    [ObservableProperty]
    private decimal _totalStockCarburantLitres;

    [ObservableProperty]
    private int _totalStockProduits;

    [ObservableProperty]
    private decimal _totalAchatsPeriode;

    #endregion

    #region Collections

    public ObservableCollection<RapportVenteCarburantProduitDto> VentesCarburantParProduit { get; } = [];
    public ObservableCollection<RapportVenteLubArticleProduitDto> VentesLubArticlesParProduit { get; } = [];
    public ObservableCollection<RapportDepenseCategorieDto> DepensesParCategorie { get; } = [];
    public ObservableCollection<RapportDepenseDetailDto> DepensesDetails { get; } = [];
    public ObservableCollection<RapportStockReservoirDto> StockCarburant { get; } = [];
    public ObservableCollection<RapportStockProduitDto> StockProduits { get; } = [];
    public ObservableCollection<RapportAchatProduitDto> AchatsParProduit { get; } = [];

    public List<int> MoisList { get; } = Enumerable.Range(1, 12).ToList();
    public List<int> AnneeList { get; } = Enumerable.Range(2020, DateTime.Now.Year - 2020 + 2).ToList();
    public List<string> MoisNoms { get; } =
    [
        "Janvier", "Fevrier", "Mars", "Avril", "Mai", "Juin",
        "Juillet", "Aout", "Septembre", "Octobre", "Novembre", "Decembre"
    ];

    #endregion

    #region Constructor

    public RapportViewModel(RapportService rapportService)
    {
        _rapportService = rapportService;

        // Default to current month
        var now = DateTime.Now;
        SelectedMois = now.Month;
        SelectedAnnee = now.Year;
    }

    #endregion

    #region Commands

    [RelayCommand]
    public async Task LoadRapportsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Determine filter to use
            DateOnly? date = null;
            string? month = null;

            if (DateSpecifique.HasValue)
            {
                date = DateOnly.FromDateTime(DateSpecifique.Value);
                PeriodeLabel = date.Value.ToString("dd/MM/yyyy");
            }
            else if (SelectedMois > 0 && SelectedAnnee > 0)
            {
                month = $"{SelectedAnnee}-{SelectedMois:D2}";
                PeriodeLabel = $"{MoisNoms[SelectedMois - 1]} {SelectedAnnee}";
            }
            else
            {
                PeriodeLabel = "Toutes les periodes";
            }

            // Load all reports
            var rapport = await _rapportService.GetRapportCompletAsync(date, month);

            // Populate Ventes
            PopulateVentes(rapport.Ventes);

            // Populate Depenses
            PopulateDepenses(rapport.Depenses);

            // Populate Stock
            PopulateStock(rapport.Stock);

            Debug.WriteLine($"[RapportViewModel] Loaded rapport for: {PeriodeLabel}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[RapportViewModel] Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApplyFilterAsync()
    {
        await LoadRapportsAsync();
    }

    [RelayCommand]
    public async Task ClearFilterAsync()
    {
        DateSpecifique = null;
        var now = DateTime.Now;
        SelectedMois = now.Month;
        SelectedAnnee = now.Year;
        await LoadRapportsAsync();
    }

    [RelayCommand]
    public async Task PrintRapportAsync()
    {
        // Placeholder for PDF generation
        await Application.Current!.MainPage!.DisplayAlert(
            "Information",
            "La generation de PDF sera implementee prochainement.",
            "OK");
    }

    #endregion

    #region Private Methods

    private void PopulateVentes(RapportVentesDto ventes)
    {
        TotalVentesCarburant = ventes.TotalVentesCarburant;
        TotalQuantiteCarburant = ventes.TotalQuantiteCarburant;
        TotalVentesLubArticles = ventes.TotalVentesLubArticles;
        TotalQuantiteLubArticles = ventes.TotalQuantiteLubArticles;
        TotalVentes = ventes.TotalVentes;

        VentesCarburantParProduit.Clear();
        foreach (var item in ventes.VentesCarburantParProduit)
        {
            VentesCarburantParProduit.Add(item);
        }

        VentesLubArticlesParProduit.Clear();
        foreach (var item in ventes.VentesLubArticlesParProduit)
        {
            VentesLubArticlesParProduit.Add(item);
        }
    }

    private void PopulateDepenses(RapportDepensesDto depenses)
    {
        TotalDepenses = depenses.TotalDepenses;
        NombreDepenses = depenses.NombreDepenses;

        DepensesParCategorie.Clear();
        foreach (var item in depenses.DepensesParCategorie)
        {
            DepensesParCategorie.Add(item);
        }

        DepensesDetails.Clear();
        foreach (var item in depenses.DepensesDetails)
        {
            DepensesDetails.Add(item);
        }
    }

    private void PopulateStock(RapportStockDto stock)
    {
        TotalStockCarburantLitres = stock.TotalStockCarburantLitres;
        TotalStockProduits = stock.TotalStockProduits;
        TotalAchatsPeriode = stock.TotalAchatsPeriode;

        StockCarburant.Clear();
        foreach (var item in stock.StockCarburant)
        {
            StockCarburant.Add(item);
        }

        StockProduits.Clear();
        foreach (var item in stock.StockProduits)
        {
            StockProduits.Add(item);
        }

        AchatsParProduit.Clear();
        foreach (var item in stock.AchatsParProduit)
        {
            AchatsParProduit.Add(item);
        }
    }

    #endregion
}

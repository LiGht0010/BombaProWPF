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
    private readonly RapportPdfService _rapportPdfService;

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
    private int _selectedMoisIndex = -1; // -1 = no selection (index in picker)

    [ObservableProperty]
    private int? _selectedAnnee; // nullable to allow "no selection"

    /// <summary>
    /// Tracks if we're using date-specific filter mode (true) or month filter mode (false/null)
    /// </summary>
    [ObservableProperty]
    private bool _isDateSpecifiqueMode;

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
    private decimal _totalVentesServices;

    [ObservableProperty]
    private int _totalQuantiteServices;

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

    #region Observable Properties - Jaugeage Analyse Data

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoJaugeageData))]
    private bool _hasJaugeageData;

    [ObservableProperty]
    private string? _jaugeageAnalyseMessage;

    [ObservableProperty]
    private string? _jaugeagePeriodeAnalyse;

    [ObservableProperty]
    private string? _jaugeageActuelInfo;

    [ObservableProperty]
    private string? _jaugeagePrecedentInfo;

    /// <summary>
    /// Inverted HasJaugeageData for binding to "no data" message visibility.
    /// </summary>
    public bool HasNoJaugeageData => !HasJaugeageData;

    #endregion

    #region Collections

    public ObservableCollection<RapportVenteCarburantProduitDto> VentesCarburantParProduit { get; } = [];
    public ObservableCollection<RapportVenteLubArticleProduitDto> VentesLubArticlesParProduit { get; } = [];
    public ObservableCollection<RapportVenteServiceDto> VentesServicesParService { get; } = [];
    public ObservableCollection<RapportDepenseCategorieDto> DepensesParCategorie { get; } = [];
    public ObservableCollection<RapportDepenseDetailDto> DepensesDetails { get; } = [];
    public ObservableCollection<RapportStockReservoirDto> StockCarburant { get; } = [];
    public ObservableCollection<RapportStockProduitDto> StockProduits { get; } = [];
    public ObservableCollection<RapportAchatProduitDto> AchatsParProduit { get; } = [];
    public ObservableCollection<RapportJaugeageReservoirComparisonDto> JaugeageComparaisons { get; } = [];

    public List<int> MoisList { get; } = Enumerable.Range(1, 12).ToList();
    public List<int?> AnneeList { get; }
    
    public List<string> MoisNomsAvecVide { get; } =
    [
        "-- Tous --",
        "Janvier", "Fevrier", "Mars", "Avril", "Mai", "Juin",
        "Juillet", "Aout", "Septembre", "Octobre", "Novembre", "Decembre"
    ];
    
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
        _rapportPdfService = new RapportPdfService();

        // Build year list with null option for "all years"
        var years = new List<int?> { null }; // null = "-- Tous --"
        years.AddRange(Enumerable.Range(2020, DateTime.Now.Year - 2020 + 2).Cast<int?>());
        AnneeList = years;

        // Default to current month (index 0 = "-- Tous --", so month index = month number)
        var now = DateTime.Now;
        SelectedMoisIndex = now.Month; // Index 1-12 maps to Jan-Dec
        SelectedAnnee = now.Year;
        IsDateSpecifiqueMode = false;
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

            // Determine filter to use based on mode
            DateOnly? date = null;
            string? month = null;

            if (IsDateSpecifiqueMode && DateSpecifique.HasValue)
            {
                // Date-specific mode
                date = DateOnly.FromDateTime(DateSpecifique.Value);
                PeriodeLabel = date.Value.ToString("dd/MM/yyyy");
            }
            else if (!IsDateSpecifiqueMode && SelectedMoisIndex > 0 && SelectedAnnee.HasValue)
            {
                // Month mode (index 0 = "-- Tous --", so valid month is 1-12)
                month = $"{SelectedAnnee.Value}-{SelectedMoisIndex:D2}";
                PeriodeLabel = $"{MoisNoms[SelectedMoisIndex - 1]} {SelectedAnnee.Value}";
            }
            else
            {
                // No filter
                PeriodeLabel = "Toutes les periodes";
            }

            // Load all reports including jaugeage analysis (all with same filter)
            var rapportTask = _rapportService.GetRapportCompletAsync(date, month);
            var jaugeageTask = _rapportService.GetRapportJaugeageAnalyseAsync(date, month);

            await Task.WhenAll(rapportTask, jaugeageTask);

            var rapport = await rapportTask;
            var jaugeageAnalyse = await jaugeageTask;

            // Populate Ventes
            PopulateVentes(rapport.Ventes);

            // Populate Depenses
            PopulateDepenses(rapport.Depenses);

            // Populate Stock
            PopulateStock(rapport.Stock);

            // Populate Jaugeage Analyse
            PopulateJaugeageAnalyse(jaugeageAnalyse);

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
        // Reset to current month mode
        DateSpecifique = null;
        IsDateSpecifiqueMode = false;
        var now = DateTime.Now;
        SelectedMoisIndex = now.Month; // Index 1-12
        SelectedAnnee = now.Year;
        await LoadRapportsAsync();
    }

    /// <summary>
    /// Called when user selects a specific date - switches to date mode
    /// </summary>
    public void OnDateSpecifiqueSelected(DateTime selectedDate)
    {
        DateSpecifique = selectedDate;
        IsDateSpecifiqueMode = true;
        // Clear month selection visual feedback
        SelectedMoisIndex = 0; // "-- Tous --"
        SelectedAnnee = null;
    }

    /// <summary>
    /// Called when user changes month/year picker - switches to month mode
    /// </summary>
    public void OnMonthYearChanged()
    {
        if (SelectedMoisIndex > 0 || SelectedAnnee.HasValue)
        {
            IsDateSpecifiqueMode = false;
            DateSpecifique = null;
        }
    }

    [RelayCommand]
    public async Task PrintRapportAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Build PDF data from current ViewModel state
            var pdfData = BuildRapportPdfData();

            // Generate PDF using dedicated RapportPdfService
            var filePath = await _rapportPdfService.GenerateRapportReportAsync(pdfData);

            Debug.WriteLine($"[RapportViewModel] PDF generated: {filePath}");

            // Open the PDF file
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur lors de la generation du PDF: {ex.Message}";
            Debug.WriteLine($"[RapportViewModel] PDF generation error: {ex.Message}");

            await Application.Current!.MainPage!.DisplayAlert(
                "Erreur",
                $"Impossible de generer le PDF: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Builds the RapportPdfData from current ViewModel collections and properties.
    /// </summary>
    private RapportPdfData BuildRapportPdfData()
    {
        return new RapportPdfData
        {
            PeriodeLabel = PeriodeLabel,
            GeneratedAt = DateTime.Now,

            // Ventes Section
            Ventes = new RapportVentesPdfData
            {
                TotalVentes = TotalVentes,
                TotalVentesCarburant = TotalVentesCarburant,
                TotalQuantiteCarburant = TotalQuantiteCarburant,
                TotalVentesLubArticles = TotalVentesLubArticles,
                TotalQuantiteLubArticles = TotalQuantiteLubArticles,
                TotalVentesServices = TotalVentesServices,
                TotalQuantiteServices = TotalQuantiteServices,
                VentesCarburantParProduit = VentesCarburantParProduit
                    .Select(v => new RapportVenteCarburantProduitPdfData
                    {
                        ProduitNom = v.ProduitNom,
                        TotalQuantite = v.TotalQuantite,
                        TotalMontant = v.TotalMontant,
                        NombrePeriodes = v.NombrePeriodes
                    }).ToList(),
                VentesLubArticlesParProduit = VentesLubArticlesParProduit
                    .Select(v => new RapportVenteLubArticleProduitPdfData
                    {
                        ProduitNom = v.ProduitNom,
                        CategorieNom = v.CategorieNom,
                        TotalQuantite = v.TotalQuantite,
                        TotalMontant = v.TotalMontant,
                        NombreVentes = v.NombreVentes
                    }).ToList(),
                VentesServicesParService = VentesServicesParService
                    .Select(v => new RapportVenteServicePdfData
                    {
                        ServiceDescription = v.ServiceDescription,
                        CategorieNom = v.CategorieNom,
                        TotalQuantite = v.TotalQuantite,
                        TotalMontant = v.TotalMontant,
                        NombreVentes = v.NombreVentes
                    }).ToList()
            },

            // Depenses Section
            Depenses = new RapportDepensesPdfData
            {
                TotalDepenses = TotalDepenses,
                NombreDepenses = NombreDepenses,
                DepensesParCategorie = DepensesParCategorie
                    .Select(d => new RapportDepenseCategoriePdfData
                    {
                        CategorieNom = d.CategorieNom,
                        TotalMontant = d.TotalMontant,
                        NombreDepenses = d.NombreDepenses
                    }).ToList(),
                DepensesDetails = DepensesDetails
                    .Select(d => new RapportDepenseDetailPdfData
                    {
                        Numero = d.Numero,
                        DateDisplay = d.DateDisplay,
                        Categorie = d.Categorie,
                        Montant = d.Montant,
                        Description = d.Description
                    }).ToList()
            },

            // Stock Section
            Stock = new RapportStockPdfData
            {
                TotalStockCarburantLitres = TotalStockCarburantLitres,
                TotalStockProduits = TotalStockProduits,
                TotalAchatsPeriode = TotalAchatsPeriode,
                StockCarburant = StockCarburant
                    .Select(s => new RapportStockReservoirPdfData
                    {
                        ReservoirNumero = s.ReservoirNumero,
                        ProduitNom = s.ProduitNom,
                        Capacite = s.Capacite,
                        NiveauActuel = s.NiveauActuel,
                        PourcentageRemplissage = s.PourcentageRemplissage
                    }).ToList(),
                StockProduits = StockProduits
                    .Select(s => new RapportStockProduitPdfData
                    {
                        ProduitNom = s.ProduitNom,
                        CategorieNom = s.CategorieNom,
                        StockActuel = s.StockActuel,
                        StockMinimum = s.StockMinimum,
                        IsLowStock = s.IsLowStock
                    }).ToList(),
                AchatsParProduit = AchatsParProduit
                    .Select(a => new RapportAchatProduitPdfData
                    {
                        ProduitNom = a.ProduitNom,
                        TotalQuantite = a.TotalQuantite,
                        TotalMontant = a.TotalMontant,
                        NombreAchats = a.NombreAchats
                    }).ToList(),
                JaugeageAnalyse = new RapportJaugeageAnalysePdfData
                {
                    HasData = HasJaugeageData,
                    Message = JaugeageAnalyseMessage,
                    PeriodeAnalyse = JaugeagePeriodeAnalyse,
                    JaugeagePrecedentInfo = JaugeagePrecedentInfo,
                    JaugeageActuelInfo = JaugeageActuelInfo,
                    Comparaisons = JaugeageComparaisons
                        .Select(j => new RapportJaugeageComparisonPdfData
                        {
                            ReservoirNumero = j.ReservoirNumero,
                            ProduitNom = j.ProduitNom,
                            VolumePrecedent = j.VolumePrecedent,
                            VolumeActuel = j.VolumeActuel,
                            StockConsomme = j.StockConsomme,
                            QuantiteVendue = j.QuantiteVendue,
                            Ecart = j.Ecart,
                            Statut = j.Statut
                        }).ToList()
                }
            }
        };
    }

    private void PopulateVentes(RapportVentesDto ventes)
    {
        TotalVentesCarburant = ventes.TotalVentesCarburant;
        TotalQuantiteCarburant = ventes.TotalQuantiteCarburant;
        TotalVentesLubArticles = ventes.TotalVentesLubArticles;
        TotalQuantiteLubArticles = ventes.TotalQuantiteLubArticles;
        TotalVentesServices = ventes.TotalVentesServices;
        TotalQuantiteServices = ventes.TotalQuantiteServices;
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

        VentesServicesParService.Clear();
        foreach (var item in ventes.VentesServicesParService)
        {
            VentesServicesParService.Add(item);
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

    private void PopulateJaugeageAnalyse(RapportJaugeageAnalyseDto analyse)
    {
        HasJaugeageData = analyse.HasData;
        JaugeageAnalyseMessage = analyse.Message;
        JaugeagePeriodeAnalyse = analyse.PeriodeAnalyse;

        if (analyse.JaugeageActuel != null)
        {
            JaugeageActuelInfo = $"{analyse.JaugeageActuel.NumeroJaugeage ?? "N/A"} - {analyse.JaugeageActuel.DateDisplay}";
        }
        else
        {
            JaugeageActuelInfo = null;
        }

        if (analyse.JaugeagePrecedent != null)
        {
            JaugeagePrecedentInfo = $"{analyse.JaugeagePrecedent.NumeroJaugeage ?? "N/A"} - {analyse.JaugeagePrecedent.DateDisplay}";
        }
        else
        {
            JaugeagePrecedentInfo = null;
        }

        JaugeageComparaisons.Clear();
        foreach (var item in analyse.Comparaisons)
        {
            JaugeageComparaisons.Add(item);
        }

        Debug.WriteLine($"[RapportViewModel] Jaugeage analysis loaded: HasData={HasJaugeageData}, Comparisons={JaugeageComparaisons.Count}");
    }

    #endregion
}

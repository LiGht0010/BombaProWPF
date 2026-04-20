using BombaProMaxWPF.Models.Dashboard;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMaxWPF.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DashboardService _dashboardService;
    private readonly IDialogService _dialogService;

    // RAW DATA (from API)
    private List<AchatAnalyticsRowDto> _achatsRawData = [];
    private List<VenteAnalyticsRowDto> _ventesRawData = [];
    private List<VenteCarburantAnalyticsRowDto> _ventesCarburantRawData = [];

    // GROUPED DATA FOR CARDS
    public ObservableCollection<ProductCardModel> AchatsParProduit { get; } = [];
    public ObservableCollection<ProductCardModel> VentesParProduit { get; } = [];
    public ObservableCollection<ProductCardModel> VentesCarburantParProduit { get; } = [];

    // FILTER PROPERTIES
    [ObservableProperty]
    private DateTime? _dateSpecifique;

    [ObservableProperty]
    private DateTime? _dateDebut;

    [ObservableProperty]
    private DateTime? _dateFin;

    [ObservableProperty]
    private int? _annee;

    [ObservableProperty]
    private string? _moisAnnee; // Format: "2025-03"

    // COMPUTED TOTALS
    [ObservableProperty]
    private decimal _totalAchats;

    [ObservableProperty]
    private decimal _totalVentes;

    [ObservableProperty]
    private decimal _totalVentesCarburant;

    // UI STATE
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAchatsCardsVisible = true;

    [ObservableProperty]
    private string _achatsToggleButtonText = "Masquer";

    [ObservableProperty]
    private bool _isVentesCardsVisible = true;

    [ObservableProperty]
    private string _ventesToggleButtonText = "Masquer";

    [ObservableProperty]
    private bool _isVentesCarburantCardsVisible = true;

    [ObservableProperty]
    private string _ventesCarburantToggleButtonText = "Masquer";

    // POPUP STATE (for card click detail view)
    [ObservableProperty]
    private ProductCardModel? _selectedProductCard;

    public ObservableCollection<AchatAnalyticsRowDto> SelectedProductAchats { get; } = [];

    [ObservableProperty]
    private decimal _selectedProductTotal;

    // CONSTRUCTOR
    public DashboardViewModel(DashboardService dashboardService, IDialogService dialogService)
    {
        _dashboardService = dashboardService;
        _dialogService = dialogService;

        // Default to current month
        var now = DateTime.Now;
        MoisAnnee = $"{now.Year}-{now.Month:D2}";
    }

    // LOAD DATA COMMAND
    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        try
        {
            IsLoading = true;
            await Task.WhenAll(
                LoadAchatsDataAsync(),
                LoadVentesDataAsync(),
                LoadVentesCarburantDataAsync()
            );
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger le tableau de bord: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAchatsDataAsync()
    {
        // Build filter parameters
        DateOnly? startDate = DateDebut.HasValue ? DateOnly.FromDateTime(DateDebut.Value) : null;
        DateOnly? endDate = DateFin.HasValue ? DateOnly.FromDateTime(DateFin.Value) : null;
        DateOnly? date = DateSpecifique.HasValue ? DateOnly.FromDateTime(DateSpecifique.Value) : null;

        // Fetch raw data from API
        _achatsRawData = await _dashboardService.GetAchatsAnalyticsAsync(
            startDate: startDate,
            endDate: endDate,
            date: date,
            year: Annee,
            month: MoisAnnee
        );

        // Group data for cards
        GroupAchatsForCards();

        // Calculate total
        TotalAchats = _achatsRawData.Sum(a => a.PrixTotal);
    }

    private async Task LoadVentesDataAsync()
    {
        // Build filter parameters
        DateOnly? startDate = DateDebut.HasValue ? DateOnly.FromDateTime(DateDebut.Value) : null;
        DateOnly? endDate = DateFin.HasValue ? DateOnly.FromDateTime(DateFin.Value) : null;
        DateOnly? date = DateSpecifique.HasValue ? DateOnly.FromDateTime(DateSpecifique.Value) : null;

        // Fetch raw data from API
        _ventesRawData = await _dashboardService.GetVentesAnalyticsAsync(
            startDate: startDate,
            endDate: endDate,
            date: date,
            year: Annee,
            month: MoisAnnee
        );

        // Group data for cards
        GroupVentesForCards();

        // Calculate total
        TotalVentes = _ventesRawData.Sum(v => v.PrixTotal);
    }

    private async Task LoadVentesCarburantDataAsync()
    {
        // Build filter parameters
        DateOnly? startDate = DateDebut.HasValue ? DateOnly.FromDateTime(DateDebut.Value) : null;
        DateOnly? endDate = DateFin.HasValue ? DateOnly.FromDateTime(DateFin.Value) : null;
        DateOnly? date = DateSpecifique.HasValue ? DateOnly.FromDateTime(DateSpecifique.Value) : null;

        // Fetch raw data from API
        _ventesCarburantRawData = await _dashboardService.GetVentesCarburantAnalyticsAsync(
            startDate: startDate,
            endDate: endDate,
            date: date,
            year: Annee,
            month: MoisAnnee
        );

        // Group data for cards
        GroupVentesCarburantForCards();

        // Calculate total
        TotalVentesCarburant = _ventesCarburantRawData.Sum(v => v.PrixTotalElectronique);
    }

    private void GroupAchatsForCards()
    {
        var grouped = _achatsRawData
            .GroupBy(a => a.ProduitId)
            .Select(g => new ProductCardModel
            {
                ProduitId = g.Key,
                ProduitNom = g.First().ProduitNom,
                CategorieNom = g.First().CategorieNom,
                TotalQuantite = g.Sum(x => x.Quantite),
                TotalMontant = g.Sum(x => x.PrixTotal)
            })
            .OrderByDescending(p => p.TotalQuantite)
            .ToList();

        AchatsParProduit.Clear();
        foreach (var item in grouped)
        {
            AchatsParProduit.Add(item);
        }
    }

    private void GroupVentesForCards()
    {
        var grouped = _ventesRawData
            .GroupBy(v => v.ProduitId)
            .Select(g => new ProductCardModel
            {
                ProduitId = g.Key,
                ProduitNom = g.First().ProduitNom,
                CategorieNom = g.First().CategorieNom,
                TotalQuantite = g.Sum(x => x.Quantite),
                TotalMontant = g.Sum(x => x.PrixTotal)
            })
            .OrderByDescending(p => p.TotalQuantite)
            .ToList();

        VentesParProduit.Clear();
        foreach (var item in grouped)
        {
            VentesParProduit.Add(item);
        }
    }

    private void GroupVentesCarburantForCards()
    {
        var grouped = _ventesCarburantRawData
            .GroupBy(v => v.ProduitId)
            .Select(g => new ProductCardModel
            {
                ProduitId = g.Key,
                ProduitNom = g.First().ProduitNom,
                CategorieNom = "Carburant",
                TotalQuantite = (int)g.Sum(x => x.QuantiteElectronique),
                TotalMontant = g.Sum(x => x.PrixTotalElectronique)
            })
            .OrderByDescending(p => p.TotalQuantite)
            .ToList();

        VentesCarburantParProduit.Clear();
        foreach (var item in grouped)
        {
            VentesCarburantParProduit.Add(item);
        }
    }

    // FILTER COMMANDS
    [RelayCommand]
    private async Task ApplyFilterAsync()
    {
        await LoadDashboardAsync();
    }

    [RelayCommand]
    private async Task ClearFilterAsync()
    {
        DateSpecifique = null;
        DateDebut = null;
        DateFin = null;
        Annee = null;
        MoisAnnee = null;

        await LoadDashboardAsync();
    }

    [RelayCommand]
    private void SetCurrentMonth()
    {
        var now = DateTime.Now;
        MoisAnnee = $"{now.Year}-{now.Month:D2}";
        DateSpecifique = null;
        DateDebut = null;
        DateFin = null;
        Annee = null;
    }

    [RelayCommand]
    private void SetCurrentYear()
    {
        Annee = DateTime.Now.Year;
        MoisAnnee = null;
        DateSpecifique = null;
        DateDebut = null;
        DateFin = null;
    }

    // VISIBILITY TOGGLE COMMANDS
    [RelayCommand]
    private void ToggleAchatsCardsVisibility()
    {
        IsAchatsCardsVisible = !IsAchatsCardsVisible;
        AchatsToggleButtonText = IsAchatsCardsVisible ? "Masquer" : "Afficher";
    }

    [RelayCommand]
    private void ToggleVentesCardsVisibility()
    {
        IsVentesCardsVisible = !IsVentesCardsVisible;
        VentesToggleButtonText = IsVentesCardsVisible ? "Masquer" : "Afficher";
    }

    [RelayCommand]
    private void ToggleVentesCarburantCardsVisibility()
    {
        IsVentesCarburantCardsVisible = !IsVentesCarburantCardsVisible;
        VentesCarburantToggleButtonText = IsVentesCarburantCardsVisible ? "Masquer" : "Afficher";
    }

    // CARD CLICK COMMAND - ACHATS
    [RelayCommand]
    private async Task ShowProductAchatsDetailAsync(ProductCardModel? card)
    {
        if (card == null) return;

        SelectedProductCard = card;

        var productAchats = _achatsRawData
            .Where(a => a.ProduitId == card.ProduitId)
            .OrderByDescending(a => a.DateAchat)
            .ToList();

        SelectedProductAchats.Clear();
        foreach (var achat in productAchats)
        {
            SelectedProductAchats.Add(achat);
        }

        SelectedProductTotal = productAchats.Sum(a => a.PrixTotal);

        await _dialogService.ShowAchatPerProductPopupAsync(card, productAchats);
    }

    // CARD CLICK COMMAND - VENTES (Lubrifiants et Articles)
    [RelayCommand]
    private async Task ShowProductVentesDetailAsync(ProductCardModel? card)
    {
        if (card == null) return;

        var productVentes = _ventesRawData
            .Where(v => v.ProduitId == card.ProduitId)
            .OrderByDescending(v => v.DateVente)
            .ToList();

        await _dialogService.ShowVentePerProductPopupAsync(card, productVentes);
    }

    // CARD CLICK COMMAND - VENTES CARBURANT (from Periode/PeriodeDetails)
    [RelayCommand]
    private async Task ShowProductVentesCarburantDetailAsync(ProductCardModel? card)
    {
        if (card == null) return;

        var productVentes = _ventesCarburantRawData
            .Where(v => v.ProduitId == card.ProduitId)
            .OrderByDescending(v => v.DateDebut)
            .ToList();

        await _dialogService.ShowVenteCarburantPerProductPopupAsync(card, productVentes);
    }
}

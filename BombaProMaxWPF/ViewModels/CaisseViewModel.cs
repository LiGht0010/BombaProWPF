using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMaxWPF.ViewModels;

public partial class CaisseViewModel : ObservableObject
{
    private readonly CaisseService _caisseService;
    private readonly JourneeNavigationService _journeeService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private DateTime _filterStartDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime _filterEndDate = DateTime.Today;

    [ObservableProperty]
    private DepotCaisseDto? _selectedDepot;

    [ObservableProperty]
    private decimal _especesPeriodes;

    [ObservableProperty]
    private decimal _especesVenteLubArticles;

    [ObservableProperty]
    private decimal _especesVenteServices;

    [ObservableProperty]
    private decimal _especesReglementCredits;

    [ObservableProperty]
    private decimal _totalDepots;

    [ObservableProperty]
    private decimal _soldeActuel;

    [ObservableProperty]
    private decimal _totalEncaisse;

    public ObservableCollection<DepotCaisseDto> Depots { get; } = new();

    public int TotalDepotsCount => Depots.Count;
    public decimal TotalMontantDepots => Depots.Sum(d => d.Montant);

    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Etape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public CaisseViewModel(JourneeNavigationService journeeService)
    {
        _caisseService = new CaisseService();
        _journeeService = journeeService;

        _journeeService.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(IsJourneeActive));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsFirstStep));
            OnPropertyChanged(nameof(IsLastStep));
            OnPropertyChanged(nameof(JourneeStepInfo));
        };
    }

    [RelayCommand]
    private async Task JourneeSuivantAsync() => await _journeeService.GoNextAsync(skipped: false);

    [RelayCommand]
    private async Task JourneePasserAsync() => await _journeeService.GoNextAsync(skipped: true);

    [RelayCommand]
    private async Task JourneePrecedentAsync() => await _journeeService.GoPreviousAsync();

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            LoadCashSummaryAsync(),
            LoadDepotsAsync()
        );
    }

    [RelayCommand]
    public async Task LoadCashSummaryAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var summary = await _caisseService.GetCashSummaryAsync();

            if (summary != null)
            {
                EspecesPeriodes = summary.EspecesPeriodes;
                EspecesVenteLubArticles = summary.EspecesVenteLubArticles;
                EspecesVenteServices = summary.EspecesVenteServices;
                EspecesReglementCredits = summary.EspecesReglementCredits;
                TotalDepots = summary.TotalDepots;
                SoldeActuel = summary.SoldeActuel;
                TotalEncaisse = summary.TotalEncaisse;

                Debug.WriteLine($"[CaisseViewModel] Loaded summary: Solde={SoldeActuel:N2}");
            }
            else
            {
                EspecesPeriodes = 0;
                EspecesVenteLubArticles = 0;
                EspecesVenteServices = 0;
                EspecesReglementCredits = 0;
                TotalDepots = 0;
                SoldeActuel = 0;
                TotalEncaisse = 0;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement du resume: {ex.Message}";
            Debug.WriteLine($"[CaisseViewModel] Error loading summary: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadCashSummaryByFilterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var summary = await _caisseService.GetCashSummaryAsync(FilterStartDate, FilterEndDate);

            if (summary != null)
            {
                EspecesPeriodes = summary.EspecesPeriodes;
                EspecesVenteLubArticles = summary.EspecesVenteLubArticles;
                EspecesVenteServices = summary.EspecesVenteServices;
                EspecesReglementCredits = summary.EspecesReglementCredits;
                TotalDepots = summary.TotalDepots;
                SoldeActuel = summary.SoldeActuel;
                TotalEncaisse = summary.TotalEncaisse;
            }

            await LoadDepotsByFilterAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[CaisseViewModel] Error loading summary by filter: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadDepotsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var depots = await _caisseService.GetAllDepotsAsync();

            Depots.Clear();
            foreach (var depot in depots)
            {
                Depots.Add(depot);
            }

            NotifyDepotTotalsChanged();
            Debug.WriteLine($"[CaisseViewModel] Loaded {Depots.Count} depots");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[CaisseViewModel] Error loading depots: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadDepotsByFilterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var depots = await _caisseService.GetDepotsByDateRangeAsync(FilterStartDate, FilterEndDate);

            Depots.Clear();
            foreach (var depot in depots)
            {
                Depots.Add(depot);
            }

            NotifyDepotTotalsChanged();
            Debug.WriteLine($"[CaisseViewModel] Loaded {Depots.Count} depots for filter");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[CaisseViewModel] Error loading depots by filter: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<DepotCaisseDto?> CreateDepotAsync(DepotCaisseDto depot)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var created = await _caisseService.CreateDepotAsync(depot);
            if (created != null)
            {
                Depots.Insert(0, created);
                NotifyDepotTotalsChanged();
                await LoadCashSummaryAsync();
                Debug.WriteLine($"[CaisseViewModel] Created depot {created.ID}");
            }

            return created;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de creation: {ex.Message}";
            Debug.WriteLine($"[CaisseViewModel] Error creating depot: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> UpdateDepotAsync(DepotCaisseDto depot)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _caisseService.UpdateDepotAsync(depot);
            if (success)
            {
                var index = Depots.ToList().FindIndex(d => d.ID == depot.ID);
                if (index >= 0)
                {
                    Depots[index] = depot;
                }
                NotifyDepotTotalsChanged();
                await LoadCashSummaryAsync();
                Debug.WriteLine($"[CaisseViewModel] Updated depot {depot.ID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de mise a jour: {ex.Message}";
            Debug.WriteLine($"[CaisseViewModel] Error updating depot: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> DeleteDepotAsync(DepotCaisseDto depot)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _caisseService.DeleteDepotAsync(depot.ID);
            if (success)
            {
                Depots.Remove(depot);
                if (SelectedDepot?.ID == depot.ID)
                {
                    SelectedDepot = null;
                }
                NotifyDepotTotalsChanged();
                await LoadCashSummaryAsync();
                Debug.WriteLine($"[CaisseViewModel] Deleted depot {depot.ID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de suppression: {ex.Message}";
            Debug.WriteLine($"[CaisseViewModel] Error deleting depot: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void NotifyDepotTotalsChanged()
    {
        OnPropertyChanged(nameof(TotalDepotsCount));
        OnPropertyChanged(nameof(TotalMontantDepots));
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }
}

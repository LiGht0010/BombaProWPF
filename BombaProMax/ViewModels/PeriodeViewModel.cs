using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMax.ViewModels;

/// <summary>
/// ViewModel for managing Periodes (shifts) and their PeriodeDetails.
/// </summary>
public partial class PeriodeViewModel : ObservableObject
{
    private readonly PeriodeService _periodeService;
    private readonly PompeService _pompeService;
    private readonly CreditTransactionService _creditTransactionService;
    private readonly JourneeNavigationService _journeeService;
    private readonly HttpClient _httpClient;

    #region Observable Properties

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private PeriodeDto? _selectedPeriode;

    [ObservableProperty]
    private PeriodeDetailsDto? _selectedDetail;

    [ObservableProperty]
    private DateTime _filterStartDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime _filterEndDate = DateTime.Today;

    #endregion

    #region Collections

    public ObservableCollection<PeriodeDto> Periodes { get; } = [];
    public ObservableCollection<PeriodeDetailsDto> CurrentPeriodeDetails { get; } = [];
    
    /// <summary>
    /// Employes list for popup selection.
    /// </summary>
    public ObservableCollection<EmployeDto> Employes { get; } = [];
    
    /// <summary>
    /// Pompes list for popup.
    /// </summary>
    public ObservableCollection<PompeDto> Pompes { get; } = [];
    
    /// <summary>
    /// Reservoirs list for popup.
    /// </summary>
    public ObservableCollection<ReservoirDto> Reservoirs { get; } = [];
    
    /// <summary>
    /// Produits list for popup.
    /// </summary>
    public ObservableCollection<ProduitDto> Produits { get; } = [];
    
    /// <summary>
    /// Pump readings for create/edit popup.
    /// </summary>
    public ObservableCollection<PompeReadingModel> PompeReadings { get; } = [];

    /// <summary>
    /// Credit transactions (carburant) for the periode popup.
    /// </summary>
    public ObservableCollection<CreditTransactionDto> PeriodeCreditTransactions { get; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Total quantity sold in current periode (sum of all details).
    /// </summary>
    public decimal TotalQuantiteVendue => CurrentPeriodeDetails.Sum(d => d.QuantiteVendue);

    /// <summary>
    /// Total revenue in current periode.
    /// </summary>
    public decimal TotalPrixTotal => CurrentPeriodeDetails.Sum(d => d.PrixTotal);

    /// <summary>
    /// Total difference between electronic and mechanical meters.
    /// </summary>
    public decimal TotalDifferenceQuantite => CurrentPeriodeDetails.Sum(d => d.DifferenceQuantite);

    /// <summary>
    /// Total difference in value.
    /// </summary>
    public decimal TotalDifferenceValeur => CurrentPeriodeDetails.Sum(d => d.DifferenceValeur);

    /// <summary>
    /// Total TPE payment from selected periode.
    /// </summary>
    public decimal TotalTPE => SelectedPeriode?.TPE ?? 0;

    /// <summary>
    /// Total Especes (cash) payment from selected periode.
    /// </summary>
    public decimal TotalEspeces => SelectedPeriode?.Especes ?? 0;

    /// <summary>
    /// Total payments (TPE + Especes) from selected periode.
    /// </summary>
    public decimal TotalPaiements => TotalTPE + TotalEspeces;

    /// <summary>
    /// Difference between expected revenue and actual payments.
    /// Positive = missing money, Negative = excess payment.
    /// </summary>
    public decimal EcartPaiement => TotalPrixTotal - TotalPaiements;

    /// <summary>
    /// Total credit amount from selected credit transactions in popup.
    /// </summary>
    public decimal TotalCredite => PeriodeCreditTransactions.Where(ct => ct.IsSelected).Sum(ct => ct.MontantTotal);

    /// <summary>
    /// Number of selected credit transactions.
    /// </summary>
    public int SelectedCreditTransactionsCount => PeriodeCreditTransactions.Count(ct => ct.IsSelected);

    #endregion

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES
    // ════════════════════════════════════════════════════════════════
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    #region Constructor

    public PeriodeViewModel(JourneeNavigationService journeeService)
    {
        _periodeService = new PeriodeService();
        _pompeService = new PompeService();
        _creditTransactionService = new CreditTransactionService();
        _journeeService = journeeService;
        
        // Create HTTP client for loading reference data
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);

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

    #endregion

    #region Reference Data Loading

    /// <summary>
    /// Loads all reference data needed for create/edit popups.
    /// </summary>
    public async Task LoadReferenceDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var employesTask = LoadEmployesAsync();
            var pompesTask = LoadPompesAsync();
            var reservoirsTask = LoadReservoirsAsync();
            var produitsTask = LoadProduitsAsync();

            await Task.WhenAll(employesTask, pompesTask, reservoirsTask, produitsTask);

            Debug.WriteLine($"[PeriodeViewModel] Loaded reference data: {Employes.Count} employes, {Pompes.Count} pompes, {Reservoirs.Count} reservoirs, {Produits.Count} produits");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error loading reference data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads employes list.
    /// </summary>
    public async Task LoadEmployesAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync(ApiConfig.Employes);
            var employes = JsonConvert.DeserializeObject<List<EmployeDto>>(json) ?? [];
            
            Employes.Clear();
            foreach (var e in employes)
            {
                Employes.Add(e);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error loading employes: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads pompes list.
    /// </summary>
    public async Task LoadPompesAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync(ApiConfig.Pompes);
            var pompes = JsonConvert.DeserializeObject<List<PompeDto>>(json) ?? [];
            
            Pompes.Clear();
            foreach (var p in pompes)
            {
                Pompes.Add(p);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error loading pompes: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads reservoirs list.
    /// </summary>
    public async Task LoadReservoirsAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync(ApiConfig.Reservoirs);
            var reservoirs = JsonConvert.DeserializeObject<List<ReservoirDto>>(json) ?? [];
            
            Reservoirs.Clear();
            foreach (var r in reservoirs)
            {
                Reservoirs.Add(r);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error loading reservoirs: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads produits list.
    /// </summary>
    public async Task LoadProduitsAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync(ApiConfig.Produits);
            var produits = JsonConvert.DeserializeObject<List<ProduitDto>>(json) ?? [];
            
            Produits.Clear();
            foreach (var p in produits)
            {
                Produits.Add(p);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error loading produits: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds pump readings list for edit popup with existing detail values.
    /// </summary>
    public void BuildPompeReadingsForEdit(List<PeriodeDetailsDto> existingDetails)
    {
        PompeReadings.Clear();

        foreach (var pompe in Pompes.Where(p => p.Statut?.ToLower() == "actif" || p.Statut?.ToLower() == "active"))
        {
            var reservoir = Reservoirs.FirstOrDefault(r => r.ID == pompe.ReservoirAssocieID);
            
            ProduitDto? produit = null;
            decimal prix = 0;
            if (reservoir?.ProduitID != null)
            {
                produit = Produits.FirstOrDefault(p => p.ID == reservoir.ProduitID);
                prix = produit?.PrixTTC ?? 0;
            }

            var existingDetail = existingDetails.FirstOrDefault(d => d.PompeID == pompe.ID);

            // When editing, prioritize existing detail values over current pump counters
            var reading = new PompeReadingModel
            {
                PompeID = pompe.ID,
                PompeNumero = pompe.Numero,
                ReservoirID = pompe.ReservoirAssocieID,
                ReservoirNumero = reservoir?.Numero,
                ProduitID = produit?.ID,
                ProduitNom = produit?.Description ?? reservoir?.ProduitNom ?? "N/A",
                PrixCarburant = existingDetail?.PrixCarburant ?? prix,
                // For DEBUT: use existing detail value, fall back to current pump counter
                CompteurElecDebut = existingDetail?.CompteurElectroniqueDebut ?? pompe.CompteurElectroniqueActuel ?? 0,
                CompteurMecaDebut = existingDetail?.CompteurMecaniqueDebut ?? pompe.CompteurMecaniqueActuel ?? 0,
                // For FIN: use existing detail value (which should be > debut if there was a sale)
                CompteurElecFin = existingDetail?.CompteurElectroniqueFinal.ToString("F2") 
                    ?? (pompe.CompteurElectroniqueActuel ?? 0).ToString("F2"),
                CompteurMecaFin = existingDetail?.CompteurMecaniqueFinal.ToString("F2") 
                    ?? (pompe.CompteurMecaniqueActuel ?? 0).ToString("F2")
            };

            PompeReadings.Add(reading);
        }

        // Log for debugging
        var detailsWithSales = PompeReadings.Count(r => r.QuantiteVendue > 0);
        Debug.WriteLine($"[PeriodeViewModel] Built {PompeReadings.Count} pump readings, {detailsWithSales} have sales, from {existingDetails.Count} existing details");
    }

    /// <summary>
    /// Builds pump readings list for create popup (new periode).
    /// </summary>
    public void BuildPompeReadingsForCreate()
    {
        BuildPompeReadingsForEdit([]);
    }

    #endregion

    #region Credit Transaction Loading

    /// <summary>
    /// Loads carburant credit transactions within a date range (for create mode).
    /// These are CTs that are not yet assigned to any periode.
    /// </summary>
    public async Task LoadCreditTransactionsByDateRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            Debug.WriteLine($"[PeriodeViewModel] Loading carburant CTs from {start} to {end}");
            
            var transactions = await _creditTransactionService.GetCarburantByDateRangeAsync(start, end);
            
            PeriodeCreditTransactions.Clear();
            foreach (var ct in transactions)
            {
                ct.IsSelected = true; // Default to selected in create mode
                PeriodeCreditTransactions.Add(ct);
            }

            NotifyCreditTotalsChanged();
            Debug.WriteLine($"[PeriodeViewModel] Loaded {PeriodeCreditTransactions.Count} carburant CTs for date range");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error loading CTs by date range: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads credit transactions linked to a specific periode (for edit mode).
    /// </summary>
    public async Task LoadCreditTransactionsByPeriodeAsync(int periodeId)
    {
        try
        {
            Debug.WriteLine($"[PeriodeViewModel] Loading CTs for periode {periodeId}");
            
            var transactions = await _creditTransactionService.GetByPeriodeIdAsync(periodeId);
            
            PeriodeCreditTransactions.Clear();
            foreach (var ct in transactions)
            {
                ct.IsSelected = true; // Already linked = selected
                PeriodeCreditTransactions.Add(ct);
            }

            NotifyCreditTotalsChanged();
            Debug.WriteLine($"[PeriodeViewModel] Loaded {PeriodeCreditTransactions.Count} CTs for periode {periodeId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error loading CTs by periode: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears credit transactions collection.
    /// </summary>
    public void ClearCreditTransactions()
    {
        PeriodeCreditTransactions.Clear();
        NotifyCreditTotalsChanged();
    }

    /// <summary>
    /// Toggles selection of a credit transaction.
    /// </summary>
    public void ToggleCreditTransactionSelection(CreditTransactionDto ct)
    {
        ct.IsSelected = !ct.IsSelected;
        NotifyCreditTotalsChanged();
    }

    /// <summary>
    /// Selects all credit transactions.
    /// </summary>
    public void SelectAllCreditTransactions()
    {
        foreach (var ct in PeriodeCreditTransactions)
        {
            ct.IsSelected = true;
        }
        NotifyCreditTotalsChanged();
    }

    /// <summary>
    /// Deselects all credit transactions.
    /// </summary>
    public void DeselectAllCreditTransactions()
    {
        foreach (var ct in PeriodeCreditTransactions)
        {
            ct.IsSelected = false;
        }
        NotifyCreditTotalsChanged();
    }

    /// <summary>
    /// Gets the list of selected credit transaction IDs.
    /// </summary>
    public List<int> GetSelectedCreditTransactionIds()
    {
        return PeriodeCreditTransactions
            .Where(ct => ct.IsSelected)
            .Select(ct => ct.CreditID)
            .ToList();
    }

    /// <summary>
    /// Notifies that credit-related computed properties have changed.
    /// </summary>
    private void NotifyCreditTotalsChanged()
    {
        OnPropertyChanged(nameof(TotalCredite));
        OnPropertyChanged(nameof(SelectedCreditTransactionsCount));
    }

    #endregion

    #region Selection Command

    /// <summary>
    /// Command to select a periode from the list.
    /// </summary>
    [RelayCommand]
    private void SelectPeriode(PeriodeDto? periode)
    {
        SelectedPeriode = periode;
    }

    /// <summary>
    /// Command to delete a periode (for XAML binding).
    /// </summary>
    [RelayCommand]
    private async Task DeleteAsync(PeriodeDto? periode)
    {
        if (periode == null) return;
        await DeletePeriodeAsync(periode);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE COMMANDS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private async Task JourneeSuivantAsync() => await _journeeService.GoNextAsync(skipped: false);

    [RelayCommand]
    private async Task JourneePasserAsync() => await _journeeService.GoNextAsync(skipped: true);

    [RelayCommand]
    private async Task JourneePrecedentAsync() => await _journeeService.GoPreviousAsync();

    #region Initialization

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadPeriodesAsync();
    }

    #endregion

    #region Periode Operations

    [RelayCommand]
    public async Task LoadPeriodesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var periodes = await _periodeService.GetAllPeriodesAsync();
            
            Periodes.Clear();
            foreach (var periode in periodes)
            {
                Periodes.Add(periode);
            }

            Debug.WriteLine($"[PeriodeViewModel] Loaded {Periodes.Count} periodes");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error loading periodes: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadPeriodesByDateRangeAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var periodes = await _periodeService.GetPeriodesByDateRangeAsync(FilterStartDate, FilterEndDate);
            
            Periodes.Clear();
            foreach (var periode in periodes)
            {
                Periodes.Add(periode);
            }

            Debug.WriteLine($"[PeriodeViewModel] Loaded {Periodes.Count} periodes for date range");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error loading periodes by date range: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Called when SelectedPeriode changes - loads the details.
    /// </summary>
    partial void OnSelectedPeriodeChanged(PeriodeDto? value)
    {
        if (value != null)
        {
            _ = LoadPeriodeDetailsAsync(value.PeriodeID);
        }
        else
        {
            CurrentPeriodeDetails.Clear();
            NotifyTotalsChanged();
        }
        
        // Notify payment properties which depend on SelectedPeriode
        OnPropertyChanged(nameof(TotalTPE));
        OnPropertyChanged(nameof(TotalEspeces));
        OnPropertyChanged(nameof(TotalPaiements));
        OnPropertyChanged(nameof(EcartPaiement));
    }

    public async Task<PeriodeDto?> CreatePeriodeAsync(PeriodeDto periode)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var created = await _periodeService.CreatePeriodeAsync(periode);
            if (created != null)
            {
                Periodes.Insert(0, created);
                Debug.WriteLine($"[PeriodeViewModel] Created periode {created.PeriodeID}");
            }

            return created;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de création: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error creating periode: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> UpdatePeriodeAsync(PeriodeDto periode)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _periodeService.UpdatePeriodeAsync(periode);
            if (success)
            {
                // Update in collection
                var index = Periodes.ToList().FindIndex(p => p.PeriodeID == periode.PeriodeID);
                if (index >= 0)
                {
                    Periodes[index] = periode;
                }
                Debug.WriteLine($"[PeriodeViewModel] Updated periode {periode.PeriodeID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de mise à jour: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error updating periode: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> DeletePeriodeAsync(PeriodeDto periode)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _periodeService.DeletePeriodeAsync(periode.PeriodeID);
            if (success)
            {
                Periodes.Remove(periode);
                if (SelectedPeriode?.PeriodeID == periode.PeriodeID)
                {
                    SelectedPeriode = null;
                }
                Debug.WriteLine($"[PeriodeViewModel] Deleted periode {periode.PeriodeID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de suppression: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error deleting periode: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    #endregion

    #region PeriodeDetails Operations

    [RelayCommand]
    public async Task LoadPeriodeDetailsAsync(int periodeId)
    {
        try
        {
            IsLoading = true;

            var details = await _periodeService.GetDetailsByPeriodeAsync(periodeId);
            
            CurrentPeriodeDetails.Clear();
            foreach (var detail in details)
            {
                CurrentPeriodeDetails.Add(detail);
            }

            // Notify computed properties
            OnPropertyChanged(nameof(TotalQuantiteVendue));
            OnPropertyChanged(nameof(TotalPrixTotal));
            OnPropertyChanged(nameof(TotalDifferenceQuantite));
            OnPropertyChanged(nameof(TotalDifferenceValeur));

            Debug.WriteLine($"[PeriodeViewModel] Loaded {CurrentPeriodeDetails.Count} details for periode {periodeId}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error loading details: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<PeriodeDetailsDto?> CreateDetailAsync(PeriodeDetailsDto detail)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var created = await _periodeService.CreateDetailAsync(detail);
            if (created != null)
            {
                CurrentPeriodeDetails.Add(created);
                NotifyTotalsChanged();
                Debug.WriteLine($"[PeriodeViewModel] Created detail {created.PeriodeDetailID}");
            }

            return created;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de création: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error creating detail: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<List<PeriodeDetailsDto>> CreateDetailsBatchAsync(List<PeriodeDetailsDto> details)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var created = await _periodeService.CreateDetailsBatchAsync(details);
            foreach (var detail in created)
            {
                CurrentPeriodeDetails.Add(detail);
            }
            
            NotifyTotalsChanged();
            Debug.WriteLine($"[PeriodeViewModel] Created {created.Count} details in batch");

            return created;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de création: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error creating details batch: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> UpdateDetailAsync(PeriodeDetailsDto detail)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _periodeService.UpdateDetailAsync(detail);
            if (success)
            {
                // Update in collection
                var index = CurrentPeriodeDetails.ToList().FindIndex(d => d.PeriodeDetailID == detail.PeriodeDetailID);
                if (index >= 0)
                {
                    CurrentPeriodeDetails[index] = detail;
                }
                NotifyTotalsChanged();
                Debug.WriteLine($"[PeriodeViewModel] Updated detail {detail.PeriodeDetailID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de mise à jour: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error updating detail: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<bool> DeleteDetailAsync(PeriodeDetailsDto detail)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _periodeService.DeleteDetailAsync(detail.PeriodeDetailID);
            if (success)
            {
                CurrentPeriodeDetails.Remove(detail);
                NotifyTotalsChanged();
                Debug.WriteLine($"[PeriodeViewModel] Deleted detail {detail.PeriodeDetailID}");
            }

            return success;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de suppression: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error deleting detail: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    #endregion

    #region Combined Operations

    /// <summary>
    /// Creates a new periode with its details in one operation.
    /// Also updates pump meters to the final readings.
    /// </summary>
    public async Task<(PeriodeDto? Periode, List<PeriodeDetailsDto> Details)> CreatePeriodeWithDetailsAsync(
        PeriodeDto periode, 
        List<PeriodeDetailsDto> details)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var result = await _periodeService.CreatePeriodeWithDetailsAsync(periode, details);
            
            if (result.Periode != null)
            {
                Periodes.Insert(0, result.Periode);
                SelectedPeriode = result.Periode;
                
                CurrentPeriodeDetails.Clear();
                foreach (var detail in result.Details)
                {
                    CurrentPeriodeDetails.Add(detail);
                }
                NotifyTotalsChanged();

                // Update pump meters to final readings
                await UpdatePumpMetersAsync(result.Details);

                Debug.WriteLine($"[PeriodeViewModel] Created periode {result.Periode.PeriodeID} with {result.Details.Count} details");
            }

            return result;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de création: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error creating periode with details: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Creates a new periode with its details from DTO.
    /// </summary>
    public async Task CreatePeriodeWithDetailsAsync(PeriodeWithDetailsDto dto)
    {
        await CreatePeriodeWithDetailsAsync(dto.Periode, dto.Details);
    }

    /// <summary>
    /// Updates a periode with its details from DTO.
    /// Uses the API endpoint that properly handles stock reversal and re-consumption.
    /// </summary>
    public async Task UpdatePeriodeWithDetailsAsync(PeriodeWithDetailsDto dto)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            // Use the new endpoint that handles stock properly
            var result = await _periodeService.UpdatePeriodeWithDetailsAsync(dto.Periode, dto.Details);

            if (result.Periode != null)
            {
                // Update in collection
                var index = Periodes.ToList().FindIndex(p => p.PeriodeID == dto.Periode.PeriodeID);
                if (index >= 0)
                {
                    Periodes[index] = result.Periode;
                }

                SelectedPeriode = result.Periode;

                CurrentPeriodeDetails.Clear();
                foreach (var detail in result.Details)
                {
                    CurrentPeriodeDetails.Add(detail);
                }
                NotifyTotalsChanged();

                // Update pump meters to final readings
                await UpdatePumpMetersAsync(result.Details);

                Debug.WriteLine($"[PeriodeViewModel] Updated periode {dto.Periode.PeriodeID} with {result.Details.Count} details (stock adjusted)");
            }
            else
            {
                ErrorMessage = "Erreur lors de la mise à jour de la période";
                Debug.WriteLine($"[PeriodeViewModel] Failed to update periode {dto.Periode.PeriodeID}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de mise à jour: {ex.Message}";
            Debug.WriteLine($"[PeriodeViewModel] Error updating periode with details: {ex.Message}");
            throw;
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Refreshes the current periode and its details.
    /// </summary>
    [RelayCommand]
    public async Task RefreshCurrentPeriodeAsync()
    {
        if (SelectedPeriode == null) return;

        try
        {
            IsLoading = true;

            var result = await _periodeService.GetPeriodeWithDetailsAsync(SelectedPeriode.PeriodeID);
            
            if (result.Periode != null)
            {
                SelectedPeriode = result.Periode;
                
                CurrentPeriodeDetails.Clear();
                foreach (var detail in result.Details)
                {
                    CurrentPeriodeDetails.Add(detail);
                }
                NotifyTotalsChanged();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeViewModel] Error refreshing current periode: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Helper Methods

    private void NotifyTotalsChanged()
    {
        OnPropertyChanged(nameof(TotalQuantiteVendue));
        OnPropertyChanged(nameof(TotalPrixTotal));
        OnPropertyChanged(nameof(TotalDifferenceQuantite));
        OnPropertyChanged(nameof(TotalDifferenceValeur));
        OnPropertyChanged(nameof(TotalTPE));
        OnPropertyChanged(nameof(TotalEspeces));
        OnPropertyChanged(nameof(TotalPaiements));
        OnPropertyChanged(nameof(EcartPaiement));
    }

    /// <summary>
    /// Updates pump meters to their final readings after a period is created.
    /// </summary>
    private async Task UpdatePumpMetersAsync(List<PeriodeDetailsDto> details)
    {
        foreach (var detail in details)
        {
            try
            {
                if (!detail.PompeID.HasValue)
                {
                    Debug.WriteLine($"[PeriodeViewModel] Skipping detail with no PompeID");
                    continue;
                }

                var pompe = await _pompeService.GetByIdAsync(detail.PompeID.Value);
                if (pompe != null)
                {
                    // Update the pump's current meters to the final values
                    pompe.CompteurElectroniqueActuel = detail.CompteurElectroniqueFinal;
                    pompe.CompteurMecaniqueActuel = detail.CompteurMecaniqueFinal;
                    
                    var success = await _pompeService.UpdateAsync(pompe);
                    if (success)
                    {
                        Debug.WriteLine($"[PeriodeViewModel] Updated pump {pompe.Numero} meters: Elec={detail.CompteurElectroniqueFinal}, Meca={detail.CompteurMecaniqueFinal}");
                    }
                    else
                    {
                        Debug.WriteLine($"[PeriodeViewModel] Failed to update pump {pompe.Numero} meters");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PeriodeViewModel] Error updating pump {detail.PompeID} meters: {ex.Message}");
                // Continue with other pumps even if one fails
            }
        }
    }

    /// <summary>
    /// Clears any error message.
    /// </summary>
    public void ClearError()
    {
        ErrorMessage = null;
    }

    #endregion
}

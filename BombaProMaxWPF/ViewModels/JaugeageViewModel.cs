using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for managing Jaugeage (tank gauging) operations.
/// Handles reservoir measurements with automatic volume calculation from calibration data.
/// </summary>
public partial class JaugeageViewModel : ObservableObject
{
    private readonly JaugeageService _jaugeageService;
    private readonly JaugeageDetailService _detailService;
    private readonly ReservoirService _reservoirService;
    private readonly EmployeService _employeService;
    private readonly JourneeNavigationService _journeeService;
    private readonly ReservoirCalibrationService _calibrationService;

    #region Observable Properties

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private string? _successMessage;
    public string? SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    // Date du jaugeage (date only, will be combined with time for UTC)
    private DateTime _dateJaugeage = DateTime.Today;
    public DateTime DateJaugeage
    {
        get => _dateJaugeage;
        set => SetProperty(ref _dateJaugeage, value);
    }

    // Numero du jaugeage (auto-generated if empty)
    private string? _numeroJaugeage;
    public string? NumeroJaugeage
    {
        get => _numeroJaugeage;
        set => SetProperty(ref _numeroJaugeage, value);
    }

    // Temoin (witness) - must be an Employe
    private EmployeDto? _selectedTemoin;
    public EmployeDto? SelectedTemoin
    {
        get => _selectedTemoin;
        set
        {
            if (SetProperty(ref _selectedTemoin, value))
            {
                OnPropertyChanged(nameof(CanSave));
            }
        }
    }

    private string? _observations;
    public string? Observations
    {
        get => _observations;
        set => SetProperty(ref _observations, value);
    }

    private JaugeageWithDetailsDto? _currentJaugeage;
    public JaugeageWithDetailsDto? CurrentJaugeage
    {
        get => _currentJaugeage;
        set
        {
            if (SetProperty(ref _currentJaugeage, value))
            {
                OnPropertyChanged(nameof(HasCurrentJaugeage));
                OnPropertyChanged(nameof(SavedJaugeageNumero));
            }
        }
    }

    public bool HasCurrentJaugeage => CurrentJaugeage != null;
    public string SavedJaugeageNumero => CurrentJaugeage?.NumeroJaugeage ?? string.Empty;

    #endregion

    #region Filter Properties

    private DateTime _filterStartDate = DateTime.Today.AddMonths(-1);
    public DateTime FilterStartDate
    {
        get => _filterStartDate;
        set => SetProperty(ref _filterStartDate, value);
    }

    private DateTime _filterEndDate = DateTime.Today;
    public DateTime FilterEndDate
    {
        get => _filterEndDate;
        set => SetProperty(ref _filterEndDate, value);
    }

    // Maximum date for DatePickers (today)
    public DateTime FilterMaxDate => DateTime.Today;

    #endregion

    #region Calibration Properties

    private ReservoirDto? _selectedCalibrationReservoir;
    public ReservoirDto? SelectedCalibrationReservoir
    {
        get => _selectedCalibrationReservoir;
        set
        {
            if (SetProperty(ref _selectedCalibrationReservoir, value))
            {
                OnPropertyChanged(nameof(HasSelectedCalibrationReservoir));
                if (value != null)
                {
                    _ = LoadCalibrationsAsync(value.ID);
                }
                else
                {
                    Calibrations.Clear();
                    NotifyCalibrationStatsChanged();
                }
            }
        }
    }

    public bool HasSelectedCalibrationReservoir => SelectedCalibrationReservoir != null;

    #endregion

    #region Collections

    public ObservableCollection<ReservoirMeasurementItem> ReservoirMeasurements { get; } = [];
    public ObservableCollection<JaugeageDto> JaugeageHistory { get; } = [];
    public ObservableCollection<EmployeDto> Employes { get; } = [];
    
    /// <summary>
    /// All reservoirs for calibration tab (with calibration status)
    /// </summary>
    public ObservableCollection<ReservoirDto> CalibrationReservoirs { get; } = [];
    
    /// <summary>
    /// Calibration data for selected reservoir
    /// </summary>
    public ObservableCollection<ReservoirCalibrationDto> Calibrations { get; } = [];

    #endregion

    #region Computed Properties

    public int TotalReservoirs => ReservoirMeasurements.Count;
    public int MeasuredCount => ReservoirMeasurements.Count(r => r.HauteurMesuree > 0);
    public decimal TotalVolume => ReservoirMeasurements.Sum(r => r.VolumeCalcule);
    public bool CanSave => MeasuredCount > 0 && SelectedTemoin != null && !IsSaving;
    public bool AllMeasured => MeasuredCount == TotalReservoirs && TotalReservoirs > 0;
    
    // Stats for the list view
    public int TotalJaugeages => JaugeageHistory.Count;
    public decimal TotalVolumeJaugeages => JaugeageHistory.Sum(j => j.TotalVolume);

    // Calibration stats
    public int TotalCalibrations => Calibrations.Count;
    public decimal CalibrationMinHauteur => Calibrations.Count > 0 ? Calibrations.Min(c => c.HauteurCm) : 0;
    public decimal CalibrationMaxHauteur => Calibrations.Count > 0 ? Calibrations.Max(c => c.HauteurCm) : 0;
    public decimal CalibrationMaxVolume => Calibrations.Count > 0 ? Calibrations.Max(c => c.VolumeLitres) : 0;
    public bool HasCalibrationData => Calibrations.Count > 0;
    
    // Reservoir calibration stats
    public int CalibratedReservoirsCount => CalibrationReservoirs.Count(r => r.HasCalibration);
    public int NotCalibratedReservoirsCount => CalibrationReservoirs.Count(r => !r.HasCalibration);

    #endregion

    #region Journee Properties

    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Etape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    #endregion

    #region Commands

    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand LoadReservoirsCommand { get; }
    public IAsyncRelayCommand LoadEmployesCommand { get; }
    public IAsyncRelayCommand SaveJaugeageCommand { get; }
    public IAsyncRelayCommand LoadHistoryCommand { get; }
    public IAsyncRelayCommand LoadJaugeagesByDateRangeCommand { get; }
    public IAsyncRelayCommand NewJaugeageCommand { get; }
    public IAsyncRelayCommand JourneeSuivantCommand { get; }
    public IAsyncRelayCommand JourneePasserCommand { get; }
    public IAsyncRelayCommand JourneePrecedentCommand { get; }
    public IAsyncRelayCommand LoadCalibrationReservoirsCommand { get; }
    public IAsyncRelayCommand RefreshCalibrationsCommand { get; }
    public IAsyncRelayCommand DeleteAllCalibrationsCommand { get; }

    #endregion

    #region Constructor

    public JaugeageViewModel(JourneeNavigationService journeeService)
    {
        _jaugeageService = new JaugeageService();
        _detailService = new JaugeageDetailService();
        _reservoirService = new ReservoirService();
        _employeService = new EmployeService();
        _journeeService = journeeService;
        _calibrationService = new ReservoirCalibrationService();

        // Initialize commands
        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        LoadReservoirsCommand = new AsyncRelayCommand(LoadReservoirsAsync);
        LoadEmployesCommand = new AsyncRelayCommand(LoadEmployesAsync);
        SaveJaugeageCommand = new AsyncRelayCommand(SaveJaugeageAsync);
        LoadHistoryCommand = new AsyncRelayCommand(LoadHistoryAsync);
        LoadJaugeagesByDateRangeCommand = new AsyncRelayCommand(LoadJaugeagesByDateRangeAsync);
        NewJaugeageCommand = new AsyncRelayCommand(NewJaugeageAsync);
        JourneeSuivantCommand = new AsyncRelayCommand(JourneeSuivantAsync);
        JourneePasserCommand = new AsyncRelayCommand(JourneePasserAsync);
        JourneePrecedentCommand = new AsyncRelayCommand(JourneePrecedentAsync);
        LoadCalibrationReservoirsCommand = new AsyncRelayCommand(LoadCalibrationReservoirsAsync);
        RefreshCalibrationsCommand = new AsyncRelayCommand(RefreshCalibrationsAsync);
        DeleteAllCalibrationsCommand = new AsyncRelayCommand(DeleteAllCalibrationsAsync);

        // Subscribe to journee changes
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

    #region Journee Commands

    private async Task JourneeSuivantAsync() => await _journeeService.GoNextAsync(skipped: false);
    private async Task JourneePasserAsync() => await _journeeService.GoNextAsync(skipped: true);
    private async Task JourneePrecedentAsync() => await _journeeService.GoPreviousAsync();

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        await LoadEmployesAsync();
        await LoadReservoirsAsync();
        await LoadHistoryAsync();
        await LoadCalibrationReservoirsAsync();
    }

    #endregion

    #region Load Employes

    public async Task LoadEmployesAsync()
    {
        try
        {
            var employes = await _employeService.GetAllEmployesAsync();
            
            Employes.Clear();
            foreach (var employe in employes)
            {
                Employes.Add(employe);
            }

            Debug.WriteLine($"[JaugeageVM] Loaded {Employes.Count} employes");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageVM] Error loading employes: {ex.Message}");
        }
    }

    #endregion

    #region Reservoir Operations

    public async Task LoadReservoirsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var reservoirs = await _reservoirService.GetAllReservoirsAsync();

            ReservoirMeasurements.Clear();
            foreach (var reservoir in reservoirs)
            {
                var item = new ReservoirMeasurementItem(reservoir, this);
                ReservoirMeasurements.Add(item);
            }

            NotifyStatsChanged();
            Debug.WriteLine($"[JaugeageVM] Loaded {ReservoirMeasurements.Count} reservoirs");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[JaugeageVM] Error loading reservoirs: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Volume Calculation

    /// <summary>
    /// Calculates volume for a reservoir based on measured height.
    /// Called automatically when user enters height.
    /// </summary>
    public async Task CalculateVolumeAsync(ReservoirMeasurementItem item)
    {
        if (item.HauteurMesuree <= 0)
        {
            item.VolumeCalcule = 0;
            item.IsInterpolated = false;
            item.CalculationStatus = "En attente";
            NotifyStatsChanged();
            return;
        }

        try
        {
            item.IsCalculating = true;
            item.CalculationStatus = "Calcul...";

            var result = await _detailService.CalculateVolumeAsync(item.ReservoirID, item.HauteurMesuree);

            if (result != null)
            {
                item.VolumeCalcule = result.VolumeLitres;
                item.IsInterpolated = result.IsInterpolated;
                item.CalculationStatus = result.IsInterpolated ? "Interpole" : "Exact";
            }
            else
            {
                item.VolumeCalcule = 0;
                item.CalculationStatus = "Pas de calibration";
            }

            NotifyStatsChanged();
        }
        catch (Exception ex)
        {
            item.CalculationStatus = "Erreur";
            Debug.WriteLine($"[JaugeageVM] Error calculating volume: {ex.Message}");
        }
        finally
        {
            item.IsCalculating = false;
        }
    }

    #endregion

    #region Save Operations

    public async Task SaveJaugeageAsync()
    {
        // Validation
        if (MeasuredCount == 0)
        {
            ErrorMessage = "Veuillez mesurer au moins un reservoir";
            return;
        }

        if (SelectedTemoin == null)
        {
            ErrorMessage = "Veuillez selectionner un temoin (employe)";
            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = null;
            SuccessMessage = null;

            // Convert local date to UTC timestamp
            // DateJaugeage is a local date, we convert it to UTC for storage
            var dateJaugeageUtc = DateTime.SpecifyKind(DateJaugeage.Date, DateTimeKind.Utc);

            Debug.WriteLine($"[JaugeageVM] Saving jaugeage:");
            Debug.WriteLine($"[JaugeageVM]   - DateJaugeage (local): {DateJaugeage}");
            Debug.WriteLine($"[JaugeageVM]   - DateJaugeage (UTC): {dateJaugeageUtc:O}");
            Debug.WriteLine($"[JaugeageVM]   - TemoinID: {SelectedTemoin.ID} ({SelectedTemoin.Nom} {SelectedTemoin.Prenom})");
            Debug.WriteLine($"[JaugeageVM]   - NumeroJaugeage: {NumeroJaugeage ?? "(auto-generate)"}");
            Debug.WriteLine($"[JaugeageVM]   - Measurements: {MeasuredCount}");

            // Create jaugeage with details
            var jaugeage = new JaugeageWithDetailsDto
            {
                DateJaugeage = dateJaugeageUtc,
                TemoinID = SelectedTemoin.ID,  // Employe ID, NOT User ID!
                NumeroJaugeage = string.IsNullOrWhiteSpace(NumeroJaugeage) ? null : NumeroJaugeage, // null = auto-generate
                Observations = Observations,
                Details = ReservoirMeasurements
                    .Where(r => r.HauteurMesuree > 0)
                    .Select(r => new JaugeageDetailDto
                    {
                        ReservoirID = r.ReservoirID,
                        HauteurMesuree = r.HauteurMesuree,
                        VolumeCalcule = r.VolumeCalcule,
                        Temperature = r.Temperature,
                        Notes = r.Notes
                    })
                    .ToList()
            };

            var result = await _jaugeageService.CreateJaugeageWithDetailsAsync(jaugeage);

            if (result != null)
            {
                CurrentJaugeage = result;
                SuccessMessage = $"Jaugeage {result.NumeroJaugeage} enregistre avec succès!";
                Debug.WriteLine($"[JaugeageVM] Saved jaugeage {result.NumeroJaugeage} with {result.Details.Count} details");
                
                // Reload history
                await LoadHistoryAsync();
            }
            else
            {
                ErrorMessage = "Échec de l'enregistrement - vérifiez les logs";
                Debug.WriteLine("[JaugeageVM] SaveJaugeageAsync returned null");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur d'enregistrement: {ex.Message}";
            Debug.WriteLine($"[JaugeageVM] Error saving jaugeage: {ex.Message}");
            Debug.WriteLine($"[JaugeageVM] Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    #endregion

    #region History Operations

    /// <summary>
    /// Loads ALL jaugeages from the database (no date filter)
    /// </summary>
    public async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load ALL jaugeages
            var history = await _jaugeageService.GetAllJaugeagesAsync();
            
            JaugeageHistory.Clear();
            foreach (var jaugeage in history.OrderByDescending(j => j.DateCreation))
            {
                JaugeageHistory.Add(jaugeage);
            }

            NotifyJaugeageStatsChanged();
            Debug.WriteLine($"[JaugeageVM] Loaded {JaugeageHistory.Count} jaugeages (all)");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageVM] Error loading history: {ex.Message}");
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads jaugeages filtered by date range
    /// </summary>
    public async Task LoadJaugeagesByDateRangeAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load all jaugeages from API
            var allJaugeages = await _jaugeageService.GetAllJaugeagesAsync();
            
            // Convert filter dates to UTC for comparison
            // DateJaugeage from API is stored in UTC
            var startDateUtc = DateTime.SpecifyKind(FilterStartDate.Date, DateTimeKind.Utc);
            var endDateUtc = DateTime.SpecifyKind(FilterEndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc); // End of day
            
            Debug.WriteLine($"[JaugeageVM] Filtering: {startDateUtc:O} to {endDateUtc:O}");
            
            var filtered = allJaugeages
                .Where(j => 
                {
                    // Ensure we compare dates properly regardless of Kind
                    var jaugeageDateUtc = j.DateJaugeage.Kind == DateTimeKind.Utc 
                        ? j.DateJaugeage 
                        : DateTime.SpecifyKind(j.DateJaugeage, DateTimeKind.Utc);
                    
                    return jaugeageDateUtc >= startDateUtc && jaugeageDateUtc <= endDateUtc;
                })
                .OrderByDescending(j => j.DateCreation)
                .ToList();
            
            JaugeageHistory.Clear();
            foreach (var jaugeage in filtered)
            {
                JaugeageHistory.Add(jaugeage);
            }

            NotifyJaugeageStatsChanged();
            Debug.WriteLine($"[JaugeageVM] Loaded {JaugeageHistory.Count} jaugeages (filtered: {FilterStartDate:d} - {FilterEndDate:d})");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageVM] Error loading filtered jaugeages: {ex.Message}");
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task NewJaugeageAsync()
    {
        CurrentJaugeage = null;
        DateJaugeage = DateTime.Today;
        NumeroJaugeage = null; // Will be auto-generated
        Observations = null;
        SuccessMessage = null;
        ErrorMessage = null;
        
        // Keep selected temoin if already set
        
        // Reset all measurements
        foreach (var item in ReservoirMeasurements)
        {
            item.Reset();
        }

        NotifyStatsChanged();
        await Task.CompletedTask;
    }

    #endregion

    #region Calibration Operations

    /// <summary>
    /// Loads all reservoirs for the calibration tab, sorted by calibration status
    /// </summary>
    public async Task LoadCalibrationReservoirsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var reservoirs = await _reservoirService.GetAllReservoirsAsync();

            CalibrationReservoirs.Clear();
            
            // Add calibrated reservoirs first, then non-calibrated
            foreach (var reservoir in reservoirs.OrderByDescending(r => r.HasCalibration).ThenBy(r => r.Numero))
            {
                CalibrationReservoirs.Add(reservoir);
            }

            OnPropertyChanged(nameof(CalibratedReservoirsCount));
            OnPropertyChanged(nameof(NotCalibratedReservoirsCount));
            
            Debug.WriteLine($"[JaugeageVM] Loaded {CalibrationReservoirs.Count} reservoirs for calibration tab ({CalibratedReservoirsCount} calibrated, {NotCalibratedReservoirsCount} not calibrated)");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[JaugeageVM] Error loading calibration reservoirs: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads calibration data for a specific reservoir
    /// </summary>
    public async Task LoadCalibrationsAsync(int reservoirId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var calibrations = await _calibrationService.GetCalibrationsByReservoirAsync(reservoirId);

            Calibrations.Clear();
            foreach (var calibration in calibrations)
            {
                Calibrations.Add(calibration);
            }

            NotifyCalibrationStatsChanged();
            Debug.WriteLine($"[JaugeageVM] Loaded {Calibrations.Count} calibrations for reservoir {reservoirId}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[JaugeageVM] Error loading calibrations: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes calibration data for selected reservoir
    /// </summary>
    public async Task RefreshCalibrationsAsync()
    {
        if (SelectedCalibrationReservoir == null) return;
        await LoadCalibrationsAsync(SelectedCalibrationReservoir.ID);
    }

    /// <summary>
    /// Imports calibration data from CSV content for a specific reservoir
    /// </summary>
    public async Task<bool> ImportCalibrationForReservoirAsync(int reservoirId, string csvContent)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var calibrations = _calibrationService.ParseCsvData(csvContent);

            if (calibrations.Count == 0)
            {
                ErrorMessage = "Aucune donnee valide trouvee dans le fichier CSV";
                return false;
            }

            var result = await _calibrationService.ImportCalibrationsAsync(reservoirId, calibrations);

            if (result != null)
            {
                Debug.WriteLine($"[JaugeageVM] Imported {result.Count} calibrations for reservoir {reservoirId}, HauteurMax: {result.HauteurMax}");
                return true;
            }
            else
            {
                ErrorMessage = "Echec de l'importation";
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur d'importation: {ex.Message}";
            Debug.WriteLine($"[JaugeageVM] Error importing calibrations for reservoir {reservoirId}: {ex.Message}");
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Deletes all calibration data for selected reservoir (command method)
    /// </summary>
    public async Task DeleteAllCalibrationsAsync()
    {
        if (SelectedCalibrationReservoir == null) return;
        await DeleteCalibrationForReservoirAsync(SelectedCalibrationReservoir.ID);
    }

    /// <summary>
    /// Deletes all calibration data for a specific reservoir
    /// </summary>
    public async Task<bool> DeleteCalibrationForReservoirAsync(int reservoirId)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _calibrationService.DeleteAllCalibrationsAsync(reservoirId);

            if (success)
            {
                Debug.WriteLine($"[JaugeageVM] Deleted all calibrations for reservoir {reservoirId}");
                return true;
            }
            else
            {
                ErrorMessage = "Echec de la suppression";
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de suppression: {ex.Message}";
            Debug.WriteLine($"[JaugeageVM] Error deleting calibrations for reservoir {reservoirId}: {ex.Message}");
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Looks up volume for a given height using calibration data for a specific reservoir
    /// </summary>
    public async Task<VolumeLookupResultDto?> LookupVolumeForReservoirAsync(int reservoirId, decimal hauteurCm)
    {
        try
        {
            return await _calibrationService.LookupVolumeAsync(reservoirId, hauteurCm);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageVM] Error looking up volume for reservoir {reservoirId}: {ex.Message}");
            return null;
        }
    }

    private void NotifyCalibrationStatsChanged()
    {
        OnPropertyChanged(nameof(TotalCalibrations));
        OnPropertyChanged(nameof(CalibrationMinHauteur));
        OnPropertyChanged(nameof(CalibrationMaxHauteur));
        OnPropertyChanged(nameof(CalibrationMaxVolume));
        OnPropertyChanged(nameof(HasCalibrationData));
    }

    #endregion

    #region Helper Methods

    public void NotifyStatsChanged()
    {
        OnPropertyChanged(nameof(TotalReservoirs));
        OnPropertyChanged(nameof(MeasuredCount));
        OnPropertyChanged(nameof(TotalVolume));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(AllMeasured));
    }

    public void NotifyJaugeageStatsChanged()
    {
        OnPropertyChanged(nameof(TotalJaugeages));
        OnPropertyChanged(nameof(TotalVolumeJaugeages));
    }

    public void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    #endregion
}

/// <summary>
/// Represents a single reservoir measurement entry in the jaugeage form.
/// </summary>
public class ReservoirMeasurementItem : ObservableObject
{
    private readonly JaugeageViewModel _viewModel;
    private System.Timers.Timer? _debounceTimer;

    public ReservoirMeasurementItem(ReservoirDto reservoir, JaugeageViewModel viewModel)
    {
        _viewModel = viewModel;
        ReservoirID = reservoir.ID;
        ReservoirNumero = reservoir.Numero;
        ProduitNom = reservoir.ProduitNom ?? "Non assigne";
        Capacite = reservoir.Capacite;
        NiveauActuel = reservoir.NiveauDeCarburant;
        HauteurMax = reservoir.HauteurMax ?? 0;
        HasCalibration = reservoir.HasCalibration;
    }

    // Reservoir info (readonly)
    public int ReservoirID { get; }
    public string ReservoirNumero { get; }
    public string ProduitNom { get; }
    public decimal Capacite { get; }
    public decimal NiveauActuel { get; }
    public decimal HauteurMax { get; }
    public bool HasCalibration { get; }

    // Measurement fields
    private decimal _hauteurMesuree;
    public decimal HauteurMesuree
    {
        get => _hauteurMesuree;
        set
        {
            if (SetProperty(ref _hauteurMesuree, value))
            {
                // Debounce the volume calculation
                _debounceTimer?.Stop();
                _debounceTimer?.Dispose();
                _debounceTimer = new System.Timers.Timer(300);
                _debounceTimer.Elapsed += async (s, e) =>
                {
                    _debounceTimer?.Stop();
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        await _viewModel.CalculateVolumeAsync(this);
                    });
                };
                _debounceTimer.Start();
            }
        }
    }

    private decimal _volumeCalcule;
    public decimal VolumeCalcule
    {
        get => _volumeCalcule;
        set => SetProperty(ref _volumeCalcule, value);
    }

    private decimal? _temperature;
    public decimal? Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    // Calculation status
    private bool _isCalculating;
    public bool IsCalculating
    {
        get => _isCalculating;
        set => SetProperty(ref _isCalculating, value);
    }

    private bool _isInterpolated;
    public bool IsInterpolated
    {
        get => _isInterpolated;
        set => SetProperty(ref _isInterpolated, value);
    }

    private string _calculationStatus = "En attente";
    public string CalculationStatus
    {
        get => _calculationStatus;
        set => SetProperty(ref _calculationStatus, value);
    }

    // Computed properties
    public bool IsMeasured => HauteurMesuree > 0;
    public string StatusColor => HasCalibration 
        ? (IsMeasured ? "#2E7D32" : "#1976D2") 
        : "#F57C00";

    public void Reset()
    {
        HauteurMesuree = 0;
        VolumeCalcule = 0;
        Temperature = null;
        Notes = null;
        IsInterpolated = false;
        CalculationStatus = "En attente";
    }
}

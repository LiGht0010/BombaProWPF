using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for managing reservoir calibration data (height to volume mapping).
/// </summary>
public class ReservoirCalibrationViewModel : ObservableObject
{
    private readonly ReservoirCalibrationService _calibrationService;
    private readonly ReservoirService _reservoirService;

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

    private ReservoirDto? _selectedReservoir;
    public ReservoirDto? SelectedReservoir
    {
        get => _selectedReservoir;
        set
        {
            if (SetProperty(ref _selectedReservoir, value))
            {
                OnPropertyChanged(nameof(HasSelectedReservoir));
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

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    #endregion

    #region Collections

    public ObservableCollection<ReservoirDto> Reservoirs { get; } = [];
    public ObservableCollection<ReservoirCalibrationDto> Calibrations { get; } = [];

    #endregion

    #region Computed Properties

    /// <summary>
    /// Total calibration entries for selected reservoir.
    /// </summary>
    public int TotalCalibrations => Calibrations.Count;

    /// <summary>
    /// Minimum height in calibration table.
    /// </summary>
    public decimal MinHauteur => Calibrations.Count > 0 ? Calibrations.Min(c => c.HauteurCm) : 0;

    /// <summary>
    /// Maximum height in calibration table.
    /// </summary>
    public decimal MaxHauteur => Calibrations.Count > 0 ? Calibrations.Max(c => c.HauteurCm) : 0;

    /// <summary>
    /// Maximum volume in calibration table.
    /// </summary>
    public decimal MaxVolume => Calibrations.Count > 0 ? Calibrations.Max(c => c.VolumeLitres) : 0;

    /// <summary>
    /// Indicates if selected reservoir has calibration data.
    /// </summary>
    public bool HasCalibrationData => Calibrations.Count > 0;

    /// <summary>
    /// Indicates if a reservoir is selected.
    /// </summary>
    public bool HasSelectedReservoir => SelectedReservoir != null;

    #endregion

    #region Constructor

    public ReservoirCalibrationViewModel()
    {
        _calibrationService = new ReservoirCalibrationService();
        _reservoirService = new ReservoirService();
        
        // Initialize commands
        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        LoadReservoirsCommand = new AsyncRelayCommand(LoadReservoirsAsync);
        RefreshCalibrationsCommand = new AsyncRelayCommand(RefreshCalibrationsAsync);
        DeleteAllCalibrationsCommand = new AsyncRelayCommand(DeleteAllCalibrationsAsync);
    }

    #endregion

    #region Commands

    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand LoadReservoirsCommand { get; }
    public IAsyncRelayCommand RefreshCalibrationsCommand { get; }
    public IAsyncRelayCommand DeleteAllCalibrationsCommand { get; }

    #endregion

    #region Methods

    public async Task InitializeAsync()
    {
        await LoadReservoirsAsync();
    }

    public async Task LoadReservoirsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var reservoirs = await _reservoirService.GetAllReservoirsAsync();

            Reservoirs.Clear();
            foreach (var reservoir in reservoirs)
            {
                Reservoirs.Add(reservoir);
            }

            Debug.WriteLine($"[CalibrationVM] Loaded {Reservoirs.Count} reservoirs");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[CalibrationVM] Error loading reservoirs: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

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
            Debug.WriteLine($"[CalibrationVM] Loaded {Calibrations.Count} calibrations for reservoir {reservoirId}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[CalibrationVM] Error loading calibrations: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshCalibrationsAsync()
    {
        if (SelectedReservoir == null) return;
        await LoadCalibrationsAsync(SelectedReservoir.ID);
    }

    public async Task<bool> ImportFromCsvAsync(string csvContent)
    {
        if (SelectedReservoir == null)
        {
            ErrorMessage = "Veuillez sélectionner un réservoir";
            return false;
        }

        var currentReservoirId = SelectedReservoir.ID;

        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var calibrations = _calibrationService.ParseCsvData(csvContent);

            if (calibrations.Count == 0)
            {
                ErrorMessage = "Aucune donnée valide trouvée dans le fichier CSV";
                return false;
            }

            var result = await _calibrationService.ImportCalibrationsAsync(currentReservoirId, calibrations);

            if (result != null)
            {
                Debug.WriteLine($"[CalibrationVM] Imported {result.Count} calibrations, HauteurMax: {result.HauteurMax}");
                await LoadCalibrationsAsync(currentReservoirId);
                await LoadReservoirsAsync();
                SelectedReservoir = Reservoirs.FirstOrDefault(r => r.ID == currentReservoirId);
                return true;
            }
            else
            {
                ErrorMessage = "Échec de l'importation";
                return false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur d'importation: {ex.Message}";
            Debug.WriteLine($"[CalibrationVM] Error importing calibrations: {ex.Message}");
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task DeleteAllCalibrationsAsync()
    {
        if (SelectedReservoir == null) return;

        var currentReservoirId = SelectedReservoir.ID;

        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var success = await _calibrationService.DeleteAllCalibrationsAsync(currentReservoirId);

            if (success)
            {
                Calibrations.Clear();
                NotifyCalibrationStatsChanged();
                await LoadReservoirsAsync();
                SelectedReservoir = Reservoirs.FirstOrDefault(r => r.ID == currentReservoirId);
                Debug.WriteLine($"[CalibrationVM] Deleted all calibrations for reservoir {SelectedReservoir?.Numero}");
            }
            else
            {
                ErrorMessage = "Échec de la suppression";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de suppression: {ex.Message}";
            Debug.WriteLine($"[CalibrationVM] Error deleting calibrations: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task<VolumeLookupResultDto?> LookupVolumeAsync(decimal hauteurCm)
    {
        if (SelectedReservoir == null) return null;

        try
        {
            return await _calibrationService.LookupVolumeAsync(SelectedReservoir.ID, hauteurCm);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CalibrationVM] Error looking up volume: {ex.Message}");
            return null;
        }
    }

    private void NotifyCalibrationStatsChanged()
    {
        OnPropertyChanged(nameof(TotalCalibrations));
        OnPropertyChanged(nameof(MinHauteur));
        OnPropertyChanged(nameof(MaxHauteur));
        OnPropertyChanged(nameof(MaxVolume));
        OnPropertyChanged(nameof(HasCalibrationData));
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }

    /// <summary>
    /// Gets sample CSV format for user reference.
    /// </summary>
    public static string GetSampleCsvFormat()
    {
        return """
            HauteurCm,VolumeLitres
            1,13
            2,38
            3,66
            4,101
            ...
            248.6,30600
            """;
    }

    #endregion
}

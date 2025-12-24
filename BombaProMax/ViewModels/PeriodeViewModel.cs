using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private readonly JourneeNavigationService _journeeService;

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
    /// </summary>
    public async Task UpdatePeriodeWithDetailsAsync(PeriodeWithDetailsDto dto)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = null;

            // Update the periode
            await UpdatePeriodeAsync(dto.Periode);

            // Delete existing details and recreate them
            // (Alternatively, implement a proper update endpoint on the API)
            var existingDetails = await _periodeService.GetDetailsByPeriodeAsync(dto.Periode.PeriodeID);
            foreach (var detail in existingDetails)
            {
                await _periodeService.DeleteDetailAsync(detail.PeriodeDetailID);
            }

            // Set the periode ID on all details
            foreach (var detail in dto.Details)
            {
                detail.PeriodeID = dto.Periode.PeriodeID;
            }

            // Create new details
            if (dto.Details.Count > 0)
            {
                await CreateDetailsBatchAsync(dto.Details);
            }

            // Reload to show updated data
            await LoadPeriodeDetailsAsync(dto.Periode.PeriodeID);

            Debug.WriteLine($"[PeriodeViewModel] Updated periode {dto.Periode.PeriodeID} with {dto.Details.Count} details");
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

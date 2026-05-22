using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the read-only Reservoir Details dialog.
/// Shows all reservoir metadata, the full calibration table, and recent jaugeages.
/// </summary>
public partial class ReservoirDetailsViewModel : ObservableObject
{
    private readonly ReservoirCalibrationService _calibrationService = new();
    private readonly JaugeageDetailService _detailService = new();
    private readonly JaugeageService _jaugeageService = new();

    public ReservoirDto Reservoir { get; }

    // ── Computed display fields ──────────────────────────────────────
    public string NumeroDisplay => Reservoir.Numero;
    public string ProduitDisplay => Reservoir.ProduitNom ?? "—";
    public string CapaciteDisplay => $"{Reservoir.Capacite:N0} L";
    public string NiveauDisplay => $"{Reservoir.NiveauDeCarburant:N0} L  ({Reservoir.PourcentageRempli:0}%)";
    public string FabricantDisplay => string.IsNullOrWhiteSpace(Reservoir.Fabricant) ? "—" : Reservoir.Fabricant;
    public string NumeroSerieDisplay => string.IsNullOrWhiteSpace(Reservoir.NumeroSerie) ? "—" : Reservoir.NumeroSerie;
    public string DiametreDisplay => Reservoir.DiametreMm.HasValue ? $"{Reservoir.DiametreMm.Value / 10m:N1} cm" : "—";
    public string DateCreationDisplay => Reservoir.DateCreation.HasValue
        ? Reservoir.DateCreation.Value.ToString("dd/MM/yyyy") : "—";

    // ── Collections ──────────────────────────────────────────────────
    public ObservableCollection<ReservoirCalibrationDto> CalibrationRows { get; } = new();
    public ObservableCollection<RecentJaugeageRow> RecentJaugeages { get; } = new();

    // ── UI state ─────────────────────────────────────────────────────
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _hasCalibration;
    [ObservableProperty] private bool _hasRecentJaugeages;

    public IAsyncRelayCommand LoadCommand { get; }

    public ReservoirDetailsViewModel(ReservoirDto reservoir)
    {
        Reservoir = reservoir;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    public Task EnsureLoadedAsync() => LoadCommand.ExecuteAsync(null);

    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;

            var calibTask = _calibrationService.GetCalibrationsByReservoirAsync(Reservoir.ID);
            var detailsTask = _detailService.GetDetailsByReservoirAsync(Reservoir.ID);
            await Task.WhenAll(calibTask, detailsTask).ConfigureAwait(false);

            // Resolve each detail's parent jaugeage (last 10, most recent first)
            var recentDetails = detailsTask.Result
                .OrderByDescending(d => d.JaugeageID)
                .Take(10)
                .ToList();

            var jaugeageTasks = recentDetails
                .Select(d => _jaugeageService.GetJaugeageByIdAsync(d.JaugeageID))
                .ToList();
            await Task.WhenAll(jaugeageTasks).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CalibrationRows.Clear();
                foreach (var c in calibTask.Result.OrderBy(c => c.HauteurCm))
                    CalibrationRows.Add(c);
                HasCalibration = CalibrationRows.Count > 0;

                RecentJaugeages.Clear();
                for (int i = 0; i < recentDetails.Count; i++)
                {
                    var j = jaugeageTasks[i].Result
                            ?? new JaugeageDto { ID = recentDetails[i].JaugeageID };
                    RecentJaugeages.Add(new RecentJaugeageRow(j));
                }
                HasRecentJaugeages = RecentJaugeages.Count > 0;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ReservoirDetailsVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

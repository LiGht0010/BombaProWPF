using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// Section-scoped view-model for the Réservoirs sub-section of the
/// Infrastructure shell. Wraps live <see cref="ReservoirDto"/> data plus the
/// most recent <see cref="JaugeageDetailDto"/> per reservoir, and exposes a
/// lean form for recording a single jaugeage from the right-hand pane.
/// </summary>
public partial class ReservoirsSectionViewModel : ObservableObject, IAsyncLoadable
{
    private readonly ReservoirService _reservoirService = new();
    private readonly JaugeageService _jaugeageService = new();
    private readonly JaugeageDetailService _detailService = new();
    private readonly EmployeService _employeService = new();

    public ObservableCollection<ReservoirCardItem> Reservoirs { get; } = new();
    public ObservableCollection<JaugeageDto> RecentReadings { get; } = new();
    public ObservableCollection<RecentJaugeageRow> RecentJaugeageRows { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;

    // ─── Form ─────────────────────────────────────────────────────────────
    [ObservableProperty] private ReservoirCardItem? _selectedReservoirForReading;
    [ObservableProperty] private decimal _hauteurProduitMm;
    [ObservableProperty] private decimal _temperatureC = 20m;
    [ObservableProperty] private decimal _hauteurEauMm;

    public IAsyncRelayCommand SaveReadingCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand<ReservoirCardItem> SelectForReadingCommand { get; }
    public IRelayCommand SaisirJaugeageGlobalCommand { get; }
    public IRelayCommand AjouterReservoirCommand { get; }
    public IAsyncRelayCommand<ReservoirCardItem> EditReservoirCommand { get; }
    public IRelayCommand<ReservoirCardItem> ViewReservoirDetailsCommand { get; }
    public IAsyncRelayCommand<ReservoirCardItem> DeleteReservoirCommand { get; }
    public IRelayCommand<RecentJaugeageRow> ViewJaugeageCommand { get; }
    public IRelayCommand<RecentJaugeageRow> EditJaugeageCommand { get; }
    public IAsyncRelayCommand<RecentJaugeageRow> DeleteJaugeageCommand { get; }

    public ReservoirsSectionViewModel()
    {
        SaveReadingCommand = new AsyncRelayCommand(SaveReadingAsync, () => !IsSaving);
        RefreshCommand = new AsyncRelayCommand(ct => RefreshAsync(ct));
        SelectForReadingCommand = new RelayCommand<ReservoirCardItem>(item =>
        {
            if (item is not null) SelectedReservoirForReading = item;
        });
        SaisirJaugeageGlobalCommand = new RelayCommand(OpenSaisirJaugeageGlobal);
        AjouterReservoirCommand = new RelayCommand(OpenAjouterReservoir);
        EditReservoirCommand = new AsyncRelayCommand<ReservoirCardItem>(OpenEditReservoirAsync);
        ViewReservoirDetailsCommand = new RelayCommand<ReservoirCardItem>(OpenReservoirDetails);
        DeleteReservoirCommand = new AsyncRelayCommand<ReservoirCardItem>(DeleteReservoirAsync);
        ViewJaugeageCommand = new RelayCommand<RecentJaugeageRow>(OpenJaugeageDetails);
        EditJaugeageCommand = new RelayCommand<RecentJaugeageRow>(OpenEditJaugeage);
        DeleteJaugeageCommand = new AsyncRelayCommand<RecentJaugeageRow>(DeleteJaugeageAsync);
    }

    private void OpenSaisirJaugeageGlobal()
    {
        try
        {
            var dialog = new Views.InfrastructurePages.Sections.Reservoirs.SaisirJaugeageGlobalDialog
            {
                Owner = Application.Current?.MainWindow
            };
            _ = dialog.ViewModel.LoadAsync();
            var ok = dialog.ShowDialog();
            if (ok == true)
            {
                _ = RefreshAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] OpenSaisirJaugeageGlobal failed: {ex}");
        }
    }

    private void OpenAjouterReservoir()
    {
        try
        {
            var dialog = new Views.InfrastructurePages.Sections.Reservoirs.NouveauReservoirDialog
            {
                Owner = Application.Current?.MainWindow
            };
            _ = dialog.ViewModel.LoadAsync();
            if (dialog.ShowDialog() == true)
                _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] OpenAjouterReservoir failed: {ex}");
        }
    }

    private async Task OpenEditReservoirAsync(ReservoirCardItem? item)
    {
        if (item is null) return;
        try
        {
            var dialog = new Views.InfrastructurePages.Sections.Reservoirs.EditReservoirDialog(item.Reservoir)
            {
                Owner = Application.Current?.MainWindow
            };
            // Await load so CalibrationLines is fully populated before the user can interact
            await dialog.ViewModel.EnsureLoadedAsync();
            if (dialog.ShowDialog() == true)
                _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] OpenEditReservoir failed: {ex}");
        }
    }

    private void OpenReservoirDetails(ReservoirCardItem? item)
    {
        if (item is null) return;
        try
        {
            var dialog = new Views.InfrastructurePages.Sections.Reservoirs.ReservoirDetailsDialog(item.Reservoir)
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir les détails: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] OpenReservoirDetails failed: {ex}");
        }
    }

    private async Task DeleteReservoirAsync(ReservoirCardItem? item)
    {
        if (item is null) return;
        try
        {
            var confirmDialog = new Views.InfrastructurePages.Sections.Reservoirs.DeleteReservoirConfirmDialog(item.Reservoir.Numero)
            {
                Owner = Application.Current?.MainWindow
            };
            if (confirmDialog.ShowDialog() != true) return;

            var ok = await _reservoirService.DeleteReservoirAsync(item.Reservoir.ID).ConfigureAwait(false);
            if (ok)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SuccessMessage = Localization.LanguageManager.Instance["ResDeleteSuccess"];
                    _ = RefreshAsync(CancellationToken.None);
                });
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    ErrorMessage = Localization.LanguageManager.Instance["ResDeleteError"]);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] DeleteReservoir failed: {ex}");
        }
    }

    private void OpenJaugeageDetails(RecentJaugeageRow? row)
    {
        if (row is null) return;
        try
        {
            var dialog = new Views.InfrastructurePages.Sections.Reservoirs.JaugeageDetailsDialog(row.Source.ID)
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir les détails: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] OpenJaugeageDetails failed: {ex}");
        }
    }

    private void OpenEditJaugeage(RecentJaugeageRow? row)
    {
        if (row is null) return;
        try
        {
            var dialog = new Views.InfrastructurePages.Sections.Reservoirs.EditJaugeageDialog(row.Source.ID)
            {
                Owner = Application.Current?.MainWindow
            };
            if (dialog.ShowDialog() == true)
                _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir l'éditeur: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] OpenEditJaugeage failed: {ex}");
        }
    }

    private async Task DeleteJaugeageAsync(RecentJaugeageRow? row)
    {
        if (row is null) return;
        try
        {
            var numero = string.IsNullOrWhiteSpace(row.Source.NumeroJaugeage)
                ? $"#JG-{row.Source.ID:0000}"
                : $"#{row.Source.NumeroJaugeage}";
            var confirmDialog = new Views.InfrastructurePages.Sections.Reservoirs.DeleteJaugeageConfirmDialog(numero)
            {
                Owner = Application.Current?.MainWindow
            };
            if (confirmDialog.ShowDialog() != true) return;

            var ok = await _jaugeageService.DeleteJaugeageAsync(row.Source.ID).ConfigureAwait(false);
            if (ok)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SuccessMessage = Localization.LanguageManager.Instance["JaugDeleteSuccess"];
                    _ = RefreshAsync(CancellationToken.None);
                });
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    ErrorMessage = Localization.LanguageManager.Instance["JaugDeleteError"]);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] DeleteJaugeage failed: {ex}");
        }
    }

    // ─── IAsyncLoadable ───────────────────────────────────────────────────

    public Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        return IsLoaded ? Task.CompletedTask : LoadAsync(ct);
    }

    public Task RefreshAsync(CancellationToken ct = default)
    {
        IsLoaded = false;
        return LoadAsync(ct);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var reservoirsTask = _reservoirService.GetAllReservoirsAsync();
            var jaugeagesTask = _jaugeageService.GetAllJaugeagesAsync();
            await Task.WhenAll(reservoirsTask, jaugeagesTask).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            var reservoirs = reservoirsTask.Result;
            var jaugeages = jaugeagesTask.Result;

            // Index jaugeages by ID so we can resolve dates for details.
            var jaugeageById = jaugeages
                .Where(j => j.ID > 0)
                .ToDictionary(j => j.ID);

            // Pull last detail per reservoir in parallel.
            var detailTasks = reservoirs.Select(async r =>
            {
                var details = await _detailService.GetDetailsByReservoirAsync(r.ID).ConfigureAwait(false);
                var last = details
                    .OrderByDescending(d => d.JaugeageID)
                    .FirstOrDefault();
                DateTime? lastDate = null;
                if (last is not null && jaugeageById.TryGetValue(last.JaugeageID, out var j))
                {
                    lastDate = (j.DateJaugeage != default) ? j.DateJaugeage : j.DateCreation;
                }
                return (Reservoir: r, LastDetail: last, LastDate: lastDate);
            }).ToList();

            var enriched = await Task.WhenAll(detailTasks).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Reservoirs.Clear();
                foreach (var (r, d, dt) in enriched)
                {
                    Reservoirs.Add(new ReservoirCardItem(r, d, dt));
                }

                RecentReadings.Clear();
                RecentJaugeageRows.Clear();
                foreach (var j in jaugeages
                             .OrderByDescending(j => j.DateCreation ?? j.DateJaugeage)
                             .Take(8))
                {
                    RecentReadings.Add(j);
                    RecentJaugeageRows.Add(new RecentJaugeageRow(j));
                }

                // Default-select first reservoir for the form.
                SelectedReservoirForReading ??= Reservoirs.FirstOrDefault();
            });

            IsLoaded = true;
        }
        catch (OperationCanceledException) { /* unloaded mid-flight */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ─── Save Reading ─────────────────────────────────────────────────────

    private async Task SaveReadingAsync()
    {
        if (SelectedReservoirForReading is null)
        {
            ErrorMessage = "Veuillez sélectionner un réservoir.";
            return;
        }
        if (HauteurProduitMm <= 0)
        {
            ErrorMessage = "Hauteur du produit invalide.";
            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = null;
            SuccessMessage = null;

            // Pick first available employe as témoin (the multi-step Jaugeage
            // workflow handles witness selection; this lean form auto-resolves).
            var employes = await _employeService.GetAllEmployesAsync().ConfigureAwait(false);
            var temoin = employes.FirstOrDefault();
            if (temoin is null)
            {
                ErrorMessage = "Aucun employé disponible comme témoin.";
                return;
            }

            // Calibration data is stored in cm; UI works in mm — convert.
            var hauteurCm = HauteurProduitMm / 10m;

            // Look up volume from calibration (mirrors JaugeageViewModel.CalculateVolumeAsync).
            var lookup = await _detailService.CalculateVolumeAsync(
                SelectedReservoirForReading.Reservoir.ID,
                hauteurCm).ConfigureAwait(false);

            var notes = HauteurEauMm > 0 ? $"Eau: {HauteurEauMm:0.##} mm" : null;

            var jaugeage = new JaugeageWithDetailsDto
            {
                DateJaugeage = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                TemoinID = temoin.ID,
                NumeroJaugeage = null, // server auto-generates
                Observations = null,
                Details =
                [
                    new JaugeageDetailDto
                    {
                        ReservoirID = SelectedReservoirForReading.Reservoir.ID,
                        HauteurMesuree = hauteurCm,
                        VolumeCalcule = lookup?.VolumeLitres ?? 0m,
                        Temperature = TemperatureC,
                        Notes = notes
                    }
                ]
            };

            var saved = await _jaugeageService.CreateJaugeageWithDetailsAsync(jaugeage).ConfigureAwait(false);
            if (saved is null)
            {
                ErrorMessage = "Échec de l'enregistrement du relevé.";
                return;
            }

            SuccessMessage = $"Relevé {saved.NumeroJaugeage} enregistré.";
            HauteurProduitMm = 0;
            HauteurEauMm = 0;

            // Refresh data so cards + recent list reflect the new reading.
            await RefreshAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur d'enregistrement: {ex.Message}";
            Debug.WriteLine($"[ReservoirsSectionVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }
}

/// <summary>
/// Display item for a single reservoir card. Wraps the live <see cref="ReservoirDto"/>
/// and the most recent <see cref="JaugeageDetailDto"/> with view-friendly formatted
/// text and computed flags (low-level alert, level-bar star sizing, etc.).
/// </summary>
public class ReservoirCardItem
{
    private const decimal LowLevelThresholdPercent = 30m;

    public ReservoirCardItem(ReservoirDto reservoir, JaugeageDetailDto? lastDetail, DateTime? lastReadingDate)
    {
        Reservoir = reservoir;
        LastDetail = lastDetail;
        LastReadingDate = lastReadingDate;
    }

    public ReservoirDto Reservoir { get; }
    public JaugeageDetailDto? LastDetail { get; }
    public DateTime? LastReadingDate { get; }

    public string Numero => Reservoir.Numero;
    public string DisplayName => string.IsNullOrWhiteSpace(Reservoir.ProduitNom)
        ? Reservoir.Numero
        : Reservoir.ProduitNom!;
    public string IdBadge => $"RÉSERVOIR #{Reservoir.Numero}";
    public string CapacityDisplay => $"Capacité Totale: {Reservoir.Capacite:N0} L";
    public string LevelDisplay => $"Niveau Actuel: {Reservoir.NiveauDeCarburant:N0} L";
    public string PercentDisplay => $"{Reservoir.PourcentageRempli:0}%";

    /// <summary>Compact "14,400 / 20,000 L" volume readout shown under the tank visual.</summary>
    public string VolumeDisplay => $"{Reservoir.NiveauDeCarburant:N0} / {Reservoir.Capacite:N0} L";

    /// <summary>Just the current volume part, bolded in the footer row.</summary>
    public string CurrentVolumeText => $"{Reservoir.NiveauDeCarburant:N0}";

    /// <summary>"/ 20,000 L" trailing part of the volume footer.</summary>
    public string CapacityVolumeText => $"/ {Reservoir.Capacite:N0} L";

    /// <summary>Status pill text — Plein (≥80%), Moyen (30–80%), Bas (&lt;30%).</summary>
    public string StatusText
    {
        get
        {
            var p = Reservoir.PourcentageRempli;
            if (p >= 80m) return "Plein";
            if (p < LowLevelThresholdPercent) return "Bas";
            return "Moyen";
        }
    }

    /// <summary>
    /// Coarse level bucket used by XAML data-triggers to pick the matching
    /// liquid-fill brush ("Plein" / "Moyen" / "Bas").
    /// </summary>
    public string LevelState
    {
        get
        {
            var p = Reservoir.PourcentageRempli;
            if (p >= 80m) return "Plein";
            if (p < LowLevelThresholdPercent) return "Bas";
            return "Moyen";
        }
    }

    /// <summary>Resource key of the localized status word for the current bucket.</summary>
    public string StatusKey
    {
        get
        {
            var p = Reservoir.PourcentageRempli;
            if (p >= 80m) return "ResStatusPlein";
            if (p < LowLevelThresholdPercent) return "ResStatusBas";
            return "ResStatusMoyen";
        }
    }

    /// <summary>True when status should render in the accent (full / mid) color.</summary>
    public bool IsStatusGood => Reservoir.PourcentageRempli >= LowLevelThresholdPercent;

    /// <summary>Calibration year/month display ("12/2023"), best-effort from audit timestamps.</summary>
    public string CalibrationDisplay
    {
        get
        {
            var d = Reservoir.DateModification ?? Reservoir.DateCreation;
            return d is null ? "—" : $"{d.Value:MM/yyyy}";
        }
    }

    /// <summary>Raw percent (0–100) as double, for the tank-fill height proportion.</summary>
    public double FillPercent => Math.Clamp((double)Reservoir.PourcentageRempli, 0.0, 100.0);

    public bool IsLowLevel => Reservoir.PourcentageRempli < LowLevelThresholdPercent;

    public System.Windows.GridLength FillColumnWidth =>
        new(Math.Max((double)Reservoir.PourcentageRempli, 0.0001), System.Windows.GridUnitType.Star);
    public System.Windows.GridLength RemainderColumnWidth =>
        new(Math.Max(100.0 - (double)Reservoir.PourcentageRempli, 0.0001), System.Windows.GridUnitType.Star);

    public bool HasLastReading => LastDetail is not null;

    // KPI tile values — calibration stores cm, multiply for mm display.
    public string LastHeightDisplay => LastDetail is not null
        ? $"{LastDetail.HauteurMesuree * 10m:N0} mm"
        : "—";
    public string LastTempDisplay => LastDetail?.Temperature is { } t
        ? $"{t:0.0}° C"
        : "—";
    public string LastWaterDisplay => ParseWaterFromNotes(LastDetail?.Notes);

    public string LastReadingTimeAgo
    {
        get
        {
            if (LastReadingDate is null) return "—";
            var dt = LastReadingDate.Value.Kind == DateTimeKind.Utc
                ? LastReadingDate.Value.ToLocalTime()
                : LastReadingDate.Value;
            var span = DateTime.Now - dt;
            if (span.TotalMinutes < 1) return $"à l'instant — {dt:HH:mm}";
            if (span.TotalHours < 1) return $"il y a {(int)span.TotalMinutes} min — {dt:HH:mm}";
            if (span.TotalHours < 24) return $"il y a {(int)span.TotalHours} h — {dt:HH:mm}";
            if (span.TotalDays < 7) return $"il y a {(int)span.TotalDays} j — {dt:HH:mm}";
            return dt.ToString("dd/MM/yyyy — HH:mm");
        }
    }

    private static string ParseWaterFromNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return "—";
        // Convention used when saving: "Eau: 4 mm"
        var idx = notes.IndexOf("Eau:", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return "—";
        var rest = notes[(idx + 4)..].Trim();
        return string.IsNullOrWhiteSpace(rest) ? "—" : rest;
    }
}

/// <summary>
/// Flattened row used by the "Derniers Jaugeages" table at the bottom of the
/// section. Wraps a <see cref="JaugeageDto"/> with view-friendly formatted text.
/// </summary>
public class RecentJaugeageRow
{
    public RecentJaugeageRow(JaugeageDto j)
    {
        Source = j;
    }

    public JaugeageDto Source { get; }

    public string Numero => string.IsNullOrWhiteSpace(Source.NumeroJaugeage)
        ? $"#JG-{Source.ID:0000}"
        : $"#{Source.NumeroJaugeage}";

    public string DateTimeDisplay
    {
        get
        {
            var dt = Source.DateJaugeage != default ? Source.DateJaugeage
                   : Source.DateCreation ?? DateTime.MinValue;
            if (dt == DateTime.MinValue) return "—";
            var local = dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;
            return local.ToString("dd MMM yyyy, HH:mm");
        }
    }

    public string VolumeDisplay => $"{Source.TotalVolume:N0}";

    public string TemoinNom => string.IsNullOrWhiteSpace(Source.TemoinNom) ? "—" : Source.TemoinNom!;

    /// <summary>2-letter initials shown in the colored avatar circle.</summary>
    public string TemoinInitials
    {
        get
        {
            var name = TemoinNom;
            if (name == "—") return "?";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[1][0])}";
            return char.ToUpper(parts[0][0]).ToString();
        }
    }

    /// <summary>Status pill text — currently always "Validé" pending workflow status field.</summary>
    public string StatusText => "Validé";
    public bool IsPending => false;
}

using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the Edit Jaugeage dialog. Loads the full jaugeage with its
/// detail rows and allows editing header fields and per-reservoir measurements.
/// </summary>
public partial class EditJaugeageViewModel : ObservableObject
{
    private readonly JaugeageService _jaugeageService = new();
    private readonly JaugeageDetailService _detailService = new();
    private readonly EmployeService _employeService = new();
    private readonly ReservoirService _reservoirService = new();

    private JaugeageWithDetailsDto? _source;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string? _errorMessage;

    // ── Editable header fields ───────────────────────────────────────
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private DateTime _dateJaugeage = DateTime.Today;
    [ObservableProperty] private EmployeDto? _selectedTemoin;
    [ObservableProperty] private string _observations = string.Empty;

    // ── Status (display-only, same as details dialog) ───────────────
    [ObservableProperty] private string _statutText = "Validé";
    [ObservableProperty] private bool _isPending;
    [ObservableProperty] private bool _hasObservations;

    public ObservableCollection<EmployeDto> Employes { get; } = new();
    public ObservableCollection<ReservoirDto> Reservoirs { get; } = new();
    public ObservableCollection<EditableJaugeageDetailRow> DetailRows { get; } = new();

    public JaugeageDraftVm Draft { get; }

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand AddNewRowCommand { get; }

    public EditJaugeageViewModel()
    {
        Draft = new JaugeageDraftVm(_detailService);
        Draft.PropertyChanged += (_, _) => AddNewRowCommand.NotifyCanExecuteChanged();

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsSaving);
        AddNewRowCommand = new RelayCommand(AddNewRow, () => Draft.CanAdd);
    }

    private void AddNewRow()
    {
        if (!Draft.CanAdd || Draft.SelectedReservoir is null) return;

        // Prevent duplicates: update if same reservoir already exists
        var existing = DetailRows.FirstOrDefault(r => r.ReservoirID == Draft.SelectedReservoir.ID && r.ID == 0);
        if (existing is not null)
        {
            existing.HauteurMesuree = Draft.HauteurCm;
            existing.VolumeCalcule = Draft.VolumeCalcule;
            existing.Notes = Draft.Notes ?? string.Empty;
        }
        else
        {
            DetailRows.Add(new EditableJaugeageDetailRow(
                Draft.SelectedReservoir,
                Draft.HauteurCm,
                Draft.VolumeCalcule,
                Draft.Notes));
        }

        Draft.Reset();
    }

    /// <summary>Loads the jaugeage and its detail lines by ID.</summary>
    public async Task LoadAsync(int jaugeageId, CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var employesTask = _employeService.GetAllEmployesAsync();
            var jaugeageTask = _jaugeageService.GetJaugeageWithDetailsAsync(jaugeageId);
            var reservoirsTask = _reservoirService.GetAllReservoirsAsync();
            await Task.WhenAll(employesTask, jaugeageTask, reservoirsTask).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var employes = employesTask.Result ?? [];
                Employes.Clear();
                foreach (var e in employes) Employes.Add(e);

                Reservoirs.Clear();
                foreach (var r in reservoirsTask.Result ?? []) Reservoirs.Add(r);
                Draft.AvailableReservoirs = Reservoirs;
                Draft.SelectedReservoir = Reservoirs.FirstOrDefault();

                var dto = jaugeageTask.Result;
                if (dto is null)
                {
                    ErrorMessage = "Impossible de charger les détails du jaugeage.";
                    return;
                }

                _source = dto;

                Numero = string.IsNullOrWhiteSpace(dto.NumeroJaugeage)
                    ? $"#JG-{dto.ID:0000}"
                    : dto.NumeroJaugeage;

                DateJaugeage = dto.DateJaugeage != default
                    ? (dto.DateJaugeage.Kind == DateTimeKind.Utc ? dto.DateJaugeage.ToLocalTime() : dto.DateJaugeage)
                    : DateTime.Today;

                SelectedTemoin = Employes.FirstOrDefault(e => e.ID == dto.TemoinID);

                Observations = dto.Observations ?? string.Empty;
                HasObservations = !string.IsNullOrWhiteSpace(Observations);

                IsPending = false;
                StatutText = "Validé";

                DetailRows.Clear();
                foreach (var d in dto.Details)
                    DetailRows.Add(new EditableJaugeageDetailRow(d));
            });
        }
        catch (OperationCanceledException) { /* dialog closed */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[EditJaugeageVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SaveAsync()
    {
        if (_source is null) return;
        try
        {
            IsSaving = true;
            ErrorMessage = null;
            SaveCommand.NotifyCanExecuteChanged();

            // Update header
            var jaugeageDto = new JaugeageDto
            {
                ID = _source.ID,
                NumeroJaugeage = Numero,
                DateJaugeage = DateJaugeage.Kind == DateTimeKind.Utc
                    ? DateJaugeage
                    : DateJaugeage.ToUniversalTime(),
                TemoinID = SelectedTemoin?.ID ?? _source.TemoinID,
                Observations = Observations,
                AjoutePar = _source.AjoutePar,
                DateCreation = _source.DateCreation,
            };

            var headerOk = await _jaugeageService.UpdateJaugeageAsync(jaugeageDto).ConfigureAwait(false);

            // Update existing rows; create new ones (ID == 0)
            var updateTasks = DetailRows
                .Where(r => r.ID > 0)
                .Select(row => _detailService.UpdateDetailAsync(row.ToDto()))
                .ToList();
            var createTasks = DetailRows
                .Where(r => r.ID == 0)
                .Select(row =>
                {
                    var dto = row.ToDto();
                    dto.JaugeageID = _source!.ID;
                    return _detailService.CreateDetailAsync(dto);
                })
                .ToList();
            await Task.WhenAll(updateTasks.Concat(createTasks.Cast<Task>())).ConfigureAwait(false);
            var allDetailOk = updateTasks.All(t => t.Result) && createTasks.All(t => t.Result is not null);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!headerOk || !allDetailOk)
                    ErrorMessage = Localization.LanguageManager.Instance["JaugEditSaveError"];
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[EditJaugeageVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }
}

/// <summary>
/// Editable row wrapping one <see cref="JaugeageDetailDto"/>.
/// CM and Volume are mutually exclusive editable fields:
/// - Typing in CM triggers a debounced API volume calculation.
/// - Typing directly in Volume zeros out CM (disabling auto-calc).
/// </summary>
public partial class EditableJaugeageDetailRow : ObservableObject
{
    private static readonly JaugeageDetailService _detailService = new();
    private readonly JaugeageDetailDto _source;
    private readonly DispatcherTimer _debounce;
    private bool _suppressSync;

    /// <summary>Row for an existing persisted detail (ID > 0).</summary>
    public EditableJaugeageDetailRow(JaugeageDetailDto source)
    {
        _source = source;
        _hauteurMesuree = source.HauteurMesuree;
        _volumeCalcule = source.VolumeCalcule;
        _temperature = source.Temperature;
        _notes = source.Notes ?? string.Empty;

        _debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _debounce.Tick += async (_, _) =>
        {
            _debounce.Stop();
            await RecalcVolumeAsync().ConfigureAwait(true);
        };
    }

    /// <summary>Row for a newly added reservoir (not yet persisted; ID == 0).</summary>
    public EditableJaugeageDetailRow(ReservoirDto reservoir, decimal hauteurCm, decimal volume, string? notes)
        : this(new JaugeageDetailDto
        {
            ID = 0,
            ReservoirID = reservoir.ID,
            ReservoirNumero = reservoir.Numero,
            ReservoirNom = reservoir.Numero,
            ProduitNom = reservoir.ProduitNom,
            HauteurMesuree = hauteurCm,
            VolumeCalcule = volume,
            Notes = notes
        })
    {
    }

    public int ID => _source.ID;
    public int ReservoirID => _source.ReservoirID;

    // ── Read-only display ────────────────────────────────────────────
    public string ReservoirDisplay
    {
        get
        {
            var num = _source.ReservoirNumero ?? $"#{_source.ReservoirID}";
            var nom = _source.ReservoirNom;
            return string.IsNullOrWhiteSpace(nom) ? num : $"{num} - {nom}";
        }
    }

    public string ProduitDisplay =>
        string.IsNullOrWhiteSpace(_source.ProduitNom) ? "—" : _source.ProduitNom!;

    // ── Editable fields ──────────────────────────────────────────────
    [ObservableProperty] private decimal _hauteurMesuree;
    [ObservableProperty] private decimal _volumeCalcule;
    [ObservableProperty] private decimal? _temperature;
    [ObservableProperty] private string _notes = string.Empty;

    /// <summary>
    /// When CM changes: if > 0 start debounce to auto-calc volume.
    /// If zeroed, just stop the timer (volume stays as-is for manual editing).
    /// </summary>
    partial void OnHauteurMesureeChanged(decimal value)
    {
        if (_suppressSync) return;
        _debounce.Stop();
        if (value > 0)
            _debounce.Start();
    }

    /// <summary>
    /// When Volume is edited directly by the user: zero out CM so
    /// auto-calc doesn't overwrite the manually entered value.
    /// </summary>
    partial void OnVolumeCalculeChanged(decimal value)
    {
        if (_suppressSync) return;
        _debounce.Stop();
        _suppressSync = true;
        HauteurMesuree = 0;
        _suppressSync = false;
    }

    private async Task RecalcVolumeAsync()
    {
        if (HauteurMesuree <= 0) return;
        try
        {
            var result = await _detailService
                .CalculateVolumeAsync(_source.ReservoirID, HauteurMesuree)
                .ConfigureAwait(true);

            if (result is not null)
            {
                _suppressSync = true;
                VolumeCalcule = result.VolumeLitres;
                _suppressSync = false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditableJaugeageDetailRow] Calc failed: {ex.Message}");
        }
    }

    /// <summary>Builds the DTO to send to the API.</summary>
    public JaugeageDetailDto ToDto() => new()
    {
        ID = _source.ID,
        JaugeageID = _source.JaugeageID,
        ReservoirID = _source.ReservoirID,
        HauteurMesuree = HauteurMesuree,
        VolumeCalcule = VolumeCalcule,
        Temperature = Temperature,
        Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
        JaugeageNumero = _source.JaugeageNumero,
        ReservoirNumero = _source.ReservoirNumero,
        ReservoirNom = _source.ReservoirNom,
        ProduitNom = _source.ProduitNom,
    };
}

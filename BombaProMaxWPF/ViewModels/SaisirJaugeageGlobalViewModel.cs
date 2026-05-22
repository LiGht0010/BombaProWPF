using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// View-model for the redesigned "Saisir Jaugeage Global" dialog. Operates on
/// an "add-from-picker-to-list" model: the user picks a reservoir, enters the
/// measured height (cm) — volume is debounce-calculated via calibration —
/// then commits the entry to the <see cref="Rows"/> list. Saving posts one
/// <see cref="JaugeageWithDetailsDto"/> aggregating every added entry.
/// </summary>
public partial class SaisirJaugeageGlobalViewModel : ObservableObject
{
    private readonly ReservoirService _reservoirService = new();
    private readonly EmployeService _employeService = new();
    private readonly JaugeageService _jaugeageService = new();
    private readonly JaugeageDetailService _detailService = new();

    public ObservableCollection<EmployeDto> Employes { get; } = new();
    public ObservableCollection<ReservoirDto> Reservoirs { get; } = new();
    public ObservableCollection<JaugeageItemVm> Rows { get; } = new();

    public JaugeageDraftVm Draft { get; }

    [ObservableProperty] private EmployeDto? _selectedTemoin;
    [ObservableProperty] private DateTime _dateJaugeage = DateTime.Today;
    [ObservableProperty] private string _numeroJaugeage = string.Empty;
    [ObservableProperty] private string? _observations;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string? _errorMessage;

    /// <summary>The persisted jaugeage; non-null after a successful save.</summary>
    public JaugeageWithDetailsDto? Result { get; private set; }

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand AddToJaugeageCommand { get; }
    public IRelayCommand<JaugeageItemVm> RemoveItemCommand { get; }

    public SaisirJaugeageGlobalViewModel()
    {
        Draft = new JaugeageDraftVm(_detailService);
        Draft.PropertyChanged += (_, _) => AddToJaugeageCommand.NotifyCanExecuteChanged();

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsSaving && Rows.Count > 0);
        AddToJaugeageCommand = new RelayCommand(AddCurrentDraft, () => Draft.CanAdd);
        RemoveItemCommand = new RelayCommand<JaugeageItemVm>(item =>
        {
            if (item is null) return;
            Rows.Remove(item);
            OnRowsChanged();
        });

        Rows.CollectionChanged += (_, _) => OnRowsChanged();
        NumeroJaugeage = BuildNumero();
    }

    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var employesTask = _employeService.GetAllEmployesAsync();
            var reservoirsTask = _reservoirService.GetAllReservoirsAsync();
            await Task.WhenAll(employesTask, reservoirsTask).ConfigureAwait(true);

            Employes.Clear();
            foreach (var e in employesTask.Result) Employes.Add(e);
            SelectedTemoin = Employes.FirstOrDefault();

            Reservoirs.Clear();
            foreach (var r in reservoirsTask.Result) Reservoirs.Add(r);
            Draft.AvailableReservoirs = Reservoirs;
            Draft.SelectedReservoir = Reservoirs.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[SaisirJaugeageGlobalVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [ObservableProperty] private decimal _totalVolume;
    [ObservableProperty] private int _measuredCount;

    private void OnRowsChanged()
    {
        TotalVolume = Rows.Sum(r => r.VolumeCalcule);
        MeasuredCount = Rows.Count;
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void AddCurrentDraft()
    {
        if (!Draft.CanAdd || Draft.SelectedReservoir is null) return;

        // Prevent duplicates: if the same reservoir was already added, refresh that row.
        var existing = Rows.FirstOrDefault(r => r.Reservoir.ID == Draft.SelectedReservoir.ID);
        if (existing is not null)
        {
            existing.HauteurCm = Draft.HauteurCm;
            existing.VolumeCalcule = Draft.VolumeCalcule;
            existing.Notes = Draft.Notes;
        }
        else
        {
            Rows.Add(new JaugeageItemVm(
                Draft.SelectedReservoir,
                Draft.HauteurCm,
                Draft.VolumeCalcule,
                Draft.Notes));
        }

        Draft.Reset();
        OnRowsChanged();
    }

    private string BuildNumero()
        => $"JG-{DateTime.Today:yyyy}-{DateTime.Now:HHmmss}";

    private async Task SaveAsync()
    {
        if (SelectedTemoin is null)
        {
            ErrorMessage = "Veuillez sélectionner un témoin.";
            return;
        }
        if (Rows.Count == 0)
        {
            ErrorMessage = "Veuillez ajouter au moins une mesure.";
            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = null;

            var dto = new JaugeageWithDetailsDto
            {
                DateJaugeage = DateTime.SpecifyKind(DateJaugeage, DateTimeKind.Utc),
                TemoinID = SelectedTemoin.ID,
                NumeroJaugeage = string.IsNullOrWhiteSpace(NumeroJaugeage) ? null : NumeroJaugeage,
                Observations = string.IsNullOrWhiteSpace(Observations) ? null : Observations,
                Details = Rows.Select(r => new JaugeageDetailDto
                {
                    ReservoirID = r.Reservoir.ID,
                    HauteurMesuree = r.HauteurCm,
                    VolumeCalcule = r.VolumeCalcule,
                    Temperature = null,
                    Notes = string.IsNullOrWhiteSpace(r.Notes) ? null : r.Notes
                }).ToList()
            };

            var saved = await _jaugeageService.CreateJaugeageWithDetailsAsync(dto).ConfigureAwait(true);
            if (saved is null)
            {
                ErrorMessage = "Échec de l'enregistrement.";
                return;
            }
            Result = saved;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[SaisirJaugeageGlobalVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }
}

/// <summary>
/// Right-hand working area: the user picks a reservoir, enters the measured
/// height in cm, the volume is debounce-calculated, then "Ajouter au jaugeage"
/// commits the draft to the items list.
/// </summary>
public partial class JaugeageDraftVm : ObservableObject
{
    private readonly JaugeageDetailService _detailService;
    private readonly DispatcherTimer _debounceTimer;

    public JaugeageDraftVm(JaugeageDetailService detailService)
    {
        _detailService = detailService;
        _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _debounceTimer.Tick += async (_, _) =>
        {
            _debounceTimer.Stop();
            await CalculateVolumeAsync().ConfigureAwait(true);
        };
    }

    public ObservableCollection<ReservoirDto>? AvailableReservoirs { get; set; }

    [ObservableProperty] private ReservoirDto? _selectedReservoir;
    [ObservableProperty] private decimal _hauteurCm;
    [ObservableProperty] private decimal _volumeCalcule;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private bool _isCalculating;

    public bool CanAdd =>
        SelectedReservoir is not null && HauteurCm > 0 && VolumeCalcule > 0 && !IsCalculating;

    partial void OnSelectedReservoirChanged(ReservoirDto? value)
    {
        // Picking a new reservoir invalidates the previous calc.
        VolumeCalcule = 0;
        OnPropertyChanged(nameof(CanAdd));
        if (HauteurCm > 0) RestartDebounce();
    }

    partial void OnHauteurCmChanged(decimal value)
    {
        _debounceTimer.Stop();
        if (value <= 0 || SelectedReservoir is null)
        {
            VolumeCalcule = 0;
            IsCalculating = false;
            OnPropertyChanged(nameof(CanAdd));
            return;
        }
        if (!SelectedReservoir.HasCalibration)
        {
            VolumeCalcule = 0;
            OnPropertyChanged(nameof(CanAdd));
            return;
        }
        RestartDebounce();
    }

    partial void OnVolumeCalculeChanged(decimal value) => OnPropertyChanged(nameof(CanAdd));
    partial void OnIsCalculatingChanged(bool value) => OnPropertyChanged(nameof(CanAdd));

    private void RestartDebounce()
    {
        IsCalculating = true;
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private async Task CalculateVolumeAsync()
    {
        if (SelectedReservoir is null) return;
        try
        {
            var result = await _detailService.CalculateVolumeAsync(SelectedReservoir.ID, HauteurCm).ConfigureAwait(true);
            VolumeCalcule = result?.VolumeLitres ?? 0m;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageDraftVm] Calc failed: {ex.Message}");
            VolumeCalcule = 0m;
        }
        finally
        {
            IsCalculating = false;
        }
    }

    public void Reset()
    {
        _debounceTimer.Stop();
        HauteurCm = 0;
        VolumeCalcule = 0;
        Notes = null;
        IsCalculating = false;
    }
}

/// <summary>
/// A reservoir entry that has been added to the jaugeage list, rendered in the
/// "ÉLÉMENTS AJOUTÉS" table at the bottom of the dialog.
/// </summary>
public partial class JaugeageItemVm : ObservableObject
{
    public ReservoirDto Reservoir { get; }

    public JaugeageItemVm(ReservoirDto reservoir, decimal hauteurCm, decimal volume, string? notes)
    {
        Reservoir = reservoir;
        _hauteurCm = hauteurCm;
        _volumeCalcule = volume;
        _notes = notes;
    }

    [ObservableProperty] private decimal _hauteurCm;
    [ObservableProperty] private decimal _volumeCalcule;
    [ObservableProperty] private string? _notes;

    public string Numero => $"{Reservoir.Numero}";
    public string DisplayName => Reservoir.Numero;
    public string ProduitNom => Reservoir.ProduitNom ?? "—";
}

using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the Edit Réservoir dialog.
/// Pre-populates all fields from an existing <see cref="ReservoirDto"/> and
/// persists changes via <see cref="ReservoirService.UpdateReservoirAsync"/>.
/// The calibration table can be fully replaced (delete-all + re-import).
/// </summary>
public partial class EditReservoirViewModel : ObservableObject
{
    private readonly ReservoirService _reservoirService = new();
    private readonly ReservoirCalibrationService _calibrationService = new();
    private readonly ProduitService _produitService = new();

    private readonly ReservoirDto _original;

    // ── Form fields ──────────────────────────────────────────────────
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private decimal _capacite;
    [ObservableProperty] private string? _fabricant;
    [ObservableProperty] private string? _numeroSerie;
    [ObservableProperty] private decimal? _diametreCm;
    [ObservableProperty] private ProduitDto? _selectedProduit;

    // ── UI state ─────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCsvTab))]
    private bool _isManualTab = true;

    public bool IsCsvTab
    {
        get => !IsManualTab;
        set => IsManualTab = !value;
    }

    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    // ── Result ───────────────────────────────────────────────────────
    public ReservoirDto? Result { get; private set; }

    public ObservableCollection<ProduitDto> Produits { get; } = new();
    public ObservableCollection<CalibrationLineItem> CalibrationLines { get; } = new();

    // ── New-row input fields ─────────────────────────────────────────
    [ObservableProperty] private string _newHauteur = string.Empty;
    [ObservableProperty] private string _newVolume = string.Empty;

    public IAsyncRelayCommand LoadCommand { get; }
    public IRelayCommand AddLineCommand { get; }
    public IRelayCommand<CalibrationLineItem> RemoveLineCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand ImportCsvCommand { get; }

    public EditReservoirViewModel(ReservoirDto reservoir)
    {
        _original = reservoir;

        // Pre-populate from existing data
        Numero = reservoir.Numero;
        Capacite = reservoir.Capacite;
        Fabricant = reservoir.Fabricant;
        NumeroSerie = reservoir.NumeroSerie;
        DiametreCm = reservoir.DiametreMm.HasValue ? reservoir.DiametreMm.Value / 10m : null;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddLineCommand = new RelayCommand(AddLine);
        RemoveLineCommand = new RelayCommand<CalibrationLineItem>(RemoveLine);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsSaving);
        ImportCsvCommand = new AsyncRelayCommand(ImportCsvAsync);
    }

    public Task EnsureLoadedAsync() => LoadCommand.ExecuteAsync(null);

    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var produitsTask = _produitService.GetAllProduitsAsync();
            var calibTask = _calibrationService.GetCalibrationsByReservoirAsync(_original.ID);
            await Task.WhenAll(produitsTask, calibTask).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Produits.Clear();
                foreach (var p in produitsTask.Result) Produits.Add(p);

                // Match selected produit by ID
                SelectedProduit = Produits.FirstOrDefault(p => p.ID == _original.ProduitID)
                                  ?? Produits.FirstOrDefault();

                CalibrationLines.Clear();
                foreach (var c in calibTask.Result.OrderBy(c => c.HauteurCm))
                    CalibrationLines.Add(new CalibrationLineItem { HauteurCm = c.HauteurCm, VolumeLitres = c.VolumeLitres });
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditReservoirVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static bool TryParseDecimal(string s, out decimal result) =>
        decimal.TryParse(s, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out result) ||
        decimal.TryParse(s, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.CurrentCulture, out result);

    private void AddLine()
    {
        if (!TryParseDecimal(NewHauteur, out var h) || !TryParseDecimal(NewVolume, out var v))
            return;

        CalibrationLines.Add(new CalibrationLineItem { HauteurCm = h, VolumeLitres = v });
        NewHauteur = string.Empty;
        NewVolume = string.Empty;
    }

    private void RemoveLine(CalibrationLineItem? item)
    {
        if (item is not null) CalibrationLines.Remove(item);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        // Auto-commit any pending new-row input before validating
        if (!string.IsNullOrWhiteSpace(NewHauteur) || !string.IsNullOrWhiteSpace(NewVolume))
            AddLine();

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = Resources.Strings.NResValidationNumero;
            return;
        }
        if (Capacite <= 0)
        {
            ErrorMessage = Resources.Strings.NResValidationCapacite;
            return;
        }

        try
        {
            IsSaving = true;

            // Check unique numero (exclude self)
            var exists = await _reservoirService
                .ReservoirNumberExistsAsync(Numero, _original.ID)
                .ConfigureAwait(false);
            if (exists)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    ErrorMessage = Resources.Strings.NResNumeroExists);
                return;
            }

            var dto = new ReservoirDto
            {
                ID = _original.ID,
                Numero = Numero.Trim(),
                Capacite = Capacite,
                NiveauDeCarburant = _original.NiveauDeCarburant,
                ProduitID = SelectedProduit?.ID,
                ProduitNom = SelectedProduit?.NumeroProduit,
                Fabricant = string.IsNullOrWhiteSpace(Fabricant) ? null : Fabricant!.Trim(),
                NumeroSerie = string.IsNullOrWhiteSpace(NumeroSerie) ? null : NumeroSerie!.Trim(),
                DiametreMm = DiametreCm.HasValue ? DiametreCm.Value * 10m : null,
                AjoutePar = _original.AjoutePar,
                DateCreation = _original.DateCreation,
            };

            var ok = await _reservoirService.UpdateReservoirAsync(dto).ConfigureAwait(false);
            if (!ok)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    ErrorMessage = Resources.Strings.NResSaveError);
                return;
            }

            // Always replace calibration table (delete-all then re-import if rows present)
            if (CalibrationLines.Count > 0)
            {
                var importItems = CalibrationLines
                    .Select(l => new ReservoirCalibrationImportDto
                    {
                        HauteurCm = l.HauteurCm,
                        VolumeLitres = l.VolumeLitres
                    }).ToList();

                // Validate no duplicate heights before touching the DB
                var hasDuplicates = importItems
                    .GroupBy(x => x.HauteurCm)
                    .Any(g => g.Count() > 1);
                if (hasDuplicates)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        ErrorMessage = "Hauteurs en double dans la table de calibration.");
                    return;
                }

                await _calibrationService.DeleteAllCalibrationsAsync(_original.ID).ConfigureAwait(false);

                var importResult = await _calibrationService
                    .ImportCalibrationsAsync(_original.ID, importItems)
                    .ConfigureAwait(false);

                if (importResult is null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        ErrorMessage = Resources.Strings.NResSaveError);
                    return;
                }
            }
            else
            {
                // No calibration rows — just clear whatever exists
                await _calibrationService.DeleteAllCalibrationsAsync(_original.ID).ConfigureAwait(false);
            }

            Result = dto;
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[EditReservoirVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task ImportCsvAsync()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Importer la table de calibration",
            Filter = "Fichiers CSV (*.csv)|*.csv",
            Multiselect = false
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            var lines = await System.IO.File.ReadAllLinesAsync(dlg.FileName).ConfigureAwait(false);
            var parsed = new List<CalibrationLineItem>();

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                if (TryParseDecimal(parts[0].Trim(), out var h) &&
                    TryParseDecimal(parts[1].Trim(), out var v))
                {
                    parsed.Add(new CalibrationLineItem { HauteurCm = h, VolumeLitres = v });
                }
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CalibrationLines.Clear();
                foreach (var item in parsed) CalibrationLines.Add(item);
                IsManualTab = true;
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur lors de l'import CSV: {ex.Message}");
        }
    }
}

using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the Nouveau Réservoir creation dialog.
/// Handles form validation, manual calibration table entry, and CSV import.
/// </summary>
public partial class NouveauReservoirViewModel : ObservableObject
{
    private readonly ReservoirService _reservoirService = new();
    private readonly ReservoirCalibrationService _calibrationService = new();
    private readonly ProduitService _produitService = new();

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

    /// <summary>Inverse of <see cref="IsManualTab"/> — drives the CSV tab RadioButton.</summary>
    public bool IsCsvTab
    {
        get => !IsManualTab;
        set => IsManualTab = !value;
    }
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private bool _isLoadingProduits;
    [ObservableProperty] private string? _errorMessage;

    // ── Result (set on successful save) ──────────────────────────────
    public ReservoirDto? Result { get; private set; }

    public ObservableCollection<ProduitDto> Produits { get; } = new();
    public ObservableCollection<CalibrationLineItem> CalibrationLines { get; } = new();

    // ── New-row input fields ─────────────────────────────────────────
    [ObservableProperty] private string _newHauteur = string.Empty;
    [ObservableProperty] private string _newVolume = string.Empty;

    // CanExecute is always true; validation happens inside AddLine()

    public IAsyncRelayCommand LoadProduitsCommand { get; }
    public IRelayCommand AddLineCommand { get; }
    public IRelayCommand<CalibrationLineItem> RemoveLineCommand { get; }
    public IRelayCommand SwitchToManualCommand { get; }
    public IRelayCommand SwitchToCsvCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand ImportCsvCommand { get; }

    public NouveauReservoirViewModel()
    {
        LoadProduitsCommand = new AsyncRelayCommand(LoadProduitsAsync);
        AddLineCommand = new RelayCommand(AddLine);
        RemoveLineCommand = new RelayCommand<CalibrationLineItem>(RemoveLine);
        SwitchToManualCommand = new RelayCommand(() => IsManualTab = true);
        SwitchToCsvCommand = new RelayCommand(() => IsManualTab = false);
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsSaving);
        ImportCsvCommand = new AsyncRelayCommand(ImportCsvAsync);
    }

    public Task LoadAsync() => LoadProduitsCommand.ExecuteAsync(null);

    private async Task LoadProduitsAsync()
    {
        try
        {
            IsLoadingProduits = true;
            var list = await _produitService.GetAllProduitsAsync().ConfigureAwait(false);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Produits.Clear();
                foreach (var p in list) Produits.Add(p);
                SelectedProduit ??= Produits.FirstOrDefault();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NouveauReservoirVM] LoadProduits failed: {ex}");
        }
        finally
        {
            IsLoadingProduits = false;
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

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = BombaProMaxWPF.Resources.Strings.NResValidationNumero;
            return;
        }
        if (Capacite <= 0)
        {
            ErrorMessage = BombaProMaxWPF.Resources.Strings.NResValidationCapacite;
            return;
        }

        try
        {
            IsSaving = true;

            // Check unique numero
            var exists = await _reservoirService.ReservoirNumberExistsAsync(Numero).ConfigureAwait(false);
            if (exists)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    ErrorMessage = BombaProMaxWPF.Resources.Strings.NResNumeroExists);
                return;
            }

            var dto = new ReservoirDto
            {
                Numero = Numero.Trim(),
                Capacite = Capacite,
                NiveauDeCarburant = 0m,
                ProduitID = SelectedProduit?.ID,
                ProduitNom = SelectedProduit?.NumeroProduit,
                Fabricant = string.IsNullOrWhiteSpace(Fabricant) ? null : Fabricant!.Trim(),
                NumeroSerie = string.IsNullOrWhiteSpace(NumeroSerie) ? null : NumeroSerie!.Trim(),
                DiametreMm = DiametreCm.HasValue ? DiametreCm.Value * 10m : null,
            };

            var saved = await _reservoirService.CreateReservoirAsync(dto).ConfigureAwait(false);

            if (saved is null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    ErrorMessage = BombaProMaxWPF.Resources.Strings.NResSaveError);
                return;
            }

            // Save calibration rows if any
            if (CalibrationLines.Count > 0)
            {
                var importItems = CalibrationLines
                    .Select(l => new ReservoirCalibrationImportDto
                    {
                        HauteurCm = l.HauteurCm,
                        VolumeLitres = l.VolumeLitres
                    }).ToList();

                await _calibrationService.ImportCalibrationsAsync(saved.ID, importItems).ConfigureAwait(false);
            }

            Result = saved;
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[NouveauReservoirVM] Save failed: {ex}");
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

            foreach (var line in lines.Skip(1)) // skip header row
            {
                var parts = line.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                if (decimal.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var h) &&
                    decimal.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var v))
                {
                    parsed.Add(new CalibrationLineItem { HauteurCm = h, VolumeLitres = v });
                }
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CalibrationLines.Clear();
                foreach (var item in parsed) CalibrationLines.Add(item);
                IsManualTab = true; // switch to manual to preview rows
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur lors de l'import CSV: {ex.Message}");
        }
    }
}

/// <summary>Editable calibration row in the manual entry table.</summary>
public partial class CalibrationLineItem : ObservableObject
{
    [ObservableProperty] private decimal _hauteurCm;
    [ObservableProperty] private decimal _volumeLitres;
}

using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the Nouveau Produit creation dialog.
/// </summary>
public partial class NouveauProduitViewModel : ObservableObject
{
    private readonly ProduitService _produitService = new();
    private readonly CategorieService _categorieService = new();

    // ── Form fields ──────────────────────────────────────────────────
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private CategorieDto? _selectedCategorie;
    [ObservableProperty] private string? _prixAchatText;   // Prix d'achat (coût)
    [ObservableProperty] private string? _prixHtText;      // Prix HT (vente hors taxe)
    [ObservableProperty] private string? _tvaText;          // TVA %
    [ObservableProperty] private string? _prixTtcText;     // Prix TTC — editable, triggers back-calc
    [ObservableProperty] private string? _stockText;
    [ObservableProperty] private string? _stockMinText;
    [ObservableProperty] private string? _delaiText;

    // ── Computed display ─────────────────────────────────────────────
    [ObservableProperty] private decimal? _marge;          // PrixHT − PrixAchat
    [ObservableProperty] private decimal? _margePct;       // Marge / PrixAchat × 100

    // ── Guard: prevents re-entrant recalculation loops ───────────────
    private bool _isRecalculating;

    // ── UI state ─────────────────────────────────────────────────────
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private bool _isLoadingCategories;
    [ObservableProperty] private string? _errorMessage;

    /// <summary>Set to true on successful save; code-behind reads this to close.</summary>
    public bool Saved { get; private set; }

    public ObservableCollection<CategorieDto> Categories { get; } = new();

    public IAsyncRelayCommand LoadCategoriesCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }

    public NouveauProduitViewModel()
    {
        LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoadingCategories = true;
            var list = await _categorieService.GetAllCategoriesAsync().ConfigureAwait(false);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Categories.Clear();
                foreach (var c in list)
                    Categories.Add(c);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NouveauProduitVM] LoadCategories failed: {ex}");
        }
        finally
        {
            IsLoadingCategories = false;
        }
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = LanguageManager.Instance["NouveauProdValidationNumero"];
            return;
        }

        try
        {
            IsSaving = true;

            var dto = new ProduitDto
            {
                NumeroProduit = Numero.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                CategorieID = SelectedCategorie?.ID,
                PrixAchat = ParseDecimal(PrixAchatText),
                PrixHT = ParseDecimal(PrixHtText),
                TVA = ParseDecimal(TvaText),
                PrixTTC = ParseDecimal(PrixTtcText),
                MargeBeneficiaire = Marge,
                MargePourcentage = MargePct,
                Stock = ParseInt(StockText),
                StockMinimum = ParseInt(StockMinText),
                DelaiDeLivraison = ParseInt(DelaiText),
            };

            var result = await _produitService.CreateProduitAsync(dto).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (result is not null)
                    Saved = true;
                else
                    ErrorMessage = LanguageManager.Instance["NouveauProdSaveError"];
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = LanguageManager.Instance["NouveauProdSaveError"];
            Debug.WriteLine($"[NouveauProduitVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    // ── Partial hooks — forward path: PrixAchat + TVA → PrixHT → PrixTTC ──
    partial void OnPrixAchatTextChanged(string? value) => RecalculateForward();
    partial void OnPrixHtTextChanged(string? value) => RecalculateForward();
    partial void OnTvaTextChanged(string? value) => RecalculateForward();

    // ── Partial hook — reverse path: PrixTTC entered → back-compute Marge ──
    partial void OnPrixTtcTextChanged(string? value) => RecalculateReverse();

    /// <summary>
    /// Forward calculation: PrixAchat + TVA % → PrixHT → PrixTTC → Marge.
    /// Runs when PrixAchat, PrixHT, or TVA changes.
    /// Prix HT is the selling price before tax. The user sets it directly.
    /// TTC = PrixHT × (1 + TVA/100).
    /// Marge = PrixHT − PrixAchat.
    /// </summary>
    private void RecalculateForward()
    {
        if (_isRecalculating) return;
        _isRecalculating = true;
        try
        {
            var achat = ParseDecimal(PrixAchatText);
            var ht = ParseDecimal(PrixHtText);
            var tva = ParseDecimal(TvaText);

            // If PrixHT is provided, compute TTC from it
            if (ht is not null)
            {
                var tvaMult = 1m + (tva ?? 0m) / 100m;
                var ttc = Math.Round(ht.Value * tvaMult, 2);
                PrixTtcText = ttc.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                // Nothing to compute TTC from — clear it
                PrixTtcText = null;
            }

            // Marge = PrixHT − PrixAchat
            if (achat is not null && ht is not null)
            {
                Marge = Math.Round(ht.Value - achat.Value, 2);
                MargePct = achat.Value > 0
                    ? Math.Round(Marge.Value / achat.Value * 100m, 2)
                    : null;
            }
            else
            {
                Marge = null;
                MargePct = null;
            }
        }
        finally
        {
            _isRecalculating = false;
        }
    }

    /// <summary>
    /// Reverse calculation: user typed TTC directly → back-compute PrixHT, then Marge.
    /// Runs when PrixTtcText changes (and the forward path is not running).
    /// PrixHT = TTC / (1 + TVA/100).
    /// </summary>
    private void RecalculateReverse()
    {
        if (_isRecalculating) return;
        _isRecalculating = true;
        try
        {
            var ttc = ParseDecimal(PrixTtcText);
            var tva = ParseDecimal(TvaText);
            var achat = ParseDecimal(PrixAchatText);

            if (ttc is null)
            {
                Marge = null;
                MargePct = null;
                return;
            }

            var tvaMult = 1m + (tva ?? 0m) / 100m;
            var ht = tvaMult > 0m
                ? Math.Round(ttc.Value / tvaMult, 4)
                : ttc.Value;

            // Write back PrixHT only if the user hasn't already set it
            if (string.IsNullOrWhiteSpace(PrixHtText))
                PrixHtText = ht.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            // Marge = PrixHT − PrixAchat
            if (achat is not null)
            {
                Marge = Math.Round(ht - achat.Value, 2);
                MargePct = achat.Value > 0
                    ? Math.Round(Marge.Value / achat.Value * 100m, 2)
                    : null;
            }
            else
            {
                Marge = null;
                MargePct = null;
            }
        }
        finally
        {
            _isRecalculating = false;
        }
    }

    private static decimal? ParseDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return decimal.TryParse(text.Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var v) ? v : null;
    }

    private static int? ParseInt(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return int.TryParse(text, out var v) ? v : null;
    }
}

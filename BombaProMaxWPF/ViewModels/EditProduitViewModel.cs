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
/// ViewModel for the Edit Produit dialog.
/// Pre-populates all fields from an existing <see cref="ProduitDto"/> and
/// persists changes via <see cref="ProduitService.UpdateProduitAsync"/>.
/// </summary>
public partial class EditProduitViewModel : ObservableObject
{
    private readonly ProduitService _produitService = new();
    private readonly CategorieService _categorieService = new();

    private readonly ProduitDto _original;

    // ── Form fields ──────────────────────────────────────────────────
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private CategorieDto? _selectedCategorie;
    [ObservableProperty] private string? _prixAchatText;
    [ObservableProperty] private string? _prixHtText;
    [ObservableProperty] private string? _tvaText;
    [ObservableProperty] private string? _prixTtcText;
    [ObservableProperty] private string? _stockText;
    [ObservableProperty] private string? _stockMinText;
    [ObservableProperty] private string? _delaiText;

    // ── Computed display ─────────────────────────────────────────────
    [ObservableProperty] private decimal? _marge;
    [ObservableProperty] private decimal? _margePct;

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

    public EditProduitViewModel(ProduitDto produit)
    {
        _original = produit;

        // Pre-populate fields from the existing product
        Numero = produit.NumeroProduit;
        Description = produit.Description;
        PrixAchatText = produit.PrixAchat?.ToString(System.Globalization.CultureInfo.InvariantCulture);
        PrixHtText = produit.PrixHT?.ToString(System.Globalization.CultureInfo.InvariantCulture);
        TvaText = produit.TVA?.ToString(System.Globalization.CultureInfo.InvariantCulture);
        PrixTtcText = produit.PrixTTC?.ToString(System.Globalization.CultureInfo.InvariantCulture);
        StockText = produit.Stock?.ToString();
        StockMinText = produit.StockMinimum?.ToString();
        DelaiText = produit.DelaiDeLivraison?.ToString();

        LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);

        // Run initial forward calc so Marge/MargePct are populated immediately
        RecalculateForward();
    }

    /// <summary>Load categories and restore the previously selected one.</summary>
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

                SelectedCategorie = Categories.FirstOrDefault(c => c.ID == _original.CategorieID);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditProduitVM] LoadCategories failed: {ex}");
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
                ID = _original.ID,
                NumeroProduit = Numero.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description!.Trim(),
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
                AjoutePar = _original.AjoutePar,
                DateCreation = _original.DateCreation,
            };

            var ok = await _produitService.UpdateProduitAsync(dto).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                    Saved = true;
                else
                    ErrorMessage = LanguageManager.Instance["EditProdSaveError"];
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = LanguageManager.Instance["EditProdSaveError"];
            Debug.WriteLine($"[EditProduitVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    // ── Partial hooks ────────────────────────────────────────────────
    partial void OnPrixAchatTextChanged(string? value) => RecalculateForward();
    partial void OnPrixHtTextChanged(string? value) => RecalculateForward();
    partial void OnTvaTextChanged(string? value) => RecalculateForward();
    partial void OnPrixTtcTextChanged(string? value) => RecalculateReverse();

    /// <summary>
    /// Forward: PrixHT × (1 + TVA/100) → PrixTTC. Marge = PrixHT − PrixAchat.
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

            if (ht is not null)
            {
                var ttc = Math.Round(ht.Value * (1m + (tva ?? 0m) / 100m), 2);
                PrixTtcText = ttc.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                PrixTtcText = null;
            }

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
    /// Reverse: TTC / (1 + TVA/100) → PrixHT (if empty). Then Marge.
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
            var ht = tvaMult > 0m ? Math.Round(ttc.Value / tvaMult, 4) : ttc.Value;

            if (string.IsNullOrWhiteSpace(PrixHtText))
                PrixHtText = ht.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

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

    // ── Helpers ──────────────────────────────────────────────────────
    private static decimal? ParseDecimal(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                   System.Globalization.CultureInfo.InvariantCulture, out var v) ||
               decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                   System.Globalization.CultureInfo.CurrentCulture, out v)
            ? v : null;
    }

    private static int? ParseInt(string? s) =>
        int.TryParse(s, out var v) ? v : null;
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;
using System.Globalization;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the Edit Produit dialog.
/// Pre-populates all fields from the existing <see cref="ProduitDto"/> and
/// persists changes via <see cref="ProduitService.UpdateProduitAsync"/>.
/// </summary>
public class EditProduitViewModel : ObservableObject
{
    private readonly ProduitService _produitService = new();
    private readonly int _produitId;
    private bool _isRecalculating;

    // ── Basic fields ──────────────────────────────────────────────────────────
    private string _numero = string.Empty;
    public string Numero
    {
        get => _numero;
        set => SetProperty(ref _numero, value);
    }

    private string? _description;
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string? _stockText;
    public string? StockText
    {
        get => _stockText;
        set => SetProperty(ref _stockText, value);
    }

    private string? _stockMinText;
    public string? StockMinText
    {
        get => _stockMinText;
        set => SetProperty(ref _stockMinText, value);
    }

    private string? _delaiText;
    public string? DelaiText
    {
        get => _delaiText;
        set => SetProperty(ref _delaiText, value);
    }

    // ── Pricing fields ────────────────────────────────────────────────────────
    private string? _prixAchatText;
    public string? PrixAchatText
    {
        get => _prixAchatText;
        set { if (SetProperty(ref _prixAchatText, value)) RecalculateForward(); }
    }

    private string? _prixHtText;
    public string? PrixHtText
    {
        get => _prixHtText;
        set { if (SetProperty(ref _prixHtText, value)) RecalculateForward(); }
    }

    private string? _tvaText;
    public string? TvaText
    {
        get => _tvaText;
        set { if (SetProperty(ref _tvaText, value)) RecalculateForward(); }
    }

    private string? _prixTtcText;
    public string? PrixTtcText
    {
        get => _prixTtcText;
        set { if (SetProperty(ref _prixTtcText, value)) RecalculateReverse(); }
    }

    // ── Computed display ──────────────────────────────────────────────────────
    private decimal? _marge;
    public decimal? Marge
    {
        get => _marge;
        private set => SetProperty(ref _marge, value);
    }

    private decimal? _margePct;
    public decimal? MargePct
    {
        get => _margePct;
        private set => SetProperty(ref _margePct, value);
    }

    // ── UI state ──────────────────────────────────────────────────────────────
    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>Set to true after a successful save; code-behind reads this to close.</summary>
    public bool Saved { get; private set; }

    public IAsyncRelayCommand SaveCommand { get; }

    public EditProduitViewModel(ProduitDto produit)
    {
        ArgumentNullException.ThrowIfNull(produit);
        _produitId = produit.ProduitId;

        // Pre-populate — suppress recalculation until all fields are set
        _isRecalculating = true;
        _numero       = produit.NumeroProduit;
        _description  = produit.Description;
        _stockText    = produit.Stock?.ToString(CultureInfo.InvariantCulture);
        _stockMinText = produit.StockMinimum?.ToString(CultureInfo.InvariantCulture);
        _delaiText    = produit.DelaiDeLivraison?.ToString(CultureInfo.InvariantCulture);
        _prixAchatText = produit.PrixAchat?.ToString("F2", CultureInfo.InvariantCulture);
        _prixHtText   = produit.PrixHT?.ToString("F2", CultureInfo.InvariantCulture);
        _tvaText      = produit.TVA?.ToString("F2", CultureInfo.InvariantCulture);
        _prixTtcText  = produit.PrixTTC?.ToString("F2", CultureInfo.InvariantCulture);
        _isRecalculating = false;

        // Compute initial margin display
        UpdateMarge(produit.PrixAchat, produit.PrixHT);

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    // ── Pricing recalculation ────────────────────────────────────────────────

    /// <summary>PrixHT + TVA → PrixTTC; Marge = PrixHT − PrixAchat.</summary>
    private void RecalculateForward()
    {
        if (_isRecalculating) return;
        _isRecalculating = true;
        try
        {
            var achat = ParseDecimal(_prixAchatText);
            var ht    = ParseDecimal(_prixHtText);
            var tva   = ParseDecimal(_tvaText);

            if (ht is not null)
            {
                var mult = 1m + (tva ?? 0m) / 100m;
                SetProperty(ref _prixTtcText,
                    Math.Round(ht.Value * mult, 2).ToString(CultureInfo.InvariantCulture),
                    nameof(PrixTtcText));
            }
            else
            {
                SetProperty(ref _prixTtcText, (string?)null, nameof(PrixTtcText));
            }

            UpdateMarge(achat, ht);
        }
        finally
        {
            _isRecalculating = false;
        }
    }

    /// <summary>PrixTTC → PrixHT (if blank); Marge = PrixHT − PrixAchat.</summary>
    private void RecalculateReverse()
    {
        if (_isRecalculating) return;
        _isRecalculating = true;
        try
        {
            var ttc   = ParseDecimal(_prixTtcText);
            var tva   = ParseDecimal(_tvaText);
            var achat = ParseDecimal(_prixAchatText);

            if (ttc is null) { Marge = null; MargePct = null; return; }

            var mult = 1m + (tva ?? 0m) / 100m;
            var ht   = mult > 0m ? Math.Round(ttc.Value / mult, 4) : ttc.Value;

            if (string.IsNullOrWhiteSpace(_prixHtText))
                SetProperty(ref _prixHtText,
                    ht.ToString("F2", CultureInfo.InvariantCulture),
                    nameof(PrixHtText));

            UpdateMarge(achat, ht);
        }
        finally
        {
            _isRecalculating = false;
        }
    }

    private void UpdateMarge(decimal? achat, decimal? ht)
    {
        if (achat is not null && ht is not null)
        {
            var m = ht.Value - achat.Value;
            Marge    = Math.Round(m, 2);
            MargePct = achat.Value > 0m
                ? Math.Round(m / achat.Value * 100m, 2)
                : (decimal?)null;
        }
        else
        {
            Marge    = null;
            MargePct = null;
        }
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Numero))
        {
            ErrorMessage = LanguageManager.Instance["NouveauProdValidationNumero"];
            return;
        }

        var dto = new ProduitDto
        {
            ProduitId       = _produitId,
            NumeroProduit   = Numero.Trim(),
            Description     = Description?.Trim(),
            PrixAchat       = ParseDecimal(_prixAchatText),
            PrixHT          = ParseDecimal(_prixHtText),
            TVA             = ParseDecimal(_tvaText),
            PrixTTC         = ParseDecimal(_prixTtcText),
            MargeBeneficiaire = Marge,
            MargePourcentage  = MargePct,
            Stock           = ParseInt(_stockText),
            StockMinimum    = ParseInt(_stockMinText),
            DelaiDeLivraison = ParseInt(_delaiText),
        };

        try
        {
            IsSaving = true;
            var ok = await _produitService.UpdateProduitAsync(dto).ConfigureAwait(false);
            if (ok)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["EditProdSaveError"];
        }
        catch (Exception ex)
        {
            ErrorMessage = LanguageManager.Instance["EditProdSaveError"];
            Debug.WriteLine($"[EditProduitVM] SaveAsync failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static decimal? ParseDecimal(string? text) =>
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static int? ParseInt(string? text) =>
        int.TryParse(text, out var v) ? v : null;
}

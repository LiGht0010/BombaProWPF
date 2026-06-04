using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the Nouveau Produit creation dialog.
/// All properties are manual so they compile independently of source-generator output.
/// </summary>
public class NouveauProduitViewModel : ObservableObject
{
    private readonly ProduitService _produitService = new();
    private bool _isRecalculating;

    // ── Basic form fields ─────────────────────────────────────────────────────
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

    // ── Pricing fields (manual → triggers recalculation) ─────────────────────
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

    /// <summary>Set to true on successful save; code-behind reads this to close.</summary>
    public bool Saved { get; private set; }

    public IAsyncRelayCommand SaveCommand { get; }

    public NouveauProduitViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    /// <summary>
    /// Forward: PrixHT + TVA → PrixTTC; Marge = PrixHT − PrixAchat.
    /// Triggered when PrixAchatText, PrixHtText, or TvaText changes.
    /// </summary>
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

    /// <summary>
    /// Reverse: TTC → PrixHT (if not already set); Marge = PrixHT − PrixAchat.
    /// Triggered when PrixTtcText changes and the forward path is not running.
    /// </summary>
    private void RecalculateReverse()
    {
        if (_isRecalculating) return;
        _isRecalculating = true;
        try
        {
            var ttc   = ParseDecimal(_prixTtcText);
            var tva   = ParseDecimal(_tvaText);
            var achat = ParseDecimal(_prixAchatText);

            if (ttc is null)
            {
                Marge    = null;
                MargePct = null;
                return;
            }

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
            Marge    = Math.Round(ht.Value - achat.Value, 2);
            MargePct = achat.Value > 0m
                ? Math.Round(Marge!.Value / achat.Value * 100m, 2)
                : null;
        }
        else
        {
            Marge    = null;
            MargePct = null;
        }
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(_numero))
        {
            ErrorMessage = LanguageManager.Instance["NouveauProdValidationNumero"];
            return;
        }

        try
        {
            IsSaving = true;

            var dto = new ProduitDto
            {
                NumeroProduit     = _numero.Trim(),
                Description       = string.IsNullOrWhiteSpace(_description) ? null : _description!.Trim(),
                PrixAchat         = ParseDecimal(_prixAchatText),
                PrixHT            = ParseDecimal(_prixHtText),
                TVA               = ParseDecimal(_tvaText),
                PrixTTC           = ParseDecimal(_prixTtcText),
                MargeBeneficiaire = _marge,
                MargePourcentage  = _margePct,
                Stock             = ParseInt(_stockText),
                StockMinimum      = ParseInt(_stockMinText),
                DelaiDeLivraison  = ParseInt(_delaiText),
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
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = LanguageManager.Instance["NouveauProdSaveError"]);
            Debug.WriteLine($"[NouveauProduitVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static decimal? ParseDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return decimal.TryParse(text.Replace(',', '.'),
            NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private static int? ParseInt(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        return int.TryParse(text, out var v) ? v : null;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the EditFournisseur dialog.
/// Pre-populates all fields from the existing <see cref="FournisseurDto"/> and
/// persists changes via <see cref="FournisseurService.UpdateFournisseurAsync"/>.
/// </summary>
public class EditFournisseurViewModel : ObservableObject
{
    private readonly FournisseurService _service = new();
    private readonly int _fournisseurId;

    // ── Informations générales ────────────────────────────────────────────────

    private string? _prenom;
    public string? Prenom
    {
        get => _prenom;
        set => SetProperty(ref _prenom, value);
    }

    private string? _nom;
    public string? Nom
    {
        get => _nom;
        set => SetProperty(ref _nom, value);
    }

    private string? _societe;
    public string? Societe
    {
        get => _societe;
        set => SetProperty(ref _societe, value);
    }

    // ── Contact & coordonnées ─────────────────────────────────────────────────

    private string? _adresse;
    public string? Adresse
    {
        get => _adresse;
        set => SetProperty(ref _adresse, value);
    }

    private string? _telephone;
    public string? Telephone
    {
        get => _telephone;
        set => SetProperty(ref _telephone, value);
    }

    private string? _email;
    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string? _contact;
    public string? Contact
    {
        get => _contact;
        set => SetProperty(ref _contact, value);
    }

    // ── Finances & conditions ─────────────────────────────────────────────────

    private string? _rib;
    public string? RIB
    {
        get => _rib;
        set => SetProperty(ref _rib, value);
    }

    private string? _conditionsPaiement;
    public string? ConditionsPaiement
    {
        get => _conditionsPaiement;
        set => SetProperty(ref _conditionsPaiement, value);
    }

    private bool _isActif;
    public bool IsActif
    {
        get => _isActif;
        set => SetProperty(ref _isActif, value);
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

    public EditFournisseurViewModel(FournisseurDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        _fournisseurId = dto.FournisseurId;

        // Pre-populate
        _prenom             = dto.Prenom;
        _nom                = dto.Nom;
        _societe            = dto.Societe;
        _adresse            = dto.Adresse;
        _telephone          = dto.Telephone;
        _email              = dto.Email;
        _contact            = dto.Contact;
        _rib                = dto.RIB;
        _conditionsPaiement = dto.ConditionsPaiement;
        _isActif            = string.Equals(dto.Statut, "Actif", StringComparison.OrdinalIgnoreCase);

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(_societe))
        {
            ErrorMessage = LanguageManager.Instance["EditFourValidationSociete"];
            return;
        }

        var dto = new FournisseurDto
        {
            FournisseurId      = _fournisseurId,
            Prenom             = NullIfBlank(_prenom),
            Nom                = NullIfBlank(_nom),
            Societe            = _societe!.Trim(),
            Adresse            = NullIfBlank(_adresse),
            Telephone          = NullIfBlank(_telephone),
            Email              = NullIfBlank(_email),
            Contact            = NullIfBlank(_contact),
            RIB                = NullIfBlank(_rib),
            ConditionsPaiement = NullIfBlank(_conditionsPaiement),
            Statut             = _isActif ? "Actif" : "Inactif",
        };

        try
        {
            IsSaving = true;
            var ok = await _service.UpdateFournisseurAsync(dto).ConfigureAwait(false);
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                    Saved = true;
                else
                    ErrorMessage = LanguageManager.Instance["EditFourSaveError"];
            });
        }
        catch (Exception ex)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = LanguageManager.Instance["EditFourSaveError"]);
            Debug.WriteLine($"[EditFournisseurVM] SaveAsync failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

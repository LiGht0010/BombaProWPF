using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;

namespace FourniPro.ViewModels;

/// <summary>ViewModel for the EditEmploye dialog.</summary>
public class EditEmployeViewModel : ObservableObject
{
    private readonly EmployeService _service = new();
    private readonly int _employeId;
    private readonly int? _originalAjoutePar;
    private readonly DateTime? _originalDateCreation;

    // ── Form fields ───────────────────────────────────────────────────────────

    private string _nom = string.Empty;
    public string Nom
    {
        get => _nom;
        set => SetProperty(ref _nom, value);
    }

    private string _prenom = string.Empty;
    public string Prenom
    {
        get => _prenom;
        set => SetProperty(ref _prenom, value);
    }

    private string? _cin;
    public string? CIN
    {
        get => _cin;
        set => SetProperty(ref _cin, value);
    }

    private string? _telephone;
    public string? Telephone
    {
        get => _telephone;
        set => SetProperty(ref _telephone, value);
    }

    private string? _address;
    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string? _poste;
    public string? Poste
    {
        get => _poste;
        set => SetProperty(ref _poste, value);
    }

    private decimal? _salaire;
    public decimal? Salaire
    {
        get => _salaire;
        set => SetProperty(ref _salaire, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    public bool Saved { get; private set; }

    // ── Commands ──────────────────────────────────────────────────────────────

    public IAsyncRelayCommand SaveCommand { get; }

    public EditEmployeViewModel(EmployeDto dto)
    {
        _employeId            = dto.EmployeId;
        _originalAjoutePar    = dto.AjoutePar;
        _originalDateCreation = dto.DateCreation;

        Nom       = dto.Nom;
        Prenom    = dto.Prenom;
        CIN       = dto.CIN;
        Telephone = dto.Telephone;
        Address   = dto.Address;
        Poste     = dto.Poste;
        Salaire   = dto.Salaire;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    /// <summary>No async lookups needed for Employe — kept for consistent dialog pattern.</summary>
    public Task LoadLookupsAsync() => Task.CompletedTask;

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Nom))
        {
            ErrorMessage = "Le nom est obligatoire.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Prenom))
        {
            ErrorMessage = "Le prénom est obligatoire.";
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new EmployeDto
            {
                EmployeId    = _employeId,
                Nom          = Nom.Trim(),
                Prenom       = Prenom.Trim(),
                CIN          = NullIfBlank(CIN),
                Telephone    = NullIfBlank(Telephone),
                Address      = NullIfBlank(Address),
                Poste        = NullIfBlank(Poste),
                Salaire      = Salaire,
                AjoutePar    = _originalAjoutePar,
                DateCreation = _originalDateCreation
            };

            var ok = await _service.UpdateEmployeAsync(dto);
            if (!ok)
            {
                ErrorMessage = "Erreur lors de la mise à jour de l'employé.";
                return;
            }

            Saved = true;
        }
        finally
        {
            IsSaving = false;
        }
    }
}

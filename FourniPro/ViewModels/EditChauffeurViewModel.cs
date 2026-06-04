using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the EditChauffeur dialog.
/// Manual properties for build stability (no source-generator partials).
/// </summary>
public class EditChauffeurViewModel : ObservableObject
{
    private readonly ChauffeurService _service = new();
    private readonly ChauffeurDto _original;

    private string _nom = string.Empty;
    public string Nom
    {
        get => _nom;
        set => SetProperty(ref _nom, value);
    }

    private string? _prenom;
    public string? Prenom
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

    private string? _numeroPermis;
    public string? NumeroPermis
    {
        get => _numeroPermis;
        set => SetProperty(ref _numeroPermis, value);
    }

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool Saved { get; private set; }

    public IAsyncRelayCommand SaveCommand { get; }

    public EditChauffeurViewModel(ChauffeurDto dto)
    {
        _original    = dto;
        Nom          = dto.Nom;
        Prenom       = dto.Prenom;
        CIN          = dto.CIN;
        Telephone    = dto.Telephone;
        NumeroPermis = dto.NumeroPermis;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Nom))
        {
            ErrorMessage = "Le nom est obligatoire.";
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new ChauffeurDto
            {
                ChauffeurId      = _original.ChauffeurId,
                Nom              = Nom.Trim(),
                Prenom           = NullIfBlank(Prenom),
                CIN              = NullIfBlank(CIN),
                Telephone        = NullIfBlank(Telephone),
                NumeroPermis     = NullIfBlank(NumeroPermis),
                AjoutePar        = _original.AjoutePar,
                DateCreation     = _original.DateCreation,
                ModifiePar       = App.CurrentUser?.UserId,
                DateModification = DateTime.UtcNow
            };

            var ok = await _service.UpdateChauffeurAsync(dto);
            if (ok)
                Saved = true;
            else
                ErrorMessage = "Erreur lors de la mise à jour du chauffeur.";
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

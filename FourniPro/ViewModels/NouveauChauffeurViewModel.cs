using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the NouveauChauffeur creation dialog.
/// Manual properties for build stability (no source-generator partials).
/// </summary>
public class NouveauChauffeurViewModel : ObservableObject
{
    private readonly ChauffeurService _service = new();

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

    public NouveauChauffeurViewModel()
    {
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
                Nom          = Nom.Trim(),
                Prenom       = NullIfBlank(Prenom),
                CIN          = NullIfBlank(CIN),
                Telephone    = NullIfBlank(Telephone),
                NumeroPermis = NullIfBlank(NumeroPermis)
            };

            var result = await _service.CreateChauffeurAsync(dto);
            if (result is not null)
                Saved = true;
            else
                ErrorMessage = "Erreur lors de la création du chauffeur.";
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the NouveauCamion creation dialog.
/// </summary>
public class NouveauCamionViewModel : ObservableObject
{
    private readonly CamionService _service = new();

    private string? _matricule;
    public string? Matricule
    {
        get => _matricule;
        set => SetProperty(ref _matricule, value);
    }

    private string? _marque;
    public string? Marque
    {
        get => _marque;
        set => SetProperty(ref _marque, value);
    }

    private double? _consommation;
    public double? Consommation
    {
        get => _consommation;
        set => SetProperty(ref _consommation, value);
    }

    private double? _kilometrage;
    public double? Kilometrage
    {
        get => _kilometrage;
        set => SetProperty(ref _kilometrage, value);
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

    public NouveauCamionViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Matricule))
        {
            ErrorMessage = "Le matricule est obligatoire.";
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new CamionDto
            {
                Matricule    = Matricule.Trim(),
                Marque       = NullIfBlank(Marque),
                Consommation = Consommation,
                Kilometrage  = Kilometrage
            };

            var result = await _service.CreateCamionAsync(dto);
            if (result is not null)
                Saved = true;
            else
                ErrorMessage = "Erreur lors de la création du camion.";
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

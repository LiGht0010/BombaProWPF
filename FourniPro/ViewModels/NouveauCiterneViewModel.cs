using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the NouveauCiterne creation dialog.
/// </summary>
public class NouveauCiterneViewModel : ObservableObject
{
    private readonly CiterneService _service = new();

    private string? _matriculeCiterne;
    public string? MatriculeCiterne
    {
        get => _matriculeCiterne;
        set => SetProperty(ref _matriculeCiterne, value);
    }

    private decimal? _capacite;
    public decimal? Capacite
    {
        get => _capacite;
        set => SetProperty(ref _capacite, value);
    }

    private int? _partitionsNumber;
    public int? PartitionsNumber
    {
        get => _partitionsNumber;
        set => SetProperty(ref _partitionsNumber, value);
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

    public NouveauCiterneViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(MatriculeCiterne))
        {
            ErrorMessage = LanguageManager.Instance["NouveauCiterneValidationMatricule"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new CiterneDto
            {
                MatriculeCiterne = MatriculeCiterne.Trim(),
                Capacite         = Capacite,
                PartitionsNumber = PartitionsNumber
            };

            var result = await _service.CreateCiterneAsync(dto);
            if (result is not null)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["NouveauCiterneSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
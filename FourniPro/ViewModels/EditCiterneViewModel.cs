using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the EditCiterne dialog.
/// </summary>
public class EditCiterneViewModel : ObservableObject
{
    private readonly CiterneService _service = new();
    private readonly CiterneDto _original;

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

    public EditCiterneViewModel(CiterneDto dto)
    {
        _original        = dto;
        MatriculeCiterne = dto.MatriculeCiterne;
        Capacite         = dto.Capacite;
        PartitionsNumber = dto.PartitionsNumber;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(MatriculeCiterne))
        {
            ErrorMessage = "Le matricule est obligatoire.";
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new CiterneDto
            {
                CiterneId        = _original.CiterneId,
                MatriculeCiterne = MatriculeCiterne.Trim(),
                Capacite         = Capacite,
                PartitionsNumber = PartitionsNumber,
                AjoutePar        = _original.AjoutePar,
                DateCreation     = _original.DateCreation,
                ModifiePar       = App.CurrentUser?.UserId,
                DateModification = DateTime.UtcNow
            };

            var ok = await _service.UpdateCiterneAsync(dto);
            if (ok)
                Saved = true;
            else
                ErrorMessage = "Erreur lors de la mise à jour de la citerne.";
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

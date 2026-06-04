using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the EditClient dialog.
/// All properties are manual (no source-generator partial methods) for build stability.
/// </summary>
public class EditClientViewModel : ObservableObject
{
    private readonly ClientService _service = new();
    private readonly int _clientId;

    // ── Fields ────────────────────────────────────────────────────────────────

    private string _nom = string.Empty;
    public string Nom
    {
        get => _nom;
        set => SetProperty(ref _nom, value);
    }

    private string _cin = string.Empty;
    public string CIN
    {
        get => _cin;
        set => SetProperty(ref _cin, value);
    }

    private string _nomSociete = string.Empty;
    public string NomSociete
    {
        get => _nomSociete;
        set => SetProperty(ref _nomSociete, value);
    }

    private string _numeroClient = string.Empty;
    public string NumeroClient
    {
        get => _numeroClient;
        set => SetProperty(ref _numeroClient, value);
    }

    private string? _contact;
    public string? Contact
    {
        get => _contact;
        set => SetProperty(ref _contact, value);
    }

    private string? _adresse;
    public string? Adresse
    {
        get => _adresse;
        set => SetProperty(ref _adresse, value);
    }

    private string? _personel;
    public string? Personel
    {
        get => _personel;
        set => SetProperty(ref _personel, value);
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

    public bool Saved { get; private set; }

    // Original audit values preserved for update payload
    private readonly ClientDto _original;

    public IAsyncRelayCommand SaveCommand { get; }

    public EditClientViewModel(ClientDto dto)
    {
        _clientId    = dto.ClientId;
        _original    = dto;

        _nom          = dto.Nom ?? string.Empty;
        _cin          = dto.CIN ?? string.Empty;
        _nomSociete   = dto.NomSociete ?? string.Empty;
        _numeroClient = dto.NumeroClient ?? string.Empty;
        _contact      = dto.Contact;
        _adresse      = dto.Adresse;
        _personel     = dto.Personel;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(_nom))
        {
            ErrorMessage = LanguageManager.Instance["EditClientValidationNom"];
            return;
        }

        if (string.IsNullOrWhiteSpace(_numeroClient))
        {
            ErrorMessage = LanguageManager.Instance["EditClientValidationNumero"];
            return;
        }

        try
        {
            IsSaving = true;

            var dto = new ClientDto
            {
                ClientId      = _clientId,
                Nom           = _nom.Trim(),
                NumeroClient  = _numeroClient.Trim(),
                CIN           = NullIfBlank(_cin) ?? string.Empty,
                NomSociete    = NullIfBlank(_nomSociete) ?? string.Empty,
                Contact       = NullIfBlank(_contact),
                Adresse       = NullIfBlank(_adresse),
                Personel      = NullIfBlank(_personel),
                AjoutePar     = _original.AjoutePar,
                DateCreation  = _original.DateCreation,
            };

            var ok = await _service.UpdateClientAsync(dto).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                    Saved = true;
                else
                    ErrorMessage = LanguageManager.Instance["EditClientSaveError"];
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = LanguageManager.Instance["EditClientSaveError"]);
            Debug.WriteLine($"[EditClientVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

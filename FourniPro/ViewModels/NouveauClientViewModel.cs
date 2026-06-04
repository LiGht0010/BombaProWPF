using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// ViewModel for the NouveauClient creation dialog.
/// All properties are manual (no source-generator partial methods) for build stability.
/// </summary>
public class NouveauClientViewModel : ObservableObject
{
    private readonly ClientService _service = new();

    // ── Informations générales ────────────────────────────────────────────────

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

    // ── Contact & coordonnées ─────────────────────────────────────────────────

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

    /// <summary>Set to true on successful save; code-behind reads this to close.</summary>
    public bool Saved { get; private set; }

    public IAsyncRelayCommand SaveCommand { get; }

    public NouveauClientViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(_nom))
        {
            ErrorMessage = LanguageManager.Instance["NouveauClientValidationNom"];
            return;
        }

        if (string.IsNullOrWhiteSpace(_numeroClient))
        {
            ErrorMessage = LanguageManager.Instance["NouveauClientValidationNumero"];
            return;
        }

        try
        {
            IsSaving = true;

            var dto = new ClientDto
            {
                Nom           = _nom.Trim(),
                NumeroClient  = _numeroClient.Trim(),
                CIN           = NullIfBlank(_cin) ?? string.Empty,
                NomSociete    = NullIfBlank(_nomSociete) ?? string.Empty,
                Contact       = NullIfBlank(_contact),
                Adresse       = NullIfBlank(_adresse),
                Personel      = NullIfBlank(_personel),
            };

            var result = await _service.CreateClientAsync(dto).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (result is not null)
                    Saved = true;
                else
                    ErrorMessage = LanguageManager.Instance["NouveauClientSaveError"];
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = LanguageManager.Instance["NouveauClientSaveError"]);
            Debug.WriteLine($"[NouveauClientVM] Save failed: {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

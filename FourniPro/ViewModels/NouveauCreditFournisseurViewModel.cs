using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FourniPro.ViewModels;

/// <summary>ViewModel for the NouveauCreditFournisseur creation dialog.</summary>
public class NouveauCreditFournisseurViewModel : ObservableObject
{
    private readonly CreditFournisseurService _creditService     = new();
    private readonly FournisseurService       _fournisseurService = new();
    private readonly AchatService             _achatService      = new();
    private readonly EmployeService           _employeService    = new();

    // ── All achats fetched once, filtered by selected fournisseur ─────────────
    private List<AchatDto> _allAchats = [];

    // ── Form fields ──────────────────────────────────────────────────────────

    private string _numeroCreditF = GenerateNumero();
    public string NumeroCreditF
    {
        get => _numeroCreditF;
        private set => SetProperty(ref _numeroCreditF, value);
    }

    private DateTime _dateCredit = DateTime.Today;
    public DateTime DateCredit
    {
        get => _dateCredit;
        set => SetProperty(ref _dateCredit, value);
    }

    // Statut is always NonPayé on create — read-only badge
    public string Statut => "NonPayé";

    private FournisseurDto? _selectedFournisseur;
    public FournisseurDto? SelectedFournisseur
    {
        get => _selectedFournisseur;
        set
        {
            if (SetProperty(ref _selectedFournisseur, value))
                RefreshAchats();
        }
    }

    private AchatDto? _selectedAchat;
    public AchatDto? SelectedAchat
    {
        get => _selectedAchat;
        set
        {
            if (SetProperty(ref _selectedAchat, value))
                MontantTotal = value?.Cout;
        }
    }

    private EmployeDto? _selectedEmploye;
    public EmployeDto? SelectedEmploye
    {
        get => _selectedEmploye;
        set => SetProperty(ref _selectedEmploye, value);
    }

    private decimal? _montantTotal;
    public decimal? MontantTotal
    {
        get => _montantTotal;
        private set => SetProperty(ref _montantTotal, value);
    }

    private string? _chequeReference;
    public string? ChequeReference
    {
        get => _chequeReference;
        set
        {
            if (SetProperty(ref _chequeReference, value))
                OnPropertyChanged(nameof(HasCheque));
        }
    }

    /// <summary>True when ChequeReference is non-empty — drives StatutCheque badge visibility.</summary>
    public bool HasCheque => !string.IsNullOrWhiteSpace(ChequeReference);

    /// <summary>Auto value — always Détenu when a cheque reference is present on creation.</summary>
    public string StatutCheque => "Détenu";

    private string? _note;
    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    // ── Lookup collections ────────────────────────────────────────────────────

    public ObservableCollection<FournisseurDto> Fournisseurs { get; } = [];
    public ObservableCollection<AchatDto>       AchatsFiltres { get; } = [];
    public ObservableCollection<EmployeDto>     Employes { get; } = [];

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
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

    public NouveauCreditFournisseurViewModel()
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public async Task LoadLookupsAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var fournisseurs = await _fournisseurService.GetAllFournisseursAsync();
            foreach (var f in fournisseurs) Fournisseurs.Add(f);

            _allAchats = await _achatService.GetAllAchatsAsync();

            var employes = await _employeService.GetAllEmployesAsync();
            foreach (var e in employes) Employes.Add(e);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NouveauCreditFournisseurVM] LoadLookups error: {ex.Message}");
            ErrorMessage = LanguageManager.Instance["NouveauCreditFournisseurLoadError"];
        }
        finally { IsLoading = false; }
    }

    /// <summary>Rebuilds AchatsFiltres for the selected fournisseur (ModePaiement == Crédit).</summary>
    private void RefreshAchats()
    {
        SelectedAchat = null;
        AchatsFiltres.Clear();

        if (SelectedFournisseur is null) return;

        var filtered = _allAchats
            .Where(a => a.FournisseurID == SelectedFournisseur.FournisseurId
                     && (string.Equals(a.ModePaiement, "Crédit",  StringComparison.OrdinalIgnoreCase)
                      || string.Equals(a.ModePaiement, "Credit",  StringComparison.OrdinalIgnoreCase)));

        foreach (var a in filtered)
            AchatsFiltres.Add(a);
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (SelectedFournisseur is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauCreditFournisseurValidationFournisseur"];
            return;
        }
        if (SelectedAchat is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauCreditFournisseurValidationAchat"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new CreditFournisseurDto
            {
                NumeroCreditF    = NumeroCreditF,
                DateCredit       = DateOnly.FromDateTime(DateCredit),
                FournisseurId    = SelectedFournisseur.FournisseurId,
                AchatId          = SelectedAchat.AchatId,
                EmployeId        = SelectedEmploye?.EmployeId,
                MontantTotal     = MontantTotal,
                Statut           = Statut,
                ChequeReference  = NullIfBlank(ChequeReference),
                StatutCheque     = HasCheque ? StatutCheque : null,
                Note             = NullIfBlank(Note),
            };

            var result = await _creditService.CreateCreditFournisseurAsync(dto);
            if (result is not null)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["NouveauCreditFournisseurSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string GenerateNumero()
    {
        var n = DateTime.Now;
        return $"CF-{n:yyyy-MM-dd-HH-mm-ss}";
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FourniPro.ViewModels;

public class EditCreditFournisseurViewModel : ObservableObject
{
    private readonly CreditFournisseurService _creditService      = new();
    private readonly FournisseurService       _fournisseurService = new();
    private readonly AchatService             _achatService       = new();
    private readonly EmployeService           _employeService     = new();

    private List<AchatDto> _allAchats = [];

    // ── Snapshot from incoming DTO ───────────────────────────────────────────
    private readonly int       _creditId;
    private readonly int?      _originalAjoutePar;
    private readonly DateTime? _originalDateCreation;
    private readonly int?      _pendingFournisseurId;
    private readonly int?      _pendingAchatId;
    private readonly int?      _pendingEmployeId;

    // ── Form fields ──────────────────────────────────────────────────────────

    private string _numeroCreditF;
    public string NumeroCreditF
    {
        get => _numeroCreditF;
        private set => SetProperty(ref _numeroCreditF, value);
    }

    private DateTime _dateCredit;
    public DateTime DateCredit
    {
        get => _dateCredit;
        set => SetProperty(ref _dateCredit, value);
    }

    private string? _selectedStatut;
    public string? SelectedStatut
    {
        get => _selectedStatut;
        set => SetProperty(ref _selectedStatut, value);
    }

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
            {
                OnPropertyChanged(nameof(HasCheque));
                if (!HasCheque) SelectedStatutCheque = null;
            }
        }
    }

    public bool HasCheque => !string.IsNullOrWhiteSpace(ChequeReference);

    private string? _selectedStatutCheque;
    public string? SelectedStatutCheque
    {
        get => _selectedStatutCheque;
        set => SetProperty(ref _selectedStatutCheque, value);
    }

    private string? _note;
    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    // ── Lookup collections ───────────────────────────────────────────────────

    public ObservableCollection<FournisseurDto> Fournisseurs   { get; } = [];
    public ObservableCollection<AchatDto>       AchatsFiltres  { get; } = [];
    public ObservableCollection<EmployeDto>     Employes       { get; } = [];

    public IReadOnlyList<string> Statuts { get; } =
        ["NonPayé", "PartielPayé", "Payé"];

    public IReadOnlyList<string> StatutsCheque { get; } =
        ["Détenu", "Encaissé"];

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

    public EditCreditFournisseurViewModel(CreditFournisseurDto dto)
    {
        _creditId             = dto.CreditFournisseurId;
        _originalAjoutePar    = dto.AjoutePar;
        _originalDateCreation = dto.DateCreation;
        _pendingFournisseurId = dto.FournisseurId;
        _pendingAchatId       = dto.AchatId;
        _pendingEmployeId     = dto.EmployeId;

        _numeroCreditF        = dto.NumeroCreditF ?? string.Empty;
        _dateCredit           = dto.DateCredit.ToDateTime(TimeOnly.MinValue);
        _selectedStatut       = dto.Statut ?? "NonPayé";
        _chequeReference      = dto.ChequeReference;
        _selectedStatutCheque = dto.StatutCheque;
        _note                 = dto.Note;
        _montantTotal         = dto.MontantTotal;

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

            // Restore FK selections after all collections are loaded
            SelectedFournisseur = Fournisseurs.FirstOrDefault(f => f.FournisseurId == _pendingFournisseurId);

            // RefreshAchats is triggered by SelectedFournisseur setter; then restore achat
            SelectedAchat = AchatsFiltres.FirstOrDefault(a => a.AchatId == _pendingAchatId);
            // Restore MontantTotal from DTO if achat not found in filtered list
            if (SelectedAchat is null) MontantTotal = _montantTotal;

            if (_pendingEmployeId.HasValue)
                SelectedEmploye = Employes.FirstOrDefault(e => e.EmployeId == _pendingEmployeId.Value);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditCreditFournisseurVM] LoadLookups error: {ex.Message}");
            ErrorMessage = LanguageManager.Instance["EditCfLoadError"];
        }
        finally { IsLoading = false; }
    }

    private void RefreshAchats()
    {
        SelectedAchat = null;
        AchatsFiltres.Clear();

        if (SelectedFournisseur is null) return;

        var filtered = _allAchats
            .Where(a => a.FournisseurID == SelectedFournisseur.FournisseurId
                     && (string.Equals(a.ModePaiement, "Crédit", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(a.ModePaiement, "Credit", StringComparison.OrdinalIgnoreCase)));

        foreach (var a in filtered)
            AchatsFiltres.Add(a);
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (SelectedFournisseur is null)
        {
            ErrorMessage = LanguageManager.Instance["EditCfValidationFournisseur"];
            return;
        }
        if (SelectedAchat is null)
        {
            ErrorMessage = LanguageManager.Instance["EditCfValidationAchat"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new CreditFournisseurDto
            {
                CreditFournisseurId = _creditId,
                NumeroCreditF       = NumeroCreditF,
                DateCredit          = DateOnly.FromDateTime(DateCredit),
                FournisseurId       = SelectedFournisseur.FournisseurId,
                AchatId             = SelectedAchat.AchatId,
                EmployeId           = SelectedEmploye?.EmployeId,
                MontantTotal        = MontantTotal,
                Statut              = SelectedStatut,
                ChequeReference     = NullIfBlank(ChequeReference),
                StatutCheque        = HasCheque ? SelectedStatutCheque : null,
                Note                = NullIfBlank(Note),
                AjoutePar           = _originalAjoutePar,
                DateCreation        = _originalDateCreation,
            };

            var ok = await _creditService.UpdateCreditFournisseurAsync(dto);
            if (ok)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["EditCfSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

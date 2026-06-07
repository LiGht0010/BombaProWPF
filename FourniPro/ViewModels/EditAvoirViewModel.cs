using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FourniPro.ViewModels;

public class EditAvoirViewModel : ObservableObject
{
    private readonly AvoirService   _service        = new();
    private readonly VenteService   _venteService   = new();
    private readonly CreditService  _creditService  = new();
    private readonly ClientService  _clientService  = new();
    private readonly EmployeService _employeService = new();

    private readonly int       _avoirId;
    private readonly int?      _originalAjoutePar;
    private readonly DateTime? _originalDateCreation;

    // Pending FK IDs to restore after lookups load
    private readonly int? _pendingVenteId;
    private readonly int? _pendingCreditId;
    private readonly int? _pendingClientId;
    private readonly int? _pendingEmployeId;

    // ── Source (fixed after creation — cannot change Vente↔Credit) ───────────

    public bool IsVenteSource  { get; }
    public bool IsCreditSource => !IsVenteSource;

    private VenteDto? _selectedVente;
    public VenteDto? SelectedVente
    {
        get => _selectedVente;
        set => SetProperty(ref _selectedVente, value);
    }

    private CreditDto? _selectedCredit;
    public CreditDto? SelectedCredit
    {
        get => _selectedCredit;
        set => SetProperty(ref _selectedCredit, value);
    }

    // ── Form fields ──────────────────────────────────────────────────────────

    private DateTime _dateAvoir;
    public DateTime DateAvoir
    {
        get => _dateAvoir;
        set => SetProperty(ref _dateAvoir, value);
    }

    private ClientDto? _selectedClient;
    public ClientDto? SelectedClient
    {
        get => _selectedClient;
        set => SetProperty(ref _selectedClient, value);
    }

    private EmployeDto? _selectedEmploye;
    public EmployeDto? SelectedEmploye
    {
        get => _selectedEmploye;
        set => SetProperty(ref _selectedEmploye, value);
    }

    private int? _quantite;
    public int? Quantite
    {
        get => _quantite;
        set => SetProperty(ref _quantite, value);
    }

    private decimal? _prixUnitaire;
    public decimal? PrixUnitaire
    {
        get => _prixUnitaire;
        set => SetProperty(ref _prixUnitaire, value);
    }

    private decimal? _montantAvoir;
    public decimal? MontantAvoir
    {
        get => _montantAvoir;
        set => SetProperty(ref _montantAvoir, value);
    }

    private string? _selectedRaison;
    public string? SelectedRaison
    {
        get => _selectedRaison;
        set => SetProperty(ref _selectedRaison, value);
    }

    private string? _note;
    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    private string? _referenceFile;
    public string? ReferenceFile
    {
        get => _referenceFile;
        set => SetProperty(ref _referenceFile, value);
    }

    // ── Lookup lists ──────────────────────────────────────────────────────────

    public ObservableCollection<VenteDto>   Ventes   { get; } = [];
    public ObservableCollection<CreditDto>  Credits  { get; } = [];
    public ObservableCollection<ClientDto>  Clients  { get; } = [];
    public ObservableCollection<EmployeDto> Employes { get; } = [];

    public IReadOnlyList<string> Raisons =>
        IsVenteSource
            ? ["Retour", "Defectueux", "ErreurFacturation", "Autre"]
            : ["Retour", "Defectueux", "ErreurFacturation", "TropPercu", "Autre"];

    // ── State ─────────────────────────────────────────────────────────────────

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

    public IAsyncRelayCommand SaveCommand               { get; }
    public IRelayCommand      BrowseReferenceFileCommand { get; }

    public EditAvoirViewModel(AvoirDto dto)
    {
        _avoirId              = dto.AvoirId;
        _originalAjoutePar    = dto.AjoutePar;
        _originalDateCreation = dto.DateCreation;
        _pendingVenteId       = dto.VenteId;
        _pendingCreditId      = dto.CreditId;
        _pendingClientId      = dto.ClientId;
        _pendingEmployeId     = dto.EmployeId;

        IsVenteSource    = dto.VenteId.HasValue;
        _dateAvoir       = dto.DateAvoir.ToDateTime(TimeOnly.MinValue);
        _quantite        = dto.Quantite;
        _prixUnitaire    = dto.PrixUnitaire;
        _montantAvoir    = dto.MontantAvoir;
        _selectedRaison  = dto.Raison;
        _note            = dto.Note;
        _referenceFile   = dto.ReferenceFile;

        SaveCommand                = new AsyncRelayCommand(SaveAsync);
        BrowseReferenceFileCommand = new RelayCommand(BrowseReferenceFile);
    }

    // ── File picker ───────────────────────────────────────────────────────────

    private void BrowseReferenceFile()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Sélectionner le fichier de référence",
            Filter = "Tous les fichiers (*.*)|*.*|Images (*.jpg;*.png)|*.jpg;*.png|PDF (*.pdf)|*.pdf"
        };
        if (dlg.ShowDialog() == true)
            ReferenceFile = dlg.FileName;
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public async Task LoadLookupsAsync()
    {
        var ventes = await _venteService.GetAllVentesAsync();
        foreach (var v in ventes) Ventes.Add(v);
        if (_pendingVenteId.HasValue)
            SelectedVente = Ventes.FirstOrDefault(v => v.VenteId == _pendingVenteId.Value);

        var credits = await _creditService.GetAllCreditsAsync();
        foreach (var c in credits) Credits.Add(c);
        if (_pendingCreditId.HasValue)
            SelectedCredit = Credits.FirstOrDefault(c => c.CreditId == _pendingCreditId.Value);

        var clients = await _clientService.GetAllClientsAsync();
        foreach (var c in clients) Clients.Add(c);
        if (_pendingClientId.HasValue)
            SelectedClient = Clients.FirstOrDefault(c => c.ClientId == _pendingClientId.Value);

        var employes = await _employeService.GetAllEmployesAsync();
        foreach (var e in employes) Employes.Add(e);
        if (_pendingEmployeId.HasValue)
            SelectedEmploye = Employes.FirstOrDefault(e => e.EmployeId == _pendingEmployeId.Value);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (IsVenteSource && SelectedVente is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauAvoirValidationVente"];
            return;
        }
        if (IsCreditSource && SelectedCredit is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauAvoirValidationCredit"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new AvoirDto
            {
                AvoirId      = _avoirId,
                DateAvoir    = DateOnly.FromDateTime(DateAvoir),
                VenteId      = IsVenteSource  ? SelectedVente?.VenteId   : null,
                CreditId     = IsCreditSource ? SelectedCredit?.CreditId : null,
                ClientId     = SelectedClient?.ClientId,
                EmployeId    = SelectedEmploye?.EmployeId,
                Quantite     = IsVenteSource ? Quantite     : null,
                PrixUnitaire = IsVenteSource ? PrixUnitaire : null,
                Raison       = SelectedRaison,
                Note         = NullIfBlank(Note),
                ReferenceFile = NullIfBlank(ReferenceFile),
                AjoutePar    = _originalAjoutePar,
                DateCreation = _originalDateCreation,
            };

            if (IsVenteSource && SelectedVente is not null)
                dto.ProduitId = SelectedVente.ProduitID;

            var ok = await _service.UpdateAsync(dto);
            if (ok)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["NouveauAvoirSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

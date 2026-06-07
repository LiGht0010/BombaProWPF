using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FourniPro.ViewModels;

public class NouveauAvoirViewModel : ObservableObject
{
    private readonly AvoirService          _service               = new();
    private readonly VenteService          _venteService          = new();
    private readonly CreditService         _creditService         = new();
    private readonly ClientService         _clientService         = new();
    private readonly EmployeService        _employeService        = new();
    private readonly PaiementCreditService _paiementCreditService = new();

    // ── Source selection ──────────────────────────────────────────────────────

    private bool _isVenteSource = true;
    public bool IsVenteSource
    {
        get => _isVenteSource;
        set
        {
            if (SetProperty(ref _isVenteSource, value))
            {
                OnPropertyChanged(nameof(IsCreditSource));
                // Clear the opposite source FK
                if (value) SelectedCredit = null;
                else       SelectedVente  = null;
                RecomputeMontant();
            }
        }
    }

    public bool IsCreditSource
    {
        get => !_isVenteSource;
        set => IsVenteSource = !value;
    }

    private VenteDto? _selectedVente;
    public VenteDto? SelectedVente
    {
        get => _selectedVente;
        set
        {
            if (SetProperty(ref _selectedVente, value))
            {
                if (value is not null)
                {
                    PrixUnitaire  = value.PrixUnitaire;
                    SelectedClient = Clients.FirstOrDefault(c => c.ClientId == value.ClientID);
                }
                RecomputeMontant();
            }
        }
    }

    private CreditDto? _selectedCredit;
    public CreditDto? SelectedCredit
    {
        get => _selectedCredit;
        set
        {
            if (SetProperty(ref _selectedCredit, value))
            {
                if (value is not null)
                {
                    SelectedClient = Clients.FirstOrDefault(c => c.ClientId == value.ClientID);
                    _ = RecomputeCreditMontantAsync(value);
                }
                else
                {
                    MontantAvoir   = null;
                    SelectedRaison = "Retour";
                }
            }
        }
    }

    // ── Form fields ──────────────────────────────────────────────────────────

    private DateTime _dateAvoir = DateTime.Today;
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
        set
        {
            if (SetProperty(ref _quantite, value))
                RecomputeMontant();
        }
    }

    private decimal? _prixUnitaire;
    public decimal? PrixUnitaire
    {
        get => _prixUnitaire;
        set
        {
            if (SetProperty(ref _prixUnitaire, value))
                RecomputeMontant();
        }
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

    /// <summary>Raisons available for current source type.</summary>
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

    public NouveauAvoirViewModel()
    {
        SaveCommand                = new AsyncRelayCommand(SaveAsync);
        BrowseReferenceFileCommand = new RelayCommand(BrowseReferenceFile);
        _selectedRaison            = "Retour";
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

        var credits = await _creditService.GetAllCreditsAsync();
        foreach (var c in credits) Credits.Add(c);

        var clients = await _clientService.GetAllClientsAsync();
        foreach (var c in clients) Clients.Add(c);

        var employes = await _employeService.GetAllEmployesAsync();
        foreach (var e in employes) Employes.Add(e);
    }

    // ── Computed Montant ──────────────────────────────────────────────────────

    private void RecomputeMontant()
    {
        if (IsVenteSource)
            MontantAvoir = (Quantite ?? 0) * (PrixUnitaire ?? 0m);
        // Credit montant is recomputed in RecomputeCreditMontantAsync
    }

    /// <summary>
    /// For credit-source avoirs: fetches all payments for the selected credit,
    /// computes the overpayment (Σpaiements − MontantTotal), pre-fills MontantAvoir,
    /// and auto-selects TropPercu when an overpayment is detected.
    /// </summary>
    private async Task RecomputeCreditMontantAsync(CreditDto credit)
    {
        var paiements   = await _paiementCreditService.GetAllAsync(credit.CreditId);
        var totalPayé   = paiements.Sum(p => p.Montant ?? 0m);
        var montantDû   = credit.MontantTotal ?? 0m;
        var trop        = totalPayé - montantDû;

        if (trop > 0m)
        {
            MontantAvoir   = trop;
            SelectedRaison = "TropPercu";
            OnPropertyChanged(nameof(Raisons)); // ensure TropPercu is visible
        }
        else
        {
            MontantAvoir   = null;
            SelectedRaison = "Retour";
        }
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
        if (!IsVenteSource && SelectedCredit is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauAvoirValidationCredit"];
            return;
        }

        IsSaving = true;
        try
        {
            var dto = new AvoirDto
            {
                DateAvoir    = DateOnly.FromDateTime(DateAvoir),
                VenteId      = IsVenteSource  ? SelectedVente?.VenteId   : null,
                CreditId     = !IsVenteSource ? SelectedCredit?.CreditId : null,
                ClientId     = SelectedClient?.ClientId,
                EmployeId    = SelectedEmploye?.EmployeId,
                Quantite     = IsVenteSource ? Quantite     : null,
                PrixUnitaire = IsVenteSource ? PrixUnitaire : null,
                Raison       = SelectedRaison,
                Note         = NullIfBlank(Note),
                ReferenceFile = NullIfBlank(ReferenceFile),
            };

            // Populate ProduitId from the selected Vente
            if (IsVenteSource && SelectedVente is not null)
                dto.ProduitId = SelectedVente.ProduitID;

            var result = await _service.CreateAsync(dto);
            if (result is not null)
                Saved = true;
            else
                ErrorMessage = LanguageManager.Instance["NouveauAvoirSaveError"];
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

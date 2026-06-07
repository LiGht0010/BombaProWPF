using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Automation;
using FourniPro.Automation.Voyage;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace FourniPro.ViewModels;

/// <summary>
/// Full edit ViewModel for an existing voyage.
/// Mirrors <see cref="NouveauVoyageViewModel"/> layout but loads existing data
/// and supports in-place edit of individual transaction / stock rows.
/// </summary>
public class EditVoyageViewModel : ObservableObject
{
    // ── Services ──────────────────────────────────────────────────────────────
    private readonly VoyageService      _voyageService    = new();
    private readonly StockVoyageService _stockService     = new();
    private readonly FraisVoyageService _fraisService     = new();
    private readonly VenteService       _venteService     = new();
    private readonly CreditService      _creditService    = new();
    private readonly AchatService       _achatService     = new();
    private readonly ProduitService     _produitService   = new();
    private readonly CamionService      _camionService    = new();
    private readonly ChauffeurService   _chauffeurService = new();
    private readonly CiterneService     _citerneService   = new();
    private readonly ClientService      _clientService    = new();
    private readonly FournisseurService _fournisseurService = new();
    private readonly AutomationRunner   _automationRunner   = new();

    // ── Voyage header ─────────────────────────────────────────────────────────
    private readonly int  _voyageId;
    private int?      _originalAjoutePar;
    private DateTime? _originalDateCreation;

    private CamionDto? _selectedCamion;
    public CamionDto? SelectedCamion
    {
        get => _selectedCamion;
        set
        {
            // Auto-fill km depart only when the USER picks a camion (not during initial data load).
            if (SetProperty(ref _selectedCamion, value) && value is not null && !IsLoading)
            {
                if (value.Kilometrage.HasValue && double.IsFinite(value.Kilometrage.Value))
                    KilometrageDepart = (decimal)value.Kilometrage.Value;
            }
        }
    }

    private ChauffeurDto? _selectedChauffeur;
    public ChauffeurDto? SelectedChauffeur
    {
        get => _selectedChauffeur;
        set => SetProperty(ref _selectedChauffeur, value);
    }

    private CiterneDto? _selectedCiterne;
    public CiterneDto? SelectedCiterne
    {
        get => _selectedCiterne;
        set => SetProperty(ref _selectedCiterne, value);
    }

    private string? _lieuDepart;
    public string? LieuDepart
    {
        get => _lieuDepart;
        set => SetProperty(ref _lieuDepart, value);
    }

    private string? _lieuTerminal;
    public string? LieuTerminal
    {
        get => _lieuTerminal;
        set => SetProperty(ref _lieuTerminal, value);
    }

    private DateTime _dateDepart;
    public DateTime DateDepart
    {
        get => _dateDepart;
        set => SetProperty(ref _dateDepart, DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private DateTime? _dateFinal;
    public DateTime? DateFinal
    {
        get => _dateFinal;
        set => SetProperty(ref _dateFinal, value.HasValue
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);
    }

    private decimal? _kilometrageDepart;
    public decimal? KilometrageDepart
    {
        get => _kilometrageDepart;
        set => SetProperty(ref _kilometrageDepart, value);
    }

    private decimal? _kilometrageFinal;
    public decimal? KilometrageFinal
    {
        get => _kilometrageFinal;
        set => SetProperty(ref _kilometrageFinal, value);
    }

    private string _statut = "InProgress";
    public string Statut
    {
        get => _statut;
        set => SetProperty(ref _statut, value);
    }

    // ── Stock tab form ────────────────────────────────────────────────────────
    private ProduitDto? _stockProduit;
    public ProduitDto? StockProduit
    {
        get => _stockProduit;
        set => SetProperty(ref _stockProduit, value);
    }

    private int? _stockQuantite;
    public int? StockQuantite
    {
        get => _stockQuantite;
        set => SetProperty(ref _stockQuantite, value);
    }

    // ── Edit-mode state for stock form ────────────────────────────────────────
    private VoyageStockItem? _editingStockItem;

    private bool _isEditingStock;
    public bool IsEditingStock
    {
        get => _isEditingStock;
        set
        {
            SetProperty(ref _isEditingStock, value);
            OnPropertyChanged(nameof(StockFormButtonLabel));
        }
    }

    public string StockFormButtonLabel =>
        IsEditingStock
            ? LanguageManager.Instance["EditVoyageBtnUpdate"]
            : "+";

    // ── Transaction tab index ─────────────────────────────────────────────────
    private int _activeTab;
    public int ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

    // ── Vente tab form ────────────────────────────────────────────────────────
    private ClientDto? _venteClient;
    public ClientDto? VenteClient
    {
        get => _venteClient;
        set => SetProperty(ref _venteClient, value);
    }

    private ProduitDto? _venteProduit;
    public ProduitDto? VenteProduit
    {
        get => _venteProduit;
        set
        {
            if (SetProperty(ref _venteProduit, value) && value is not null && !_isEditingTransaction)
                VentePrix = value.PrixTTC;
        }
    }

    private int? _venteQuantite;
    public int? VenteQuantite
    {
        get => _venteQuantite;
        set { if (SetProperty(ref _venteQuantite, value)) RecomputeVente(); }
    }

    private decimal? _ventePrix;
    public decimal? VentePrix
    {
        get => _ventePrix;
        set { if (SetProperty(ref _ventePrix, value)) RecomputeVente(); }
    }

    private decimal? _venteRemise;
    public decimal? VenteRemise
    {
        get => _venteRemise;
        set { if (SetProperty(ref _venteRemise, value)) RecomputeVente(); }
    }

    private decimal? _venteTotal;
    public decimal? VenteTotal
    {
        get => _venteTotal;
        private set => SetProperty(ref _venteTotal, value);
    }

    private string? _ventePayment;
    public string? VentePayment
    {
        get => _ventePayment;
        set => SetProperty(ref _ventePayment, value);
    }

    private string? _venteNote;
    public string? VenteNote
    {
        get => _venteNote;
        set => SetProperty(ref _venteNote, value);
    }

    private DateOnly _venteDate = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly VenteDate
    {
        get => _venteDate;
        set => SetProperty(ref _venteDate, value);
    }

    // ── Crédit tab form ───────────────────────────────────────────────────────
    private ClientDto? _creditClient;
    public ClientDto? CreditClient
    {
        get => _creditClient;
        set => SetProperty(ref _creditClient, value);
    }

    private ProduitDto? _creditProduit;
    public ProduitDto? CreditProduit
    {
        get => _creditProduit;
        set
        {
            if (SetProperty(ref _creditProduit, value) && value is not null && !_isEditingTransaction)
                CreditPrix = value.PrixTTC;
        }
    }

    private int? _creditQuantite;
    public int? CreditQuantite
    {
        get => _creditQuantite;
        set { if (SetProperty(ref _creditQuantite, value)) RecomputeCredit(); }
    }

    private decimal? _creditPrix;
    public decimal? CreditPrix
    {
        get => _creditPrix;
        set { if (SetProperty(ref _creditPrix, value)) RecomputeCredit(); }
    }

    private decimal? _creditRemise;
    public decimal? CreditRemise
    {
        get => _creditRemise;
        set { if (SetProperty(ref _creditRemise, value)) RecomputeCredit(); }
    }

    private decimal? _creditTotal;
    public decimal? CreditTotal
    {
        get => _creditTotal;
        private set => SetProperty(ref _creditTotal, value);
    }

    private string? _creditChequeRef;
    public string? CreditChequeRef
    {
        get => _creditChequeRef;
        set
        {
            if (SetProperty(ref _creditChequeRef, value))
                OnPropertyChanged(nameof(CreditHasCheque));
        }
    }

    public bool CreditHasCheque => !string.IsNullOrWhiteSpace(CreditChequeRef);

    private string? _creditNote;
    public string? CreditNote
    {
        get => _creditNote;
        set => SetProperty(ref _creditNote, value);
    }

    private DateOnly _creditDate = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly CreditDate
    {
        get => _creditDate;
        set => SetProperty(ref _creditDate, value);
    }

    // ── Achat tab form ────────────────────────────────────────────────────────
    private FournisseurDto? _achatFournisseur;
    public FournisseurDto? AchatFournisseur
    {
        get => _achatFournisseur;
        set => SetProperty(ref _achatFournisseur, value);
    }

    private ProduitDto? _achatProduit;
    public ProduitDto? AchatProduit
    {
        get => _achatProduit;
        set
        {
            if (SetProperty(ref _achatProduit, value) && value is not null && !_isEditingTransaction)
                AchatPrix = value.PrixAchat;
        }
    }

    private int? _achatQuantite;
    public int? AchatQuantite
    {
        get => _achatQuantite;
        set { if (SetProperty(ref _achatQuantite, value)) OnPropertyChanged(nameof(AchatCout)); }
    }

    private decimal? _achatPrix;
    public decimal? AchatPrix
    {
        get => _achatPrix;
        set { if (SetProperty(ref _achatPrix, value)) OnPropertyChanged(nameof(AchatCout)); }
    }

    public decimal? AchatCout => AchatPrix.HasValue && AchatQuantite.HasValue
        ? AchatPrix.Value * AchatQuantite.Value : (decimal?)null;

    private bool _achatDefectueux;
    public bool AchatDefectueux
    {
        get => _achatDefectueux;
        set => SetProperty(ref _achatDefectueux, value);
    }

    private string? _achatDescription;
    public string? AchatDescription
    {
        get => _achatDescription;
        set => SetProperty(ref _achatDescription, value);
    }

    private DateOnly _achatDate = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly AchatDate
    {
        get => _achatDate;
        set => SetProperty(ref _achatDate, value);
    }

    // ── Frais tab form ────────────────────────────────────────────────────────
    private string? _fraisType;
    public string? FraisType
    {
        get => _fraisType;
        set => SetProperty(ref _fraisType, value);
    }

    private decimal? _fraisMontant;
    public decimal? FraisMontant
    {
        get => _fraisMontant;
        set => SetProperty(ref _fraisMontant, value);
    }

    private string? _fraisDescription;
    public string? FraisDescription
    {
        get => _fraisDescription;
        set => SetProperty(ref _fraisDescription, value);
    }

    // ── Edit-mode state for transaction form ──────────────────────────────────
    private VoyageTransactionItem? _editingTransactionItem;
    private bool _isEditingTransaction;

    private bool _isEditingTx;
    public bool IsEditingTx
    {
        get => _isEditingTx;
        set
        {
            SetProperty(ref _isEditingTx, value);
            OnPropertyChanged(nameof(TransactionFormButtonLabel));
        }
    }

    public string TransactionFormButtonLabel =>
        IsEditingTx
            ? LanguageManager.Instance["EditVoyageBtnUpdate"]
            : LanguageManager.Instance["NouveauVoyageAddTransaction"];

    // ── Staged collections ────────────────────────────────────────────────────
    public ObservableCollection<VoyageStockItem>       StockItems        { get; } = [];
    public ObservableCollection<VoyageTransactionItem> Transactions      { get; } = [];
    public ObservableCollection<StockRestantItem>      StockRestantItems { get; } = [];

    private decimal _fraisTotal;
    /// <summary>Running total of all Frais transactions for this voyage.</summary>
    public decimal FraisTotal
    {
        get => _fraisTotal;
        private set => SetProperty(ref _fraisTotal, value);
    }

    // ── Lookup lists ──────────────────────────────────────────────────────────
    public ObservableCollection<CamionDto>      Camions      { get; } = [];
    public ObservableCollection<ChauffeurDto>   Chauffeurs   { get; } = [];
    public ObservableCollection<CiterneDto>     Citernes     { get; } = [];
    public ObservableCollection<ClientDto>      Clients      { get; } = [];
    public ObservableCollection<ProduitDto>     Produits     { get; } = [];
    public ObservableCollection<FournisseurDto> Fournisseurs { get; } = [];

    public IReadOnlyList<string> PaymentMethods { get; } = ["TPE", "Virement", "Especes"];
    public IReadOnlyList<string> FraisTypes     { get; } = ["Carburant", "Péage", "Réparation", "Autre"];
    public IReadOnlyList<string> Statuts        { get; } = ["InProgress", "Completed", "Cancelled"];

    // ── UI state ──────────────────────────────────────────────────────────────
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

    // ── Commands ──────────────────────────────────────────────────────────────
    public IRelayCommand                          AddStockCommand          { get; }
    public IRelayCommand<VoyageStockItem>         RemoveStockCommand       { get; }
    public IRelayCommand<VoyageStockItem>         EditStockCommand         { get; }
    public IRelayCommand                          AddVenteCommand          { get; }
    public IRelayCommand                          AddCreditCommand         { get; }
    public IRelayCommand                          AddAchatCommand          { get; }
    public IRelayCommand                          AddFraisCommand          { get; }
    public IRelayCommand<VoyageTransactionItem>   RemoveTransactionCommand { get; }
    public IRelayCommand<VoyageTransactionItem>   EditTransactionCommand   { get; }
    public IAsyncRelayCommand                     SaveCommand              { get; }

    /// <summary>
    /// Raised when a transaction row's Edit button is clicked.
    /// Parameter is the tab index (0=Vente,1=Crédit,2=Achat,3=Frais).
    /// Code-behind subscribes to switch the visible tab panel.
    /// </summary>
    public event Action<int>? EditTransactionRequested;

    /// <summary>Raised when a stock row's Edit button is clicked (code-behind shows the stock panel).</summary>
    public event Action? EditStockRequested;

    // ── Constructor ───────────────────────────────────────────────────────────
    public EditVoyageViewModel(VoyageCardItem voyage)
    {
        _voyageId    = voyage.VoyageId;
        _statut      = voyage.Statut;
        _lieuDepart  = voyage.LieuDepart;
        _lieuTerminal = voyage.LieuTerminal;
        _kilometrageDepart = voyage.KilometrageDepart;
        _kilometrageFinal  = voyage.KilometrageFinal;
        _dateDepart  = voyage.DateDepart.HasValue
            ? DateTime.SpecifyKind(voyage.DateDepart.Value, DateTimeKind.Utc)
            : DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
        _dateFinal   = voyage.DateFinal.HasValue
            ? DateTime.SpecifyKind(voyage.DateFinal.Value, DateTimeKind.Utc)
            : null;

        AddStockCommand          = new RelayCommand(AddOrUpdateStock);
        RemoveStockCommand       = new AsyncRelayCommand<VoyageStockItem>(RemoveStockAsync);
        EditStockCommand         = new RelayCommand<VoyageStockItem>(item =>
        {
            BeginEditStock(item);
            EditStockRequested?.Invoke();
        });
        AddVenteCommand          = new AsyncRelayCommand(AddOrUpdateVenteAsync);
        AddCreditCommand         = new AsyncRelayCommand(AddOrUpdateCreditAsync);
        AddAchatCommand          = new AsyncRelayCommand(AddOrUpdateAchatAsync);
        AddFraisCommand          = new AsyncRelayCommand(AddOrUpdateFraisAsync);
        RemoveTransactionCommand = new AsyncRelayCommand<VoyageTransactionItem>(RemoveTransactionAsync);
        EditTransactionCommand   = new RelayCommand<VoyageTransactionItem>(item =>
        {
            int tab = BeginEditTransaction(item);
            EditTransactionRequested?.Invoke(tab);
        });
        SaveCommand              = new AsyncRelayCommand(SaveVoyageHeaderAsync);
    }

    // ── Lookups + initial data load ───────────────────────────────────────────
    public async Task LoadLookupsAsync()
    {
        IsLoading = true;
        try
        {
            var dto         = await _voyageService.GetVoyageByIdAsync(_voyageId);
            var tCamions    = _camionService.GetAllCamionsAsync();
            var tChauffeurs = _chauffeurService.GetAllChauffeursAsync();
            var tCiternes   = _citerneService.GetAllCiternesAsync();
            var tClients    = _clientService.GetAllClientsAsync();
            var tProduits   = _produitService.GetAllProduitsAsync();
            var tFourns     = _fournisseurService.GetAllFournisseursAsync();
            var tVentes     = _venteService.GetByVoyageAsync(_voyageId);
            var tCredits    = _creditService.GetByVoyageAsync(_voyageId);
            var tStocks     = _stockService.GetByVoyageAsync(_voyageId);
            var tFrais      = _fraisService.GetByVoyageAsync(_voyageId);

            await Task.WhenAll(tCamions, tChauffeurs, tCiternes, tClients, tProduits, tFourns,
                               tVentes, tCredits, tStocks, tFrais);

            foreach (var x in tCamions.Result)    Camions.Add(x);
            foreach (var x in tChauffeurs.Result) Chauffeurs.Add(x);
            foreach (var x in tCiternes.Result)   Citernes.Add(x);
            foreach (var x in tClients.Result)    Clients.Add(x);
            foreach (var x in tProduits.Result)   Produits.Add(x);
            foreach (var x in tFourns.Result)     Fournisseurs.Add(x);

            if (dto is not null)
            {
                _originalAjoutePar    = dto.AjoutePar;
                _originalDateCreation = dto.DateCreation;
                SelectedCamion    = tCamions.Result.FirstOrDefault(x => x.CamionId    == dto.CamionId);
                SelectedChauffeur = tChauffeurs.Result.FirstOrDefault(x => x.ChauffeurId == dto.ChauffeurId);
                SelectedCiterne   = tCiternes.Result.FirstOrDefault(x => x.CiterneId  == dto.CiterneId);
            }

            // Populate existing stocks
            foreach (var s in tStocks.Result)
                StockItems.Add(new VoyageStockItem
                {
                    Id         = s.StockVoyageId,
                    IsExisting = true,
                    ProduitId  = s.ProduitId,
                    ProduitNom = s.ProduitNom ?? string.Empty,
                    Quantite   = s.Quantite
                });

            // Populate existing ventes
            foreach (var v in tVentes.Result)
                Transactions.Add(new VoyageTransactionItem
                {
                    Id         = v.VenteId,
                    IsExisting = true,
                    NumeroRef  = v.NumeroVente,
                    AjoutePar  = v.AjoutePar,
                    DateCreation = v.DateCreation,
                    Type       = "Vente",
                    Partie     = v.ClientNom ?? string.Empty,
                    ProduitNom = v.ProduitNom,
                    Quantite   = v.Quantite,
                    PrixUnitaire = v.PrixUnitaire,
                    Remise     = v.Remise,
                    MontantTotal = v.MontantTotal,
                    Date       = v.DateVente,
                    ClientId   = v.ClientID,
                    ProduitId  = v.ProduitID,
                    PaymentMethod = v.PaymentMethod,
                    Note       = v.Note
                });

            // Populate existing credits
            foreach (var c in tCredits.Result)
                Transactions.Add(new VoyageTransactionItem
                {
                    Id         = c.CreditId,
                    IsExisting = true,
                    NumeroRef  = c.NumeroCredit,
                    AjoutePar  = c.AjoutePar,
                    DateCreation = c.DateCreation,
                    Type       = "Crédit",
                    Partie     = c.ClientNom ?? string.Empty,
                    ProduitNom = c.ProduitNom,
                    Quantite   = c.Quantite,
                    PrixUnitaire = c.PrixUnitaire,
                    Remise     = c.Remise,
                    MontantTotal = c.MontantTotal,
                    Date       = c.DateCredit,
                    ClientId   = c.ClientID,
                    ProduitId  = c.ProduitID,
                    ChequeReference = c.ChequeReference,
                    HasCheque  = !string.IsNullOrWhiteSpace(c.ChequeReference),
                    Note       = c.Note
                });

            // Populate existing frais
            foreach (var f in tFrais.Result)
                Transactions.Add(new VoyageTransactionItem
                {
                    Id         = f.FraisVoyageId,
                    IsExisting = true,
                    Type       = "Frais",
                    Partie     = f.Type ?? string.Empty,
                    MontantTotal = f.Montant,
                    FraisType  = f.Type,
                    Description = f.Description
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EditVoyageVM] LoadLookups error: {ex.Message}");
            }
            finally
            {
                // Always recalculate on whatever data was successfully loaded.
                RecalculateStockRestant();
                RecalculateFraisTotal();
                IsLoading = false;
            }
    }

    // ── Computation ───────────────────────────────────────────────────────────
    private void RecomputeVente()
    {
        if (VenteQuantite is null || VentePrix is null) { VenteTotal = null; return; }
        var r = VenteRemise ?? 0m;
        VenteTotal = Math.Round((decimal)VenteQuantite.Value * VentePrix.Value * (1 - r / 100m), 2);
    }

    private void RecomputeCredit()
    {
        if (CreditQuantite is null || CreditPrix is null) { CreditTotal = null; return; }
        var r = CreditRemise ?? 0m;
        CreditTotal = Math.Round((decimal)CreditQuantite.Value * CreditPrix.Value * (1 - r / 100m), 2);
    }

    /// <summary>Rebuilds StockRestantItems from StockItems, then applies all transactions.</summary>
    private void RecalculateStockRestant()
    {
        StockRestantItems.Clear();

        foreach (var s in StockItems)
        {
            if (s.ProduitId is null) continue;
            StockRestantItems.Add(new StockRestantItem
            {
                ProduitId        = s.ProduitId.Value,
                ProduitNom       = s.ProduitNom,
                QuantiteInitiale = s.Quantite ?? 0,
                QuantiteRestante = s.Quantite ?? 0
            });
        }

        foreach (var t in Transactions)
        {
            if (t.ProduitId is null || t.Quantite is null) continue;
            var row = StockRestantItems.FirstOrDefault(r => r.ProduitId == t.ProduitId.Value);
            if (row is null) continue;

            row.QuantiteRestante += t.Type switch
            {
                "Vente"  => -t.Quantite.Value,
                "Crédit" => -t.Quantite.Value,
                "Achat"  => +t.Quantite.Value,
                _        => 0
            };
        }
    }

    /// <summary>Recomputes FraisTotal from all Frais transactions.</summary>
    private void RecalculateFraisTotal()
    {
        FraisTotal = Transactions
            .Where(t => t.Type == "Frais")
            .Sum(t => t.MontantTotal ?? 0m);
    }

    // ── Edit-in-place: begin ──────────────────────────────────────────────────
    /// <summary>
    /// Called when user clicks the Edit (pencil) button on a stock row.
    /// Pre-fills the stock form and flags edit mode.
    /// The code-behind will ensure the Stock panel is shown.
    /// </summary>
    public void BeginEditStock(VoyageStockItem? item)
    {
        if (item is null) return;
        _editingStockItem = item;
        IsEditingStock    = true;

        StockProduit  = Produits.FirstOrDefault(p => p.ProduitId == item.ProduitId);
        StockQuantite = item.Quantite;
    }

    /// <summary>
    /// Called when user clicks the Edit (pencil) button on a transaction row.
    /// Pre-fills the matching tab form and flags edit mode.
    /// Returns the tab index (0=Vente, 1=Crédit, 2=Achat, 3=Frais) so code-behind can switch.
    /// </summary>
    public int BeginEditTransaction(VoyageTransactionItem? item)
    {
        if (item is null) return 0;
        _editingTransactionItem = item;
        _isEditingTransaction   = true;
        IsEditingTx             = true;

        switch (item.Type)
        {
            case "Vente":
                VenteClient   = Clients.FirstOrDefault(c => c.ClientId == item.ClientId);
                VenteProduit  = Produits.FirstOrDefault(p => p.ProduitId == item.ProduitId);
                VenteQuantite = item.Quantite;
                VentePrix     = item.PrixUnitaire;
                VenteRemise   = item.Remise;
                VenteTotal    = item.MontantTotal;
                VentePayment  = item.PaymentMethod;
                VenteNote     = item.Note;
                VenteDate     = item.Date ?? DateOnly.FromDateTime(DateTime.Today);
                _isEditingTransaction = false;
                return 0;

            case "Crédit":
                CreditClient   = Clients.FirstOrDefault(c => c.ClientId == item.ClientId);
                CreditProduit  = Produits.FirstOrDefault(p => p.ProduitId == item.ProduitId);
                CreditQuantite = item.Quantite;
                CreditPrix     = item.PrixUnitaire;
                CreditRemise   = item.Remise;
                CreditTotal    = item.MontantTotal;
                CreditChequeRef = item.ChequeReference;
                CreditNote     = item.Note;
                CreditDate     = item.Date ?? DateOnly.FromDateTime(DateTime.Today);
                _isEditingTransaction = false;
                return 1;

            case "Achat":
                AchatFournisseur = Fournisseurs.FirstOrDefault(f => f.FournisseurId == item.FournisseurId);
                AchatProduit     = Produits.FirstOrDefault(p => p.ProduitId == item.ProduitId);
                AchatQuantite    = item.Quantite;
                AchatPrix        = item.PrixUnitaire;
                AchatDefectueux  = item.LivraisonDefectueux;
                AchatDescription = item.Description;
                AchatDate        = item.Date ?? DateOnly.FromDateTime(DateTime.Today);
                _isEditingTransaction = false;
                return 2;

            case "Frais":
                FraisType        = item.FraisType;
                FraisMontant     = item.MontantTotal;
                FraisDescription = item.Description;
                _isEditingTransaction = false;
                return 3;
        }
        _isEditingTransaction = false;
        return 0;
    }

    // ── Stock add / update ────────────────────────────────────────────────────
    private async void AddOrUpdateStock()
    {
        if (StockProduit is null || StockQuantite is null or <= 0) return;

        if (IsEditingStock && _editingStockItem is not null)
        {
            // Update existing
            var item = _editingStockItem;
            item.ProduitId  = StockProduit.ProduitId;
            item.ProduitNom = StockProduit.Description ?? StockProduit.ProduitId.ToString()!;
            item.Quantite   = StockQuantite;

            if (item.IsExisting)
            {
                await _stockService.UpdateAsync(new StockVoyageDto
                {
                    StockVoyageId = item.Id,
                    VoyageId      = _voyageId,
                    ProduitId     = item.ProduitId,
                    Quantite      = item.Quantite
                });
            }

            // Refresh display
            var idx = StockItems.IndexOf(item);
            if (idx >= 0) { StockItems.RemoveAt(idx); StockItems.Insert(idx, item); }

            CancelStockEdit();
        }
        else
        {
            // Add new — will be saved on voyage header Save? No, immediately POST for edit dialog
            var dto = await _stockService.CreateAsync(new StockVoyageDto
            {
                VoyageId  = _voyageId,
                ProduitId = StockProduit.ProduitId,
                Quantite  = StockQuantite
            });

            StockItems.Add(new VoyageStockItem
            {
                Id         = dto?.StockVoyageId ?? 0,
                IsExisting = dto is not null,
                ProduitId  = StockProduit.ProduitId,
                ProduitNom = StockProduit.Description ?? StockProduit.ProduitId.ToString()!,
                Quantite   = StockQuantite
            });

            StockProduit  = null;
            StockQuantite = null;
        }
        RecalculateStockRestant();
    }

    private void CancelStockEdit()
    {
        _editingStockItem = null;
        IsEditingStock    = false;
        StockProduit      = null;
        StockQuantite     = null;
    }

    private async Task RemoveStockAsync(VoyageStockItem? item)
    {
        if (item is null) return;
        if (item.IsExisting) await _stockService.DeleteAsync(item.Id);
        StockItems.Remove(item);
        if (_editingStockItem == item) CancelStockEdit();
        RecalculateStockRestant();
    }

    // ── Transaction add / update ──────────────────────────────────────────────
    private async Task AddOrUpdateVenteAsync()
    {
        if (VenteClient is null || VenteProduit is null || VenteQuantite is null or <= 0) return;

        if (IsEditingTx && _editingTransactionItem?.Type == "Vente")
        {
            var item = _editingTransactionItem;
            item.Partie      = VenteClient.Nom ?? string.Empty;
            item.ProduitNom  = VenteProduit.Description;
            item.Quantite    = VenteQuantite;
            item.PrixUnitaire = VentePrix;
            item.Remise      = VenteRemise;
            item.MontantTotal = VenteTotal;
            item.Date        = VenteDate;
            item.ClientId    = VenteClient.ClientId;
            item.ProduitId   = VenteProduit.ProduitId;
            item.PaymentMethod = VentePayment;
            item.Note        = NullIfBlank(VenteNote);

            if (item.IsExisting)
            {
                await _venteService.UpdateVenteAsync(new VenteDto
                {
                    VenteId      = item.Id,
                    NumeroVente  = item.NumeroRef,
                    DateVente    = item.Date ?? DateOnly.FromDateTime(DateTime.Today),
                    VoyageID     = _voyageId,
                    ClientID     = item.ClientId,
                    ProduitID    = item.ProduitId,
                    Quantite     = item.Quantite,
                    PrixUnitaire = item.PrixUnitaire,
                    Remise       = item.Remise,
                    MontantTotal = item.MontantTotal,
                    PaymentMethod = item.PaymentMethod,
                    Note         = item.Note,
                    AjoutePar    = item.AjoutePar,
                    DateCreation = item.DateCreation
                });
            }

            RefreshTransactionRow(item);
            CancelTransactionEdit();
        }
        else
        {
            var dto = await _venteService.CreateVenteAsync(new VenteDto
            {
                NumeroVente  = $"V-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}",
                DateVente    = VenteDate,
                VoyageID     = _voyageId,
                ClientID     = VenteClient.ClientId,
                ProduitID    = VenteProduit.ProduitId,
                Quantite     = VenteQuantite,
                PrixUnitaire = VentePrix,
                Remise       = VenteRemise,
                MontantTotal = VenteTotal,
                PaymentMethod = VentePayment,
                Note         = NullIfBlank(VenteNote)
            });

            Transactions.Add(new VoyageTransactionItem
            {
                Id         = dto?.VenteId ?? 0,
                IsExisting = dto is not null,
                NumeroRef  = dto?.NumeroVente,
                AjoutePar  = dto?.AjoutePar,
                DateCreation = dto?.DateCreation,
                Type       = "Vente",
                Partie     = VenteClient.Nom ?? string.Empty,
                ProduitNom = VenteProduit.Description,
                Quantite   = VenteQuantite,
                PrixUnitaire = VentePrix,
                Remise     = VenteRemise,
                MontantTotal = VenteTotal,
                Date       = VenteDate,
                ClientId   = VenteClient.ClientId,
                ProduitId  = VenteProduit.ProduitId,
                PaymentMethod = VentePayment,
                Note       = NullIfBlank(VenteNote)
            });

            ResetVenteForm();
        }
        RecalculateStockRestant();
    }

    private async Task AddOrUpdateCreditAsync()
    {
        if (CreditClient is null || CreditProduit is null || CreditQuantite is null or <= 0) return;
        var hasChq = CreditHasCheque;

        if (IsEditingTx && _editingTransactionItem?.Type == "Crédit")
        {
            var item = _editingTransactionItem;
            item.Partie       = CreditClient.Nom ?? string.Empty;
            item.ProduitNom   = CreditProduit.Description;
            item.Quantite     = CreditQuantite;
            item.PrixUnitaire = CreditPrix;
            item.Remise       = CreditRemise;
            item.MontantTotal = CreditTotal;
            item.Date         = CreditDate;
            item.ClientId     = CreditClient.ClientId;
            item.ProduitId    = CreditProduit.ProduitId;
            item.ChequeReference = hasChq ? NullIfBlank(CreditChequeRef) : null;
            item.HasCheque    = hasChq;
            item.Note         = NullIfBlank(CreditNote);

            if (item.IsExisting)
            {
                await _creditService.UpdateCreditAsync(new CreditDto
                {
                    CreditId        = item.Id,
                    NumeroCredit    = item.NumeroRef,
                    DateCredit      = item.Date ?? DateOnly.FromDateTime(DateTime.Today),
                    VoyageID        = _voyageId,
                    ClientID        = item.ClientId,
                    ProduitID       = item.ProduitId,
                    Quantite        = item.Quantite,
                    PrixUnitaire    = item.PrixUnitaire,
                    Remise          = item.Remise,
                    MontantTotal    = item.MontantTotal,
                    ChequeReference = item.ChequeReference,
                    StatutCheque    = hasChq ? "Détenu" : null,
                    Note            = item.Note,
                    AjoutePar       = item.AjoutePar,
                    DateCreation    = item.DateCreation
                });
            }

            RefreshTransactionRow(item);
            CancelTransactionEdit();
        }
        else
        {
            var dto = await _creditService.CreateCreditAsync(new CreditDto
            {
                NumeroCredit    = $"C-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}",
                DateCredit      = CreditDate,
                VoyageID        = _voyageId,
                ClientID        = CreditClient.ClientId,
                ProduitID       = CreditProduit.ProduitId,
                Quantite        = CreditQuantite,
                PrixUnitaire    = CreditPrix,
                Remise          = CreditRemise,
                MontantTotal    = CreditTotal,
                Statut          = "Impayé",
                ChequeReference = hasChq ? NullIfBlank(CreditChequeRef) : null,
                StatutCheque    = hasChq ? "Détenu" : null,
                Note            = NullIfBlank(CreditNote)
            });

            Transactions.Add(new VoyageTransactionItem
            {
                Id         = dto?.CreditId ?? 0,
                IsExisting = dto is not null,
                NumeroRef  = dto?.NumeroCredit,
                AjoutePar  = dto?.AjoutePar,
                DateCreation = dto?.DateCreation,
                Type       = "Crédit",
                Partie     = CreditClient.Nom ?? string.Empty,
                ProduitNom = CreditProduit.Description,
                Quantite   = CreditQuantite,
                PrixUnitaire = CreditPrix,
                Remise     = CreditRemise,
                MontantTotal = CreditTotal,
                Date       = CreditDate,
                HasCheque  = hasChq,
                ClientId   = CreditClient.ClientId,
                ProduitId  = CreditProduit.ProduitId,
                ChequeReference = hasChq ? NullIfBlank(CreditChequeRef) : null,
                Note       = NullIfBlank(CreditNote)
            });

            ResetCreditForm();
        }
        RecalculateStockRestant();
    }

    private async Task AddOrUpdateAchatAsync()
    {
        if (AchatFournisseur is null || AchatProduit is null || AchatQuantite is null or <= 0) return;

        if (IsEditingTx && _editingTransactionItem?.Type == "Achat")
        {
            var item = _editingTransactionItem;
            item.Partie      = AchatFournisseur.DisplayName ?? string.Empty;
            item.ProduitNom  = AchatProduit.Description;
            item.Quantite    = AchatQuantite;
            item.PrixUnitaire = AchatPrix;
            item.MontantTotal = AchatCout;
            item.Date        = AchatDate;
            item.FournisseurId = AchatFournisseur.FournisseurId;
            item.ProduitId   = AchatProduit.ProduitId;
            item.LivraisonDefectueux = AchatDefectueux;
            item.Description = NullIfBlank(AchatDescription);

            if (item.IsExisting)
            {
                await _achatService.UpdateAchatAsync(new AchatDto
                {
                    AchatId              = item.Id,
                    Numero               = item.NumeroRef,
                    Date                 = item.Date ?? DateOnly.FromDateTime(DateTime.Today),
                    FournisseurID        = item.FournisseurId,
                    ProduitID            = item.ProduitId,
                    Quantite             = item.Quantite,
                    PrixAchatUnitaire    = item.PrixUnitaire,
                    Cout                 = item.MontantTotal,
                    LivraisonDefectueuse = item.LivraisonDefectueux,
                    Description          = item.Description,
                    AjoutePar            = item.AjoutePar,
                    DateCreation         = item.DateCreation
                });
            }

            RefreshTransactionRow(item);
            CancelTransactionEdit();
        }
        else
        {
            var dto = await _achatService.CreateAchatAsync(new AchatDto
            {
                Numero               = $"A-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}",
                Date                 = AchatDate,
                FournisseurID        = AchatFournisseur.FournisseurId,
                ProduitID            = AchatProduit.ProduitId,
                Quantite             = AchatQuantite,
                PrixAchatUnitaire    = AchatPrix,
                Cout                 = AchatCout,
                LivraisonDefectueuse = AchatDefectueux,
                Description          = NullIfBlank(AchatDescription)
            });

            Transactions.Add(new VoyageTransactionItem
            {
                Id         = dto?.AchatId ?? 0,
                IsExisting = dto is not null,
                NumeroRef  = dto?.Numero,
                AjoutePar  = dto?.AjoutePar,
                DateCreation = dto?.DateCreation,
                Type       = "Achat",
                Partie     = AchatFournisseur.DisplayName ?? string.Empty,
                ProduitNom = AchatProduit.Description,
                Quantite   = AchatQuantite,
                PrixUnitaire = AchatPrix,
                MontantTotal = AchatCout,
                Date       = AchatDate,
                FournisseurId = AchatFournisseur.FournisseurId,
                ProduitId  = AchatProduit.ProduitId,
                LivraisonDefectueux = AchatDefectueux,
                Description = NullIfBlank(AchatDescription)
            });

            ResetAchatForm();
        }
        RecalculateStockRestant();
    }

    private async Task AddOrUpdateFraisAsync()
    {
        if (string.IsNullOrWhiteSpace(FraisType) || FraisMontant is null or <= 0) return;

        if (IsEditingTx && _editingTransactionItem?.Type == "Frais")
        {
            var item = _editingTransactionItem;
            item.Partie      = FraisType!;
            item.FraisType   = FraisType;
            item.MontantTotal = FraisMontant;
            item.Description = NullIfBlank(FraisDescription);

            if (item.IsExisting)
            {
                await _fraisService.UpdateAsync(new FraisVoyageDto
                {
                    FraisVoyageId = item.Id,
                    VoyageId      = _voyageId,
                    Type          = item.FraisType,
                    Montant       = item.MontantTotal,
                    Description   = item.Description
                });
            }

            RefreshTransactionRow(item);
            CancelTransactionEdit();
        }
        else
        {
            var dto = await _fraisService.CreateAsync(new FraisVoyageDto
            {
                VoyageId    = _voyageId,
                Type        = FraisType,
                Montant     = FraisMontant,
                Description = NullIfBlank(FraisDescription)
            });

            Transactions.Add(new VoyageTransactionItem
            {
                Id         = dto?.FraisVoyageId ?? 0,
                IsExisting = dto is not null,
                Type       = "Frais",
                Partie     = FraisType!,
                MontantTotal = FraisMontant,
                FraisType  = FraisType,
                Description = NullIfBlank(FraisDescription)
            });

            ResetFraisForm();
        }
        RecalculateFraisTotal();
    }

    private async Task RemoveTransactionAsync(VoyageTransactionItem? item)
    {
        if (item is null) return;
        if (item.IsExisting)
        {
            switch (item.Type)
            {
                case "Vente":  await _venteService.DeleteVenteAsync(item.Id);    break;
                case "Crédit": await _creditService.DeleteCreditAsync(item.Id);  break;
                case "Achat":  await _achatService.DeleteAchatAsync(item.Id);    break;
                case "Frais":  await _fraisService.DeleteAsync(item.Id);         break;
            }
        }
        Transactions.Remove(item);
        if (_editingTransactionItem == item) CancelTransactionEdit();
        RecalculateStockRestant();
        RecalculateFraisTotal();
    }

    private void CancelTransactionEdit()
    {
        _editingTransactionItem = null;
        IsEditingTx             = false;
        ResetVenteForm();
        ResetCreditForm();
        ResetAchatForm();
        ResetFraisForm();
    }

    private void RefreshTransactionRow(VoyageTransactionItem item)
    {
        var idx = Transactions.IndexOf(item);
        if (idx >= 0) { Transactions.RemoveAt(idx); Transactions.Insert(idx, item); }
    }

    // ── Form resets ───────────────────────────────────────────────────────────
    private void ResetVenteForm()
    {
        VenteClient = null; VenteProduit = null; VenteQuantite = null;
        VentePrix   = null; VenteRemise  = null; VenteTotal    = null;
        VentePayment = null; VenteNote   = null;
        VenteDate = DateOnly.FromDateTime(DateTime.Today);
    }

    private void ResetCreditForm()
    {
        CreditClient = null; CreditProduit = null; CreditQuantite = null;
        CreditPrix   = null; CreditRemise  = null; CreditTotal    = null;
        CreditChequeRef = null; CreditNote = null;
        CreditDate = DateOnly.FromDateTime(DateTime.Today);
    }

    private void ResetAchatForm()
    {
        AchatFournisseur = null; AchatProduit = null; AchatQuantite = null;
        AchatPrix = null; AchatDefectueux = false; AchatDescription = null;
        AchatDate = DateOnly.FromDateTime(DateTime.Today);
    }

    private void ResetFraisForm()
    {
        FraisType = null; FraisMontant = null; FraisDescription = null;
    }

    // ── Save voyage header ────────────────────────────────────────────────────
    private async Task SaveVoyageHeaderAsync()
    {
        ErrorMessage = null;
        if (SelectedChauffeur is null)
        {
            ErrorMessage = LanguageManager.Instance["EditVoyageValidationChauffeur"];
            return;
        }

        IsSaving = true;
        try
        {
            var ok = await _voyageService.UpdateVoyageAsync(new VoyageDto
            {
                VoyageId          = _voyageId,
                CamionId          = SelectedCamion?.CamionId,
                ChauffeurId       = SelectedChauffeur.ChauffeurId,
                CiterneId         = SelectedCiterne?.CiterneId,
                LieuDepart        = NullIfBlank(LieuDepart),
                LieuTerminal      = NullIfBlank(LieuTerminal),
                DateDepart        = DateDepart,
                DateFinal         = DateFinal,
                KilometrageDepart = KilometrageDepart,
                KilometrageFinal  = KilometrageFinal,
                Statut            = Statut,
                AjoutePar         = _originalAjoutePar,
                DateCreation      = _originalDateCreation
            });

            if (!ok) { ErrorMessage = LanguageManager.Instance["EditVoyageSaveError"]; return; }

            // AutomationHook: notify automations that voyage status may have changed
            if (SelectedCamion is not null)
                await _automationRunner.RunAsync(
                    AutomationTrigger.VoyageStatusChanged,
                    new VoyageStatusContext(
                        SelectedCamion.CamionId,
                        Statut,
                        KilometrageFinal.HasValue ? (int)KilometrageFinal.Value : null));

            Saved = true;
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

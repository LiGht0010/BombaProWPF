using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;

namespace FourniPro.ViewModels;

// ── Local staging POCO (not persisted until Save) ─────────────────────────────

/// <summary>A staged transaction row added inside the voyage create dialog.</summary>
public class VoyageTransactionItem
{
    public string Type            { get; set; } = string.Empty;  // Vente | Crédit | Achat | Frais
    public string Partie          { get; set; } = string.Empty;  // Client / Fournisseur name
    public string? ProduitNom     { get; set; }
    public int?    Quantite       { get; set; }
    public decimal? PrixUnitaire  { get; set; }
    public decimal? Remise        { get; set; }
    public decimal? MontantTotal  { get; set; }
    public bool    HasCheque      { get; set; }

    // raw form data kept for POST
    public int?    ClientId       { get; set; }
    public int?    FournisseurId  { get; set; }
    public int?    ProduitId      { get; set; }
    public string? PaymentMethod  { get; set; }
    public string? Note           { get; set; }
    public string? ChequeReference { get; set; }
    public bool    LivraisonDefectueuse { get; set; }
    public string? Description    { get; set; }
    public string? FraisType      { get; set; }
}

/// <summary>A staged stock row (produit + quantite) for the voyage.</summary>
public class VoyageStockItem
{
    public int?   ProduitId  { get; set; }
    public string ProduitNom { get; set; } = string.Empty;
    public int?   Quantite   { get; set; }
}

// ── ViewModel ─────────────────────────────────────────────────────────────────

/// <summary>ViewModel for NouveauVoyageDialog.</summary>
public class NouveauVoyageViewModel : ObservableObject
{
    // ── Services ──────────────────────────────────────────────────────────────

    private readonly VoyageService      _voyageService  = new();
    private readonly StockVoyageService _stockService   = new();
    private readonly FraisVoyageService _fraisService   = new();
    private readonly VenteService       _venteService   = new();
    private readonly CreditService      _creditService  = new();
    private readonly AchatService       _achatService   = new();
    private readonly ProduitService     _produitService = new();
    private readonly CamionService      _camionService  = new();
    private readonly ChauffeurService   _chauffeurService = new();
    private readonly CiterneService     _citerneService   = new();
    private readonly ClientService      _clientService    = new();
    private readonly FournisseurService _fournisseurService = new();

    // ── Voyage core fields ────────────────────────────────────────────────────

    private CamionDto? _selectedCamion;
    public CamionDto? SelectedCamion
    {
        get => _selectedCamion;
        set => SetProperty(ref _selectedCamion, value);
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

    private DateTime _dateDepart = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
    public DateTime DateDepart
    {
        get => _dateDepart;
        set => SetProperty(ref _dateDepart, DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }

    private decimal? _kilometrageDepart;
    public decimal? KilometrageDepart
    {
        get => _kilometrageDepart;
        set => SetProperty(ref _kilometrageDepart, value);
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
            if (SetProperty(ref _venteProduit, value) && value is not null)
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
            if (SetProperty(ref _creditProduit, value) && value is not null)
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
            if (SetProperty(ref _achatProduit, value) && value is not null)
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

    // ── Staged collections ────────────────────────────────────────────────────

    public ObservableCollection<VoyageStockItem>       StockItems   { get; } = [];
    public ObservableCollection<VoyageTransactionItem> Transactions { get; } = [];

    // ── Lookup lists ──────────────────────────────────────────────────────────

    public ObservableCollection<CamionDto>      Camions      { get; } = [];
    public ObservableCollection<ChauffeurDto>   Chauffeurs   { get; } = [];
    public ObservableCollection<CiterneDto>     Citernes     { get; } = [];
    public ObservableCollection<ClientDto>      Clients      { get; } = [];
    public ObservableCollection<ProduitDto>     Produits     { get; } = [];
    public ObservableCollection<FournisseurDto> Fournisseurs { get; } = [];

    public IReadOnlyList<string> PaymentMethods { get; } = ["TPE", "Virement", "Especes"];
    public IReadOnlyList<string> FraisTypes     { get; } = ["Carburant", "Péage", "Réparation", "Autre"];

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

    public IRelayCommand           AddStockCommand       { get; }
    public IRelayCommand<VoyageStockItem>       RemoveStockCommand    { get; }
    public IRelayCommand           AddVenteCommand       { get; }
    public IRelayCommand           AddCreditCommand      { get; }
    public IRelayCommand           AddAchatCommand       { get; }
    public IRelayCommand           AddFraisCommand       { get; }
    public IRelayCommand<VoyageTransactionItem> RemoveTransactionCommand { get; }
    public IAsyncRelayCommand      SaveCommand           { get; }

    public NouveauVoyageViewModel()
    {
        AddStockCommand          = new RelayCommand(AddStock);
        RemoveStockCommand       = new RelayCommand<VoyageStockItem>(RemoveStock);
        AddVenteCommand          = new RelayCommand(AddVente);
        AddCreditCommand         = new RelayCommand(AddCredit);
        AddAchatCommand          = new RelayCommand(AddAchat);
        AddFraisCommand          = new RelayCommand(AddFrais);
        RemoveTransactionCommand = new RelayCommand<VoyageTransactionItem>(RemoveTransaction);
        SaveCommand              = new AsyncRelayCommand(SaveAsync);
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public async Task LoadLookupsAsync()
    {
        IsLoading = true;
        try
        {
            var camions      = await _camionService.GetAllCamionsAsync();
            var chauffeurs   = await _chauffeurService.GetAllChauffeursAsync();
            var citernes     = await _citerneService.GetAllCiternesAsync();
            var clients      = await _clientService.GetAllClientsAsync();
            var produits     = await _produitService.GetAllProduitsAsync();
            var fournisseurs = await _fournisseurService.GetAllFournisseursAsync();

            foreach (var x in camions)      Camions.Add(x);
            foreach (var x in chauffeurs)   Chauffeurs.Add(x);
            foreach (var x in citernes)     Citernes.Add(x);
            foreach (var x in clients)      Clients.Add(x);
            foreach (var x in produits)     Produits.Add(x);
            foreach (var x in fournisseurs) Fournisseurs.Add(x);
        }
        finally { IsLoading = false; }
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

    // ── Add/Remove staged rows ────────────────────────────────────────────────

    private void AddStock()
    {
        if (StockProduit is null || StockQuantite is null or <= 0) return;
        StockItems.Add(new VoyageStockItem
        {
            ProduitId  = StockProduit.ProduitId,
            ProduitNom = StockProduit.Description ?? StockProduit.ProduitId.ToString(),
            Quantite   = StockQuantite
        });
        StockProduit  = null;
        StockQuantite = null;
    }

    private void RemoveStock(VoyageStockItem? item)
    {
        if (item is not null) StockItems.Remove(item);
    }

    private void AddVente()
    {
        if (VenteClient is null || VenteProduit is null || VenteQuantite is null or <= 0) return;
        Transactions.Add(new VoyageTransactionItem
        {
            Type          = "Vente",
            Partie        = VenteClient.Nom,
            ProduitNom    = VenteProduit.Description,
            Quantite      = VenteQuantite,
            PrixUnitaire  = VentePrix,
            Remise        = VenteRemise,
            MontantTotal  = VenteTotal,
            ClientId      = VenteClient.ClientId,
            ProduitId     = VenteProduit.ProduitId,
            PaymentMethod = VentePayment,
            Note          = NullIfBlank(VenteNote)
        });
        ResetVenteForm();
    }

    private void AddCredit()
    {
        if (CreditClient is null || CreditProduit is null || CreditQuantite is null or <= 0) return;
        var hasChq = CreditHasCheque;
        Transactions.Add(new VoyageTransactionItem
        {
            Type           = "Crédit",
            Partie         = CreditClient.Nom,
            ProduitNom     = CreditProduit.Description,
            Quantite       = CreditQuantite,
            PrixUnitaire   = CreditPrix,
            Remise         = CreditRemise,
            MontantTotal   = CreditTotal,
            HasCheque      = hasChq,
            ClientId       = CreditClient.ClientId,
            ProduitId      = CreditProduit.ProduitId,
            ChequeReference = hasChq ? NullIfBlank(CreditChequeRef) : null,
            Note           = NullIfBlank(CreditNote)
        });
        ResetCreditForm();
    }

    private void AddAchat()
    {
        if (AchatFournisseur is null || AchatProduit is null || AchatQuantite is null or <= 0) return;
        Transactions.Add(new VoyageTransactionItem
        {
            Type                = "Achat",
            Partie              = AchatFournisseur.DisplayName,
            ProduitNom          = AchatProduit.Description,
            Quantite            = AchatQuantite,
            PrixUnitaire        = AchatPrix,
            MontantTotal        = AchatCout,
            FournisseurId       = AchatFournisseur.FournisseurId,
            ProduitId           = AchatProduit.ProduitId,
            LivraisonDefectueuse = AchatDefectueux,
            Description         = NullIfBlank(AchatDescription)
        });
        ResetAchatForm();
    }

    private void AddFrais()
    {
        if (string.IsNullOrWhiteSpace(FraisType) || FraisMontant is null or <= 0) return;
        Transactions.Add(new VoyageTransactionItem
        {
            Type         = "Frais",
            Partie       = FraisType!,
            MontantTotal = FraisMontant,
            FraisType    = FraisType,
            Description  = NullIfBlank(FraisDescription)
        });
        ResetFraisForm();
    }

    private void RemoveTransaction(VoyageTransactionItem? item)
    {
        if (item is not null) Transactions.Remove(item);
    }

    // ── Form resets ───────────────────────────────────────────────────────────

    private void ResetVenteForm()
    {
        VenteClient = null; VenteProduit = null; VenteQuantite = null;
        VentePrix   = null; VenteRemise  = null; VenteTotal    = null;
        VentePayment = null; VenteNote   = null;
    }

    private void ResetCreditForm()
    {
        CreditClient = null; CreditProduit = null; CreditQuantite = null;
        CreditPrix   = null; CreditRemise  = null; CreditTotal    = null;
        CreditChequeRef = null; CreditNote = null;
    }

    private void ResetAchatForm()
    {
        AchatFournisseur = null; AchatProduit = null; AchatQuantite = null;
        AchatPrix = null; AchatDefectueux = false; AchatDescription = null;
    }

    private void ResetFraisForm()
    {
        FraisType = null; FraisMontant = null; FraisDescription = null;
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (SelectedChauffeur is null)
        {
            ErrorMessage = LanguageManager.Instance["NouveauVoyageValidationChauffeur"];
            return;
        }

        IsSaving = true;
        try
        {
            // 1. Create the voyage header
            var voyageDto = new VoyageDto
            {
                CamionId          = SelectedCamion?.CamionId,
                ChauffeurId       = SelectedChauffeur.ChauffeurId,
                CiterneId         = SelectedCiterne?.CiterneId,
                DateDepart        = DateDepart,
                LieuDepart        = NullIfBlank(LieuDepart),
                LieuTerminal      = NullIfBlank(LieuTerminal),
                KilometrageDepart = KilometrageDepart,
                Statut            = "InProgress"
            };

            var createdVoyage = await _voyageService.CreateVoyageAsync(voyageDto);
            if (createdVoyage is null)
            {
                ErrorMessage = LanguageManager.Instance["NouveauVoyageSaveError"];
                return;
            }

            int voyageId = createdVoyage.VoyageId;

            // 2. Stock rows
            foreach (var s in StockItems)
                await _stockService.CreateAsync(new StockVoyageDto
                {
                    VoyageId  = voyageId,
                    ProduitId = s.ProduitId,
                    Quantite  = s.Quantite
                });

            // 3. Transactions
            var today    = DateOnly.FromDateTime(DateTime.Today);
            var num      = DateTime.Now;

            foreach (var t in Transactions)
            {
                switch (t.Type)
                {
                    case "Vente":
                        await _venteService.CreateVenteAsync(new VenteDto
                        {
                            NumeroVente   = $"V-{num:yyyy-MM-dd-HH-mm-ss}",
                            DateVente     = today,
                            VoyageID      = voyageId,
                            ClientID      = t.ClientId,
                            ProduitID     = t.ProduitId,
                            Quantite      = t.Quantite,
                            PrixUnitaire  = t.PrixUnitaire,
                            Remise        = t.Remise,
                            MontantTotal  = t.MontantTotal,
                            PaymentMethod = t.PaymentMethod,
                            Note          = t.Note
                        });
                        break;

                    case "Crédit":
                        await _creditService.CreateCreditAsync(new CreditDto
                        {
                            NumeroCredit    = $"C-{num:yyyy-MM-dd-HH-mm-ss}",
                            DateCredit      = today,
                            VoyageID        = voyageId,
                            ClientID        = t.ClientId,
                            ProduitID       = t.ProduitId,
                            Quantite        = t.Quantite,
                            PrixUnitaire    = t.PrixUnitaire,
                            Remise          = t.Remise,
                            MontantTotal    = t.MontantTotal,
                            Statut          = "Impayé",
                            ChequeReference = t.ChequeReference,
                            StatutCheque    = t.HasCheque ? "Détenu" : null,
                            Note            = t.Note
                        });
                        break;

                    case "Achat":
                        await _achatService.CreateAchatAsync(new AchatDto
                        {
                            Numero               = $"A-{num:yyyy-MM-dd-HH-mm-ss}",
                            Date                 = today,
                            FournisseurID        = t.FournisseurId,
                            ProduitID            = t.ProduitId,
                            Quantite             = t.Quantite,
                            PrixAchatUnitaire    = t.PrixUnitaire,
                            Cout                 = t.MontantTotal,
                            LivraisonDefectueuse = t.LivraisonDefectueuse,
                            Description          = t.Description
                        });
                        break;

                    case "Frais":
                        await _fraisService.CreateAsync(new FraisVoyageDto
                        {
                            VoyageId    = voyageId,
                            Type        = t.FraisType,
                            Montant     = t.MontantTotal,
                            Description = t.Description
                        });
                        break;
                }
            }

            Saved = true;
        }
        finally { IsSaving = false; }
    }

    private static string? NullIfBlank(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMax.ViewModels;

[QueryProperty(nameof(ClientId), "clientId")]
public partial class ClientCreditManagementViewModel : ObservableObject
{
    private readonly ClientService _clientService;
    private readonly CreditTransactionService _transactionService;
    private readonly ReglementCreditService _reglementService;
    private readonly BilanCreditService _bilanService;
    private readonly ProduitService _produitService;
    private readonly MoyensPaiementService _moyensPaiementService;
    
    // ============================
    // NEW SERVICES FOR FACTURES/BL
    // ============================
    private readonly FactureService _factureService;
    private readonly BonLivraisonService _blService;

    // ============================
    // OBSERVABLE PROPERTIES
    // ============================

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ClientDto? _currentClient;

    [ObservableProperty]
    private BilanCreditDto? _bilan;

    [ObservableProperty]
    private CreditTransactionDto? _selectedTransaction;

    [ObservableProperty]
    private ReglementCreditDto? _selectedReglement;

    [ObservableProperty]
    private int _selectedTabIndex;

    private int _clientId;
    
    /// <summary>
    /// Client ID received from navigation query parameter.
    /// </summary>
    public int ClientId
    {
        get => _clientId;
        set
        {
            if (SetProperty(ref _clientId, value) && value > 0)
            {
                _ = LoadClientByIdAsync(value);
            }
        }
    }

    // ============================
    // FACTURES/BL OBSERVABLE PROPERTIES
    // ============================

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Statistics - Factures
    [ObservableProperty]
    private int _totalFactures;

    [ObservableProperty]
    private decimal _totalMontantFactures;

    [ObservableProperty]
    private int _facturesPayees;

    [ObservableProperty]
    private int _facturesNonPayees;

    // Statistics - Bons de Livraison
    [ObservableProperty]
    private int _totalBonsLivraison;

    [ObservableProperty]
    private decimal _totalMontantBL;

    [ObservableProperty]
    private int _blFacturesCount;

    [ObservableProperty]
    private int _blNonFacturesCount;

    // Filter states for Factures
    [ObservableProperty]
    private string _factureFilterMode = "all"; // all, paid, unpaid

    [ObservableProperty]
    private bool _isFactureFilterAll = true;

    [ObservableProperty]
    private bool _isFactureFilterPaid;

    [ObservableProperty]
    private bool _isFactureFilterUnpaid;

    // Filter states for BL
    [ObservableProperty]
    private string _blFilterMode = "all"; // all, invoiced, notInvoiced

    [ObservableProperty]
    private bool _isBlFilterAll = true;

    [ObservableProperty]
    private bool _isBlFilterInvoiced;

    [ObservableProperty]
    private bool _isBlFilterNotInvoiced;

    // BL Selection Properties (for merge and facturation)
    [ObservableProperty]
    private int _selectedBLCount;

    [ObservableProperty]
    private decimal _selectedBLTotal;

    [ObservableProperty]
    private bool _canMergeBLs;

    [ObservableProperty]
    private bool _canCreateFactureFromBLs;

    // Facture Selection Properties (for merge)
    [ObservableProperty]
    private int _selectedFactureCount;

    [ObservableProperty]
    private decimal _selectedFactureTotal;

    [ObservableProperty]
    private bool _canMergeFactures;

    // ============================
    // CT (Credit Transaction) SELECTION PROPERTIES
    // ============================

    // CT Filter states
    [ObservableProperty]
    private string _ctFilterMode = "available"; // available, inBL, invoiced

    [ObservableProperty]
    private bool _isCTFilterAvailable = true;

    [ObservableProperty]
    private bool _isCTFilterInBL;

    [ObservableProperty]
    private bool _isCTFilterInvoiced;

    // CT Selection Properties (for BL/Facture creation)
    [ObservableProperty]
    private int _selectedCTCount;

    [ObservableProperty]
    private decimal _selectedCTTotal;

    [ObservableProperty]
    private bool _canCreateBLFromCTs;

    [ObservableProperty]
    private bool _canCreateFactureFromCTs;

    // ============================
    // COLLECTIONS
    // ============================

    public ObservableCollection<CreditTransactionDto> Transactions { get; } = [];
    public ObservableCollection<ReglementCreditDto> Reglements { get; } = [];
    public ObservableCollection<ProduitDto> Produits { get; } = [];
    public ObservableCollection<ServiceDto> Services { get; } = [];
    public ObservableCollection<MoyensPaiementDto> MoyensPaiement { get; } = [];
    
    // Factures/BL Collections
    public ObservableCollection<FactureDto> Factures { get; } = [];
    public ObservableCollection<BonLivraisonDto> BonsLivraison { get; } = [];

    // ============================
    // CONSTRUCTOR
    // ============================

    public ClientCreditManagementViewModel()
    {
        _clientService = new ClientService();
        _transactionService = new CreditTransactionService();
        _reglementService = new ReglementCreditService();
        _bilanService = new BilanCreditService();
        _produitService = new ProduitService();
        _moyensPaiementService = new MoyensPaiementService();
        
        // Initialize Factures/BL services
        _factureService = new FactureService();
        _blService = new BonLivraisonService();
    }

    // ============================
    // INITIALIZATION
    // ============================

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadProduitsAsync();
        await LoadServicesAsync();
        await LoadMoyensPaiementAsync();
    }

    /// <summary>
    /// Loads client data by ID (from query parameter).
    /// </summary>
    private async Task LoadClientByIdAsync(int clientId)
    {
        try
        {
            IsLoading = true;
            
            // Load supporting data first
            await InitializeAsync();
            
            // Load the client
            var client = await _clientService.GetClientByIdAsync(clientId);
            if (client != null)
            {
                CurrentClient = client;
                await LoadClientDataAsync();
            }
            else
            {
                Debug.WriteLine($"Client with ID {clientId} not found");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading client by ID: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ============================
    // LOAD METHODS
    // ============================

    private async Task LoadProduitsAsync()
    {
        try
        {
            var produits = await _produitService.GetAllProduitsAsync();
            Produits.Clear();
            foreach (var produit in produits)
            {
                Produits.Add(produit);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading produits: {ex.Message}");
        }
    }

    private async Task LoadServicesAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://localhost:7100/api/Services");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var services = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServiceDto>>(json) ?? [];
                Services.Clear();
                foreach (var service in services)
                {
                    Services.Add(service);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading services: {ex.Message}");
        }
    }

    private async Task LoadMoyensPaiementAsync()
    {
        try
        {
            var moyens = await _moyensPaiementService.GetAllAsync();
            MoyensPaiement.Clear();
            foreach (var moyen in moyens)
            {
                MoyensPaiement.Add(moyen);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading moyens de paiement: {ex.Message}");
        }
    }

    // ============================
    // CLIENT DATA LOADING
    // ============================

    [RelayCommand]
    public async Task LoadClientDataAsync()
    {
        if (CurrentClient == null) return;

        try
        {
            IsLoading = true;

            // Load all data in parallel (including Factures/BL)
            var bilanTask = _bilanService.GetByClientAsync(CurrentClient.ID);
            var transactionsTask = _transactionService.GetByClientAsync(CurrentClient.ID);
            var reglementsTask = _reglementService.GetByClientAsync(CurrentClient.ID);
            var facturesTask = _factureService.GetByClientAsync(CurrentClient.ID);
            var blsTask = _blService.GetByClientAsync(CurrentClient.ID);

            await Task.WhenAll(bilanTask, transactionsTask, reglementsTask, facturesTask, blsTask);

            Bilan = await bilanTask;

            Transactions.Clear();
            foreach (var transaction in await transactionsTask)
            {
                Transactions.Add(transaction);
            }

            Reglements.Clear();
            foreach (var reglement in await reglementsTask)
            {
                Reglements.Add(reglement);
            }

            // Load Factures with filters
            var factures = await facturesTask;
            Factures.Clear();
            foreach (var facture in factures.OrderByDescending(f => f.DateFacture))
            {
                facture.IsSelected = false;
                Factures.Add(facture);
            }
            ClearFactureSelection();
            CalculateFactureStatistics();

            // Load BLs with filters
            var bls = await blsTask;
            BonsLivraison.Clear();
            foreach (var bl in bls.OrderByDescending(b => b.DateBL))
            {
                bl.IsSelected = false;
                BonsLivraison.Add(bl);
            }
            ClearBLSelection();
            CalculateBLStatistics();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading client data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearClientData()
    {
        Bilan = null;
        Transactions.Clear();
        Reglements.Clear();
        Factures.Clear();
        BonsLivraison.Clear();
    }

    // ============================
    // REFRESH BILAN
    // ============================

    [RelayCommand]
    public async Task RefreshBilanAsync()
    {
        if (CurrentClient == null) return;

        try
        {
            IsLoading = true;
            Bilan = await _bilanService.RecalculateAsync(CurrentClient.ID);
            
            // Also refresh transactions and reglements
            await LoadClientDataAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing bilan: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ============================
    // TRANSACTION COMMANDS
    // ============================

    public async Task AddTransactionAsync(CreditTransactionDto transaction)
    {
        ArgumentNullException.ThrowIfNull(CurrentClient);

        try
        {
            IsLoading = true;
            transaction.ClientID = CurrentClient.ID;
            
            Debug.WriteLine($"Creating transaction for client {CurrentClient.ID}: {transaction.NumeroTransaction}");
            Debug.WriteLine($"Transaction data: ProduitID={transaction.ProduitID}, ServiceID={transaction.ServiceID}, Quantite={transaction.Quantite}, PrixTTC={transaction.PrixTTC}");
            
            var created = await _transactionService.CreateAsync(transaction);
            if (created != null)
            {
                Debug.WriteLine($"Transaction created successfully with ID: {created.CreditID}");
                Transactions.Insert(0, created);
                // Bilan is auto-updated by the service, but we reload to get fresh data
                Bilan = await _bilanService.GetByClientAsync(CurrentClient.ID);
            }
            else
            {
                throw new InvalidOperationException("La création de la transaction a échoué. Le serveur n'a pas retourné de données.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding transaction: {ex.Message}");
            throw; // Re-throw to let the caller handle it
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdateTransactionAsync(CreditTransactionDto transaction)
    {
        ArgumentNullException.ThrowIfNull(CurrentClient);

        try
        {
            IsLoading = true;
            var success = await _transactionService.UpdateAsync(transaction);
            if (success)
            {
                // Reload transactions to get updated data
                await LoadClientDataAsync();
            }
            else
            {
                throw new InvalidOperationException("La mise à jour de la transaction a échoué.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating transaction: {ex.Message}");
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeleteTransactionAsync(CreditTransactionDto? transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(CurrentClient);

        try
        {
            IsLoading = true;
            var success = await _transactionService.DeleteAsync(transaction.CreditID);
            if (success)
            {
                Transactions.Remove(transaction);
                Bilan = await _bilanService.GetByClientAsync(CurrentClient.ID);
            }
            else
            {
                throw new InvalidOperationException("La suppression de la transaction a échoué.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting transaction: {ex.Message}");
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ============================
    // REGLEMENT COMMANDS
    // ============================

    public async Task AddReglementAsync(ReglementCreditDto reglement)
    {
        ArgumentNullException.ThrowIfNull(CurrentClient);

        try
        {
            IsLoading = true;
            reglement.ClientID = CurrentClient.ID;
            
            var created = await _reglementService.CreateAsync(reglement);
            if (created != null)
            {
                Reglements.Insert(0, created);
                Bilan = await _bilanService.GetByClientAsync(CurrentClient.ID);
            }
            else
            {
                throw new InvalidOperationException("La création du règlement a échoué.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error adding reglement: {ex.Message}");
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdateReglementAsync(ReglementCreditDto reglement)
    {
        ArgumentNullException.ThrowIfNull(CurrentClient);

        try
        {
            IsLoading = true;
            var success = await _reglementService.UpdateAsync(reglement);
            if (success)
            {
                await LoadClientDataAsync();
            }
            else
            {
                throw new InvalidOperationException("La mise à jour du règlement a échoué.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating reglement: {ex.Message}");
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DeleteReglementAsync(ReglementCreditDto? reglement)
    {
        ArgumentNullException.ThrowIfNull(reglement);
        ArgumentNullException.ThrowIfNull(CurrentClient);

        try
        {
            IsLoading = true;
            var success = await _reglementService.DeleteAsync(reglement.ReglementID);
            if (success)
            {
                Reglements.Remove(reglement);
                Bilan = await _bilanService.GetByClientAsync(CurrentClient.ID);
            }
            else
            {
                throw new InvalidOperationException("La suppression du règlement a échoué.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting reglement: {ex.Message}");
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ============================
    // UTILITY METHODS
    // ============================

    public async Task<string> GenerateTransactionNumeroAsync()
    {
        return await _transactionService.GenerateNextNumeroAsync();
    }

    public ProduitDto? GetProduitById(int? id)
    {
        if (id == null) return null;
        return Produits.FirstOrDefault(p => p.ID == id);
    }

    public ServiceDto? GetServiceById(int? id)
    {
        if (id == null) return null;
        return Services.FirstOrDefault(s => s.ID == id);
    }

    public MoyensPaiementDto? GetMoyenPaiementById(int id)
    {
        return MoyensPaiement.FirstOrDefault(m => m.ID == id);
    }

    // ════════════════════════════════════════════════════════════════
    // CT (CREDIT TRANSACTION) FILTER COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task SetCTFilterAvailableAsync()
    {
        CtFilterMode = "available";
        IsCTFilterAvailable = true;
        IsCTFilterInBL = false;
        IsCTFilterInvoiced = false;
        await LoadTransactionsWithFilterAsync();
    }

    [RelayCommand]
    private async Task SetCTFilterInBLAsync()
    {
        CtFilterMode = "inBL";
        IsCTFilterAvailable = false;
        IsCTFilterInBL = true;
        IsCTFilterInvoiced = false;
        await LoadTransactionsWithFilterAsync();
    }

    [RelayCommand]
    private async Task SetCTFilterInvoicedAsync()
    {
        CtFilterMode = "invoiced";
        IsCTFilterAvailable = false;
        IsCTFilterInBL = false;
        IsCTFilterInvoiced = true;
        await LoadTransactionsWithFilterAsync();
    }

    /// <summary>
    /// Loads transactions with the current filter applied.
    /// </summary>
    private async Task LoadTransactionsWithFilterAsync()
    {
        if (CurrentClient == null) return;

        try
        {
            IsLoading = true;

            List<CreditTransactionDto> cts = CtFilterMode switch
            {
                "inBL" => await _transactionService.GetInBLByClientAsync(CurrentClient.ID),
                "invoiced" => await _transactionService.GetInvoicedByClientAsync(CurrentClient.ID),
                _ => await _transactionService.GetAvailableByClientAsync(CurrentClient.ID)
            };

            Transactions.Clear();
            foreach (var ct in cts.OrderByDescending(c => c.DateCredit))
            {
                ct.IsSelected = false;
                Transactions.Add(ct);
            }

            ClearCTSelection();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading transactions with filter: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CT SELECTION COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void ToggleCTSelection(CreditTransactionDto? ct)
    {
        if (ct == null) return;
        
        // Only allow selection for non-invoiced transactions
        if (ct.Facture) return;
        
        ct.IsSelected = !ct.IsSelected;
        RecalculateCTSelection();
    }

    [RelayCommand]
    private void SelectAllCTs()
    {
        // Only select available (non-invoiced, non-BL) transactions
        foreach (var ct in Transactions.Where(c => !c.Facture && !c.EstEnBL))
        {
            ct.IsSelected = true;
        }
        RecalculateCTSelection();
    }

    [RelayCommand]
    private void DeselectAllCTs()
    {
        foreach (var ct in Transactions)
        {
            ct.IsSelected = false;
        }
        RecalculateCTSelection();
    }

    private void ClearCTSelection()
    {
        SelectedCTCount = 0;
        SelectedCTTotal = 0;
        CanCreateBLFromCTs = false;
        CanCreateFactureFromCTs = false;
    }

    [RelayCommand]
    public void RecalculateCTSelection()
    {
        var selectedCTs = Transactions.Where(c => c.IsSelected && !c.Facture).ToList();
        SelectedCTCount = selectedCTs.Count;
        SelectedCTTotal = selectedCTs.Sum(c => c.MontantTotal);
        
        // Can create BL only from "available" transactions (not in BL yet)
        CanCreateBLFromCTs = SelectedCTCount > 0 && CtFilterMode == "available";
        
        // Can create Facture from "available" OR "inBL" transactions
        CanCreateFactureFromCTs = SelectedCTCount > 0 && (CtFilterMode == "available" || CtFilterMode == "inBL");
        
        OnPropertyChanged(nameof(Transactions));
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE BL FROM CTs COMMAND
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task CreateBLFromSelectedCTsAsync()
    {
        if (!CanCreateBLFromCTs || CurrentClient == null)
        {
            await ShowAlertAsync("Attention", "Veuillez sélectionner au moins 1 transaction disponible.");
            return;
        }

        var selectedCTs = Transactions.Where(c => c.IsSelected && !c.Facture && !c.EstEnBL).ToList();

        var confirm = await ShowConfirmationAsync(
            "Créer un Bon de Livraison",
            $"Voulez-vous créer un BL avec {selectedCTs.Count} transaction(s)?\n\n" +
            $"Montant total: {SelectedCTTotal:N2} MAD\n\n" +
            "Le BL pourra être facturé ultérieurement.");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var request = new CreateBLFromCTsDto
            {
                CreditTransactionIds = selectedCTs.Select(c => c.CreditID).ToList(),
                ClientID = CurrentClient.ID,
                DateBL = DateOnly.FromDateTime(DateTime.Now)
            };

            var result = await _blService.CreateFromCreditTransactionsAsync(request);

            if (result.Success)
            {
                await ShowAlertAsync("Succès",
                    $"Bon de Livraison {result.NumeroBL} créé avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"Transactions converties: {result.CTsConverted}");

                // Refresh all data
                await LoadClientDataAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la création du BL.");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE FACTURE FROM CTs COMMAND (Direct Facturation)
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task CreateFactureFromSelectedCTsAsync()
    {
        if (!CanCreateFactureFromCTs || CurrentClient == null)
        {
            await ShowAlertAsync("Attention", "Veuillez sélectionner au moins 1 transaction.");
            return;
        }

        var selectedCTs = Transactions.Where(c => c.IsSelected && !c.Facture).ToList();

        var confirm = await ShowConfirmationAsync(
            "Facturation Directe",
            $"Voulez-vous créer une facture directement avec {selectedCTs.Count} transaction(s)?\n\n" +
            $"Montant total: {SelectedCTTotal:N2} MAD\n\n" +
            "Les transactions seront marquées comme facturées.");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var request = new CreateFactureFromCTsDto
            {
                CreditTransactionIds = selectedCTs.Select(c => c.CreditID).ToList(),
                ClientID = CurrentClient.ID,
                DateFacture = DateOnly.FromDateTime(DateTime.Now)
            };

            var result = await _factureService.CreateFromCreditTransactionsAsync(request);

            if (result.Success)
            {
                await ShowAlertAsync("Succès",
                    $"Facture {result.NumeroFacture} créée avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"Transactions facturées: {result.CTsConverted}");

                // Refresh all data
                await LoadClientDataAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la facturation.");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // FACTURES/BL LOADING METHODS
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads factures for the current client with filters applied.
    /// </summary>
    private async Task LoadFacturesInternalAsync()
    {
        if (CurrentClient == null) return;

        List<FactureDto> factures = await _factureService.GetByClientAsync(CurrentClient.ID);

        // Apply status filter
        factures = FactureFilterMode switch
        {
            "paid" => factures.Where(f => IsPaid(f.Statut)).ToList(),
            "unpaid" => factures.Where(f => !IsPaid(f.Statut)).ToList(),
            _ => factures
        };

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            factures = factures.Where(f =>
                (f.NumeroFacture != null && f.NumeroFacture.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (f.ClientNom != null && f.ClientNom.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        Factures.Clear();
        foreach (var facture in factures.OrderByDescending(f => f.DateFacture))
        {
            facture.IsSelected = false;
            Factures.Add(facture);
        }

        ClearFactureSelection();
        CalculateFactureStatistics();
    }

    /// <summary>
    /// Loads bons de livraison for the current client with filters applied.
    /// </summary>
    private async Task LoadBonsLivraisonInternalAsync()
    {
        if (CurrentClient == null) return;

        List<BonLivraisonDto> bls = await _blService.GetByClientAsync(CurrentClient.ID);

        // Apply status filter
        bls = BlFilterMode switch
        {
            "invoiced" => bls.Where(b => b.EstFacture).ToList(),
            "notInvoiced" => bls.Where(b => !b.EstFacture).ToList(),
            _ => bls
        };

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            bls = bls.Where(b =>
                (b.NumeroBL != null && b.NumeroBL.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (b.ClientNom != null && b.ClientNom.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        BonsLivraison.Clear();
        foreach (var bl in bls.OrderByDescending(b => b.DateBL))
        {
            bl.IsSelected = false;
            BonsLivraison.Add(bl);
        }

        ClearBLSelection();
        CalculateBLStatistics();
    }

    /// <summary>
    /// Refreshes all Factures/BL data.
    /// </summary>
    [RelayCommand]
    public async Task RefreshFacturesBLAsync()
    {
        if (CurrentClient == null) return;

        try
        {
            IsLoading = true;
            await LoadFacturesInternalAsync();
            await LoadBonsLivraisonInternalAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing factures/BL: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // STATISTICS CALCULATION
    // ════════════════════════════════════════════════════════════════

    private void CalculateFactureStatistics()
    {
        TotalFactures = Factures.Count;
        TotalMontantFactures = Factures.Sum(f => f.MontantTotal ?? 0);
        FacturesPayees = Factures.Count(f => IsPaid(f.Statut));
        FacturesNonPayees = Factures.Count(f => !IsPaid(f.Statut));
    }

    private void CalculateBLStatistics()
    {
        TotalBonsLivraison = BonsLivraison.Count;
        TotalMontantBL = BonsLivraison.Sum(b => b.MontantTotal);
        BlFacturesCount = BonsLivraison.Count(b => b.EstFacture);
        BlNonFacturesCount = BonsLivraison.Count(b => !b.EstFacture);
    }

    /// <summary>
    /// Checks if a facture status is "Payée" (handles encoding variations).
    /// </summary>
    private static bool IsPaid(string? statut)
    {
        if (string.IsNullOrWhiteSpace(statut)) return false;
        var normalized = statut.ToLower().Trim();
        return normalized == "payée" || normalized == "payee";
    }

    // ════════════════════════════════════════════════════════════════
    // FACTURE FILTER COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task SetFactureFilterAllAsync()
    {
        FactureFilterMode = "all";
        IsFactureFilterAll = true;
        IsFactureFilterPaid = false;
        IsFactureFilterUnpaid = false;
        await LoadFacturesInternalAsync();
    }

    [RelayCommand]
    private async Task SetFactureFilterPaidAsync()
    {
        FactureFilterMode = "paid";
        IsFactureFilterAll = false;
        IsFactureFilterPaid = true;
        IsFactureFilterUnpaid = false;
        await LoadFacturesInternalAsync();
    }

    [RelayCommand]
    private async Task SetFactureFilterUnpaidAsync()
    {
        FactureFilterMode = "unpaid";
        IsFactureFilterAll = false;
        IsFactureFilterPaid = false;
        IsFactureFilterUnpaid = true;
        await LoadFacturesInternalAsync();
    }

    // ════════════════════════════════════════════════════════════════
    // BL FILTER COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task SetBLFilterAllAsync()
    {
        BlFilterMode = "all";
        IsBlFilterAll = true;
        IsBlFilterInvoiced = false;
        IsBlFilterNotInvoiced = false;
        await LoadBonsLivraisonInternalAsync();
    }

    [RelayCommand]
    private async Task SetBLFilterInvoicedAsync()
    {
        BlFilterMode = "invoiced";
        IsBlFilterAll = false;
        IsBlFilterInvoiced = true;
        IsBlFilterNotInvoiced = false;
        await LoadBonsLivraisonInternalAsync();
    }

    [RelayCommand]
    private async Task SetBLFilterNotInvoicedAsync()
    {
        BlFilterMode = "notInvoiced";
        IsBlFilterAll = false;
        IsBlFilterInvoiced = false;
        IsBlFilterNotInvoiced = true;
        await LoadBonsLivraisonInternalAsync();
    }

    // ════════════════════════════════════════════════════════════════
    // BL SELECTION COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void ToggleBLSelection(BonLivraisonDto? bl)
    {
        if (bl == null) return;
        bl.IsSelected = !bl.IsSelected;
        RecalculateBLSelection();
    }

    [RelayCommand]
    private void SelectAllBLs()
    {
        foreach (var bl in BonsLivraison.Where(b => !b.EstFacture))
        {
            bl.IsSelected = true;
        }
        RecalculateBLSelection();
    }

    [RelayCommand]
    private void DeselectAllBLs()
    {
        foreach (var bl in BonsLivraison)
        {
            bl.IsSelected = false;
        }
        RecalculateBLSelection();
    }

    private void ClearBLSelection()
    {
        SelectedBLCount = 0;
        SelectedBLTotal = 0;
        CanMergeBLs = false;
        CanCreateFactureFromBLs = false;
    }

    [RelayCommand]
    public void RecalculateBLSelection()
    {
        var selectedBLs = BonsLivraison.Where(b => b.IsSelected && !b.EstFacture).ToList();
        SelectedBLCount = selectedBLs.Count;
        SelectedBLTotal = selectedBLs.Sum(b => b.MontantTotal);
        CanMergeBLs = SelectedBLCount >= 2;
        CanCreateFactureFromBLs = SelectedBLCount >= 1;
        OnPropertyChanged(nameof(BonsLivraison));
    }

    // ════════════════════════════════════════════════════════════════
    // FACTURE SELECTION COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void ToggleFactureSelection(FactureDto? facture)
    {
        if (facture == null) return;
        if (IsPaid(facture.Statut)) return;
        
        facture.IsSelected = !facture.IsSelected;
        RecalculateFactureSelection();
    }

    [RelayCommand]
    private void SelectAllFactures()
    {
        foreach (var facture in Factures.Where(f => !IsPaid(f.Statut)))
        {
            facture.IsSelected = true;
        }
        RecalculateFactureSelection();
    }

    [RelayCommand]
    private void DeselectAllFactures()
    {
        foreach (var facture in Factures)
        {
            facture.IsSelected = false;
        }
        RecalculateFactureSelection();
    }

    private void ClearFactureSelection()
    {
        SelectedFactureCount = 0;
        SelectedFactureTotal = 0;
        CanMergeFactures = false;
    }

    [RelayCommand]
    public void RecalculateFactureSelection()
    {
        var selectedUnpaid = Factures.Where(f => f.IsSelected && !IsPaid(f.Statut)).ToList();
        SelectedFactureCount = selectedUnpaid.Count;
        SelectedFactureTotal = selectedUnpaid.Sum(f => f.MontantTotal ?? 0);
        CanMergeFactures = SelectedFactureCount >= 2;
        OnPropertyChanged(nameof(Factures));
    }

    // ════════════════════════════════════════════════════════════════
    // SIMPLE DIALOG HELPERS (using Application.Current.MainPage)
    // ════════════════════════════════════════════════════════════════

    private static Page? CurrentPage => Application.Current?.MainPage;

    private static async Task ShowAlertAsync(string title, string message)
    {
        if (CurrentPage != null)
            await CurrentPage.DisplayAlert(title, message, "OK");
    }

    private static async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        if (CurrentPage != null)
            return await CurrentPage.DisplayAlert(title, message, "Oui", "Non");
        return false;
    }

    // ════════════════════════════════════════════════════════════════
    // MERGE BLs COMMAND
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task MergeBLsAsync()
    {
        if (!CanMergeBLs || CurrentClient == null)
        {
            await ShowAlertAsync("Attention", "Veuillez sélectionner au moins 2 bons de livraison non facturés.");
            return;
        }

        var selectedBLs = BonsLivraison.Where(b => b.IsSelected && !b.EstFacture).ToList();

        var confirm = await ShowConfirmationAsync(
            "Fusionner les Bons de Livraison",
            $"Voulez-vous fusionner {selectedBLs.Count} BL(s) en un seul?\n\n" +
            $"Montant total: {SelectedBLTotal:N2} MAD\n\n" +
            "Les BLs sélectionnés seront supprimés et remplacés par un nouveau BL consolidé.");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var request = new MergeBLsDto
            {
                BonLivraisonIds = selectedBLs.Select(b => b.ID).ToList(),
                ClientID = CurrentClient.ID,
                DateBL = DateOnly.FromDateTime(DateTime.Now),
                Notes = $"BL consolidé à partir de: {string.Join(", ", selectedBLs.Select(b => b.NumeroBL))}"
            };

            var result = await _blService.MergeBLsAsync(request);

            if (result.Success)
            {
                await ShowAlertAsync("Succès",
                    $"BL consolidé {result.NewNumeroBL} créé avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"BLs fusionnés: {result.BLsMerged}");

                await RefreshFacturesBLAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la fusion des BLs.");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE FACTURE FROM BLs COMMAND
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task CreateFactureFromBLsAsync()
    {
        if (!CanCreateFactureFromBLs || CurrentClient == null)
        {
            await ShowAlertAsync("Attention", "Veuillez sélectionner au moins 1 bon de livraison non facturé.");
            return;
        }

        var selectedBLs = BonsLivraison.Where(b => b.IsSelected && !b.EstFacture).ToList();

        var confirm = await ShowConfirmationAsync(
            "Créer une Facture",
            $"Voulez-vous créer une facture à partir de {selectedBLs.Count} BL(s)?\n\n" +
            $"BLs sélectionnés:\n{string.Join("\n", selectedBLs.Select(b => $"  • {b.NumeroBL}"))}\n\n" +
            $"Montant total: {SelectedBLTotal:N2} MAD\n\n" +
            "Les BLs seront marqués comme facturés.");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var request = new CreateFactureFromBLsDto
            {
                BonLivraisonIds = selectedBLs.Select(b => b.ID).ToList(),
                ClientID = CurrentClient.ID,
                DateFacture = DateOnly.FromDateTime(DateTime.Now)
            };

            var result = await _factureService.CreateFactureFromBLsAsync(request);

            if (result.Success)
            {
                await ShowAlertAsync("Succès",
                    $"Facture {result.NumeroFacture} créée avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"BLs facturés: {result.BLsFactures}");

                await RefreshFacturesBLAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la création de la facture.");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // MERGE FACTURES COMMAND
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task MergeFacturesAsync()
    {
        if (!CanMergeFactures || CurrentClient == null)
        {
            await ShowAlertAsync("Attention", "Veuillez sélectionner au moins 2 factures non payées.");
            return;
        }

        var selectedFactures = Factures.Where(f => f.IsSelected && !IsPaid(f.Statut)).ToList();

        var confirm = await ShowConfirmationAsync(
            "Fusionner les Factures",
            $"Voulez-vous fusionner {selectedFactures.Count} facture(s) en une seule?\n\n" +
            $"Montant total: {SelectedFactureTotal:N2} MAD\n\n" +
            "Les factures sélectionnées seront supprimées et remplacées par une nouvelle facture consolidée.");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var request = new MergeFacturesDto
            {
                FactureIds = selectedFactures.Select(f => f.ID).ToList(),
                ClientID = CurrentClient.ID,
                DateFacture = DateOnly.FromDateTime(DateTime.Now)
            };

            var result = await _factureService.MergeFacturesAsync(request);

            if (result.Success)
            {
                await ShowAlertAsync("Succès",
                    $"Facture consolidée {result.NewNumeroFacture} créée avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"Factures fusionnées: {result.FacturesMerged}");

                await RefreshFacturesBLAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la fusion des factures.");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // DELETE FACTURE COMMAND
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task DeleteFactureAsync(FactureDto? facture)
    {
        if (facture == null) return;

        var confirm = await ShowConfirmationAsync(
            "Supprimer la facture",
            $"Voulez-vous supprimer la facture {facture.NumeroFacture}?\n\n" +
            "Les transactions associées redeviendront disponibles.");

        if (!confirm) return;

        try
        {
            IsLoading = true;
            var success = await _factureService.DeleteAsync(facture.ID);
            if (success)
            {
                await ShowAlertAsync("Succès", "Facture supprimée.");
                await LoadFacturesInternalAsync();
            }
            else
            {
                await ShowAlertAsync("Erreur", "Impossible de supprimer la facture.");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // DELETE BL COMMAND
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task DeleteBLAsync(BonLivraisonDto? bl)
    {
        if (bl == null) return;

        if (bl.EstFacture)
        {
            await ShowAlertAsync(
                "Impossible",
                "Ce bon de livraison est déjà facturé et ne peut pas être supprimé.");
            return;
        }

        var confirm = await ShowConfirmationAsync(
            "Supprimer le bon de livraison",
            $"Voulez-vous supprimer le BL {bl.NumeroBL}?\n\n" +
            "Les transactions associées redeviendront disponibles.");

        if (!confirm) return;

        try
        {
            IsLoading = true;
            var success = await _blService.DeleteAsync(bl.ID);
            if (success)
            {
                await ShowAlertAsync("Succès", "Bon de livraison supprimé.");
                await LoadBonsLivraisonInternalAsync();
            }
            else
            {
                await ShowAlertAsync("Erreur", "Impossible de supprimer le bon de livraison.");
            }
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // SEARCH TEXT CHANGED HANDLER
    // ════════════════════════════════════════════════════════════════

    partial void OnSearchTextChanged(string value)
    {
        _ = RefreshFacturesBLAsync();
    }
}

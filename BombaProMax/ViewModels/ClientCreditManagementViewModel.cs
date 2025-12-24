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
    // COLLECTIONS
    // ============================

    public ObservableCollection<CreditTransactionDto> Transactions { get; } = [];
    public ObservableCollection<ReglementCreditDto> Reglements { get; } = [];
    public ObservableCollection<ProduitDto> Produits { get; } = [];
    public ObservableCollection<ServiceDto> Services { get; } = [];
    public ObservableCollection<MoyensPaiementDto> MoyensPaiement { get; } = [];

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

            // Load all data in parallel
            var bilanTask = _bilanService.GetByClientAsync(CurrentClient.ID);
            var transactionsTask = _transactionService.GetByClientAsync(CurrentClient.ID);
            var reglementsTask = _reglementService.GetByClientAsync(CurrentClient.ID);

            await Task.WhenAll(bilanTask, transactionsTask, reglementsTask);

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
}

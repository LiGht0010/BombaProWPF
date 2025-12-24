using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

[QueryProperty(nameof(ClientId), "clientId")]
public partial class FactureViewModel : ObservableObject
{
    private readonly CreditTransactionService _ctService;
    private readonly BonLivraisonService _blService;
    private readonly FactureService _factureService;
    private readonly ClientService _clientService;
    private readonly IDialogService _dialogService;

    // ════════════════════════════════════════════════════════════════
    // OBSERVABLE COLLECTIONS
    // ════════════════════════════════════════════════════════════════
    public ObservableCollection<CreditTransactionDto> AvailableCTs { get; } = [];
    public ObservableCollection<CreditTransactionDto> SelectedCTs { get; } = [];
    public ObservableCollection<FactureDto> Factures { get; } = [];

    // ════════════════════════════════════════════════════════════════
    // OBSERVABLE PROPERTIES
    // ════════════════════════════════════════════════════════════════
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ClientDto? _currentClient;

    [ObservableProperty]
    private decimal _totalSelected;

    [ObservableProperty]
    private int _countSelected;

    [ObservableProperty]
    private bool _canProcess;

    [ObservableProperty]
    private bool _canCreateBL;

    [ObservableProperty]
    private bool _canCreateFacture;

    [ObservableProperty]
    private string _filterMode = "available"; // available, inBL, invoiced

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
                _ = LoadClientAndDataAsync(value);
            }
        }
    }

    // Statistics
    [ObservableProperty]
    private int _totalCTsDisponibles;

    [ObservableProperty]
    private int _totalFactures;

    [ObservableProperty]
    private decimal _totalMontantDisponible;

    // Filter display
    [ObservableProperty]
    private bool _isFilterAvailable = true;

    [ObservableProperty]
    private bool _isFilterInBL;

    [ObservableProperty]
    private bool _isFilterInvoiced;

    // ════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ════════════════════════════════════════════════════════════════
    public FactureViewModel(
        CreditTransactionService ctService,
        BonLivraisonService blService,
        FactureService factureService,
        ClientService clientService,
        IDialogService dialogService)
    {
        _ctService = ctService;
        _blService = blService;
        _factureService = factureService;
        _clientService = clientService;
        _dialogService = dialogService;
    }

    // ════════════════════════════════════════════════════════════════
    // INITIALIZATION
    // ════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Loads client by ID and then loads all related data.
    /// </summary>
    private async Task LoadClientAndDataAsync(int clientId)
    {
        try
        {
            IsLoading = true;
            
            var client = await _clientService.GetClientByIdAsync(clientId);
            if (client != null)
            {
                CurrentClient = client;
                await LoadCTsForClientAsync();
                await LoadFacturesAsync();
                CalculateStatistics();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les données: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        // If ClientId is already set (from query param), data is already loading
        if (ClientId > 0 && CurrentClient != null)
        {
            return;
        }
        
        // Fallback: if no client ID, show message
        if (ClientId <= 0)
        {
            await _dialogService.ShowAlertAsync("Information", "Veuillez sélectionner un client depuis la liste des clients.");
        }
    }

    private async Task LoadFacturesAsync()
    {
        if (CurrentClient == null) return;
        
        var factures = await _factureService.GetByClientAsync(CurrentClient.ID);
        Factures.Clear();
        foreach (var facture in factures.OrderByDescending(f => f.DateFacture).Take(20))
        {
            Factures.Add(facture);
        }
    }

    [RelayCommand]
    private async Task LoadCTsForClientAsync()
    {
        if (CurrentClient == null)
        {
            AvailableCTs.Clear();
            ClearSelection();
            CalculateStatistics();
            return;
        }

        try
        {
            IsLoading = true;
            
            List<CreditTransactionDto> cts = FilterMode switch
            {
                "inBL" => await _ctService.GetInBLByClientAsync(CurrentClient.ID),
                "invoiced" => await _ctService.GetInvoicedByClientAsync(CurrentClient.ID),
                _ => await _ctService.GetAvailableByClientAsync(CurrentClient.ID)
            };

            AvailableCTs.Clear();
            foreach (var ct in cts)
            {
                ct.IsSelected = false;
                AvailableCTs.Add(ct);
            }
            ClearSelection();
            CalculateStatistics();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les transactions: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // FILTER COMMANDS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private async Task SetFilterAvailableAsync()
    {
        FilterMode = "available";
        IsFilterAvailable = true;
        IsFilterInBL = false;
        IsFilterInvoiced = false;
        await LoadCTsForClientAsync();
    }

    [RelayCommand]
    private async Task SetFilterInBLAsync()
    {
        FilterMode = "inBL";
        IsFilterAvailable = false;
        IsFilterInBL = true;
        IsFilterInvoiced = false;
        await LoadCTsForClientAsync();
    }

    [RelayCommand]
    private async Task SetFilterInvoicedAsync()
    {
        FilterMode = "invoiced";
        IsFilterAvailable = false;
        IsFilterInBL = false;
        IsFilterInvoiced = true;
        await LoadCTsForClientAsync();
    }

    // ════════════════════════════════════════════════════════════════
    // SELECTION COMMANDS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private void ToggleCTSelection(CreditTransactionDto? ct)
    {
        if (ct == null) return;

        ct.IsSelected = !ct.IsSelected;
        RecalculateSelection();
    }

    [RelayCommand]
    private void SelectAllCTs()
    {
        foreach (var ct in AvailableCTs)
        {
            ct.IsSelected = true;
        }
        RecalculateSelection();
    }

    [RelayCommand]
    private void DeselectAllCTs()
    {
        foreach (var ct in AvailableCTs)
        {
            ct.IsSelected = false;
        }
        RecalculateSelection();
    }

    private void ClearSelection()
    {
        SelectedCTs.Clear();
        TotalSelected = 0;
        CountSelected = 0;
        CanProcess = false;
        CanCreateBL = false;
        CanCreateFacture = false;
    }

    /// <summary>
    /// Recalculates selection totals. Exposed as command for CheckBox CheckedChanged event.
    /// </summary>
    [RelayCommand]
    private void RecalculateSelection()
    {
        SelectedCTs.Clear();
        foreach (var ct in AvailableCTs.Where(c => c.IsSelected))
        {
            SelectedCTs.Add(ct);
        }

        CountSelected = SelectedCTs.Count;
        TotalSelected = SelectedCTs.Sum(ct => ct.MontantTotal);
        
        // CanCreateBL: only from "available" transactions (not yet in BL)
        CanCreateBL = CountSelected > 0 && FilterMode == "available";
        
        // CanCreateFacture: from "available" OR "inBL" transactions (not yet invoiced)
        CanCreateFacture = CountSelected > 0 && (FilterMode == "available" || FilterMode == "inBL");
        
        // General CanProcess for backward compatibility
        CanProcess = CanCreateBL || CanCreateFacture;

        OnPropertyChanged(nameof(AvailableCTs));
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE BL COMMAND (⭐ Convert CTs to BL)
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private async Task CreateBLFromSelectedAsync()
    {
        if (!CanCreateBL || CurrentClient == null)
        {
            await _dialogService.ShowAlertAsync("Attention", "Veuillez sélectionner au moins une transaction disponible.");
            return;
        }

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Créer un Bon de Livraison",
            $"Voulez-vous créer un BL avec {CountSelected} transaction(s) ?\n\n" +
            $"Montant total: {TotalSelected:N2} DH\n\n" +
            "Le BL pourra être facturé ultérieurement.");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var request = new CreateBLFromCTsDto
            {
                CreditTransactionIds = SelectedCTs.Select(ct => ct.CreditID).ToList(),
                ClientID = CurrentClient.ID,
                DateBL = DateOnly.FromDateTime(DateTime.Now)
            };

            var result = await _blService.CreateFromCreditTransactionsAsync(request);

            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Succès", 
                    $"Bon de Livraison {result.NumeroBL} créé avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} DH\n" +
                    $"Transactions converties: {result.CTsConverted}");

                await RefreshAllDataAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true 
                    ? string.Join("\n", result.Errors) 
                    : result.Message;
                await _dialogService.ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la création du BL.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE FACTURE DIRECT COMMAND (⭐ Convert CTs directly to Facture)
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private async Task CreateFactureDirectAsync()
    {
        if (!CanCreateFacture || CurrentClient == null)
        {
            await _dialogService.ShowAlertAsync("Attention", "Veuillez sélectionner au moins une transaction.");
            return;
        }

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Facturation Directe",
            $"Voulez-vous créer une facture directement avec {CountSelected} transaction(s) ?\n\n" +
            $"Montant total: {TotalSelected:N2} DH\n\n" +
            "Les transactions seront marquées comme facturées.");

        if (!confirm) return;

        try
        {
            IsLoading = true;

            var request = new CreateFactureFromCTsDto
            {
                CreditTransactionIds = SelectedCTs.Select(ct => ct.CreditID).ToList(),
                ClientID = CurrentClient.ID,
                DateFacture = DateOnly.FromDateTime(DateTime.Now)
            };

            var result = await _factureService.CreateFromCreditTransactionsAsync(request);

            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Succès", 
                    $"Facture {result.NumeroFacture} créée avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} DH\n" +
                    $"Transactions facturées: {result.CTsConverted}");

                await RefreshAllDataAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true 
                    ? string.Join("\n", result.Errors) 
                    : result.Message;
                await _dialogService.ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la facturation.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes all data after an operation.
    /// </summary>
    private async Task RefreshAllDataAsync()
    {
        await LoadCTsForClientAsync();
        await LoadFacturesAsync();
        CalculateStatistics();
    }

    // ════════════════════════════════════════════════════════════════
    // FACTURE ACTIONS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private async Task ViewFactureDetailsAsync(FactureDto? facture)
    {
        if (facture == null) return;

        await _dialogService.ShowAlertAsync(
            $"Facture {facture.NumeroFacture}",
            $"Date: {facture.DateFacture:dd/MM/yyyy}\n" +
            $"Client: {facture.ClientNom}\n" +
            $"Montant: {facture.MontantTotal:N2} DH\n" +
            $"Statut: {facture.Statut}");
    }

    [RelayCommand]
    private async Task MarkFactureAsPaidAsync(FactureDto? facture)
    {
        if (facture == null) return;

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Marquer comme payée",
            $"Voulez-vous marquer la facture {facture.NumeroFacture} comme payée?");

        if (!confirm) return;

        try
        {
            var success = await _factureService.MarkAsPaidAsync(facture.ID);
            if (success)
            {
                facture.Statut = "Payée";
                facture.DatePaiement = DateOnly.FromDateTime(DateTime.Now);
                await _dialogService.ShowAlertAsync("Succès", "Facture marquée comme payée.");
                await LoadFacturesAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de mettre à jour le statut.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteFactureAsync(FactureDto? facture)
    {
        if (facture == null) return;

        var confirm = await _dialogService.ShowConfirmationAsync(
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
                await _dialogService.ShowAlertAsync("Succès", "Facture supprimée. Les transactions sont maintenant disponibles.");
                await RefreshAllDataAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de supprimer la facture.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // STATISTICS
    // ════════════════════════════════════════════════════════════════
    private void CalculateStatistics()
    {
        TotalCTsDisponibles = AvailableCTs.Count;
        TotalFactures = Factures.Count;
        TotalMontantDisponible = AvailableCTs.Sum(ct => ct.MontantTotal);
    }
}

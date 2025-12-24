using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

/// <summary>
/// ViewModel for the FactureEtBL page that displays both Factures and Bons de Livraison
/// filtered by a specific client passed via navigation.
/// </summary>
[QueryProperty(nameof(ClientId), "clientId")]
public partial class FactureEtBLViewModel : ObservableObject
{
    private readonly FactureService _factureService;
    private readonly BonLivraisonService _blService;
    private readonly ClientService _clientService;
    private readonly IDialogService _dialogService;

    // ????????????????????????????????????????????????????????????????
    // OBSERVABLE COLLECTIONS
    // ????????????????????????????????????????????????????????????????
    public ObservableCollection<FactureDto> Factures { get; } = [];
    public ObservableCollection<BonLivraisonDto> BonsLivraison { get; } = [];

    // ????????????????????????????????????????????????????????????????
    // OBSERVABLE PROPERTIES
    // ????????????????????????????????????????????????????????????????
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ClientDto? _currentClient;

    [ObservableProperty]
    private string _searchText = string.Empty;

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

    // Filter states for BL - using explicit properties due to naming
    [ObservableProperty]
    private string _blFilterMode = "all"; // all, invoiced, notInvoiced

    [ObservableProperty]
    private bool _isBlFilterAll = true;

    [ObservableProperty]
    private bool _isBlFilterInvoiced;

    [ObservableProperty]
    private bool _isBlFilterNotInvoiced;

    // ????????????????????????????????????????????????????????????????
    // BL SELECTION PROPERTIES (for merge and facturation)
    // ????????????????????????????????????????????????????????????????
    [ObservableProperty]
    private int _selectedBLCount;

    [ObservableProperty]
    private decimal _selectedBLTotal;

    [ObservableProperty]
    private bool _canMergeBLs;

    [ObservableProperty]
    private bool _canCreateFactureFromBLs;

    // ????????????????????????????????????????????????????????????????
    // FACTURE SELECTION PROPERTIES (for merge)
    // ????????????????????????????????????????????????????????????????
    [ObservableProperty]
    private int _selectedFactureCount;

    [ObservableProperty]
    private decimal _selectedFactureTotal;

    [ObservableProperty]
    private bool _canMergeFactures;

    // ????????????????????????????????????????????????????????????????
    // CONSTRUCTOR
    // ????????????????????????????????????????????????????????????????
    public FactureEtBLViewModel(
        FactureService factureService,
        BonLivraisonService blService,
        ClientService clientService,
        IDialogService dialogService)
    {
        _factureService = factureService;
        _blService = blService;
        _clientService = clientService;
        _dialogService = dialogService;
    }

    // ????????????????????????????????????????????????????????????????
    // INITIALIZATION
    // ????????????????????????????????????????????????????????????????
    
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
                await LoadFacturesInternalAsync();
                await LoadBonsLivraisonInternalAsync();
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
    public async Task InitializeAsync()
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

    // ????????????????????????????????????????????????????????????????
    // FACTURES LOADING
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private async Task LoadFacturesAsync()
    {
        try
        {
            IsLoading = true;
            await LoadFacturesInternalAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les factures: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

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

    // ????????????????????????????????????????????????????????????????
    // BONS LIVRAISON LOADING
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private async Task LoadBonsLivraisonAsync()
    {
        try
        {
            IsLoading = true;
            await LoadBonsLivraisonInternalAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les bons de livraison: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

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

    // ????????????????????????????????????????????????????????????????
    // REFRESH COMMANDS
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    public async Task RefreshAllAsync()
    {
        if (CurrentClient == null) return;

        try
        {
            IsLoading = true;
            await LoadFacturesInternalAsync();
            await LoadBonsLivraisonInternalAsync();
            CalculateStatistics();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur de rafraîchissement: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ????????????????????????????????????????????????????????????????
    // FACTURE FILTER COMMANDS
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private async Task SetFactureFilterAllAsync()
    {
        FactureFilterMode = "all";
        IsFactureFilterAll = true;
        IsFactureFilterPaid = false;
        IsFactureFilterUnpaid = false;
        await LoadFacturesAsync();
    }

    [RelayCommand]
    private async Task SetFactureFilterPaidAsync()
    {
        FactureFilterMode = "paid";
        IsFactureFilterAll = false;
        IsFactureFilterPaid = true;
        IsFactureFilterUnpaid = false;
        await LoadFacturesAsync();
    }

    [RelayCommand]
    private async Task SetFactureFilterUnpaidAsync()
    {
        FactureFilterMode = "unpaid";
        IsFactureFilterAll = false;
        IsFactureFilterPaid = false;
        IsFactureFilterUnpaid = true;
        await LoadFacturesAsync();
    }

    // ????????????????????????????????????????????????????????????????
    // BL FILTER COMMANDS
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private async Task SetBLFilterAllAsync()
    {
        BlFilterMode = "all";
        IsBlFilterAll = true;
        IsBlFilterInvoiced = false;
        IsBlFilterNotInvoiced = false;
        await LoadBonsLivraisonAsync();
    }

    [RelayCommand]
    private async Task SetBLFilterInvoicedAsync()
    {
        BlFilterMode = "invoiced";
        IsBlFilterAll = false;
        IsBlFilterInvoiced = true;
        IsBlFilterNotInvoiced = false;
        await LoadBonsLivraisonAsync();
    }

    [RelayCommand]
    private async Task SetBLFilterNotInvoicedAsync()
    {
        BlFilterMode = "notInvoiced";
        IsBlFilterAll = false;
        IsBlFilterInvoiced = false;
        IsBlFilterNotInvoiced = true;
        await LoadBonsLivraisonAsync();
    }

    // ????????????????????????????????????????????????????????????????
    // BL SELECTION COMMANDS (for merge and facturation)
    // ????????????????????????????????????????????????????????????????
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
        // Only select non-invoiced BLs for merging
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

    /// <summary>
    /// Recalculates BL selection. Exposed as command for CheckBox CheckedChanged event.
    /// </summary>
    [RelayCommand]
    private void RecalculateBLSelection()
    {
        var selectedBLs = BonsLivraison.Where(b => b.IsSelected && !b.EstFacture).ToList();
        SelectedBLCount = selectedBLs.Count;
        SelectedBLTotal = selectedBLs.Sum(b => b.MontantTotal);
        // Need at least 2 BLs to merge
        CanMergeBLs = SelectedBLCount >= 2;
        // Need at least 1 BL to create facture
        CanCreateFactureFromBLs = SelectedBLCount >= 1;
        OnPropertyChanged(nameof(BonsLivraison));
    }

    // ????????????????????????????????????????????????????????????????
    // FACTURE SELECTION COMMANDS (for merge)
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private void ToggleFactureSelection(FactureDto? facture)
    {
        if (facture == null) return;
        // Only allow selection of unpaid factures
        if (IsPaid(facture.Statut)) return;
        
        facture.IsSelected = !facture.IsSelected;
        RecalculateFactureSelection();
    }

    [RelayCommand]
    private void SelectAllFactures()
    {
        // Only select unpaid factures for merging
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

    /// <summary>
    /// Recalculates Facture selection. Exposed as command for CheckBox CheckedChanged event.
    /// </summary>
    [RelayCommand]
    private void RecalculateFactureSelection()
    {
        // Count ALL selected factures (for UI display)
        var allSelected = Factures.Where(f => f.IsSelected).ToList();
        
        // Filter to only unpaid for merge functionality
        var selectedUnpaid = allSelected.Where(f => !IsPaid(f.Statut)).ToList();
        
        SelectedFactureCount = selectedUnpaid.Count;
        SelectedFactureTotal = selectedUnpaid.Sum(f => f.MontantTotal ?? 0);
        
        // Need at least 2 unpaid Factures to merge
        CanMergeFactures = SelectedFactureCount >= 2;
        
        System.Diagnostics.Debug.WriteLine($"[RecalculateFactureSelection] Total selected: {allSelected.Count}, Unpaid selected: {selectedUnpaid.Count}, CanMerge: {CanMergeFactures}");
        
        OnPropertyChanged(nameof(Factures));
    }

    /// <summary>
    /// Checks if a facture status is "Payée" (handles encoding variations)
    /// </summary>
    private static bool IsPaid(string? statut)
    {
        if (string.IsNullOrWhiteSpace(statut)) return false;
        var normalized = statut.ToLower().Trim();
        return normalized == "payée" || normalized == "payee";
    }

    // ????????????????????????????????????
    // MERGE BLs COMMAND
    // ????????????????????????????????????
    [RelayCommand]
    private async Task MergeBLsAsync()
    {
        if (!CanMergeBLs || CurrentClient == null)
        {
            await _dialogService.ShowAlertAsync("Attention", "Veuillez sélectionner au moins 2 bons de livraison non facturés.");
            return;
        }

        var selectedBLs = BonsLivraison.Where(b => b.IsSelected && !b.EstFacture).ToList();

        var confirm = await _dialogService.ShowConfirmationAsync(
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
                await _dialogService.ShowAlertAsync("Succès",
                    $"BL consolidé {result.NewNumeroBL} créé avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"BLs fusionnés: {result.BLsMerged}");

                await RefreshAllAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await _dialogService.ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la fusion des BLs.");
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

    // ????????????????????????????????????
    // CREATE FACTURE FROM BLs COMMAND (? BL ? Facture conversion)
    // ????????????????????????????????????
    [RelayCommand]
    private async Task CreateFactureFromBLsAsync()
    {
        if (!CanCreateFactureFromBLs || CurrentClient == null)
        {
            await _dialogService.ShowAlertAsync("Attention", "Veuillez sélectionner au moins 1 bon de livraison non facturé.");
            return;
        }

        var selectedBLs = BonsLivraison.Where(b => b.IsSelected && !b.EstFacture).ToList();

        var confirm = await _dialogService.ShowConfirmationAsync(
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
                await _dialogService.ShowAlertAsync("Succès",
                    $"Facture {result.NumeroFacture} créée avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"BLs facturés: {result.BLsFactures}");

                await RefreshAllAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await _dialogService.ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la création de la facture.");
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

    // ????????????????????????????????????
    // MERGE FACTURES COMMAND
    // ????????????????????????????????????
    [RelayCommand]
    private async Task MergeFacturesAsync()
    {
        if (!CanMergeFactures || CurrentClient == null)
        {
            await _dialogService.ShowAlertAsync("Attention", "Veuillez sélectionner au moins 2 factures non payées.");
            return;
        }

        var selectedFactures = Factures.Where(f => f.IsSelected && !IsPaid(f.Statut)).ToList();

        var confirm = await _dialogService.ShowConfirmationAsync(
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
                await _dialogService.ShowAlertAsync("Succès",
                    $"Facture consolidée {result.NewNumeroFacture} créée avec succès!\n\n" +
                    $"Montant: {result.MontantTotal:N2} MAD\n" +
                    $"Factures fusionnées: {result.FacturesMerged}");

                await RefreshAllAsync();
            }
            else
            {
                var errorMsg = result.Errors?.Any() == true
                    ? string.Join("\n", result.Errors)
                    : result.Message;
                await _dialogService.ShowAlertAsync("Erreur", errorMsg ?? "Erreur lors de la fusion des factures.");
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

    // ????????????????????????????????????????????????????????????????
    // FACTURE ACTIONS
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private async Task ViewFactureDetailsAsync(FactureDto? facture)
    {
        if (facture == null) return;

        var details = $"Numéro: {facture.NumeroFacture}\n" +
                      $"Date: {facture.DateFacture:dd/MM/yyyy}\n" +
                      $"Client: {facture.ClientNom}\n" +
                      $"Montant: {facture.MontantTotal:N2} MAD\n" +
                      $"Statut: {facture.Statut}";

        if (facture.DatePaiement.HasValue)
        {
            details += $"\nDate Paiement: {facture.DatePaiement:dd/MM/yyyy}";
        }

        await _dialogService.ShowAlertAsync($"Facture {facture.NumeroFacture}", details);
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
            IsLoading = true;
            var success = await _factureService.MarkAsPaidAsync(facture.ID);
            if (success)
            {
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
        finally
        {
            IsLoading = false;
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
                await _dialogService.ShowAlertAsync("Succès", "Facture supprimée.");
                await LoadFacturesAsync();
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

    // ????????????????????????????????????????????????????????????????
    // BON LIVRAISON ACTIONS
    // ????????????????????????????????????????????????????????????????
    [RelayCommand]
    private async Task ViewBLDetailsAsync(BonLivraisonDto? bl)
    {
        if (bl == null) return;

        var statusText = bl.EstFacture ? "Facturé" : "Non Facturé";
        var details = $"Numéro: {bl.NumeroBL}\n" +
                      $"Date: {bl.DateBL:dd/MM/yyyy}\n" +
                      $"Client: {bl.ClientNom}\n" +
                      $"Montant: {bl.MontantTotal:N2} MAD\n" +
                      $"Statut: {statusText}";

        if (!string.IsNullOrWhiteSpace(bl.Notes))
        {
            details += $"\nNotes: {bl.Notes}";
        }

        await _dialogService.ShowAlertAsync($"Bon de Livraison {bl.NumeroBL}", details);
    }

    [RelayCommand]
    private async Task DeleteBLAsync(BonLivraisonDto? bl)
    {
        if (bl == null) return;

        if (bl.EstFacture)
        {
            await _dialogService.ShowAlertAsync(
                "Impossible",
                "Ce bon de livraison est déjà facturé et ne peut pas être supprimé.");
            return;
        }

        var confirm = await _dialogService.ShowConfirmationAsync(
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
                await _dialogService.ShowAlertAsync("Succès", "Bon de livraison supprimé.");
                await LoadBonsLivraisonAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Impossible de supprimer le bon de livraison.");
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

    // ????????????????????????????????????????????????????????????????
    // STATISTICS
    // ????????????????????????????????????????????????????????????????
    private void CalculateStatistics()
    {
        CalculateFactureStatistics();
        CalculateBLStatistics();
    }

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

    // ????????????????????????????????????????????????????????????????
    // PROPERTY CHANGED HANDLERS
    // ????????????????????????????????????????????????????????????????
    partial void OnSearchTextChanged(string value)
    {
        _ = RefreshAllAsync();
    }
}

using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMaxWPF.ViewModels;

public partial class AchatViewModel : ObservableObject
{
    private readonly AchatService _achatService;
    private readonly AchatAllocationService _allocationService;
    private readonly IDialogService _dialogService;
    private readonly JourneeNavigationService _journeeService;

    // Master list for sorting/filtering
    private List<AchatDto> _allAchats = [];

    public ObservableCollection<AchatDto> Achats { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private AchatDto? _selectedAchat;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Statistics
    [ObservableProperty]
    private int _thisMonthCount;

    [ObservableProperty]
    private int _totalVolume;

    [ObservableProperty]
    private decimal _totalCost;

    // ════════════════════════════════════════════════════════════════
    // SORTING PROPERTIES
    // ════════════════════════════════════════════════════════════════
    [ObservableProperty]
    private string _currentSortColumn = "Date";

    [ObservableProperty]
    private bool _isSortAscending = false; // Default descending for Date

    // Sort indicator texts for each column
    public string NumeroSortIndicator => GetSortIndicator("Numero");
    public string DateSortIndicator => GetSortIndicator("Date");
    public string FournisseurSortIndicator => GetSortIndicator("Fournisseur");
    public string ProduitSortIndicator => GetSortIndicator("Produit");
    public string QuantiteSortIndicator => GetSortIndicator("Quantite");
    public string CoutSortIndicator => GetSortIndicator("Cout");

    private string GetSortIndicator(string column)
    {
        if (CurrentSortColumn != column) return "";
        return IsSortAscending ? " ↑" : " ↓";
    }

    private void NotifySortIndicators()
    {
        OnPropertyChanged(nameof(NumeroSortIndicator));
        OnPropertyChanged(nameof(DateSortIndicator));
        OnPropertyChanged(nameof(FournisseurSortIndicator));
        OnPropertyChanged(nameof(ProduitSortIndicator));
        OnPropertyChanged(nameof(QuantiteSortIndicator));
        OnPropertyChanged(nameof(CoutSortIndicator));
    }

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES
    // ════════════════════════════════════════════════════════════════
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public AchatViewModel(
        AchatService achatService,
        AchatAllocationService allocationService,
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _achatService = achatService;
        _allocationService = allocationService;
        _dialogService = dialogService;
        _journeeService = journeeService;

        _journeeService.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(IsJourneeActive));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsFirstStep));
            OnPropertyChanged(nameof(IsLastStep));
            OnPropertyChanged(nameof(JourneeStepInfo));
        };
    }

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE COMMANDS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private async Task JourneeSuivantAsync() => await _journeeService.GoNextAsync(skipped: false);

    [RelayCommand]
    private async Task JourneePasserAsync() => await _journeeService.GoNextAsync(skipped: true);

    [RelayCommand]
    private async Task JourneePrecedentAsync() => await _journeeService.GoPreviousAsync();

    // ════════════════════════════════════════════════════════════════
    // SORTING COMMANDS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    private void SortByColumn(string column)
    {
        if (CurrentSortColumn == column)
        {
            // Toggle sort direction
            IsSortAscending = !IsSortAscending;
        }
        else
        {
            // New column - default to ascending (except Date which defaults to descending)
            CurrentSortColumn = column;
            IsSortAscending = column != "Date";
        }

        ApplySorting();
        NotifySortIndicators();
    }

    private void ApplySorting()
    {
        var sorted = CurrentSortColumn switch
        {
            "Numero" => IsSortAscending
                ? _allAchats.OrderBy(a => a.Numero).ToList()
                : _allAchats.OrderByDescending(a => a.Numero).ToList(),
            "Date" => IsSortAscending
                ? _allAchats.OrderBy(a => a.Date).ToList()
                : _allAchats.OrderByDescending(a => a.Date).ToList(),
            "Fournisseur" => IsSortAscending
                ? _allAchats.OrderBy(a => a.FournisseurNom).ToList()
                : _allAchats.OrderByDescending(a => a.FournisseurNom).ToList(),
            "Produit" => IsSortAscending
                ? _allAchats.OrderBy(a => a.ProduitNom).ToList()
                : _allAchats.OrderByDescending(a => a.ProduitNom).ToList(),
            "Quantite" => IsSortAscending
                ? _allAchats.OrderBy(a => a.Quantite ?? 0).ToList()
                : _allAchats.OrderByDescending(a => a.Quantite ?? 0).ToList(),
            "Cout" => IsSortAscending
                ? _allAchats.OrderBy(a => a.Cout ?? 0).ToList()
                : _allAchats.OrderByDescending(a => a.Cout ?? 0).ToList(),
            _ => _allAchats.OrderByDescending(a => a.Date).ToList()
        };

        Achats.Clear();
        foreach (var achat in sorted)
        {
            Achats.Add(achat);
        }
    }

    // ════════════════════════════════════════════════════════════════
    // EXISTING COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    public async Task LoadAchatsAsync()
    {
        try
        {
            IsLoading = true;
            var achats = await _achatService.GetAllAsync();
            
            // Store in master list and apply default sorting (Date descending)
            _allAchats = achats.OrderByDescending(a => a.Date).ToList();
            
            Achats.Clear();
            foreach (var achat in _allAchats)
            {
                Achats.Add(achat);
            }

            CalculateStatistics();
            NotifySortIndicators();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les achats: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateStatistics()
    {
        var startOfMonth = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
        ThisMonthCount = Achats.Count(a => a.Date >= startOfMonth);
        TotalVolume = Achats.Where(a => a.Quantite.HasValue).Sum(a => a.Quantite!.Value);
        TotalCost = Achats.Where(a => a.Cout.HasValue).Sum(a => a.Cout!.Value);
    }

    [RelayCommand]
    private async Task AddAchatAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.AddAchat)
        {
            var newAchat = await _dialogService.ShowAchatCreatePopupAsync();
            if (newAchat != null)
            {
                _allAchats.Insert(0, newAchat);
                Achats.Insert(0, newAchat);
                CalculateStatistics();

                // Auto-open allocation popup for fuel products
                var allocationResult = await _dialogService.ShowAchatAllocationPopupForNewAchatAsync(newAchat);
                if (allocationResult?.Success == true)
                {
                    await _dialogService.ShowAlertAsync("Succès", 
                        allocationResult.Message ?? "Achat créé et allocation effectuée avec succès");
                }
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des achats");
        }
    }

    [RelayCommand]
    private async Task EditAchatAsync(AchatDto? achat)
    {
        if (achat == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser == null || !currentUser.AddAchat)
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des achats");
            return;
        }

        // Store old quantity before edit - use a copy of the value
        var oldQuantite = achat.Quantite ?? 0;
        System.Diagnostics.Debug.WriteLine($"[AchatVM] EditAchatAsync: AchatID={achat.ID}, OldQuantite (before popup)={oldQuantite}");

        var success = await _dialogService.ShowAchatEditPopupAsync(achat);
        System.Diagnostics.Debug.WriteLine($"[AchatVM] EditAchatAsync: Popup returned success={success}, achat.Quantite (after popup)={achat.Quantite}");
        
        if (!success) return;

        // Reload the achat to get updated values from DB
        var updatedAchat = await _achatService.GetByIdAsync(achat.ID);
        if (updatedAchat == null)
        {
            await LoadAchatsAsync();
            return;
        }

        var newQuantite = updatedAchat.Quantite ?? 0;
        System.Diagnostics.Debug.WriteLine($"[AchatVM] EditAchatAsync: NewQuantite (from DB)={newQuantite}, Difference={newQuantite - oldQuantite}");

        // Check if this is a fuel product that needs allocation adjustment
        var allocationStatus = await _allocationService.CheckAchatAllocationStatusAsync(achat.ID);
        System.Diagnostics.Debug.WriteLine($"[AchatVM] EditAchatAsync: AllocationStatus: EstCarburant={allocationStatus?.EstCarburant}, TotalAlloue={allocationStatus?.TotalAlloue}");
        
        if (allocationStatus?.EstCarburant == true && allocationStatus.TotalAlloue > 0)
        {
            // Fuel product with existing allocations - use adjustment flow
            var quantityDiff = Math.Abs(oldQuantite - newQuantite);
            System.Diagnostics.Debug.WriteLine($"[AchatVM] EditAchatAsync: Quantity difference = {quantityDiff}, needs adjustment = {quantityDiff > 0.01m}");

            if (quantityDiff > 0.01m)
            {
                // Quantity changed - need to adjust allocations
                var adjustResult = await ShowAllocationAdjustmentPopupAsync(updatedAchat, oldQuantite, newQuantite);
                
                if (adjustResult?.Success == true)
                {
                    await _dialogService.ShowAlertAsync("Succès", 
                        adjustResult.Message ?? "Achat modifié et allocations ajustées avec succès");
                }
                else if (adjustResult != null)
                {
                    // Adjustment failed - show error but achat was still updated
                    await _dialogService.ShowAlertAsync("Attention", 
                        $"L'achat a été modifié mais l'ajustement des allocations a échoué:\n{adjustResult.Message}");
                }
            }
            else
            {
                // No quantity change - just refresh
                System.Diagnostics.Debug.WriteLine("[AchatVM] EditAchatAsync: No quantity change detected");
                await _dialogService.ShowAlertAsync("Succès", "Achat modifié avec succès");
            }
        }
        else if (allocationStatus?.EstCarburant == true && allocationStatus.TotalAlloue == 0)
        {
            // Fuel product with no allocations yet - show allocation popup
            var allocationResult = await _dialogService.ShowAchatAllocationPopupForNewAchatAsync(updatedAchat);
            if (allocationResult?.Success == true)
            {
                await _dialogService.ShowAlertAsync("Succès", 
                    allocationResult.Message ?? "Achat modifié et allocation effectuée avec succès");
            }
        }
        else
        {
            // Non-fuel product
            await _dialogService.ShowAlertAsync("Succès", "Achat modifié avec succès");
        }

        await LoadAchatsAsync();
    }

    /// <summary>
    /// Shows the allocation adjustment popup when editing a fuel achat with existing allocations.
    /// </summary>
    private async Task<AllocationAdjustmentResultDto?> ShowAllocationAdjustmentPopupAsync(
        AchatDto achat, 
        decimal oldQuantite, 
        decimal newQuantite)
    {
        System.Diagnostics.Debug.WriteLine($"[AchatVM] ShowAllocationAdjustmentPopupAsync called: AchatID={achat.ID}, OldQte={oldQuantite}, NewQte={newQuantite}");

        // Get current allocations preview
        var preview = await _allocationService.GetAdjustmentPreviewAsync(achat.ID);
        if (preview == null)
        {
            System.Diagnostics.Debug.WriteLine($"[AchatVM] GetAdjustmentPreviewAsync returned null for Achat {achat.ID}");
            return new AllocationAdjustmentResultDto
            {
                Success = false,
                Message = "Impossible de récupérer les allocations actuelles",
                AchatId = achat.ID
            };
        }

        System.Diagnostics.Debug.WriteLine($"[AchatVM] Preview returned: {preview.Allocations.Count} allocations");
        foreach (var alloc in preview.Allocations)
        {
            System.Diagnostics.Debug.WriteLine($"[AchatVM]   Reservoir {alloc.ReservoirNumero}: CurrentQte={alloc.CurrentQuantite}, Consumed={alloc.ConsumedQuantite}, MaxReducible={alloc.MaxReducible}");
        }

        var difference = newQuantite - oldQuantite;
        var isIncrease = difference > 0;

        System.Diagnostics.Debug.WriteLine($"[AchatVM] Difference={difference}, IsIncrease={isIncrease}");

        // Build info message
        var changeInfo = isIncrease 
            ? $"Augmentation de {Math.Abs(difference):N2}L"
            : $"Réduction de {Math.Abs(difference):N2}L";

        // Show allocation details and ask for confirmation
        var allocationsInfo = string.Join("\n", preview.Allocations.Select(a => 
            $"  • {a.ReservoirNumero}: {a.CurrentQuantite:N2}L (consommé: {a.ConsumedQuantite:N2}L, max réductible: {a.MaxReducible:N2}L)"));

        var message = $"Quantité: {oldQuantite:N2}L → {newQuantite:N2}L ({changeInfo})\n\n" +
                      $"Allocations actuelles:\n{allocationsInfo}\n\n";

        if (isIncrease)
        {
            message += "L'augmentation sera ajoutée aux allocations existantes proportionnellement.\n\n" +
                       "Voulez-vous procéder à l'ajustement automatique?";
        }
        else
        {
            // Check if reduction is possible
            var totalMaxReducible = preview.Allocations.Sum(a => a.MaxReducible);
            var totalConsumed = preview.Allocations.Sum(a => a.ConsumedQuantite);
            var reductionNeeded = Math.Abs(difference);
            
            System.Diagnostics.Debug.WriteLine($"[AchatVM] TotalMaxReducible={totalMaxReducible}, TotalConsumed={totalConsumed}, ReductionNeeded={reductionNeeded}");

            if (reductionNeeded > totalMaxReducible)
            {
                System.Diagnostics.Debug.WriteLine($"[AchatVM] Reduction impossible: {reductionNeeded}L > {totalMaxReducible}L available");
                
                // Build a detailed message explaining why reduction is not possible
                var detailMessage = new System.Text.StringBuilder();
                detailMessage.AppendLine($"La réduction demandée ({reductionNeeded:N2}L) dépasse la quantité réductible ({totalMaxReducible:N2}L).");
                detailMessage.AppendLine();
                
                if (totalConsumed > 0)
                {
                    detailMessage.AppendLine($"⚠️ {totalConsumed:N2}L de cet achat ont déjà été vendus (consommés via les périodes).");
                    detailMessage.AppendLine();
                    detailMessage.AppendLine("Détails par réservoir:");
                    foreach (var alloc in preview.Allocations)
                    {
                        var status = alloc.ConsumedQuantite >= alloc.CurrentQuantite 
                            ? "entièrement vendu" 
                            : $"partiellement vendu ({alloc.ConsumedQuantite:N2}L)";
                        detailMessage.AppendLine($"  • {alloc.ReservoirNumero}: {alloc.CurrentQuantite:N2}L alloués, {status}");
                    }
                    detailMessage.AppendLine();
                    
                    if (totalMaxReducible > 0)
                    {
                        detailMessage.AppendLine($"Réduction maximale possible: {totalMaxReducible:N2}L");
                    }
                    else
                    {
                        detailMessage.AppendLine("Aucune réduction n'est possible car tout le stock a été vendu.");
                    }
                }
                else
                {
                    detailMessage.AppendLine("Le stock n'est pas disponible pour cette réduction.");
                }

                await _dialogService.ShowAlertAsync("Réduction impossible", detailMessage.ToString());
                
                return new AllocationAdjustmentResultDto
                {
                    Success = false,
                    Message = totalConsumed > 0 
                        ? $"Réduction impossible - {totalConsumed:N2}L déjà vendus sur cet achat"
                        : "Réduction impossible - stock insuffisant",
                    AchatId = achat.ID
                };
            }

            message += "La réduction sera appliquée aux allocations existantes proportionnellement.\n\n" +
                       "Voulez-vous procéder à l'ajustement automatique?";
        }

        var confirm = await _dialogService.ShowConfirmationAsync("Ajustement des allocations", message);
        System.Diagnostics.Debug.WriteLine($"[AchatVM] User confirmation: {confirm}");

        if (!confirm)
        {
            return new AllocationAdjustmentResultDto
            {
                Success = false,
                Message = "Ajustement annulé par l'utilisateur",
                AchatId = achat.ID
            };
        }

        // Calculate proportional adjustment for each allocation
        var adjustmentItems = new List<AllocationAdjustmentItemDto>();
        var totalCurrentAllocated = preview.Allocations.Sum(a => a.CurrentQuantite);

        System.Diagnostics.Debug.WriteLine($"[AchatVM] TotalCurrentAllocated={totalCurrentAllocated}");

        foreach (var alloc in preview.Allocations)
        {
            decimal newAllocQuantite;
            
            if (totalCurrentAllocated > 0)
            {
                // Proportional distribution
                var proportion = alloc.CurrentQuantite / totalCurrentAllocated;
                newAllocQuantite = Math.Round(newQuantite * proportion, 2);
            }
            else
            {
                newAllocQuantite = 0;
            }

            // For decrease, ensure we don't go below consumed amount
            if (!isIncrease)
            {
                var minAllowed = alloc.CurrentQuantite - alloc.MaxReducible;
                newAllocQuantite = Math.Max(newAllocQuantite, minAllowed);
            }

            System.Diagnostics.Debug.WriteLine($"[AchatVM]   Reservoir {alloc.ReservoirId}: NewQuantite={newAllocQuantite}");

            adjustmentItems.Add(new AllocationAdjustmentItemDto
            {
                AllocationId = alloc.AllocationId,
                ReservoirId = alloc.ReservoirId,
                NewQuantite = newAllocQuantite
            });
        }

        // Adjust rounding differences to match exact new quantity
        var totalAdjusted = adjustmentItems.Sum(a => a.NewQuantite);
        var roundingDiff = newQuantite - totalAdjusted;
        if (Math.Abs(roundingDiff) > 0.01m && adjustmentItems.Count > 0)
        {
            // Add rounding difference to the largest allocation
            var largestAlloc = adjustmentItems.OrderByDescending(a => a.NewQuantite).First();
            largestAlloc.NewQuantite += roundingDiff;
        }

        // Submit the adjustment
        var request = new AdjustAllocationsRequestDto
        {
            AchatId = achat.ID,
            NewAchatQuantite = newQuantite,
            Notes = $"Ajustement automatique: {oldQuantite:N2}L → {newQuantite:N2}L",
            Allocations = adjustmentItems
        };

        System.Diagnostics.Debug.WriteLine($"[AchatVM] Calling AdjustAllocationsAsync with {adjustmentItems.Count} items");
        var result = await _allocationService.AdjustAllocationsAsync(request);
        System.Diagnostics.Debug.WriteLine($"[AchatVM] AdjustAllocationsAsync result: Success={result?.Success}, Message={result?.Message}");

        return result;
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(AchatDto? achat)
    {
        if (achat == null) return;
        await _dialogService.ShowAchatDetailsPopupAsync(achat);
    }

    [RelayCommand]
    private async Task ShowAllocationAsync(AchatDto? achat)
    {
        if (achat == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.AddAchat)
        {
            var result = await _dialogService.ShowAchatAllocationPopupAsync(achat);
            if (result?.Success == true)
            {
                await _dialogService.ShowAlertAsync("Succès", result.Message ?? "Allocation effectuée avec succès");
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de gérer les allocations");
        }
    }

    [RelayCommand]
    private async Task DeleteAchatAsync(AchatDto? achat)
    {
        if (achat == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.AddAchat)
        {
            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer l'achat '{achat.Numero}'?\n\nAttention: Cette action peut affecter le stock du produit.");

            if (confirm)
            {
                try
                {
                    var success = await _achatService.DeleteAsync(achat.ID);
                    if (success)
                    {
                        _allAchats.Remove(achat);
                        Achats.Remove(achat);
                        CalculateStatistics();
                        await _dialogService.ShowAlertAsync("Succès", "Achat supprimé avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression de l'achat");
                    }
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors de la suppression: {ex.Message}");
                }
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des achats");
        }
    }

    [RelayCommand]
    private async Task ShowAchatRowDetailsAsync(AchatDto? achat)
    {
        if (achat == null) return;

        var defectStatus = achat.LivraisonDefectueuse == true ? "⚠️ Oui" : "Non";

        await _dialogService.ShowAlertAsync("Détails de l'achat",
            $"Numéro: {achat.Numero}\n" +
            $"Date: {achat.Date:dd/MM/yyyy}\n" +
            $"Fournisseur: {achat.FournisseurNom ?? "N/A"}\n" +
            $"Produit: {achat.ProduitNom ?? "N/A"}\n" +
            $"Chauffeur: {achat.ChauffeurNom ?? "N/A"}\n" +
            $"Camion: {achat.CamionImmatriculation ?? "N/A"}\n" +
            $"Quantité: {achat.Quantite} L\n" +
            $"Prix Unitaire: {achat.PrixAchatUnitaire:N2} DH\n" +
            $"Coût Total: {achat.Cout:N2} DH\n" +
            $"Livraison défectueuse: {defectStatus}");
    }

    [RelayCommand]
    private async Task SearchAchatsAsync()
    {
        try
        {
            IsLoading = true;
            
            var allAchats = await _achatService.GetAllAsync();
            
            List<AchatDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = allAchats;
            }
            else
            {
                var searchTerm = SearchText.ToLower();
                results = allAchats.Where(a =>
                    (a.Numero?.ToLower().Contains(searchTerm) ?? false) ||
                    (a.FournisseurNom?.ToLower().Contains(searchTerm) ?? false) ||
                    (a.ProduitNom?.ToLower().Contains(searchTerm) ?? false) ||
                    (a.ChauffeurNom?.ToLower().Contains(searchTerm) ?? false) ||
                    (a.CamionImmatriculation?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
            }

            // Store and apply current sorting
            _allAchats = results;
            ApplySorting();
            CalculateStatistics();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors de la recherche: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FilterByThisMonthAsync()
    {
        try
        {
            IsLoading = true;
            var startDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = DateOnly.FromDateTime(DateTime.Now);
            
            var achats = await _achatService.GetByDateRangeAsync(startDate, endDate);
            
            // Store and apply current sorting
            _allAchats = achats;
            ApplySorting();
            CalculateStatistics();

            if (achats.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Information", "Aucun achat ce mois-ci.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors du filtrage: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

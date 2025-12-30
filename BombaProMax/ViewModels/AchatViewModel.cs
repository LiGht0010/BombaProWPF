using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class AchatViewModel : ObservableObject
{
    private readonly AchatService _achatService;
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
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _achatService = achatService;
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
        if (currentUser != null && currentUser.AddAchat)
        {
            var success = await _dialogService.ShowAchatEditPopupAsync(achat);
            if (success)
            {
                await LoadAchatsAsync();

                // Clear existing allocations and open allocation popup for re-allocation
                var allocationResult = await _dialogService.ClearAndShowAllocationPopupAsync(achat);
                if (allocationResult?.Success == true)
                {
                    await _dialogService.ShowAlertAsync("Succès", 
                        allocationResult.Message ?? "Achat modifié et ré-allocation effectuée avec succès");
                }
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des achats");
        }
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

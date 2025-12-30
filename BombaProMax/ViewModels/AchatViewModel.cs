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
    // EXISTING COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    public async Task LoadAchatsAsync()
    {
        try
        {
            IsLoading = true;
            var achats = await _achatService.GetAllAsync();
            Achats.Clear();
            foreach (var achat in achats)
            {
                Achats.Add(achat);
            }

            CalculateStatistics();
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

            Achats.Clear();
            foreach (var achat in results)
            {
                Achats.Add(achat);
            }
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
            
            Achats.Clear();
            foreach (var achat in achats)
            {
                Achats.Add(achat);
            }
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

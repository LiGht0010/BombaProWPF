using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class ProduitViewModel : ObservableObject
{
    private readonly ProduitService _produitService;
    private readonly IDialogService _dialogService;
    private readonly JourneeNavigationService _journeeService;

    public ObservableCollection<ProduitDto> Produits { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ProduitDto? _selectedProduit;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES (bound to service)
    // ════════════════════════════════════════════════════════════════
    
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public ProduitViewModel(
        ProduitService produitService, 
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _produitService = produitService;
        _dialogService = dialogService;
        _journeeService = journeeService;

        // Subscribe to journée service property changes
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
    private async Task JourneeSuivantAsync()
    {
        await _journeeService.GoNextAsync(skipped: false);
    }

    [RelayCommand]
    private async Task JourneePasserAsync()
    {
        await _journeeService.GoNextAsync(skipped: true);
    }

    [RelayCommand]
    private async Task JourneePrecedentAsync()
    {
        await _journeeService.GoPreviousAsync();
    }

    // ════════════════════════════════════════════════════════════════
    // EXISTING COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    public async Task LoadProduitsAsync()
    {
        try
        {
            IsLoading = true;
            var produits = await _produitService.GetAllProduitsAsync();
            Produits.Clear();
            foreach (var produit in produits)
            {
                Produits.Add(produit);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les produits: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddProduitAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newProduit = await _dialogService.ShowProduitCreatePopupAsync();
            if (newProduit != null)
            {
                Produits.Insert(0, newProduit);
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des produits");
        }
    }

    [RelayCommand]
    private async Task EditProduitAsync(ProduitDto? produit)
    {
        if (produit == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var success = await _dialogService.ShowProduitEditPopupAsync(produit);
            if (success)
            {
                await LoadProduitsAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des produits");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(ProduitDto? produit)
    {
        if (produit == null) return;
        await _dialogService.ShowProduitDetailsPopupAsync(produit);
    }

    [RelayCommand]
    private async Task DeleteProduitAsync(ProduitDto? produit)
    {
        if (produit == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var displayName = !string.IsNullOrWhiteSpace(produit.Description)
                ? produit.Description
                : produit.NumeroProduit;

            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer le produit '{displayName}'?");

            if (confirm)
            {
                try
                {
                    var success = await _produitService.DeleteProduitAsync(produit.ID);
                    if (success)
                    {
                        Produits.Remove(produit);
                        await _dialogService.ShowAlertAsync("Succès", "Produit supprimé avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression. Le produit peut être utilisé dans des achats ou factures.");
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
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des produits");
        }
    }

    [RelayCommand]
    private async Task ShowProduitRowDetailsAsync(ProduitDto? produit)
    {
        if (produit == null) return;

        await _dialogService.ShowAlertAsync("Détails du produit",
            $"Numéro: {produit.NumeroProduit}\n" +
            $"Description: {produit.Description ?? "N/A"}\n" +
            $"Catégorie: {produit.CategorieNom ?? "Non catégorisé"}\n" +
            $"Prix HT: {produit.PrixHT:F2} DH\n" +
            $"TVA: {produit.TVA}%\n" +
            $"Prix TTC: {produit.PrixTTC:F2} DH\n" +
            $"Stock: {produit.Stock ?? 0}\n" +
            $"Stock Min: {produit.StockMinimum ?? 0}");
    }

    [RelayCommand]
    private async Task SearchProduitsAsync()
    {
        try
        {
            IsLoading = true;

            List<ProduitDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _produitService.GetAllProduitsAsync();
            }
            else
            {
                results = await _produitService.SearchProduitsAsync(SearchText);
            }

            Produits.Clear();
            foreach (var produit in results)
            {
                Produits.Add(produit);
            }
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
    private async Task ShowLowStockAsync()
    {
        try
        {
            IsLoading = true;
            var lowStockProduits = await _produitService.GetLowStockProduitsAsync();

            Produits.Clear();
            foreach (var produit in lowStockProduits)
            {
                Produits.Add(produit);
            }

            if (lowStockProduits.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Information", "Aucun produit avec stock faible.");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Stock faible", $"{lowStockProduits.Count} produit(s) avec stock faible trouvé(s).");
            }
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
}

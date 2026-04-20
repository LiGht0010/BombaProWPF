using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMaxWPF.ViewModels;

public partial class ProduitViewModel : ObservableObject
{
    private readonly ProduitService _produitService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<ProduitDto> Produits { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ProduitDto? _selectedProduit;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ProduitViewModel(
        ProduitService produitService, 
        IDialogService dialogService)
    {
        _produitService = produitService;
        _dialogService = dialogService;
    }

    // ════════════════════════════════════════════════════════════════
    // COMMANDS
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

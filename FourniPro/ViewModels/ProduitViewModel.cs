using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace FourniPro.ViewModels;

public partial class ProduitViewModel : ObservableObject
{
    private readonly ProduitService _produitService = new();

    public ObservableCollection<ProduitDto> Produits { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ProduitDto? _selectedProduit;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => _ = SearchProduitsAsync();

    // ════════════════════════════════════════════════════════════════
    // LOAD
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    public async Task LoadProduitsAsync()
    {
        IsLoading = true;
        try
        {
            var produits = await _produitService.GetAllProduitsAsync();
            Produits.Clear();
            foreach (var p in produits)
                Produits.Add(p);
        }
        catch (Exception ex)
        {
            ShowError($"Impossible de charger les produits: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // SEARCH
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task SearchProduitsAsync()
    {
        IsLoading = true;
        try
        {
            var results = await _produitService.SearchProduitsAsync(SearchText);
            Produits.Clear();
            foreach (var p in results)
                Produits.Add(p);
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors de la recherche: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // LOW STOCK
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ShowLowStockAsync()
    {
        IsLoading = true;
        try
        {
            var lowStock = await _produitService.GetLowStockProduitsAsync();
            Produits.Clear();
            foreach (var p in lowStock)
                Produits.Add(p);

            var msg = lowStock.Count == 0
                ? "Aucun produit avec stock faible."
                : $"{lowStock.Count} produit(s) avec stock faible.";
            MessageBox.Show(msg, "Stock faible", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task AddProduitAsync()
    {
        if (!CanManageProduits()) return;

        // TODO: open create dialog and get back a filled ProduitDto
        // var newProduit = await ShowCreateDialogAsync();
        // if (newProduit is null) return;

        // var created = await _produitService.CreateProduitAsync(newProduit);
        // if (created is not null)
        //     Produits.Insert(0, created);
        MessageBox.Show("Dialogue de création à implémenter.", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ════════════════════════════════════════════════════════════════
    // EDIT
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task EditProduitAsync(ProduitDto? produit)
    {
        if (produit is null) return;
        if (!CanManageProduits()) return;

        // TODO: open edit dialog, then:
        // var success = await _produitService.UpdateProduitAsync(produit);
        // if (success) await LoadProduitsAsync();
        MessageBox.Show("Dialogue d'édition à implémenter.", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);

        await Task.CompletedTask;
    }

    // ════════════════════════════════════════════════════════════════
    // DELETE
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task DeleteProduitAsync(ProduitDto? produit)
    {
        if (produit is null) return;
        if (!CanManageProduits()) return;

        var confirm = MessageBox.Show(
            $"Êtes-vous sûr de vouloir supprimer '{produit.DisplayName}' ?",
            "Confirmer la suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            var success = await _produitService.DeleteProduitAsync(produit.ProduitId);
            if (success)
            {
                Produits.Remove(produit);
                MessageBox.Show("Produit supprimé avec succès.", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowError("Échec de la suppression. Le produit est peut-être référencé ailleurs.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors de la suppression: {ex.Message}");
        }
    }

    // ════════════════════════════════════════════════════════════════
    // DETAILS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    private void ShowDetails(ProduitDto? produit)
    {
        if (produit is null) return;

        MessageBox.Show(
            $"Numéro: {produit.NumeroProduit}\n" +
            $"Description: {produit.Description ?? "N/A"}\n" +
            $"Prix Achat: {produit.PrixAchat:F2} DH\n" +
            $"Prix HT: {produit.PrixHT:F2} DH\n" +
            $"TVA: {produit.TVA}%\n" +
            $"Prix TTC: {produit.PrixTTC:F2} DH\n" +
            $"Marge: {produit.MargeBeneficiaire:F2} DH ({produit.MargePourcentage:F1}%)\n" +
            $"Stock: {produit.Stock ?? 0} / Min: {produit.StockMinimum ?? 0}",
            "Détails du produit", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════

    private static bool CanManageProduits()
    {
        var user = App.CurrentUser;
        if (user is null || !user.HasPermission("produits.manage"))
        {
            MessageBox.Show("Vous n'avez pas la permission de gérer les produits.",
                "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private static void ShowError(string message) =>
        MessageBox.Show(message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
}

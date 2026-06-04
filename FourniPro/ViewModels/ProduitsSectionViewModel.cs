using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Produits;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped view-model for the Produits sub-section of the
/// Infrastructure shell. Lists all products and exposes add / edit / delete
/// actions.
/// </summary>
public partial class ProduitsSectionViewModel : ObservableObject
{
    private readonly ProduitService _produitService = new();
    private bool _isLoaded;

    public ObservableCollection<ProduitCardItem> Produits { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand AddProduitCommand { get; }
    public IRelayCommand<ProduitCardItem> EditProduitCommand { get; }
    public IRelayCommand<ProduitCardItem> DetailProduitCommand { get; }
    public IAsyncRelayCommand<ProduitCardItem> DeleteProduitCommand { get; }

    public ProduitsSectionViewModel()
    {
        RefreshCommand       = new AsyncRelayCommand(ct => RefreshAsync(ct));
        AddProduitCommand    = new RelayCommand(OpenAddProduit);
        EditProduitCommand   = new RelayCommand<ProduitCardItem>(OpenEditProduit);
        DetailProduitCommand = new RelayCommand<ProduitCardItem>(OpenDetailProduit);
        DeleteProduitCommand = new AsyncRelayCommand<ProduitCardItem>(DeleteProduitAsync);
    }

    /// <summary>Loads products once; subsequent calls are no-ops unless <see cref="RefreshAsync"/> is used.</summary>
    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (_isLoaded) return;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        _isLoaded = false;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = null;

            var list = await _produitService.GetAllProduitsAsync().ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Produits.Clear();
                foreach (var p in list)
                    Produits.Add(new ProduitCardItem(p));
                _isLoaded = true;
            });
        }
        catch (OperationCanceledException) { /* navigated away — no-op */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[ProduitsSectionVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAddProduit()
    {
        try
        {
            ErrorMessage = null;
            var dialog = new NouveauProduitDialog
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
            if (dialog.ViewModel.Saved)
                _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ProduitsSectionVM] OpenAddProduit failed: {ex}");
        }
    }

    private void OpenEditProduit(ProduitCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var dialog = new EditProduitDialog(item.Dto)
            {
                Owner = Application.Current?.MainWindow
            };
            dialog.ShowDialog();
            if (dialog.ViewModel.Saved)
                _ = RefreshAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ProduitsSectionVM] OpenEditProduit failed: {ex}");
        }
    }

    private void OpenDetailProduit(ProduitCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            var detail = new DetailProduitDialog(item.Dto)
            {
                Owner = Application.Current?.MainWindow
            };
            detail.ShowDialog();

            if (detail.ShouldEdit)
            {
                // Chain directly into the edit dialog
                var edit = new EditProduitDialog(item.Dto)
                {
                    Owner = Application.Current?.MainWindow
                };
                edit.ShowDialog();
                if (edit.ViewModel.Saved)
                    _ = RefreshAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Impossible d'ouvrir le dialogue: {ex.Message}";
            Debug.WriteLine($"[ProduitsSectionVM] OpenDetailProduit failed: {ex}");
        }
    }

    private async Task DeleteProduitAsync(ProduitCardItem? item)
    {
        if (item is null) return;

        var confirm = MessageBox.Show(
            $"Supprimer « {item.Description} » ?",
            "Confirmer la suppression",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            ErrorMessage   = null;
            SuccessMessage = null;

            var ok = await _produitService.DeleteProduitAsync(item.ProduitId).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                {
                    Produits.Remove(item);
                    SuccessMessage = LanguageManager.Instance["ProdDeleteSuccess"];
                }
                else
                {
                    ErrorMessage = LanguageManager.Instance["ProdDeleteError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
                ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[ProduitsSectionVM] Delete failed: {ex}");
        }
    }
}

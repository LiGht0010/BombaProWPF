using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// Section-scoped view-model for the Produits sub-section of the
/// Infrastructure shell. Lists all products and exposes add / edit / delete
/// actions via self-contained dialogs (no IDialogService dependency).
/// </summary>
public partial class ProduitsSectionViewModel : ObservableObject, IAsyncLoadable
{
    private readonly ProduitService _produitService = new();

    public ObservableCollection<ProduitCardItem> Produits { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _successMessage;
    [ObservableProperty] private string _searchText = string.Empty;

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand AddProduitCommand { get; }
    public IRelayCommand<ProduitCardItem> EditProduitCommand { get; }
    public IAsyncRelayCommand<ProduitCardItem> DeleteProduitCommand { get; }

    public ProduitsSectionViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(ct => RefreshAsync(ct));
        AddProduitCommand = new RelayCommand(OpenAddProduit);
        EditProduitCommand = new RelayCommand<ProduitCardItem>(OpenEditProduit);
        DeleteProduitCommand = new AsyncRelayCommand<ProduitCardItem>(DeleteProduitAsync);
    }

    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (IsLoaded) return;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        IsLoaded = false;
        await LoadAsync(ct).ConfigureAwait(false);
    }

    private async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var list = await _produitService.GetAllProduitsAsync().ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Produits.Clear();
                foreach (var p in list)
                    Produits.Add(new ProduitCardItem(p));
                IsLoaded = true;
            });
        }
        catch (OperationCanceledException) { /* navigated away */ }
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
        // TODO: open NouveauProduitDialog when implemented
        ErrorMessage = null;
    }

    private void OpenEditProduit(ProduitCardItem? item)
    {
        if (item is null) return;
        // TODO: open EditProduitDialog when implemented
        ErrorMessage = null;
    }

    private async Task DeleteProduitAsync(ProduitCardItem? item)
    {
        if (item is null) return;
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            var ok = await _produitService.DeleteProduitAsync(item.ID).ConfigureAwait(false);

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

/// <summary>
/// Wraps a <see cref="ProduitDto"/> and exposes display-friendly computed
/// properties for the products table card.
/// </summary>
public class ProduitCardItem(ProduitDto dto)
{
    public int ID => dto.ID;
    public string Numero => dto.NumeroProduit;
    public string Description => string.IsNullOrWhiteSpace(dto.Description) ? "—" : dto.Description;
    public string Categorie => string.IsNullOrWhiteSpace(dto.CategorieNom) ? "—" : dto.CategorieNom!;
    public string PrixAchatDisplay => dto.PrixAchat.HasValue ? $"{dto.PrixAchat:N2} DA" : "—";
    public string PrixTtcDisplay => dto.PrixTTC.HasValue ? $"{dto.PrixTTC:N2} DA" : "—";
    public string MargeDisplay => dto.MargePourcentage.HasValue ? $"{dto.MargePourcentage:N1}%" : "—";
    public int Stock => dto.Stock ?? 0;
    public int StockMinimum => dto.StockMinimum ?? 0;

    /// <summary>"Ok" | "Bas" | "Rupture" — drives the badge colour.</summary>
    public string StockState =>
        Stock <= 0 ? "Rupture" :
        StockMinimum > 0 && Stock <= StockMinimum ? "Bas" : "Ok";

    public string StockDisplay => Stock.ToString();
}

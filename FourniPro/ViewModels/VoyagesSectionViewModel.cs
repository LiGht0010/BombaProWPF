using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped viewmodel for Voyages (Voyage + StockVoyage + FraisVoyage).
/// </summary>
public class VoyagesSectionViewModel : ObservableObject
{
    private readonly VoyageService      _voyageService      = new();
    private readonly StockVoyageService _stockService       = new();
    private readonly FraisVoyageService _fraisService       = new();
    private bool _loaded;

    // ── Voyages ──────────────────────────────────────────────────────────────

    private ObservableCollection<VoyageCardItem> _voyages = [];
    public ObservableCollection<VoyageCardItem> Voyages
    {
        get => _voyages;
        set => SetProperty(ref _voyages, value);
    }

    // ── StockVoyages ─────────────────────────────────────────────────────────

    private ObservableCollection<StockVoyageCardItem> _stocks = [];
    public ObservableCollection<StockVoyageCardItem> Stocks
    {
        get => _stocks;
        set => SetProperty(ref _stocks, value);
    }

    // ── FraisVoyages ─────────────────────────────────────────────────────────

    private ObservableCollection<FraisVoyageCardItem> _frais = [];
    public ObservableCollection<FraisVoyageCardItem> Frais
    {
        get => _frais;
        set => SetProperty(ref _frais, value);
    }

    // ── Loading ───────────────────────────────────────────────────────────────

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public IRelayCommand RefreshCommand              { get; }
    public IRelayCommand AddVoyageCommand             { get; }
    public IRelayCommand<VoyageCardItem>      DeleteVoyageCommand      { get; }
    public IRelayCommand<StockVoyageCardItem> DeleteStockCommand       { get; }
    public IRelayCommand<FraisVoyageCardItem> DeleteFraisCommand       { get; }

    public VoyagesSectionViewModel()
    {
        RefreshCommand        = new AsyncRelayCommand(RefreshAsync);
        AddVoyageCommand      = new RelayCommand(OpenAddVoyageDialog);
        DeleteVoyageCommand   = new AsyncRelayCommand<VoyageCardItem>(DeleteVoyageAsync);
        DeleteStockCommand    = new AsyncRelayCommand<StockVoyageCardItem>(DeleteStockAsync);
        DeleteFraisCommand    = new AsyncRelayCommand<FraisVoyageCardItem>(DeleteFraisAsync);
    }

    private void OpenAddVoyageDialog()
    {
        // Delegate to code-behind via the OpenAddDialog action
        OpenAddDialog?.Invoke();
    }

    /// <summary>Set by VoyagesSection code-behind to open NouveauVoyageDialog on the UI thread.</summary>
    public Action? OpenAddDialog { get; set; }

    public async Task EnsureLoadedAsync()
    {
        if (!_loaded) await LoadAsync();
    }

    public async Task RefreshAsync()
    {
        _loaded = false;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var voyageDtos = await _voyageService.GetAllVoyagesAsync();
            Voyages = new ObservableCollection<VoyageCardItem>(
                voyageDtos.Select(d => new VoyageCardItem
                {
                    VoyageId           = d.VoyageId,
                    CamionMatricule    = d.CamionMatricule,
                    ChauffeurNom       = d.ChauffeurNom,
                    CiterneMatricule   = d.CiterneMatricule,
                    DateDepart         = d.DateDepart,
                    DateFinal          = d.DateFinal,
                    LieuDepart         = d.LieuDepart,
                    LieuTerminal       = d.LieuTerminal,
                    KilometrageDepart  = d.KilometrageDepart,
                    KilometrageFinal   = d.KilometrageFinal,
                    Statut             = d.Statut,
                }));

            var stockDtos = await _stockService.GetAllAsync();
            Stocks = new ObservableCollection<StockVoyageCardItem>(
                stockDtos.Select(d => new StockVoyageCardItem
                {
                    StockVoyageId = d.StockVoyageId,
                    VoyageId      = d.VoyageId,
                    ProduitNom    = d.ProduitNom,
                    Quantite      = d.Quantite,
                }));

            var fraisDtos = await _fraisService.GetAllAsync();
            Frais = new ObservableCollection<FraisVoyageCardItem>(
                fraisDtos.Select(d => new FraisVoyageCardItem
                {
                    FraisVoyageId = d.FraisVoyageId,
                    VoyageId      = d.VoyageId,
                    Type          = d.Type,
                    Montant       = d.Montant,
                    Description   = d.Description,
                }));

            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoyagesSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private async Task DeleteVoyageAsync(VoyageCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer le voyage VOY-{item.VoyageId:D5} ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _voyageService.DeleteVoyageAsync(item.VoyageId);
        if (ok)
        {
            Voyages.Remove(item);
            // Remove orphaned stock/frais from local collections too
            foreach (var s in Stocks.Where(x => x.VoyageId == item.VoyageId).ToList())
                Stocks.Remove(s);
            foreach (var f in Frais.Where(x => x.VoyageId == item.VoyageId).ToList())
                Frais.Remove(f);
        }
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task DeleteStockAsync(StockVoyageCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer cet enregistrement de stock ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _stockService.DeleteAsync(item.StockVoyageId);
        if (ok) Stocks.Remove(item);
        else    MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task DeleteFraisAsync(FraisVoyageCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer ce frais de voyage ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _fraisService.DeleteAsync(item.FraisVoyageId);
        if (ok) Frais.Remove(item);
        else    MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

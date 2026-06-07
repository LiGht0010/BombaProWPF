using CommunityToolkit.Mvvm.ComponentModel;
using FourniPro.Models;
using FourniPro.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FourniPro.ViewModels;

/// <summary>
/// Loads and exposes the per-voyage transaction and stock data for DetailVoyageDialog.
/// </summary>
public class DetailVoyageViewModel : ObservableObject
{
    private readonly VenteService        _venteService   = new();
    private readonly CreditService       _creditService  = new();
    private readonly StockVoyageService  _stockService   = new();
    private readonly FraisVoyageService  _fraisService   = new();

    public VoyageCardItem Voyage { get; }

    // ── Collections ──────────────────────────────────────────────────────────

    private ObservableCollection<VenteDto> _ventes = [];
    public ObservableCollection<VenteDto> Ventes
    {
        get => _ventes;
        private set => SetProperty(ref _ventes, value);
    }

    private ObservableCollection<CreditDto> _credits = [];
    public ObservableCollection<CreditDto> Credits
    {
        get => _credits;
        private set => SetProperty(ref _credits, value);
    }

    private ObservableCollection<StockVoyageDto> _stocks = [];
    public ObservableCollection<StockVoyageDto> Stocks
    {
        get => _stocks;
        private set => SetProperty(ref _stocks, value);
    }

    private ObservableCollection<FraisVoyageDto> _frais = [];
    public ObservableCollection<FraisVoyageDto> Frais
    {
        get => _frais;
        private set => SetProperty(ref _frais, value);
    }

    // ── Loading state ─────────────────────────────────────────────────────────

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public DetailVoyageViewModel(VoyageCardItem voyage)
    {
        Voyage = voyage;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var tVentes  = _venteService.GetByVoyageAsync(Voyage.VoyageId);
            var tCredits = _creditService.GetByVoyageAsync(Voyage.VoyageId);
            var tStocks  = _stockService.GetByVoyageAsync(Voyage.VoyageId);
            var tFrais   = _fraisService.GetByVoyageAsync(Voyage.VoyageId);

            await Task.WhenAll(tVentes, tCredits, tStocks, tFrais);

            Ventes  = new ObservableCollection<VenteDto>(tVentes.Result);
            Credits = new ObservableCollection<CreditDto>(tCredits.Result);
            Stocks  = new ObservableCollection<StockVoyageDto>(tStocks.Result);
            Frais   = new ObservableCollection<FraisVoyageDto>(tFrais.Result);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DetailVoyageVM] Load error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

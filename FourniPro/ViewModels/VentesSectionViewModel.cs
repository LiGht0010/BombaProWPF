using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Automation;
using FourniPro.Automation.Vente;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Ventes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped viewmodel for the Ventes list and actions.
/// </summary>
public class VentesSectionViewModel : ObservableObject
{
    private readonly VenteService     _service          = new();
    private readonly AutomationRunner _automationRunner = new();
    private bool _loaded;

    private ObservableCollection<VenteCardItem> _ventes = [];
    public ObservableCollection<VenteCardItem> Ventes
    {
        get => _ventes;
        set => SetProperty(ref _ventes, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddVenteCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<VenteCardItem> DetailVenteCommand { get; }
    public IRelayCommand<VenteCardItem> EditVenteCommand { get; }
    public IRelayCommand<VenteCardItem> DeleteVenteCommand { get; }

    public VentesSectionViewModel()
    {
        AddVenteCommand    = new RelayCommand(OpenAddVente);
        RefreshCommand     = new AsyncRelayCommand(RefreshAsync);
        DetailVenteCommand = new AsyncRelayCommand<VenteCardItem>(OpenDetailVenteAsync);
        EditVenteCommand   = new AsyncRelayCommand<VenteCardItem>(OpenEditVenteAsync);
        DeleteVenteCommand = new AsyncRelayCommand<VenteCardItem>(DeleteVenteAsync);
    }

    public async Task EnsureLoadedAsync()
    {
        if (!_loaded) await LoadAsync();
    }

    private async Task RefreshAsync()
    {
        _loaded = false;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var dtos = await _service.GetAllVentesAsync();
            Ventes = new ObservableCollection<VenteCardItem>(
                dtos.Select(d => new VenteCardItem
                {
                    VenteId       = d.VenteId,
                    NumeroVente   = d.NumeroVente,
                    DateVente     = d.DateVente,
                    ClientNom     = d.ClientNom,
                    ProduitNom    = d.ProduitNom,
                    EmployeNom    = d.EmployeNom,
                    VoyageID      = d.VoyageID,
                    VoyageNumero  = d.VoyageNumero,
                    Quantite      = d.Quantite,
                    PrixUnitaire  = d.PrixUnitaire,
                    MontantTotal  = d.MontantTotal,
                    PaymentMethod = d.PaymentMethod
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VentesSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddVente()
    {
        var dlg = new NouveauVenteDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenEditVenteAsync(VenteCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetVenteByIdAsync(item.VenteId);
        if (dto is null) return;

        var dlg = new EditVenteDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            await RefreshAsync();
    }

    private async Task OpenDetailVenteAsync(VenteCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetVenteByIdAsync(item.VenteId);
        if (dto is null) return;

        var dlg = new DetailVenteDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();

        if (dlg.ShouldEdit)
        {
            var editDlg = new EditVenteDialog(dto)
            {
                Owner = Application.Current?.MainWindow
            };
            editDlg.ShowDialog();
            if (editDlg.ViewModel.Saved)
                await RefreshAsync();
        }
    }

    private async Task DeleteVenteAsync(VenteCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer la vente \"{item.NumeroVente}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        // Fetch full DTO to get ClientId and ProduitId before the record is removed.
        var dto = await _service.GetVenteByIdAsync(item.VenteId);

        var ok = await _service.DeleteVenteAsync(item.VenteId);
        if (ok)
        {
            Ventes.Remove(item);

            if (dto is not null)
            {
                await _automationRunner.RunAsync(
                    AutomationTrigger.VenteDeleted,
                    new VenteDeletedContext(
                        VenteId:      dto.VenteId,
                        ClientId:     dto.ClientID     ?? 0,
                        ProduitId:    dto.ProduitID    ?? 0,
                        Quantite:     dto.Quantite     ?? 0,
                        PrixUnitaire: dto.PrixUnitaire ?? 0m,
                        MontantTotal: dto.MontantTotal ?? 0m));
            }
        }
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

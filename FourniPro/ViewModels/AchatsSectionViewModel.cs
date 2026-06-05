using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Achats;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped viewmodel for the Achats list and actions.
/// </summary>
public class AchatsSectionViewModel : ObservableObject
{
    private readonly AchatService _service = new();
    private bool _loaded;

    private ObservableCollection<AchatCardItem> _achats = [];
    public ObservableCollection<AchatCardItem> Achats
    {
        get => _achats;
        set => SetProperty(ref _achats, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddAchatCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<AchatCardItem> DetailAchatCommand { get; }
    public IRelayCommand<AchatCardItem> EditAchatCommand { get; }
    public IRelayCommand<AchatCardItem> DeleteAchatCommand { get; }

    public AchatsSectionViewModel()
    {
        AddAchatCommand    = new RelayCommand(OpenAddAchat);
        RefreshCommand     = new AsyncRelayCommand(RefreshAsync);
        DetailAchatCommand = new AsyncRelayCommand<AchatCardItem>(OpenDetailAchatAsync);
        EditAchatCommand   = new AsyncRelayCommand<AchatCardItem>(OpenEditAchatAsync);
        DeleteAchatCommand = new AsyncRelayCommand<AchatCardItem>(DeleteAchatAsync);
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
            var dtos = await _service.GetAllAchatsAsync();
            Achats = new ObservableCollection<AchatCardItem>(
                dtos.Select(d => new AchatCardItem
                {
                    AchatId              = d.AchatId,
                    Numero               = d.Numero,
                    Date                 = d.Date,
                    FournisseurNom       = d.FournisseurNom,
                    ProduitNom           = d.ProduitNom,
                    EmployeNom           = d.EmployeNom,
                    Quantite             = d.Quantite,
                    PrixAchatUnitaire    = d.PrixAchatUnitaire,
                    Cout                 = d.Cout,
                    LivraisonDefectueuse = d.LivraisonDefectueuse
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AchatsSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddAchat()
    {
        var dlg = new NouveauAchatDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenEditAchatAsync(AchatCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetAchatByIdAsync(item.AchatId);
        if (dto is null) return;

        var dlg = new EditAchatDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            await RefreshAsync();
    }

    private async Task OpenDetailAchatAsync(AchatCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetAchatByIdAsync(item.AchatId);
        if (dto is null) return;

        var dlg = new DetailAchatDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();

        if (dlg.ShouldEdit)
        {
            var editDlg = new EditAchatDialog(dto)
            {
                Owner = Application.Current?.MainWindow
            };
            editDlg.ShowDialog();
            if (editDlg.ViewModel.Saved)
                await RefreshAsync();
        }
    }

    private async Task DeleteAchatAsync(AchatCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer l'achat \"{item.Numero}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteAchatAsync(item.AchatId);
        if (ok)
            Achats.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Camions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped viewmodel for the Camions list and actions.
/// </summary>
public class CamionsSectionViewModel : ObservableObject
{
    private readonly CamionService _service = new();
    private bool _loaded;

    private ObservableCollection<CamionCardItem> _camions = [];
    public ObservableCollection<CamionCardItem> Camions
    {
        get => _camions;
        set => SetProperty(ref _camions, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddCamionCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<CamionCardItem> DetailCamionCommand { get; }
    public IRelayCommand<CamionCardItem> EditCamionCommand { get; }
    public IRelayCommand<CamionCardItem> DeleteCamionCommand { get; }

    public CamionsSectionViewModel()
    {
        AddCamionCommand    = new RelayCommand(OpenAddCamion);
        RefreshCommand      = new AsyncRelayCommand(RefreshAsync);
        DetailCamionCommand = new AsyncRelayCommand<CamionCardItem>(OpenDetailCamionAsync);
        EditCamionCommand   = new AsyncRelayCommand<CamionCardItem>(OpenEditCamionAsync);
        DeleteCamionCommand = new AsyncRelayCommand<CamionCardItem>(DeleteCamionAsync);
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
            var dtos = await _service.GetAllCamionsAsync();
            Camions = new ObservableCollection<CamionCardItem>(
                dtos.Select(d => new CamionCardItem
                {
                    CamionId     = d.CamionId,
                    Matricule    = d.Matricule,
                    Marque       = d.Marque,
                    Consommation = d.Consommation,
                    Kilometrage  = d.Kilometrage
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CamionsSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddCamion()
    {
        var dlg = new NouveauCamionDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenEditCamionAsync(CamionCardItem? item)
    {
        if (item is null) return;
        var dto = await _service.GetCamionByIdAsync(item.CamionId);
        if (dto is null) return;

        var dlg = new EditCamionDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenDetailCamionAsync(CamionCardItem? item)
    {
        if (item is null) return;
        var dto = await _service.GetCamionByIdAsync(item.CamionId);
        if (dto is null) return;

        var dlg = new DetailCamionDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ShouldEdit)
            await OpenEditCamionAsync(item);
    }

    private async Task DeleteCamionAsync(CamionCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer le camion \"{item.Matricule}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteCamionAsync(item.CamionId);
        if (ok)
            Camions.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

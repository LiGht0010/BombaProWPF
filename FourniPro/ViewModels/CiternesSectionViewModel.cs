using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Citernes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped viewmodel for the Citernes list and actions.
/// </summary>
public class CiternesSectionViewModel : ObservableObject
{
    private readonly CiterneService _service = new();
    private bool _loaded;

    private ObservableCollection<CiterneCardItem> _citernes = [];
    public ObservableCollection<CiterneCardItem> Citernes
    {
        get => _citernes;
        set => SetProperty(ref _citernes, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddCiterneCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<CiterneCardItem> DetailCiterneCommand { get; }
    public IRelayCommand<CiterneCardItem> EditCiterneCommand { get; }
    public IRelayCommand<CiterneCardItem> DeleteCiterneCommand { get; }

    public CiternesSectionViewModel()
    {
        AddCiterneCommand    = new RelayCommand(OpenAddCiterne);
        RefreshCommand       = new AsyncRelayCommand(RefreshAsync);
        DetailCiterneCommand = new AsyncRelayCommand<CiterneCardItem>(OpenDetailCiterneAsync);
        EditCiterneCommand   = new AsyncRelayCommand<CiterneCardItem>(OpenEditCiterneAsync);
        DeleteCiterneCommand = new AsyncRelayCommand<CiterneCardItem>(DeleteCiterneAsync);
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
            var dtos = await _service.GetAllCiternesAsync();
            Citernes = new ObservableCollection<CiterneCardItem>(
                dtos.Select(d => new CiterneCardItem
                {
                    CiterneId        = d.CiterneId,
                    MatriculeCiterne = d.MatriculeCiterne,
                    Capacite         = d.Capacite,
                    PartitionsNumber = d.PartitionsNumber
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CiternesSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddCiterne()
    {
        var dlg = new NouveauCiterneDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenEditCiterneAsync(CiterneCardItem? item)
    {
        if (item is null) return;
        var dto = await _service.GetCiterneByIdAsync(item.CiterneId);
        if (dto is null) return;

        var dlg = new EditCiterneDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenDetailCiterneAsync(CiterneCardItem? item)
    {
        if (item is null) return;
        var dto = await _service.GetCiterneByIdAsync(item.CiterneId);
        if (dto is null) return;

        var dlg = new DetailCiterneDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ShouldEdit)
            await OpenEditCiterneAsync(item);
    }

    private async Task DeleteCiterneAsync(CiterneCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer la citerne \"{item.MatriculeCiterne}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteCiterneAsync(item.CiterneId);
        if (ok)
            Citernes.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

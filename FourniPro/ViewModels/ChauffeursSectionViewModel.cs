using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Chauffeurs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped viewmodel for the Chauffeurs list and actions.
/// Manual properties for build stability (no source-generator partials).
/// </summary>
public class ChauffeursSectionViewModel : ObservableObject
{
    private readonly ChauffeurService _service = new();
    private bool _loaded;

    private ObservableCollection<ChauffeurCardItem> _chauffeurs = [];
    public ObservableCollection<ChauffeurCardItem> Chauffeurs
    {
        get => _chauffeurs;
        set => SetProperty(ref _chauffeurs, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddChauffeurCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<ChauffeurCardItem> DetailChauffeurCommand { get; }
    public IRelayCommand<ChauffeurCardItem> EditChauffeurCommand { get; }
    public IRelayCommand<ChauffeurCardItem> DeleteChauffeurCommand { get; }

    public ChauffeursSectionViewModel()
    {
        AddChauffeurCommand    = new RelayCommand(OpenAddChauffeur);
        RefreshCommand         = new AsyncRelayCommand(RefreshAsync);
        DetailChauffeurCommand = new AsyncRelayCommand<ChauffeurCardItem>(OpenDetailChauffeurAsync);
        EditChauffeurCommand   = new AsyncRelayCommand<ChauffeurCardItem>(OpenEditChauffeurAsync);
        DeleteChauffeurCommand = new AsyncRelayCommand<ChauffeurCardItem>(DeleteChauffeurAsync);
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
            var dtos = await _service.GetAllChauffeursAsync();
            Chauffeurs = new ObservableCollection<ChauffeurCardItem>(
                dtos.Select(d => new ChauffeurCardItem
                {
                    ChauffeurId  = d.ChauffeurId,
                    Nom          = d.Nom,
                    Prenom       = d.Prenom,
                    CIN          = d.CIN,
                    Telephone    = d.Telephone,
                    NumeroPermis = d.NumeroPermis
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ChauffeursSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddChauffeur()
    {
        var dlg = new NouveauChauffeurDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenEditChauffeurAsync(ChauffeurCardItem? item)
    {
        if (item is null) return;
        var dto = await _service.GetChauffeurByIdAsync(item.ChauffeurId);
        if (dto is null) return;

        var dlg = new EditChauffeurDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenDetailChauffeurAsync(ChauffeurCardItem? item)
    {
        if (item is null) return;
        var dto = await _service.GetChauffeurByIdAsync(item.ChauffeurId);
        if (dto is null) return;

        var dlg = new DetailChauffeurDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ShouldEdit)
            await OpenEditChauffeurAsync(item);
    }

    private async Task DeleteChauffeurAsync(ChauffeurCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer le chauffeur \"{item.NomComplet}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteChauffeurAsync(item.ChauffeurId);
        if (ok)
            Chauffeurs.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

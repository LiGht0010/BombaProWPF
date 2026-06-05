using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Employes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>
/// Section-scoped viewmodel for the Employes list and actions.
/// </summary>
public class EmployesSectionViewModel : ObservableObject
{
    private readonly EmployeService _service = new();
    private bool _loaded;

    private ObservableCollection<EmployeCardItem> _employes = [];
    public ObservableCollection<EmployeCardItem> Employes
    {
        get => _employes;
        set => SetProperty(ref _employes, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddEmployeCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<EmployeCardItem> DetailEmployeCommand { get; }
    public IRelayCommand<EmployeCardItem> EditEmployeCommand { get; }
    public IRelayCommand<EmployeCardItem> DeleteEmployeCommand { get; }

    public EmployesSectionViewModel()
    {
        AddEmployeCommand    = new RelayCommand(OpenAddEmploye);
        RefreshCommand       = new AsyncRelayCommand(RefreshAsync);
        DetailEmployeCommand = new AsyncRelayCommand<EmployeCardItem>(OpenDetailEmployeAsync);
        EditEmployeCommand   = new AsyncRelayCommand<EmployeCardItem>(OpenEditEmployeAsync);
        DeleteEmployeCommand = new AsyncRelayCommand<EmployeCardItem>(DeleteEmployeAsync);
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
            var dtos = await _service.GetAllEmployesAsync();
            Employes = new ObservableCollection<EmployeCardItem>(
                dtos.Select(d => new EmployeCardItem
                {
                    EmployeId = d.EmployeId,
                    Nom       = d.Nom,
                    Prenom    = d.Prenom,
                    CIN       = d.CIN,
                    Telephone = d.Telephone,
                    Poste     = d.Poste,
                    Salaire   = d.Salaire
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmployesSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddEmploye()
    {
        var dlg = new NouveauEmployeDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenEditEmployeAsync(EmployeCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetEmployeByIdAsync(item.EmployeId);
        if (dto is null) return;

        var dlg = new EditEmployeDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            await RefreshAsync();
    }

    private async Task OpenDetailEmployeAsync(EmployeCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetEmployeByIdAsync(item.EmployeId);
        if (dto is null) return;

        var dlg = new DetailEmployeDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();

        if (dlg.ShouldEdit)
        {
            var editDlg = new EditEmployeDialog(dto)
            {
                Owner = Application.Current?.MainWindow
            };
            editDlg.ShowDialog();
            if (editDlg.ViewModel.Saved)
                await RefreshAsync();
        }
    }

    private async Task DeleteEmployeAsync(EmployeCardItem? item)
    {
        if (item is null) return;

        var hasRelated = await _service.HasRelatedRecordsAsync(item.EmployeId);
        if (hasRelated)
        {
            MessageBox.Show(
                "Cet employé ne peut pas être supprimé car il est lié à des achats ou des ventes.",
                "Suppression impossible",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Supprimer l'employé \"{item.Prenom} {item.Nom}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteEmployeAsync(item.EmployeId);
        if (ok)
            Employes.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

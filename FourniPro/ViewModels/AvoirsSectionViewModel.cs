using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Avoirs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

public class AvoirsSectionViewModel : ObservableObject
{
    private readonly AvoirService _service = new();
    private bool _loaded;

    private ObservableCollection<AvoirCardItem> _avoirs = [];
    public ObservableCollection<AvoirCardItem> Avoirs
    {
        get => _avoirs;
        set => SetProperty(ref _avoirs, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand                      AddAvoirCommand    { get; }
    public IRelayCommand                      RefreshCommand     { get; }
    public IRelayCommand<AvoirCardItem>       DetailAvoirCommand { get; }
    public IRelayCommand<AvoirCardItem>       EditAvoirCommand   { get; }
    public IRelayCommand<AvoirCardItem>       DeleteAvoirCommand { get; }

    public AvoirsSectionViewModel()
    {
        AddAvoirCommand    = new RelayCommand(OpenAddAvoir);
        RefreshCommand     = new AsyncRelayCommand(RefreshAsync);
        DetailAvoirCommand = new AsyncRelayCommand<AvoirCardItem>(OpenDetailAsync);
        EditAvoirCommand   = new AsyncRelayCommand<AvoirCardItem>(OpenEditAsync);
        DeleteAvoirCommand = new AsyncRelayCommand<AvoirCardItem>(DeleteAsync);
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
            var dtos = await _service.GetAllAsync();
            Avoirs = new ObservableCollection<AvoirCardItem>(
                dtos.Select(d => new AvoirCardItem
                {
                    AvoirId      = d.AvoirId,
                    NumeroAvoir  = d.NumeroAvoir,
                    DateAvoir    = d.DateAvoir,
                    NumeroVente  = d.NumeroVente,
                    NumeroCredit = d.NumeroCredit,
                    ClientNom    = d.ClientNom,
                    MontantAvoir = d.MontantAvoir,
                    Raison       = d.Raison,
                    EmployeNom   = d.EmployeNom,
                    Note         = d.Note,
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AvoirsSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddAvoir()
    {
        var dlg = new NouveauAvoirDialog { Owner = Application.Current.MainWindow };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved) _ = RefreshAsync();
    }

    private async Task OpenDetailAsync(AvoirCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetByIdAsync(item.AvoirId);
        if (dto is null) return;

        var dlg = new DetailAvoirDialog(dto) { Owner = Application.Current.MainWindow };
        dlg.ShowDialog();

        if (dlg.ShouldEdit) await OpenEditFromDtoAsync(dto);
    }

    private async Task OpenEditAsync(AvoirCardItem? item)
    {
        if (item is null) return;
        var dto = await _service.GetByIdAsync(item.AvoirId);
        if (dto is null) return;
        await OpenEditFromDtoAsync(dto);
    }

    private async Task OpenEditFromDtoAsync(AvoirDto dto)
    {
        var dlg = new EditAvoirDialog(dto) { Owner = Application.Current.MainWindow };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved) await RefreshAsync();
    }

    private async Task DeleteAsync(AvoirCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer l'avoir {item.NumeroAvoir} ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteAsync(item.AvoirId);
        if (ok)
            Avoirs.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

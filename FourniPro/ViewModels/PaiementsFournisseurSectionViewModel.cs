using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.PaiementsFournisseur;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

public class PaiementsFournisseurSectionViewModel : ObservableObject
{
    private readonly PaiementFournisseurService _service = new();
    private bool _loaded;

    private ObservableCollection<PaiementFournisseurCardItem> _paiements = [];
    public ObservableCollection<PaiementFournisseurCardItem> Paiements
    {
        get => _paiements;
        set => SetProperty(ref _paiements, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddPaiementCommand    { get; }
    public IRelayCommand RefreshCommand        { get; }
    public IRelayCommand<PaiementFournisseurCardItem> DetailPaiementCommand { get; }
    public IRelayCommand<PaiementFournisseurCardItem> EditPaiementCommand   { get; }
    public IRelayCommand<PaiementFournisseurCardItem> DeletePaiementCommand { get; }

    public PaiementsFournisseurSectionViewModel()
    {
        AddPaiementCommand    = new RelayCommand(OpenAddPaiement);
        RefreshCommand        = new AsyncRelayCommand(RefreshAsync);
        DetailPaiementCommand = new AsyncRelayCommand<PaiementFournisseurCardItem>(OpenDetailAsync);
        EditPaiementCommand   = new AsyncRelayCommand<PaiementFournisseurCardItem>(OpenEditAsync);
        DeletePaiementCommand = new AsyncRelayCommand<PaiementFournisseurCardItem>(DeleteAsync);
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
            Paiements = new ObservableCollection<PaiementFournisseurCardItem>(
                dtos.Select(d => new PaiementFournisseurCardItem
                {
                    PaiementFournisseurId = d.PaiementFournisseurId,
                    CreditFournisseurId   = d.CreditFournisseurId,
                    NumeroCreditF         = d.NumeroCreditF,
                    DatePaiement          = d.DatePaiement,
                    Montant               = d.Montant,
                    PaymentMethod         = d.PaymentMethod,
                    Reference             = d.Reference,
                    EmployeNom            = d.EmployeNom,
                    Note                  = d.Note,
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementsFournisseurSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddPaiement()
    {
        var dlg = new NouveauPaiementFournisseurDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() == true && dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenDetailAsync(PaiementFournisseurCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetByIdAsync(item.PaiementFournisseurId);
        if (dto is null) return;

        var dlg = new DetailPaiementFournisseurDialog(dto) { Owner = Application.Current.MainWindow };
        dlg.ShowDialog();

        if (dlg.ShouldEdit)
        {
            var editDlg = new EditPaiementFournisseurDialog(dto) { Owner = Application.Current.MainWindow };
            if (editDlg.ShowDialog() == true && editDlg.ViewModel.Saved)
                _ = RefreshAsync();
        }
    }

    private async Task OpenEditAsync(PaiementFournisseurCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetByIdAsync(item.PaiementFournisseurId);
        if (dto is null) return;

        var dlg = new EditPaiementFournisseurDialog(dto) { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() == true && dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task DeleteAsync(PaiementFournisseurCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer le paiement \"{item.NumeroCreditF}\" du {item.DatePaiement:dd/MM/yyyy} ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteAsync(item.PaiementFournisseurId);
        if (ok)
            Paiements.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

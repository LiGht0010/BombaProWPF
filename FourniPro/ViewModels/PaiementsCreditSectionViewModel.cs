using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.PaiementsCredit;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

public class PaiementsCreditSectionViewModel : ObservableObject
{
    private readonly PaiementCreditService _service = new();
    private bool _loaded;

    private ObservableCollection<PaiementCreditCardItem> _paiements = [];
    public ObservableCollection<PaiementCreditCardItem> Paiements
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
    public IRelayCommand<PaiementCreditCardItem> DetailPaiementCommand { get; }
    public IRelayCommand<PaiementCreditCardItem> EditPaiementCommand   { get; }
    public IRelayCommand<PaiementCreditCardItem> DeletePaiementCommand { get; }

    public PaiementsCreditSectionViewModel()
    {
        AddPaiementCommand    = new RelayCommand(OpenAddPaiement);
        RefreshCommand        = new AsyncRelayCommand(RefreshAsync);
        DetailPaiementCommand = new AsyncRelayCommand<PaiementCreditCardItem>(OpenDetailAsync);
        EditPaiementCommand   = new AsyncRelayCommand<PaiementCreditCardItem>(OpenEditAsync);
        DeletePaiementCommand = new AsyncRelayCommand<PaiementCreditCardItem>(DeleteAsync);
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
            Paiements = new ObservableCollection<PaiementCreditCardItem>(
                dtos.Select(d => new PaiementCreditCardItem
                {
                    PaiementCreditId = d.PaiementCreditId,
                    CreditId         = d.CreditId,
                    NumeroCredit     = d.NumeroCredit,
                    DatePaiement     = d.DatePaiement,
                    Montant          = d.Montant,
                    PaymentMethod    = d.PaymentMethod,
                    Reference        = d.Reference,
                    EmployeNom       = d.EmployeNom,
                    Note             = d.Note,
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementsCreditSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddPaiement()
    {
        var dlg = new NouveauPaiementCreditDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenDetailAsync(PaiementCreditCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetByIdAsync(item.PaiementCreditId);
        if (dto is null) return;

        var dlg = new DetailPaiementCreditDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();

        if (dlg.ShouldEdit)
        {
            var editDlg = new EditPaiementCreditDialog(dto)
            {
                Owner = Application.Current?.MainWindow
            };
            editDlg.ShowDialog();
            if (editDlg.ViewModel.Saved)
                await RefreshAsync();
        }
    }

    private async Task OpenEditAsync(PaiementCreditCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetByIdAsync(item.PaiementCreditId);
        if (dto is null) return;

        var dlg = new EditPaiementCreditDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            await RefreshAsync();
    }

    private async Task DeleteAsync(PaiementCreditCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer le paiement \"{item.NumeroCredit}\" du {item.DatePaiement:dd/MM/yyyy} ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteAsync(item.PaiementCreditId);
        if (ok)
            Paiements.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

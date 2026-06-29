using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.CreditsFournisseur;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>Section-scoped viewmodel for the CréditsFournisseur list and actions.</summary>
public class CreditsFournisseurSectionViewModel : ObservableObject
{
    private readonly CreditFournisseurService _service = new();
    private bool _loaded;

    private ObservableCollection<CreditFournisseurCardItem> _credits = [];
    public ObservableCollection<CreditFournisseurCardItem> Credits
    {
        get => _credits;
        set => SetProperty(ref _credits, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public IRelayCommand AddCreditCommand    { get; }
    public IRelayCommand RefreshCommand      { get; }
    public IRelayCommand<CreditFournisseurCardItem> DetailCreditCommand { get; }
    public IRelayCommand<CreditFournisseurCardItem> EditCreditCommand   { get; }
    public IRelayCommand<CreditFournisseurCardItem> DeleteCreditCommand { get; }

    public CreditsFournisseurSectionViewModel()
    {
        AddCreditCommand    = new RelayCommand(OpenAddCredit);
        RefreshCommand      = new AsyncRelayCommand(RefreshAsync);
        DetailCreditCommand = new AsyncRelayCommand<CreditFournisseurCardItem>(OpenDetailAsync);
        EditCreditCommand   = new AsyncRelayCommand<CreditFournisseurCardItem>(OpenEditAsync);
        DeleteCreditCommand = new AsyncRelayCommand<CreditFournisseurCardItem>(DeleteAsync);
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
            Credits = new ObservableCollection<CreditFournisseurCardItem>(
                dtos.Select(d => new CreditFournisseurCardItem
                {
                    CreditFournisseurId = d.CreditFournisseurId,
                    NumeroCreditF       = d.NumeroCreditF,
                    DateCredit          = d.DateCredit,
                    AchatNumero         = d.AchatNumero,
                    FournisseurNom      = d.FournisseurNom,
                    EmployeNom          = d.EmployeNom,
                    MontantTotal        = d.MontantTotal,
                    Statut              = d.Statut,
                    ChequeReference     = d.ChequeReference,
                    StatutCheque        = d.StatutCheque,
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditsFournisseurSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddCredit()
    {
        var dlg = new NouveauCreditFournisseurDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenDetailAsync(CreditFournisseurCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetByIdAsync(item.CreditFournisseurId);
        if (dto is null) return;

        var dlg = new DetailCreditFournisseurDialog(dto) { Owner = Application.Current.MainWindow };
        dlg.ShowDialog();

        if (dlg.ShouldEdit)
        {
            var editDlg = new EditCreditFournisseurDialog(dto) { Owner = Application.Current.MainWindow };
            if (editDlg.ShowDialog() == true && editDlg.ViewModel.Saved)
                _ = RefreshAsync();
        }
    }

    private async Task OpenEditAsync(CreditFournisseurCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetByIdAsync(item.CreditFournisseurId);
        if (dto is null) return;

        var dlg = new EditCreditFournisseurDialog(dto) { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() == true && dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task DeleteAsync(CreditFournisseurCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer le crédit fournisseur \"{item.NumeroCreditF}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteCreditFournisseurAsync(item.CreditFournisseurId);
        if (ok)
            Credits.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

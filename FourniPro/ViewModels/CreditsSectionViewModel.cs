using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Views.InfrastructurePages.Sections.Credits;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace FourniPro.ViewModels;

/// <summary>Section-scoped viewmodel for the Credits list and actions.</summary>
public class CreditsSectionViewModel : ObservableObject
{
    private readonly CreditService _service = new();
    private bool _loaded;

    private ObservableCollection<CreditCardItem> _credits = [];
    public ObservableCollection<CreditCardItem> Credits
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

    public IRelayCommand AddCreditCommand { get; }
    public IRelayCommand RefreshCommand   { get; }
    public IRelayCommand<CreditCardItem> DetailCreditCommand { get; }
    public IRelayCommand<CreditCardItem> EditCreditCommand   { get; }
    public IRelayCommand<CreditCardItem> DeleteCreditCommand { get; }

    public CreditsSectionViewModel()
    {
        AddCreditCommand    = new RelayCommand(OpenAddCredit);
        RefreshCommand      = new AsyncRelayCommand(RefreshAsync);
        DetailCreditCommand = new AsyncRelayCommand<CreditCardItem>(OpenDetailCreditAsync);
        EditCreditCommand   = new AsyncRelayCommand<CreditCardItem>(OpenEditCreditAsync);
        DeleteCreditCommand = new AsyncRelayCommand<CreditCardItem>(DeleteCreditAsync);
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
            var dtos = await _service.GetAllCreditsAsync();
            Credits = new ObservableCollection<CreditCardItem>(
                dtos.Select(d => new CreditCardItem
                {
                    CreditId        = d.CreditId,
                    NumeroCredit    = d.NumeroCredit,
                    DateCredit      = d.DateCredit,
                    ClientNom       = d.ClientNom,
                    ProduitNom      = d.ProduitNom,
                    EmployeNom      = d.EmployeNom,
                    VoyageID        = d.VoyageID,
                    VoyageNumero    = d.VoyageNumero,
                    Quantite        = d.Quantite,
                    PrixUnitaire    = d.PrixUnitaire,
                    MontantTotal    = d.MontantTotal,
                    Statut          = d.Statut,
                    ChequeReference = d.ChequeReference,
                    StatutCheque    = d.StatutCheque,
                }));
            _loaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditsSectionVM] Load error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    private void OpenAddCredit()
    {
        var dlg = new NouveauCreditDialog
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            _ = RefreshAsync();
    }

    private async Task OpenEditCreditAsync(CreditCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetCreditByIdAsync(item.CreditId);
        if (dto is null) return;

        var dlg = new EditCreditDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();
        if (dlg.ViewModel.Saved)
            await RefreshAsync();
    }

    private async Task OpenDetailCreditAsync(CreditCardItem? item)
    {
        if (item is null) return;

        var dto = await _service.GetCreditByIdAsync(item.CreditId);
        if (dto is null) return;

        var dlg = new DetailCreditDialog(dto)
        {
            Owner = Application.Current?.MainWindow
        };
        dlg.ShowDialog();

        if (dlg.ShouldEdit)
        {
            var editDlg = new EditCreditDialog(dto)
            {
                Owner = Application.Current?.MainWindow
            };
            editDlg.ShowDialog();
            if (editDlg.ViewModel.Saved)
                await RefreshAsync();
        }
    }

    private async Task DeleteCreditAsync(CreditCardItem? item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            $"Supprimer le crédit \"{item.NumeroCredit}\" ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        var ok = await _service.DeleteCreditAsync(item.CreditId);
        if (ok)
            Credits.Remove(item);
        else
            MessageBox.Show("Erreur lors de la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

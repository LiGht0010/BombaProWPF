using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for stock withdrawal operations.
/// Super Admin only feature to manually remove stock from reservoirs.
/// </summary>
public partial class StockWithdrawalViewModel : ObservableObject
{
    private readonly StockWithdrawalService _withdrawalService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<StockWithdrawalHistoryDto> WithdrawalHistory { get; } = [];

    public decimal TotalHistoryQuantite => WithdrawalHistory.Sum(w => w.Quantite);

    public StockWithdrawalViewModel(IDialogService dialogService)
    {
        _withdrawalService = new StockWithdrawalService();
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Load all history without date filter
            var history = await _withdrawalService.GetWithdrawalHistoryAsync();

            WithdrawalHistory.Clear();
            foreach (var record in history)
            {
                WithdrawalHistory.Add(record);
            }

            OnPropertyChanged(nameof(TotalHistoryQuantite));
            Debug.WriteLine($"[StockWithdrawalViewModel] Loaded {WithdrawalHistory.Count} history records");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[StockWithdrawalViewModel] Error loading history: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private async Task AddWithdrawalAsync()
    {
        var result = await _dialogService.ShowStockWithdrawalCreatePopupAsync();
        
        if (result != null && result.Success)
        {
            // Show success message with FIFO breakdown
            var lotsInfo = result.LotsAffectes?.Count > 0
                ? "\n\nLots affectes (FIFO):\n" + string.Join("\n", 
                    result.LotsAffectes.Select(l => 
                        $"- Lot #{l.StockLotID}: -{l.QuantiteRetiree:N2}L " +
                        $"({l.QuantiteAvant:N2}L -> {l.QuantiteApres:N2}L)" +
                        (l.EstEpuise ? " [EPUISE]" : "")))
                : "";

            await _dialogService.ShowAlertAsync("Succes",
                $"Retrait effectue avec succes!\n\n" +
                $"Quantite retiree: {result.QuantiteRetiree:N2} L\n" +
                $"Nouveau niveau: {result.NouveauNiveau:N2} L" +
                lotsInfo);

            // Refresh the history
            await LoadHistoryAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteWithdrawalAsync(StockWithdrawalHistoryDto withdrawal)
    {
        if (withdrawal == null)
            return;

        // Confirm deletion
        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Confirmer la suppression",
            $"Voulez-vous vraiment annuler ce retrait?\n\n" +
            $"Reservoir: {withdrawal.ReservoirNumero}\n" +
            $"Quantite: {withdrawal.QuantiteFormatted}\n" +
            $"Date: {withdrawal.DateRetraitFormatted}\n\n" +
            $"Le stock sera restaure dans les lots correspondants.");

        if (!confirmed)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var result = await _withdrawalService.DeleteWithdrawalAsync(withdrawal.ID);

            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Succes",
                    $"Retrait annule avec succes!\n\n" +
                    $"Quantite restauree: {withdrawal.Quantite:N2} L\n" +
                    (result.Message ?? ""));

                // Refresh the history
                await LoadHistoryAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur",
                    result.Message ?? "Erreur lors de l'annulation du retrait");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[StockWithdrawalViewModel] Error deleting withdrawal: {ex.Message}");
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

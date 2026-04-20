using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMaxWPF.ViewModels;

public partial class CiterneViewModel : ObservableObject
{
    private readonly CiterneService _citerneService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<CiterneDto> Citernes { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private CiterneDto? _selectedCiterne;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public CiterneViewModel(
        CiterneService citerneService, 
        IDialogService dialogService)
    {
        _citerneService = citerneService;
        _dialogService = dialogService;
    }

    // ════════════════════════════════════════════════════════════════
    // COMMANDS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    public async Task LoadCiternesAsync()
    {
        try
        {
            IsLoading = true;
            var citernes = await _citerneService.GetAllCiternesAsync();
            Citernes.Clear();
            foreach (var citerne in citernes)
            {
                Citernes.Add(citerne);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les citernes: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddCiterneAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newCiterne = await _dialogService.ShowCiterneCreatePopupAsync();
            if (newCiterne != null)
            {
                Citernes.Insert(0, newCiterne);
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des citernes");
        }
    }

    [RelayCommand]
    private async Task EditCiterneAsync(CiterneDto? citerne)
    {
        if (citerne == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var success = await _dialogService.ShowCiterneEditPopupAsync(citerne);
            if (success)
            {
                await LoadCiternesAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des citernes");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(CiterneDto? citerne)
    {
        if (citerne == null) return;
        await _dialogService.ShowCiterneDetailsPopupAsync(citerne);
    }

    [RelayCommand]
    private async Task DeleteCiterneAsync(CiterneDto? citerne)
    {
        if (citerne == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var displayName = !string.IsNullOrWhiteSpace(citerne.MatriculeCiterne) 
                ? citerne.MatriculeCiterne 
                : $"Citerne {citerne.Capacite:N0}L";

            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer la citerne '{displayName}'?");

            if (confirm)
            {
                try
                {
                    var success = await _citerneService.DeleteCiterneAsync(citerne.ID);
                    if (success)
                    {
                        Citernes.Remove(citerne);
                        await _dialogService.ShowAlertAsync("Succès", "Citerne supprimée avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression. La citerne peut être utilisée par des camions.");
                    }
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors de la suppression: {ex.Message}");
                }
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des citernes");
        }
    }

    [RelayCommand]
    private async Task ShowCiterneRowDetailsAsync(CiterneDto? citerne)
    {
        if (citerne == null) return;

        await _dialogService.ShowAlertAsync("Détails de la citerne",
            $"ID: {citerne.ID}\n" +
            $"Matricule: {citerne.MatriculeCiterne ?? "N/A"}\n" +
            $"Capacité: {citerne.Capacite:N0} L\n" +
            $"Partitions: {citerne.PartitionsNumber ?? 0}\n" +
            $"Fournisseur: {citerne.FournisseurNom ?? "N/A"}");
    }

    [RelayCommand]
    private async Task SearchCiternesAsync()
    {
        try
        {
            IsLoading = true;
            
            List<CiterneDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _citerneService.GetAllCiternesAsync();
            }
            else
            {
                results = await _citerneService.SearchCiternesAsync(SearchText);
            }

            Citernes.Clear();
            foreach (var citerne in results)
            {
                Citernes.Add(citerne);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors de la recherche: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

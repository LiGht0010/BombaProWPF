using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMaxWPF.ViewModels;

public partial class CamionViewModel : ObservableObject
{
    private readonly CamionService _camionService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<CamionDto> Camions { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private CamionDto? _selectedCamion;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public CamionViewModel(
        CamionService camionService, 
        IDialogService dialogService)
    {
        _camionService = camionService;
        _dialogService = dialogService;
    }

    // ════════════════════════════════════════════════════════════════
    // COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    public async Task LoadCamionsAsync()
    {
        try
        {
            IsLoading = true;
            var camions = await _camionService.GetAllCamionsAsync();
            Camions.Clear();
            foreach (var camion in camions)
            {
                Camions.Add(camion);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les camions: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddCamionAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newCamion = await _dialogService.ShowCamionCreatePopupAsync();
            if (newCamion != null)
            {
                Camions.Insert(0, newCamion);
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des camions");
        }
    }

    [RelayCommand]
    private async Task EditCamionAsync(CamionDto? camion)
    {
        if (camion == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var success = await _dialogService.ShowCamionEditPopupAsync(camion);
            if (success)
            {
                await LoadCamionsAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des camions");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(CamionDto? camion)
    {
        if (camion == null) return;
        await _dialogService.ShowCamionDetailsPopupAsync(camion);
    }

    [RelayCommand]
    private async Task DeleteCamionAsync(CamionDto? camion)
    {
        if (camion == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var displayName = !string.IsNullOrWhiteSpace(camion.Matricule) 
                ? camion.Matricule 
                : $"Camion ID {camion.ID}";

            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer le camion '{displayName}'?");

            if (confirm)
            {
                try
                {
                    var success = await _camionService.DeleteCamionAsync(camion.ID);
                    if (success)
                    {
                        Camions.Remove(camion);
                        await _dialogService.ShowAlertAsync("Succès", "Camion supprimé avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression. Le camion peut être utilisé dans des achats.");
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
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des camions");
        }
    }

    [RelayCommand]
    private async Task ShowCamionRowDetailsAsync(CamionDto? camion)
    {
        if (camion == null) return;

        var citerneInfo = camion.CiterneID.HasValue
            ? $"Citerne: {camion.CiterneNumero ?? $"ID {camion.CiterneID}"}"
            : "Aucune citerne assignée";

        await _dialogService.ShowAlertAsync("Détails du camion",
            $"ID: {camion.ID}\n" +
            $"Matricule: {camion.Matricule ?? "N/A"}\n" +
            $"Marque: {camion.Marque ?? "N/A"}\n" +
            $"Fournisseur: {camion.FournisseurNom ?? "N/A"}\n" +
            $"{citerneInfo}");
    }

    [RelayCommand]
    private async Task SearchCamionsAsync()
    {
        try
        {
            IsLoading = true;
            
            List<CamionDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _camionService.GetAllCamionsAsync();
            }
            else
            {
                results = await _camionService.SearchCamionsAsync(SearchText);
            }

            Camions.Clear();
            foreach (var camion in results)
            {
                Camions.Add(camion);
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

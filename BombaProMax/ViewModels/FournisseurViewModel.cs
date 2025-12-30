using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class FournisseurViewModel : ObservableObject
{
    private readonly FournisseurService _fournisseurService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<FournisseurDto> Fournisseurs { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private FournisseurDto? _selectedFournisseur;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public FournisseurViewModel(
        FournisseurService fournisseurService, 
        IDialogService dialogService)
    {
        _fournisseurService = fournisseurService;
        _dialogService = dialogService;
    }

    // ════════════════════════════════════════════════════════════════
    // COMMANDS
    // ════════════════════════════════════════════════════════════════

    [RelayCommand]
    public async Task LoadFournisseursAsync()
    {
        try
        {
            IsLoading = true;
            var fournisseurs = await _fournisseurService.GetAllFournisseursAsync();
            Fournisseurs.Clear();
            foreach (var fournisseur in fournisseurs)
            {
                Fournisseurs.Add(fournisseur);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les fournisseurs: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddFournisseurAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newFournisseur = await _dialogService.ShowFournisseurCreatePopupAsync();
            if (newFournisseur != null)
            {
                Fournisseurs.Insert(0, newFournisseur);
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des fournisseurs");
        }
    }

    [RelayCommand]
    private async Task EditFournisseurAsync(FournisseurDto? fournisseur)
    {
        if (fournisseur == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var success = await _dialogService.ShowFournisseurEditPopupAsync(fournisseur);
            if (success)
            {
                await LoadFournisseursAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des fournisseurs");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(FournisseurDto? fournisseur)
    {
        if (fournisseur == null) return;
        await _dialogService.ShowFournisseurDetailsPopupAsync(fournisseur);
    }

    [RelayCommand]
    private async Task DeleteFournisseurAsync(FournisseurDto? fournisseur)
    {
        if (fournisseur == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var displayName = !string.IsNullOrWhiteSpace(fournisseur.Societe) 
                ? fournisseur.Societe 
                : $"{fournisseur.Prenom} {fournisseur.Nom}".Trim();

            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer le fournisseur '{displayName}'?");

            if (confirm)
            {
                try
                {
                    var success = await _fournisseurService.DeleteFournisseurAsync(fournisseur.ID);
                    if (success)
                    {
                        Fournisseurs.Remove(fournisseur);
                        await _dialogService.ShowAlertAsync("Succès", "Fournisseur supprimé avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression. Le fournisseur peut être utilisé dans d'autres enregistrements.");
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
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des fournisseurs");
        }
    }

    [RelayCommand]
    private async Task ShowFournisseurRowDetailsAsync(FournisseurDto? fournisseur)
    {
        if (fournisseur == null) return;

        var fullName = $"{fournisseur.Prenom} {fournisseur.Nom}".Trim();
        if (string.IsNullOrWhiteSpace(fullName)) fullName = "N/A";

        await _dialogService.ShowAlertAsync("Détails du fournisseur",
            $"Nom: {fullName}\n" +
            $"Société: {fournisseur.Societe ?? "N/A"}\n" +
            $"Contact: {fournisseur.Contact ?? "N/A"}\n" +
            $"Email: {fournisseur.Email ?? "N/A"}\n" +
            $"Téléphone: {fournisseur.Telephone ?? "N/A"}\n" +
            $"Statut: {fournisseur.Statut}");
    }

    [RelayCommand]
    private async Task SearchFournisseursAsync()
    {
        try
        {
            IsLoading = true;
            
            List<FournisseurDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _fournisseurService.GetAllFournisseursAsync();
            }
            else
            {
                results = await _fournisseurService.SearchFournisseursAsync(SearchText);
            }

            Fournisseurs.Clear();
            foreach (var fournisseur in results)
            {
                Fournisseurs.Add(fournisseur);
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

    [RelayCommand]
    private async Task ToggleStatusAsync(FournisseurDto? fournisseur)
    {
        if (fournisseur == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newStatus = fournisseur.Statut == "Actif" ? "Inactif" : "Actif";
            var success = await _fournisseurService.UpdateFournisseurStatusAsync(fournisseur.ID, newStatus);
            
            if (success)
            {
                fournisseur.Statut = newStatus;
                await LoadFournisseursAsync();
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Échec de la mise à jour du statut");
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des fournisseurs");
        }
    }
}

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
    private readonly JourneeNavigationService _journeeService;

    public ObservableCollection<FournisseurDto> Fournisseurs { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private FournisseurDto? _selectedFournisseur;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES (bound to service)
    // ════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Whether the journée workflow is currently active.
    /// </summary>
    public bool IsJourneeActive => _journeeService.IsJourneeActive;

    /// <summary>
    /// Whether we can navigate to the previous step.
    /// </summary>
    public bool CanGoPrevious => _journeeService.CanGoPrevious;

    /// <summary>
    /// Whether we can navigate to the next step.
    /// </summary>
    public bool CanGoNext => _journeeService.CanGoNext;

    /// <summary>
    /// Whether this is the first step (hide Précédent button).
    /// </summary>
    public bool IsFirstStep => _journeeService.IsFirstStep;

    /// <summary>
    /// Whether this is the last step (show Terminer instead of Suivant).
    /// </summary>
    public bool IsLastStep => _journeeService.IsLastStep;

    /// <summary>
    /// Current step info for display.
    /// </summary>
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public FournisseurViewModel(
        FournisseurService fournisseurService, 
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _fournisseurService = fournisseurService;
        _dialogService = dialogService;
        _journeeService = journeeService;

        // Subscribe to journée service property changes
        _journeeService.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(IsJourneeActive));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsFirstStep));
            OnPropertyChanged(nameof(IsLastStep));
            OnPropertyChanged(nameof(JourneeStepInfo));
        };
    }

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE COMMANDS
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Navigate to the next step in the journée workflow.
    /// </summary>
    [RelayCommand]
    private async Task JourneeSuivantAsync()
    {
        await _journeeService.GoNextAsync(skipped: false);
    }

    /// <summary>
    /// Skip the current step and navigate to the next one.
    /// </summary>
    [RelayCommand]
    private async Task JourneePasserAsync()
    {
        await _journeeService.GoNextAsync(skipped: true);
    }

    /// <summary>
    /// Navigate to the previous step in the journée workflow.
    /// </summary>
    [RelayCommand]
    private async Task JourneePrecedentAsync()
    {
        await _journeeService.GoPreviousAsync();
    }

    // ════════════════════════════════════════════════════════════════
    // EXISTING COMMANDS
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

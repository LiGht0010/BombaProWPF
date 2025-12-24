using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class ChauffeurViewModel : ObservableObject
{
    private readonly ChauffeurService _chauffeurService;
    private readonly IDialogService _dialogService;
    private readonly JourneeNavigationService _journeeService;

    public ObservableCollection<ChauffeurDto> Chauffeurs { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ChauffeurDto? _selectedChauffeur;

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES
    // ════════════════════════════════════════════════════════════════
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public ChauffeurViewModel(
        ChauffeurService chauffeurService, 
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _chauffeurService = chauffeurService;
        _dialogService = dialogService;
        _journeeService = journeeService;

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
    [RelayCommand]
    private async Task JourneeSuivantAsync() => await _journeeService.GoNextAsync(skipped: false);

    [RelayCommand]
    private async Task JourneePasserAsync() => await _journeeService.GoNextAsync(skipped: true);

    [RelayCommand]
    private async Task JourneePrecedentAsync() => await _journeeService.GoPreviousAsync();

    // ════════════════════════════════════════════════════════════════
    // EXISTING COMMANDS
    // ════════════════════════════════════════════════════════════════
    [RelayCommand]
    public async Task LoadChauffeursAsync()
    {
        try
        {
            IsLoading = true;
            var chauffeurs = await _chauffeurService.GetAllChauffeursAsync();
            Chauffeurs.Clear();
            foreach (var chauffeur in chauffeurs)
            {
                Chauffeurs.Add(chauffeur);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les chauffeurs: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddChauffeurAsync()
    {
        var currentUser = App.CurrentUser;
        // Using EditLivreur permission for chauffeurs (delivery drivers)
        if (currentUser != null && (currentUser.EditLivreur || currentUser.IsAdmin || currentUser.IsSuperAdmin))
        {
            var newChauffeur = await _dialogService.ShowChauffeurCreatePopupAsync();
            if (newChauffeur != null)
            {
                Chauffeurs.Add(newChauffeur);
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des chauffeurs");
        }
    }

    [RelayCommand]
    private async Task EditChauffeurAsync(ChauffeurDto? chauffeur)
    {
        if (chauffeur == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && (currentUser.EditLivreur || currentUser.IsAdmin || currentUser.IsSuperAdmin))
        {
            var success = await _dialogService.ShowChauffeurEditPopupAsync(chauffeur);
            if (success)
            {
                await LoadChauffeursAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des chauffeurs");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(ChauffeurDto? chauffeur)
    {
        if (chauffeur == null) return;
        await _dialogService.ShowChauffeurDetailsPopupAsync(chauffeur);
    }

    [RelayCommand]
    private async Task DeleteChauffeurAsync(ChauffeurDto? chauffeur)
    {
        if (chauffeur == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && (currentUser.EditLivreur || currentUser.IsAdmin || currentUser.IsSuperAdmin))
        {
            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer {chauffeur.Nom} {chauffeur.Prenom}?");

            if (confirm)
            {
                try
                {
                    var success = await _chauffeurService.DeleteChauffeurAsync(chauffeur.ID);
                    if (success)
                    {
                        Chauffeurs.Remove(chauffeur);
                        await _dialogService.ShowAlertAsync("Succès", "Chauffeur supprimé avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression du chauffeur");
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
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des chauffeurs");
        }
    }

    [RelayCommand]
    private async Task ShowChauffeurRowDetailsAsync(ChauffeurDto? chauffeur)
    {
        if (chauffeur == null) return;

        await _dialogService.ShowAlertAsync("Détails du chauffeur",
            $"Nom: {chauffeur.Nom}\n" +
            $"Prénom: {chauffeur.Prenom ?? "N/A"}\n" +
            $"CIN: {chauffeur.CIN ?? "N/A"}\n" +
            $"Téléphone: {chauffeur.Telephone ?? "N/A"}\n" +
            $"N° Permis: {chauffeur.NumeroPermis ?? "N/A"}\n" +
            $"Fournisseur: {chauffeur.FournisseurNom ?? "N/A"}\n" +
            $"Créé le: {chauffeur.DateCreation:dd/MM/yyyy}");
    }
}

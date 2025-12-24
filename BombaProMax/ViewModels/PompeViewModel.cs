using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class PompeViewModel : ObservableObject
{
    private readonly PompeService _pompeService;
    private readonly IDialogService _dialogService;
    private readonly JourneeNavigationService _journeeService;

    public ObservableCollection<PompeDto> Pompes { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private PompeDto? _selectedPompe;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Statistics
    [ObservableProperty]
    private int _activesCount;

    [ObservableProperty]
    private int _maintenanceCount;

    [ObservableProperty]
    private int _horsServiceCount;

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES
    // ════════════════════════════════════════════════════════════════
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public PompeViewModel(
        PompeService pompeService, 
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _pompeService = pompeService;
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
    public async Task LoadPompesAsync()
    {
        try
        {
            IsLoading = true;
            var pompes = await _pompeService.GetAllAsync();
            Pompes.Clear();
            foreach (var pompe in pompes)
            {
                Pompes.Add(pompe);
            }

            // Calculate statistics
            CalculateStatistics();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les pompes: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateStatistics()
    {
        ActivesCount = Pompes.Count(p => 
            p.Statut?.Equals("Actif", StringComparison.OrdinalIgnoreCase) == true ||
            p.Statut?.Equals("Active", StringComparison.OrdinalIgnoreCase) == true);
        
        MaintenanceCount = Pompes.Count(p => 
            p.Statut?.Contains("Maintenance", StringComparison.OrdinalIgnoreCase) == true);
        
        HorsServiceCount = Pompes.Count(p => 
            p.Statut?.Contains("Hors", StringComparison.OrdinalIgnoreCase) == true ||
            p.Statut?.Equals("Inactif", StringComparison.OrdinalIgnoreCase) == true);
    }

    [RelayCommand]
    private async Task AddPompeAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newPompe = await _dialogService.ShowPompeCreatePopupAsync();
            if (newPompe != null)
            {
                Pompes.Insert(0, newPompe);
                CalculateStatistics();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des pompes");
        }
    }

    [RelayCommand]
    private async Task EditPompeAsync(PompeDto? pompe)
    {
        if (pompe == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var success = await _dialogService.ShowPompeEditPopupAsync(pompe);
            if (success)
            {
                await LoadPompesAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des pompes");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(PompeDto? pompe)
    {
        if (pompe == null) return;
        await _dialogService.ShowPompeDetailsPopupAsync(pompe);
    }

    [RelayCommand]
    private async Task DeletePompeAsync(PompeDto? pompe)
    {
        if (pompe == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer la pompe '{pompe.Numero}'?");

            if (confirm)
            {
                try
                {
                    var success = await _pompeService.DeleteAsync(pompe.ID);
                    if (success)
                    {
                        Pompes.Remove(pompe);
                        CalculateStatistics();
                        await _dialogService.ShowAlertAsync("Succès", "Pompe supprimée avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression de la pompe");
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
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des pompes");
        }
    }

    [RelayCommand]
    private async Task ShowPompeRowDetailsAsync(PompeDto? pompe)
    {
        if (pompe == null) return;

        var discrepancy = pompe.CompteurElectroniqueActuel.HasValue && pompe.CompteurMecaniqueActuel.HasValue
            ? Math.Abs(pompe.CompteurElectroniqueActuel.Value - pompe.CompteurMecaniqueActuel.Value)
            : 0;

        await _dialogService.ShowAlertAsync("Détails de la pompe",
            $"Numéro: {pompe.Numero}\n" +
            $"Statut: {pompe.Statut}\n" +
            $"Réservoir: {pompe.ReservoirNumero ?? "Non assigné"}\n" +
            $"Compteur Électronique: {pompe.CompteurElectroniqueActuel:N2} L\n" +
            $"Compteur Mécanique: {pompe.CompteurMecaniqueActuel:N2} L\n" +
            $"Écart compteurs: {discrepancy:N2} L\n" +
            $"Date création: {pompe.DateCreation:dd/MM/yyyy}");
    }

    [RelayCommand]
    private async Task SearchPompesAsync()
    {
        try
        {
            IsLoading = true;
            
            var allPompes = await _pompeService.GetAllAsync();
            
            List<PompeDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = allPompes;
            }
            else
            {
                var searchTerm = SearchText.ToLower();
                results = allPompes.Where(p =>
                    (p.Numero?.ToLower().Contains(searchTerm) ?? false) ||
                    (p.Statut?.ToLower().Contains(searchTerm) ?? false) ||
                    (p.ReservoirNumero?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
            }

            Pompes.Clear();
            foreach (var pompe in results)
            {
                Pompes.Add(pompe);
            }
            CalculateStatistics();
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
    private async Task FilterByStatusAsync(string status)
    {
        try
        {
            IsLoading = true;
            var pompes = await _pompeService.GetByStatusAsync(status);
            
            Pompes.Clear();
            foreach (var pompe in pompes)
            {
                Pompes.Add(pompe);
            }

            if (pompes.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Information", $"Aucune pompe avec le statut '{status}'.");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors du filtrage: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ShowHighDiscrepancyAsync()
    {
        try
        {
            IsLoading = true;
            var pompes = await _pompeService.GetWithHighDiscrepancyAsync();
            
            Pompes.Clear();
            foreach (var pompe in pompes)
            {
                Pompes.Add(pompe);
            }

            if (pompes.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Information", "Aucune pompe avec un écart de compteurs élevé.");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Écarts détectés", $"{pompes.Count} pompe(s) avec écart > 10L trouvée(s).");
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

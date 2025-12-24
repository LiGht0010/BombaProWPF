using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMax.ViewModels;

public partial class ReservoirViewModel : ObservableObject
{
    private readonly ReservoirService _reservoirService;
    private readonly IDialogService _dialogService;
    private readonly JourneeNavigationService _journeeService;

    public ObservableCollection<ReservoirDto> Reservoirs { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ReservoirDto? _selectedReservoir;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Statistics
    [ObservableProperty]
    private decimal _totalCapacity;

    [ObservableProperty]
    private decimal _totalFuelLevel;

    [ObservableProperty]
    private decimal _fillPercentage;

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES
    // ════════════════════════════════════════════════════════════════
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public ReservoirViewModel(
        ReservoirService reservoirService, 
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _reservoirService = reservoirService;
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
    public async Task LoadReservoirsAsync()
    {
        try
        {
            IsLoading = true;
            var reservoirs = await _reservoirService.GetAllReservoirsAsync();
            Reservoirs.Clear();
            foreach (var reservoir in reservoirs)
            {
                Reservoirs.Add(reservoir);
            }

            // Calculate statistics
            await CalculateStatisticsAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les réservoirs: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CalculateStatisticsAsync()
    {
        try
        {
            var stats = await _reservoirService.GetCapacityStatisticsAsync();
            if (stats.TryGetValue("TotalCapacity", out var capacity))
                TotalCapacity = capacity;
            if (stats.TryGetValue("TotalFuelLevel", out var fuel))
                TotalFuelLevel = fuel;
            
            FillPercentage = TotalCapacity > 0 ? Math.Round((TotalFuelLevel / TotalCapacity) * 100, 1) : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating statistics: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddReservoirAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newReservoir = await _dialogService.ShowReservoirCreatePopupAsync();
            if (newReservoir != null)
            {
                Reservoirs.Insert(0, newReservoir);
                await CalculateStatisticsAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des réservoirs");
        }
    }

    [RelayCommand]
    private async Task EditReservoirAsync(ReservoirDto? reservoir)
    {
        if (reservoir == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var success = await _dialogService.ShowReservoirEditPopupAsync(reservoir);
            if (success)
            {
                await LoadReservoirsAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des réservoirs");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(ReservoirDto? reservoir)
    {
        if (reservoir == null) return;
        await _dialogService.ShowReservoirDetailsPopupAsync(reservoir);
    }

    [RelayCommand]
    private async Task DeleteReservoirAsync(ReservoirDto? reservoir)
    {
        if (reservoir == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer le réservoir '{reservoir.Numero}'?");

            if (confirm)
            {
                try
                {
                    var success = await _reservoirService.DeleteReservoirAsync(reservoir.ID);
                    if (success)
                    {
                        Reservoirs.Remove(reservoir);
                        await CalculateStatisticsAsync();
                        await _dialogService.ShowAlertAsync("Succès", "Réservoir supprimé avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression. Le réservoir peut être utilisé par des pompes.");
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
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des réservoirs");
        }
    }

    [RelayCommand]
    private async Task ShowReservoirRowDetailsAsync(ReservoirDto? reservoir)
    {
        if (reservoir == null) return;

        var fillPercent = reservoir.Capacite > 0 
            ? Math.Round((reservoir.NiveauDeCarburant / reservoir.Capacite) * 100, 1) 
            : 0;

        await _dialogService.ShowAlertAsync("Détails du réservoir",
            $"Numéro: {reservoir.Numero}\n" +
            $"Type carburant: {reservoir.ProduitNom ?? "Non assigné"}\n" +
            $"Capacité: {reservoir.Capacite:N0} L\n" +
            $"Niveau actuel: {reservoir.NiveauDeCarburant:N0} L\n" +
            $"Taux de remplissage: {fillPercent}%\n" +
            $"Espace disponible: {reservoir.Capacite - reservoir.NiveauDeCarburant:N0} L\n" +
            $"Date création: {reservoir.DateCreation:dd/MM/yyyy}");
    }

    [RelayCommand]
    private async Task SearchReservoirsAsync()
    {
        try
        {
            IsLoading = true;
            
            List<ReservoirDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _reservoirService.GetAllReservoirsAsync();
            }
            else
            {
                results = await _reservoirService.SearchReservoirsAsync(SearchText);
            }

            Reservoirs.Clear();
            foreach (var reservoir in results)
            {
                Reservoirs.Add(reservoir);
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
    private async Task ShowLowFuelAsync()
    {
        try
        {
            IsLoading = true;
            var lowFuelReservoirs = await _reservoirService.GetLowFuelReservoirsAsync();
            
            Reservoirs.Clear();
            foreach (var reservoir in lowFuelReservoirs)
            {
                Reservoirs.Add(reservoir);
            }

            if (lowFuelReservoirs.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Information", "Aucun réservoir avec niveau faible.");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Niveau faible", $"{lowFuelReservoirs.Count} réservoir(s) avec niveau faible trouvé(s).");
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

using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BombaProMaxWPF.ViewModels;

public partial class ClientViewModel : ObservableObject
{
    private readonly ClientService _clientService;
    private readonly IDialogService _dialogService;
    private readonly JourneeNavigationService _journeeService;

    public ObservableCollection<ClientDto> Clients { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ClientDto? _selectedClient;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Statistics
    [ObservableProperty]
    private int _withCompanyCount;

    [ObservableProperty]
    private int _thisMonthCount;

    // ════════════════════════════════════════════════════════════════
    // JOURNÉE PROPERTIES
    // ════════════════════════════════════════════════════════════════
    public bool IsJourneeActive => _journeeService.IsJourneeActive;
    public bool CanGoPrevious => _journeeService.CanGoPrevious;
    public bool CanGoNext => _journeeService.CanGoNext;
    public bool IsFirstStep => _journeeService.IsFirstStep;
    public bool IsLastStep => _journeeService.IsLastStep;
    public string JourneeStepInfo => $"Étape {_journeeService.CurrentStepNumber}/{_journeeService.TotalSteps}: {_journeeService.CurrentStepName}";

    public ClientViewModel(
        ClientService clientService, 
        IDialogService dialogService,
        JourneeNavigationService journeeService)
    {
        _clientService = clientService;
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
    public async Task LoadClientsAsync()
    {
        try
        {
            IsLoading = true;
            var clients = await _clientService.GetAllClientsAsync();
            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }

            // Calculate statistics
            CalculateStatistics();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Impossible de charger les clients: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateStatistics()
    {
        WithCompanyCount = Clients.Count(c => !string.IsNullOrWhiteSpace(c.NomSociete));
        
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        ThisMonthCount = Clients.Count(c => c.DateCreation >= startOfMonth);
    }

    [RelayCommand]
    private async Task AddClientAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var newClient = await _dialogService.ShowClientCreatePopupAsync();
            if (newClient != null)
            {
                Clients.Insert(0, newClient);
                CalculateStatistics();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de créer des clients");
        }
    }

    [RelayCommand]
    private async Task EditClientAsync(ClientDto? client)
    {
        if (client == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var success = await _dialogService.ShowClientEditPopupAsync(client);
            if (success)
            {
                await LoadClientsAsync();
            }
        }
        else
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier des clients");
        }
    }

    [RelayCommand]
    private async Task ShowDetailsAsync(ClientDto? client)
    {
        if (client == null) return;
        await _dialogService.ShowClientDetailsPopupAsync(client);
    }

    [RelayCommand]
    private async Task ShowCreditAsync(ClientDto? client)
    {
        if (client == null) return;

        await _dialogService.ShowClientCreditManagementAsync(client);
    }

    [RelayCommand]
    private async Task DeleteClientAsync(ClientDto? client)
    {
        if (client == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            var confirm = await _dialogService.ShowConfirmationAsync("Confirmer la suppression",
                $"Êtes-vous sûr de vouloir supprimer '{client.Nom}'?");

            if (confirm)
            {
                try
                {
                    var success = await _clientService.DeleteClientAsync(client.ID);
                    if (success)
                    {
                        Clients.Remove(client);
                        CalculateStatistics();
                        await _dialogService.ShowAlertAsync("Succès", "Client supprimé avec succès");
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression. Le client peut avoir des factures ou crédits associés.");
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
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer des clients");
        }
    }

    [RelayCommand]
    private async Task ShowClientRowDetailsAsync(ClientDto? client)
    {
        if (client == null) return;

        await _dialogService.ShowAlertAsync("Détails du client",
            $"Numéro: {client.NumeroClient}\n" +
            $"Nom: {client.Nom}\n" +
            $"Contact: {client.Contact ?? "N/A"}\n" +
            $"CIN: {client.CIN}\n" +
            $"Société: {client.NomSociete}\n" +
            $"Créé le: {client.DateCreation:dd/MM/yyyy}");
    }

    [RelayCommand]
    private async Task SearchClientsAsync()
    {
        try
        {
            IsLoading = true;
            
            List<ClientDto> results;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _clientService.GetAllClientsAsync();
            }
            else
            {
                results = await _clientService.SearchClientsAsync(SearchText);
            }

            Clients.Clear();
            foreach (var client in results)
            {
                Clients.Add(client);
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
}

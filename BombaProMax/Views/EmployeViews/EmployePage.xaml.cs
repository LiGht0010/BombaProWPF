using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using BombaProMax.Services;
using BombaProMax.Models;

namespace BombaProMax.Views.EmployeViews;

public partial class EmployePage : ContentPage
{
    private readonly EmployeService _employeService;
    public ObservableCollection<EmployeDto> Employes { get; set; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public EmployePage()
    {
        InitializeComponent();
        _employeService = new EmployeService();
        Employes = new ObservableCollection<EmployeDto>();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEmployes();
    }

    private async void LoadEmployes()
    {
        try
        {
            IsLoading = true;
            var employes = await _employeService.GetAllEmployesAsync();
            Employes.Clear();
            foreach (var employe in employes)
            {
                Employes.Add(employe);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger les employés: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void OnAddEmployeClicked(object sender, EventArgs e)
    {
        // Check permission before allowing employe creation
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            // Open the employe creation popup
            var popup = new EmployeCreatePopup(_employeService);
            var result = await this.ShowPopupAsync(popup);

            // If an employe was created, refresh the list
            if (result is bool success && success)
            {
                LoadEmployes();
            }
        }
        else
        {
            await DisplayAlert("Accès refusé", "Vous n'avez pas la permission de créer des employés", "OK");
        }
    }

    private async void OnEditEmployeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is EmployeDto employe)
        {
            // Check permission before allowing employe edit
            var currentUser = App.CurrentUser;
            if (currentUser != null && currentUser.EditClients)
            {
                // Open the employe edit popup
                var popup = new EmployeEditPopup(_employeService, employe);
                var result = await this.ShowPopupAsync(popup);

                // If the employe was updated, refresh the list
                if (result is bool success && success)
                {
                    LoadEmployes();
                }
            }
            else
            {
                await DisplayAlert("Accès refusé", "Vous n'avez pas la permission de modifier des employés", "OK");
            }
        }
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is EmployeDto selectedEmploye)
        {
            // Use custom popup with CommunityToolkit.Maui
            var popup = new EmployeDetailsPopup(selectedEmploye, _employeService);
            await this.ShowPopupAsync(popup);
        }
    }

    private async void OnDeleteEmployeClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is EmployeDto employe)
        {
            await DeleteEmploye(employe);
        }
    }

    private async Task DeleteEmploye(EmployeDto employe)
    {
        // Check permission before allowing employe deletion
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            // Check if employe has related records
            var hasRelatedRecords = await _employeService.HasRelatedRecordsAsync(employe.ID);

            string message = hasRelatedRecords
                ? $"?? ATTENTION: L'employé {employe.Prenom} {employe.Nom} a des enregistrements liés (jaugeages, crédits, etc.).\n\nÊtes-vous sûr de vouloir le supprimer?"
                : $"Êtes-vous sûr de vouloir supprimer {employe.Prenom} {employe.Nom} ?";

            var confirm = await DisplayAlert("Confirmer la suppression", message, "Oui", "Non");

            if (confirm)
            {
                try
                {
                    var success = await _employeService.DeleteEmployeAsync(employe.ID);
                    if (success)
                    {
                        Employes.Remove(employe);
                        await DisplayAlert("Succès", "Employé supprimé avec succès", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Erreur", "Échec de la suppression de l'employé", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erreur", $"Erreur lors de la suppression: {ex.Message}", "OK");
                }
            }
        }
        else
        {
            await DisplayAlert("Accès refusé", "Vous n'avez pas la permission de supprimer des employés", "OK");
        }
    }

    private async Task EditEmploye(EmployeDto employe)
    {
        // Check permission before allowing employe edit
        var currentUser = App.CurrentUser;
        if (currentUser != null && currentUser.EditClients)
        {
            // Open the employe edit popup
            var popup = new EmployeEditPopup(_employeService, employe);
            var result = await this.ShowPopupAsync(popup);

            // If the employe was updated, refresh the list
            if (result is bool success && success)
            {
                LoadEmployes();
            }
        }
        else
        {
            await DisplayAlert("Accès refusé", "Vous n'avez pas la permission de modifier des employés", "OK");
        }
    }

    private async Task ShowEmployeDetails(EmployeDto employe)
    {
        var popup = new EmployeDetailsPopup(employe, _employeService);
        await this.ShowPopupAsync(popup);
    }

    // Swipe event handlers
    private async void OnDetailsSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is EmployeDto employe)
        {
            await ShowEmployeDetails(employe);
        }
    }

    private async void OnEditSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is EmployeDto employe)
        {
            await EditEmploye(employe);
        }
    }

    private async void OnDeleteSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is EmployeDto employe)
        {
            await DeleteEmploye(employe);
        }
    }

    // Row tap handler
    private async void OnRowTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is EmployeDto employe)
        {
            await ShowEmployeDetails(employe);
        }
    }
}
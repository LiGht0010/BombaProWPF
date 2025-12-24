using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.EmployeViews;

public partial class EmployeDetailsPopup : Popup
{
    private readonly EmployeService _employeService;
    private readonly EmployeDto _employe;

    public EmployeDetailsPopup(EmployeDto employe, EmployeService employeService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _employe = employe;
        _employeService = employeService;

        LoadEmployeDetails();
    }

    private async void LoadEmployeDetails()
    {
        try
        {
            // Display basic information
            EmployeNameLabel.Text = $"{_employe.Prenom} {_employe.Nom}";
            EmployePosteLabel.Text = _employe.Poste ?? "Non spécifié";
            EmployeIdLabel.Text = $"ID: {_employe.ID}";

            // Personal Information
            CINLabel.Text = _employe.CIN ?? "Non spécifié";
            TelephoneLabel.Text = _employe.Telephone ?? "Non spécifié";
            AddressLabel.Text = _employe.Address ?? "Non spécifié";
            SalaireLabel.Text = _employe.Salaire.HasValue 
                ? $"{_employe.Salaire.Value:N2} MAD" 
                : "Non spécifié";

            // Employment Information
            PosteDetailLabel.Text = _employe.Poste ?? "Non spécifié";
            DateCreationLabel.Text = _employe.DateCreation.HasValue 
                ? _employe.DateCreation.Value.ToString("dd/MM/yyyy HH:mm") 
                : "N/A";
            DateModificationLabel.Text = _employe.DateModification.HasValue 
                ? _employe.DateModification.Value.ToString("dd/MM/yyyy HH:mm") 
                : "Jamais modifié";
            AjouteParLabel.Text = _employe.AjoutePar.HasValue 
                ? $"Utilisateur ID: {_employe.AjoutePar.Value}" 
                : "N/A";

            // Load statistics
            await LoadEmployeStatistics();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading employe details: {ex.Message}");
            await Application.Current!.MainPage!.DisplayAlert(
                "Erreur",
                $"Impossible de charger les détails: {ex.Message}",
                "OK");
        }
    }

    private async Task LoadEmployeStatistics()
    {
        try
        {
            // Since EmployeDto doesn't include navigation properties,
            // we display default values. Statistics should come from dedicated API endpoints.
            // TODO: Add API endpoints for employe statistics if needed
            
            JaugeagesCountLabel.Text = "N/A";
            CreditTransactionsCountLabel.Text = "N/A";
            CreditBalanceLabel.Text = "N/A";
            ActivitySummaryLabel.Text = "Statistiques non disponibles";

            // Check if employe has related records via API
            var hasRelated = await _employeService.HasRelatedRecordsAsync(_employe.ID);
            if (hasRelated)
            {
                ActivitySummaryLabel.Text = "Activités enregistrées";
            }
            else
            {
                ActivitySummaryLabel.Text = "Aucune activité enregistrée";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading employe statistics: {ex.Message}");
            JaugeagesCountLabel.Text = "0";
            CreditTransactionsCountLabel.Text = "0";
            CreditBalanceLabel.Text = "0,00 MAD";
            ActivitySummaryLabel.Text = "Erreur lors du chargement";
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
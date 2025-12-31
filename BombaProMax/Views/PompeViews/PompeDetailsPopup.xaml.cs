using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PompeViews;

public partial class PompeDetailsPopup : Popup
{
    private readonly PompeService _pompeService;
    private readonly UserService _userService;
    private readonly PompeDto _pompe;

    public PompeDetailsPopup(PompeDto pompe, PompeService pompeService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _pompe = pompe;
        _pompeService = pompeService;
        _userService = new UserService();

        LoadPompeDetails();
    }

    private async void LoadPompeDetails()
    {
        try
        {
            // Display basic information in header
            PompeNumeroLabel.Text = _pompe.Numero ?? "N/A";
            PompeStatutLabel.Text = _pompe.Statut ?? "Non spécifié";
            PompeIdLabel.Text = $"ID: {_pompe.ID}";

            // Get meter values
            decimal electronique = _pompe.CompteurElectroniqueActuel ?? 0m;
            decimal mecanique = _pompe.CompteurMecaniqueActuel ?? 0m;
            decimal discrepancy = Math.Abs(electronique - mecanique);

            // Display statistics in cards
            CompteurElectroniqueLabel.Text = $"{electronique:N2} L";
            CompteurMecaniqueLabel.Text = $"{mecanique:N2} L";
            DiscrepancyLabel.Text = $"{discrepancy:N2} L";

            // Set discrepancy status color and message
            if (discrepancy == 0)
            {
                DiscrepancyLabel.TextColor = Color.FromArgb("#27AE60"); // Green
                DiscrepancyStatusLabel.Text = "? Compteurs synchronisés";
                DiscrepancyStatusLabel.TextColor = Color.FromArgb("#27AE60");
            }
            else if (discrepancy <= 10)
            {
                DiscrepancyLabel.TextColor = Color.FromArgb("#F39C12"); // Orange
                DiscrepancyStatusLabel.Text = "?? Écart acceptable";
                DiscrepancyStatusLabel.TextColor = Color.FromArgb("#F39C12");
            }
            else
            {
                DiscrepancyLabel.TextColor = Color.FromArgb("#C62828"); // Red
                DiscrepancyStatusLabel.Text = "?? Écart important - Vérification requise";
                DiscrepancyStatusLabel.TextColor = Color.FromArgb("#C62828");
            }

            // Display detailed information
            NumeroDetailLabel.Text = _pompe.Numero ?? "N/A";
            StatutDetailLabel.Text = _pompe.Statut ?? "Non spécifié";
            ReservoirDetailLabel.Text = _pompe.ReservoirNumero ?? "Non assigné";
            CarburantDetailLabel.Text = _pompe.CarburantNom ?? "Non spécifié";
            CompteurElectroniqueDetailLabel.Text = $"{electronique:N3} Litres";
            CompteurMecaniqueDetailLabel.Text = $"{mecanique:N3} Litres";

            // Display reservoir information
            if (_pompe.ReservoirAssocieID.HasValue)
            {
                ReservoirNumeroLabel.Text = _pompe.ReservoirNumero ?? "N/A";
                ReservoirCapaciteLabel.Text = _pompe.ReservoirCapacite.HasValue 
                    ? $"{_pompe.ReservoirCapacite.Value:N2} Litres" 
                    : "N/A";
                ReservoirNiveauLabel.Text = _pompe.ReservoirNiveauDeCarburant.HasValue 
                    ? $"{_pompe.ReservoirNiveauDeCarburant.Value:N2} Litres" 
                    : "N/A";
            }
            else
            {
                ReservoirNumeroLabel.Text = "Non assigné";
                ReservoirCapaciteLabel.Text = "N/A";
                ReservoirNiveauLabel.Text = "N/A";
            }

            // Audit information - dates
            DateCreationLabel.Text = _pompe.DateCreation.HasValue 
                ? _pompe.DateCreation.Value.ToString("dd/MM/yyyy HH:mm") 
                : "N/A";
            DateModificationLabel.Text = _pompe.DateModification.HasValue 
                ? _pompe.DateModification.Value.ToString("dd/MM/yyyy HH:mm") 
                : "Jamais modifié";

            // Load user names asynchronously
            await LoadAuditUserNamesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading pompe details: {ex.Message}");
            await Application.Current!.MainPage!.DisplayAlert(
                "Erreur",
                $"Impossible de charger les détails: {ex.Message}",
                "OK");
        }
    }

    private async Task LoadAuditUserNamesAsync()
    {
        try
        {
            // Load created by user name
            var createdByName = await _userService.GetUserNameByIdAsync(_pompe.AjoutePar);
            AjouteParLabel.Text = createdByName;

            // Load modified by user name
            var modifiedByName = await _userService.GetUserNameByIdAsync(_pompe.ModifiePar);
            ModifieParLabel.Text = modifiedByName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PompeDetailsPopup] Error loading audit info: {ex.Message}");
            AjouteParLabel.Text = "Erreur de chargement";
            ModifieParLabel.Text = "Erreur de chargement";
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
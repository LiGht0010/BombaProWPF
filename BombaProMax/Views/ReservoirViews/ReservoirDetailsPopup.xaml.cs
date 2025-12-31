using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ReservoirViews;

public partial class ReservoirDetailsPopup : Popup
{
    private readonly ReservoirService _reservoirService;
    private readonly UserService _userService;
    private readonly ReservoirDto _reservoir;

    public ReservoirDetailsPopup(ReservoirDto reservoir, ReservoirService reservoirService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _reservoir = reservoir;
        _reservoirService = reservoirService;
        _userService = new UserService();

        LoadReservoirDetails();
        LoadAuditInfoAsync();
    }

    private void LoadReservoirDetails()
    {
        try
        {
            // Display basic information in header
            ReservoirNumeroLabel.Text = _reservoir.Numero ?? "N/A";
            ReservoirProduitLabel.Text = _reservoir.ProduitNom ?? "Type non spécifié";
            ReservoirIdLabel.Text = $"ID: {_reservoir.ID}";

            // Calculate statistics
            decimal capacite = _reservoir.Capacite;
            decimal niveau = _reservoir.NiveauDeCarburant;
            decimal espaceDisponible = capacite - niveau;
            decimal percentage = capacite > 0 ? (niveau / capacite) * 100 : 0;

            // Display statistics in cards
            CapaciteLabel.Text = $"{capacite:N2} L";
            NiveauLabel.Text = $"{niveau:N2} L";
            EspaceDisponibleLabel.Text = $"{espaceDisponible:N2} L";

            // Display percentage and progress bar
            PercentageLabel.Text = $"{percentage:N1}%";
            FuelProgressBar.Progress = (double)(percentage / 100);

            // Change progress bar color based on level
            if (percentage < 20)
            {
                FuelProgressBar.ProgressColor = Colors.Red;
            }
            else if (percentage < 50)
            {
                FuelProgressBar.ProgressColor = Colors.Orange;
            }
            else
            {
                FuelProgressBar.ProgressColor = Color.FromArgb("#27AE60");
            }

            // Display detailed information
            NumeroDetailLabel.Text = _reservoir.Numero ?? "N/A";
            ProduitDetailLabel.Text = _reservoir.ProduitNom ?? "Non spécifié";
            CapaciteDetailLabel.Text = $"{capacite:N2} Litres";
            NiveauDetailLabel.Text = $"{niveau:N2} Litres";
            EspaceDetailLabel.Text = $"{espaceDisponible:N2} Litres";

            // Audit information - dates
            DateCreationLabel.Text = _reservoir.DateCreation.HasValue 
                ? _reservoir.DateCreation.Value.ToString("dd/MM/yyyy HH:mm") 
                : "N/A";
            DateModificationLabel.Text = _reservoir.DateModification.HasValue 
                ? _reservoir.DateModification.Value.ToString("dd/MM/yyyy HH:mm") 
                : "Jamais modifié";

            // Set initial loading state for user names
            AjouteParLabel.Text = "Chargement...";
            ModifieParLabel.Text = "Chargement...";

            // Set default values for related records
            PompesCountLabel.Text = "Voir dans le module Pompes";
            AllocationsCountLabel.Text = "Voir dans le module Allocations";
            JaugeagesCountLabel.Text = "Voir dans le module Jaugeages";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reservoir details: {ex.Message}");
        }
    }

    private async void LoadAuditInfoAsync()
    {
        try
        {
            // Load created by user name
            var createdByName = await _userService.GetUserNameByIdAsync(_reservoir.AjoutePar);
            AjouteParLabel.Text = createdByName;

            // Load modified by user name
            var modifiedByName = await _userService.GetUserNameByIdAsync(_reservoir.ModifiePar);
            ModifieParLabel.Text = modifiedByName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReservoirDetailsPopup] Error loading audit info: {ex.Message}");
            AjouteParLabel.Text = "Erreur de chargement";
            ModifieParLabel.Text = "Erreur de chargement";
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
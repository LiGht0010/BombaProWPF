using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ReservoirViews;

public partial class ReservoirDetailsPopup : Popup
{
    private readonly ReservoirService _reservoirService;
    private readonly ReservoirDto _reservoir;

    public ReservoirDetailsPopup(ReservoirDto reservoir, ReservoirService reservoirService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _reservoir = reservoir;
        _reservoirService = reservoirService;

        LoadReservoirDetails();
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

            // Audit information
            DateCreationLabel.Text = _reservoir.DateCreation.HasValue 
                ? _reservoir.DateCreation.Value.ToString("dd/MM/yyyy HH:mm") 
                : "N/A";
            AjouteParLabel.Text = _reservoir.AjoutePar.HasValue 
                ? $"Utilisateur ID: {_reservoir.AjoutePar.Value}" 
                : "N/A";
            DateModificationLabel.Text = _reservoir.DateModification.HasValue 
                ? _reservoir.DateModification.Value.ToString("dd/MM/yyyy HH:mm") 
                : "Jamais modifié";
            ModifieParLabel.Text = _reservoir.ModifiePar.HasValue 
                ? $"Utilisateur ID: {_reservoir.ModifiePar.Value}" 
                : "N/A";

            // Set default values for related records (navigation properties not available in DTO)
            PompesCountLabel.Text = "Voir dans le module Pompes";
            AllocationsCountLabel.Text = "Voir dans le module Allocations";
            JaugeagesCountLabel.Text = "Voir dans le module Jaugeages";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reservoir details: {ex.Message}");
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
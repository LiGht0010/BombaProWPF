using CommunityToolkit.Maui.Views;
using BombaProMax.Models;
using BombaProMax.Services;

namespace BombaProMax.Views.CamionViews;

public partial class CamionDetailsPopup : Popup
{
    private readonly CamionDto _camion;
    private readonly AchatService _achatService;

    public CamionDetailsPopup(CamionDto camion)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _camion = camion;
        _achatService = new AchatService();

        LoadCamionDetailsAsync();
    }

    private async void LoadCamionDetailsAsync()
    {
        if (_camion != null)
        {
            // Basic Information
            IdLabel.Text = _camion.ID.ToString();
            MatriculeLabel.Text = _camion.Matricule ?? "N/A";
            MatriculeValueLabel.Text = _camion.Matricule ?? "N/A";
            MarqueLabel.Text = _camion.Marque ?? "N/A";

            // Assignment Information - Fournisseur
            FournisseurLabel.Text = _camion.FournisseurNom ?? "Non assigné";

            // Assignment Information - Citerne
            if (_camion.CiterneID.HasValue)
            {
                CiterneStatusBadge.BackgroundColor = Color.FromArgb("#27AE60"); // Green
                CiterneStatusLabel.Text = "Assignée";
                CiterneDetailsLabel.Text = _camion.CiterneNumero ?? $"ID: {_camion.CiterneID}";
            }
            else
            {
                CiterneStatusBadge.BackgroundColor = Color.FromArgb("#9E9E9E"); // Gray
                CiterneStatusLabel.Text = "Non assignée";
                CiterneDetailsLabel.Text = "Aucune citerne";
            }

            // Metadata
            DateCreationLabel.Text = _camion.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
            DateModificationLabel.Text = _camion.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

            // Load related records count
            try
            {
                var achats = await _achatService.GetByCamionIdAsync(_camion.ID);
                AchatsCountLabel.Text = achats.Count.ToString();
            }
            catch
            {
                AchatsCountLabel.Text = "—";
            }
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}

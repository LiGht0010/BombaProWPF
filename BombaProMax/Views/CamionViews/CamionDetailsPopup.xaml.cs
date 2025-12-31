using CommunityToolkit.Maui.Views;
using BombaProMax.Models;
using BombaProMax.Services;

namespace BombaProMax.Views.CamionViews;

public partial class CamionDetailsPopup : Popup
{
    private readonly CamionDto _camion;
    private readonly AchatService _achatService;
    private readonly UserService _userService;

    public CamionDetailsPopup(CamionDto camion)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _camion = camion;
        _achatService = new AchatService();
        _userService = new UserService();

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

            // Metadata - dates
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

            // Load audit user names
            await LoadAuditUserNamesAsync();
        }
    }

    private async Task LoadAuditUserNamesAsync()
    {
        try
        {
            var createdByName = await _userService.GetUserNameByIdAsync(_camion.AjoutePar);
            CreatedByLabel.Text = createdByName;

            var modifiedByName = await _userService.GetUserNameByIdAsync(_camion.ModifiePar);
            ModifiedByLabel.Text = modifiedByName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CamionDetailsPopup] Error loading audit info: {ex.Message}");
            CreatedByLabel.Text = "Erreur de chargement";
            ModifiedByLabel.Text = "Erreur de chargement";
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}

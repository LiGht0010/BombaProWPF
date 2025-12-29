using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ChauffeurViews;

public partial class ChauffeurDetailsPopup : Popup
{
    private readonly ChauffeurDto _chauffeur;
    private readonly AchatService _achatService;

    public ChauffeurDetailsPopup(ChauffeurDto chauffeur)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = true;
        _chauffeur = chauffeur;
        _achatService = new AchatService();

        // Populate the details
        PopulateDetailsAsync();
    }

    private async void PopulateDetailsAsync()
    {
        // Header
        ChauffeurNameLabel.Text = $"{_chauffeur.Nom} {_chauffeur.Prenom}".Trim();
        ChauffeurCINLabel.Text = _chauffeur.CIN ?? "N/A";

        // Statistics - Load Achats count via service
        try
        {
            var achats = await _achatService.GetByChauffeurIdAsync(_chauffeur.ID);
            TotalAchatsLabel.Text = achats.Count.ToString();
        }
        catch
        {
            TotalAchatsLabel.Text = "—";
        }

        FournisseurLabel.Text = _chauffeur.FournisseurNom ?? "Non assigné";

        // Details
        NomDetailLabel.Text = _chauffeur.Nom ?? "N/A";
        PrenomDetailLabel.Text = _chauffeur.Prenom ?? "N/A";
        CINDetailLabel.Text = _chauffeur.CIN ?? "N/A";
        TelephoneDetailLabel.Text = _chauffeur.Telephone ?? "N/A";
        PermisDetailLabel.Text = _chauffeur.NumeroPermis ?? "N/A";
        
        DateCreationLabel.Text = _chauffeur.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
        DateModificationLabel.Text = _chauffeur.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        CloseAsync(null);
    }
}
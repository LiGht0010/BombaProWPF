using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ChauffeurViews;

public partial class ChauffeurDetailsPopup : Popup
{
    private readonly ChauffeurDto _chauffeur;

    public ChauffeurDetailsPopup(ChauffeurDto chauffeur)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = true;
        _chauffeur = chauffeur;

        // Populate the details
        PopulateDetails();
    }

    private void PopulateDetails()
    {
        // Header
        ChauffeurNameLabel.Text = $"{_chauffeur.Nom} {_chauffeur.Prenom}".Trim();
        ChauffeurCINLabel.Text = _chauffeur.CIN ?? "N/A";

        // Statistics - ChauffeurDto doesn't have Achats collection, show placeholder
        TotalAchatsLabel.Text = "—";
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
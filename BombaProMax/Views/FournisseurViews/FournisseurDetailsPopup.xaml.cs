using CommunityToolkit.Maui.Views;
using BombaProMax.Models;

namespace BombaProMax.Views.FournisseurViews;

public partial class FournisseurDetailsPopup : Popup
{
    private readonly FournisseurDto _fournisseur;

    public FournisseurDetailsPopup(FournisseurDto fournisseur)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        
        _fournisseur = fournisseur;
        
        LoadFournisseurDetails();
    }

    private void LoadFournisseurDetails()
    {
        if (_fournisseur != null)
        {
            // Basic Information
            PrenomLabel.Text = _fournisseur.Prenom ?? "N/A";
            NomLabel.Text = _fournisseur.Nom ?? "N/A";
            
            // Full name for header
            var fullName = string.Join(" ", 
                new[] { _fournisseur.Prenom, _fournisseur.Nom }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            FullNameLabel.Text = string.IsNullOrWhiteSpace(fullName) ? "Fournisseur" : fullName;
            
            SocieteLabel.Text = _fournisseur.Societe ?? "N/A";
            StatutLabel.Text = _fournisseur.Statut ?? "N/A";
            
            // Set status color
            StatutBadge.BackgroundColor = GetStatusColor(_fournisseur.Statut);
            
            // Contact Information
            AdresseLabel.Text = _fournisseur.Adresse ?? "Non renseignée";
            TelephoneLabel.Text = _fournisseur.Telephone ?? "Non renseigné";
            EmailLabel.Text = _fournisseur.Email ?? "Non renseigné";
            ContactLabel.Text = _fournisseur.Contact ?? "Non renseigné";
            
            // Financial Information
            RIBLabel.Text = _fournisseur.RIB ?? "Non renseigné";
            ConditionsPaiementLabel.Text = _fournisseur.ConditionsPaiement ?? "Non renseignées";
            
            // Related records count - DTOs don't have navigation properties
            AchatsCountLabel.Text = "—";
            CamionsCountLabel.Text = "—";
            CiternesCountLabel.Text = "—";
            ChauffeursCountLabel.Text = "—";
        }
    }

    private Color GetStatusColor(string? statut)
    {
        return (statut?.ToLower()) switch
        {
            "actif" => Color.FromArgb("#27AE60"),
            "inactif" => Color.FromArgb("#C62828"),
            "en attente" => Color.FromArgb("#FF9800"),
            "suspendu" => Color.FromArgb("#9E9E9E"),
            _ => Color.FromArgb("#336860")
        };
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
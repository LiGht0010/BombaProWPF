using CommunityToolkit.Maui.Views;
using BombaProMax.Services;
using BombaProMax.Models;

namespace BombaProMax.Views.FournisseurViews;

public partial class FournisseurEditPopup : Popup
{
    private readonly FournisseurService _fournisseurService;
    private readonly FournisseurDto _fournisseur;

    public FournisseurEditPopup(FournisseurService fournisseurService, FournisseurDto fournisseur)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        
        _fournisseurService = fournisseurService;
        _fournisseur = fournisseur;
        
        LoadFournisseurData();
    }

    private void LoadFournisseurData()
    {
        if (_fournisseur != null)
        {
            PrenomEntry.Text = _fournisseur.Prenom ?? string.Empty;
            NomEntry.Text = _fournisseur.Nom ?? string.Empty;
            SocieteEntry.Text = _fournisseur.Societe ?? string.Empty;
            AdresseEntry.Text = _fournisseur.Adresse ?? string.Empty;
            TelephoneEntry.Text = _fournisseur.Telephone ?? string.Empty;
            EmailEntry.Text = _fournisseur.Email ?? string.Empty;
            ContactEntry.Text = _fournisseur.Contact ?? string.Empty;
            RIBEntry.Text = _fournisseur.RIB ?? string.Empty;
            ConditionsPaiementEntry.Text = _fournisseur.ConditionsPaiement ?? string.Empty;
            
            // Set status picker
            var statut = _fournisseur.Statut ?? "Actif";
            var statutIndex = StatutPicker.Items.IndexOf(statut);
            if (statutIndex >= 0)
            {
                StatutPicker.SelectedIndex = statutIndex;
            }
            else
            {
                StatutPicker.SelectedIndex = 0; // Default to "Actif"
            }
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(false);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Validate inputs - At least name or company is required
        if (string.IsNullOrWhiteSpace(PrenomEntry.Text) && 
            string.IsNullOrWhiteSpace(NomEntry.Text) && 
            string.IsNullOrWhiteSpace(SocieteEntry.Text))
        {
            ErrorLabel.Text = "Veuillez saisir au moins un nom (Prénom, Nom ou Société)";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            // Update the fournisseur object
            _fournisseur.Prenom = string.IsNullOrWhiteSpace(PrenomEntry.Text) ? null : PrenomEntry.Text.Trim();
            _fournisseur.Nom = string.IsNullOrWhiteSpace(NomEntry.Text) ? null : NomEntry.Text.Trim();
            _fournisseur.Societe = string.IsNullOrWhiteSpace(SocieteEntry.Text) ? null : SocieteEntry.Text.Trim();
            _fournisseur.Adresse = string.IsNullOrWhiteSpace(AdresseEntry.Text) ? null : AdresseEntry.Text.Trim();
            _fournisseur.Telephone = string.IsNullOrWhiteSpace(TelephoneEntry.Text) ? null : TelephoneEntry.Text.Trim();
            _fournisseur.Email = string.IsNullOrWhiteSpace(EmailEntry.Text) ? null : EmailEntry.Text.Trim();
            _fournisseur.Contact = string.IsNullOrWhiteSpace(ContactEntry.Text) ? null : ContactEntry.Text.Trim();
            _fournisseur.RIB = string.IsNullOrWhiteSpace(RIBEntry.Text) ? null : RIBEntry.Text.Trim();
            _fournisseur.ConditionsPaiement = string.IsNullOrWhiteSpace(ConditionsPaiementEntry.Text) ? null : ConditionsPaiementEntry.Text.Trim();
            _fournisseur.Statut = StatutPicker.SelectedItem?.ToString() ?? "Actif";

            var success = await _fournisseurService.UpdateFournisseurAsync(_fournisseur);

            if (success)
            {
                await CloseAsync(true);
            }
            else
            {
                ErrorLabel.Text = "Erreur lors de la mise à jour du fournisseur";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }
}
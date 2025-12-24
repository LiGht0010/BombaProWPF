using CommunityToolkit.Maui.Views;
using BombaProMax.Services;
using BombaProMax.Models;

namespace BombaProMax.Views.FournisseurViews;

public partial class FournisseurCreatePopup : Popup
{
    private readonly FournisseurService _fournisseurService;

    public FournisseurCreatePopup(FournisseurService fournisseurService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        
        _fournisseurService = fournisseurService;
        
        // Set default status
        StatutPicker.SelectedIndex = 0; // "Actif" by default
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(false);
    }

    private async void OnCreateClicked(object sender, EventArgs e)
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
            var fournisseur = new FournisseurDto
            {
                Prenom = string.IsNullOrWhiteSpace(PrenomEntry.Text) ? null : PrenomEntry.Text.Trim(),
                Nom = string.IsNullOrWhiteSpace(NomEntry.Text) ? null : NomEntry.Text.Trim(),
                Societe = string.IsNullOrWhiteSpace(SocieteEntry.Text) ? null : SocieteEntry.Text.Trim(),
                Adresse = string.IsNullOrWhiteSpace(AdresseEntry.Text) ? null : AdresseEntry.Text.Trim(),
                Telephone = string.IsNullOrWhiteSpace(TelephoneEntry.Text) ? null : TelephoneEntry.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(EmailEntry.Text) ? null : EmailEntry.Text.Trim(),
                Contact = string.IsNullOrWhiteSpace(ContactEntry.Text) ? null : ContactEntry.Text.Trim(),
                RIB = string.IsNullOrWhiteSpace(RIBEntry.Text) ? null : RIBEntry.Text.Trim(),
                ConditionsPaiement = string.IsNullOrWhiteSpace(ConditionsPaiementEntry.Text) ? null : ConditionsPaiementEntry.Text.Trim(),
                Statut = StatutPicker.SelectedItem?.ToString() ?? "Actif"
            };

            var result = await _fournisseurService.CreateFournisseurAsync(fournisseur);

            if (result != null)
            {
                await CloseAsync(result); // Return the created DTO
            }
            else
            {
                ErrorLabel.Text = "Erreur lors de la création du fournisseur";
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
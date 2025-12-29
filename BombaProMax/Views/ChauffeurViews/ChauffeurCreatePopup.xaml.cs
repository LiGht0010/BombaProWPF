using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ChauffeurViews;

public partial class ChauffeurCreatePopup : Popup
{
    private readonly ChauffeurService _chauffeurService;
    private readonly FournisseurService _fournisseurService;
    private List<FournisseurDto> _fournisseurs = [];
    private List<FournisseurDto?> _fournisseursWithNone = [];

    public ChauffeurCreatePopup(ChauffeurService chauffeurService, FournisseurService fournisseurService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        _chauffeurService = chauffeurService;
        _fournisseurService = fournisseurService;

        // Load fournisseurs for picker
        LoadFournisseursAsync();
    }

    private async void LoadFournisseursAsync()
    {
        try
        {
            _fournisseurs = await _fournisseurService.GetAllFournisseursAsync();

            // Create a list with a "None" placeholder (null with display text handled via converter or we use a wrapper)
            // For simplicity, we'll add a fake "Aucun" FournisseurDto at the start
            _fournisseursWithNone = [new FournisseurDto { ID = 0, Societe = "Aucun" }, .. _fournisseurs];

            FournisseurPicker.ItemsSource = _fournisseursWithNone;
            FournisseurPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading fournisseurs: {ex.Message}");
        }
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        try
        {
            // Hide previous error
            ErrorLabel.IsVisible = false;

            // Validate input
            if (string.IsNullOrWhiteSpace(NomEntry.Text))
            {
                ShowError("Le nom est obligatoire");
                return;
            }

            // Create new chauffeur DTO
            var newChauffeur = new ChauffeurDto
            {
                Nom = NomEntry.Text.Trim(),
                Prenom = string.IsNullOrWhiteSpace(PrenomEntry.Text) ? null : PrenomEntry.Text.Trim(),
                CIN = string.IsNullOrWhiteSpace(CINEntry.Text) ? null : CINEntry.Text.Trim(),
                Telephone = string.IsNullOrWhiteSpace(TelephoneEntry.Text) ? null : TelephoneEntry.Text.Trim(),
                NumeroPermis = string.IsNullOrWhiteSpace(NumeroPermisEntry.Text) ? null : NumeroPermisEntry.Text.Trim()
            };

            // Set fournisseur if selected (ID > 0 means a real fournisseur was selected)
            if (FournisseurPicker.SelectedItem is FournisseurDto selectedFournisseur && selectedFournisseur.ID > 0)
            {
                newChauffeur.FournisseurID = selectedFournisseur.ID;
                newChauffeur.FournisseurNom = selectedFournisseur.Nom;
            }

            // Save to database via service
            var createdChauffeur = await _chauffeurService.CreateChauffeurAsync(newChauffeur);

            if (createdChauffeur != null)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Succès",
                    $"Chauffeur {newChauffeur.Nom} {newChauffeur.Prenom} créé avec succès!",
                    "OK");
                Close(createdChauffeur);
            }
            else
            {
                ShowError("Erreur lors de la création du chauffeur");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Une erreur s'est produite: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}
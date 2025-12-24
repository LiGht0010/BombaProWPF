using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ChauffeurViews;

public partial class ChauffeurCreatePopup : Popup
{
    private readonly ChauffeurService _chauffeurService;
    private readonly FournisseurService _fournisseurService;
    private List<FournisseurDto> _fournisseurs = new();

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

            // Add a "None" option at the beginning
            var displayList = new List<string> { "Aucun" };
            displayList.AddRange(_fournisseurs.Select(f => $"{f.Nom} {f.Prenom} - {f.Societe ?? ""}".Trim()));

            FournisseurPicker.ItemsSource = displayList;
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

            // Set fournisseur if selected (index > 0 means a fournisseur was selected)
            if (FournisseurPicker.SelectedIndex > 0)
            {
                var selectedFournisseur = _fournisseurs[FournisseurPicker.SelectedIndex - 1];
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
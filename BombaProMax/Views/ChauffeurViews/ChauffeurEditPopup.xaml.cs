using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ChauffeurViews;

public partial class ChauffeurEditPopup : Popup
{
    private readonly ChauffeurService _chauffeurService;
    private readonly FournisseurService _fournisseurService;
    private readonly ChauffeurDto _chauffeur;
    private List<FournisseurDto> _fournisseurs = new();

    public ChauffeurEditPopup(ChauffeurDto chauffeur)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        _chauffeur = chauffeur;
        _chauffeurService = new ChauffeurService();
        _fournisseurService = new FournisseurService();

        // Populate form with existing data
        PopulateForm();
        
        // Load fournisseurs for picker
        LoadFournisseursAsync();
    }

    private void PopulateForm()
    {
        NomEntry.Text = _chauffeur.Nom;
        PrenomEntry.Text = _chauffeur.Prenom;
        CINEntry.Text = _chauffeur.CIN;
        TelephoneEntry.Text = _chauffeur.Telephone;
        NumeroPermisEntry.Text = _chauffeur.NumeroPermis;
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
            
            // Select the current fournisseur if any
            if (_chauffeur.FournisseurID.HasValue)
            {
                var index = _fournisseurs.FindIndex(f => f.ID == _chauffeur.FournisseurID.Value);
                FournisseurPicker.SelectedIndex = index >= 0 ? index + 1 : 0;
            }
            else
            {
                FournisseurPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading fournisseurs: {ex.Message}");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(NomEntry.Text))
            {
                ShowError("Le nom est requis");
                return;
            }

            // Update chauffeur properties
            _chauffeur.Nom = NomEntry.Text.Trim();
            _chauffeur.Prenom = string.IsNullOrWhiteSpace(PrenomEntry.Text) ? null : PrenomEntry.Text.Trim();
            _chauffeur.CIN = string.IsNullOrWhiteSpace(CINEntry.Text) ? null : CINEntry.Text.Trim();
            _chauffeur.Telephone = string.IsNullOrWhiteSpace(TelephoneEntry.Text) ? null : TelephoneEntry.Text.Trim();
            _chauffeur.NumeroPermis = string.IsNullOrWhiteSpace(NumeroPermisEntry.Text) ? null : NumeroPermisEntry.Text.Trim();

            // Set fournisseur if selected (index > 0 means a fournisseur was selected)
            if (FournisseurPicker.SelectedIndex > 0)
            {
                var selectedFournisseur = _fournisseurs[FournisseurPicker.SelectedIndex - 1];
                _chauffeur.FournisseurID = selectedFournisseur.ID;
                _chauffeur.FournisseurNom = selectedFournisseur.Nom;
            }
            else
            {
                _chauffeur.FournisseurID = null;
                _chauffeur.FournisseurNom = null;
            }

            // Save to database via service
            var success = await _chauffeurService.UpdateChauffeurAsync(_chauffeur);

            if (success)
            {
                // Close popup and return success
                await CloseAsync(true);
            }
            else
            {
                ShowError("Erreur lors de la mise à jour du chauffeur");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        CloseAsync(false);
    }
}
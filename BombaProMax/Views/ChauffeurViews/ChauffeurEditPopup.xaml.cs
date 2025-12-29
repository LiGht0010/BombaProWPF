using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ChauffeurViews;

public partial class ChauffeurEditPopup : Popup
{
    private readonly ChauffeurService _chauffeurService;
    private readonly FournisseurService _fournisseurService;
    private readonly ChauffeurDto _chauffeur;
    private List<FournisseurDto> _fournisseurs = [];
    private List<FournisseurDto?> _fournisseursWithNone = [];

    public ChauffeurEditPopup(ChauffeurDto chauffeur)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        _chauffeur = chauffeur;
        _chauffeurService = new ChauffeurService();
        _fournisseurService = new FournisseurService();

        // Load data and populate form - single async call to avoid race conditions
        LoadDataAndPopulateFormAsync();
    }

    private async void LoadDataAndPopulateFormAsync()
    {
        try
        {
            // Load fournisseurs first
            _fournisseurs = await _fournisseurService.GetAllFournisseursAsync();

            // Create list with "None" placeholder at the start
            _fournisseursWithNone = [new FournisseurDto { ID = 0, Societe = "Aucun" }, .. _fournisseurs];

            FournisseurPicker.ItemsSource = _fournisseursWithNone;

            // Now populate all form fields after data is loaded
            NomEntry.Text = _chauffeur.Nom;
            PrenomEntry.Text = _chauffeur.Prenom;
            CINEntry.Text = _chauffeur.CIN;
            TelephoneEntry.Text = _chauffeur.Telephone;
            NumeroPermisEntry.Text = _chauffeur.NumeroPermis;

            // Select the current fournisseur if any
            if (_chauffeur.FournisseurID.HasValue)
            {
                var selectedFournisseur = _fournisseursWithNone.FirstOrDefault(f => f?.ID == _chauffeur.FournisseurID.Value);
                if (selectedFournisseur != null)
                {
                    FournisseurPicker.SelectedItem = selectedFournisseur;
                }
                else
                {
                    FournisseurPicker.SelectedIndex = 0;
                }
            }
            else
            {
                FournisseurPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
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

            // Set fournisseur if selected (ID > 0 means a real fournisseur was selected)
            if (FournisseurPicker.SelectedItem is FournisseurDto selectedFournisseur && selectedFournisseur.ID > 0)
            {
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
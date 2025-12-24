using CommunityToolkit.Maui.Views;
using BombaProMax.Services;
using BombaProMax.Models;

namespace BombaProMax.Views.CamionViews;

public partial class CamionEditPopup : Popup
{
    private readonly CamionService _camionService;
    private readonly FournisseurService _fournisseurService;
    private readonly CiterneService _citerneService;
    private readonly CamionDto _camion;
    private List<FournisseurDto> _fournisseurs = new();
    private List<CiterneDto> _citernes = new();

    public CamionEditPopup(CamionService camionService, FournisseurService fournisseurService, CiterneService citerneService, CamionDto camion)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        
        _camionService = camionService;
        _fournisseurService = fournisseurService;
        _citerneService = citerneService;
        _camion = camion;
        
        LoadPickerDataAndSetValues();
    }

    private async void LoadPickerDataAndSetValues()
    {
        try
        {
            // Load fournisseurs
            _fournisseurs = await _fournisseurService.GetActiveFournisseursAsync();
            if (_fournisseurs.Count > 0)
            {
                int selectedFournisseurIndex = -1;
                for (int i = 0; i < _fournisseurs.Count; i++)
                {
                    var fournisseur = _fournisseurs[i];
                    var displayName = $"{fournisseur.Prenom} {fournisseur.Nom}".Trim();
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        displayName = fournisseur.Societe ?? $"ID: {fournisseur.ID}";
                    }
                    FournisseurPicker.Items.Add(displayName);

                    if (fournisseur.ID == _camion.FournisseurID)
                    {
                        selectedFournisseurIndex = i;
                    }
                }
                FournisseurPicker.SelectedIndex = selectedFournisseurIndex;
            }

            // Load all citernes
            _citernes = await _citerneService.GetAllCiternesAsync();

            CiternePicker.Items.Add("Aucune"); // Add "None" option
            int selectedCiterneIndex = 0; // Default to "Aucune"
            
            if (_citernes.Count > 0)
            {
                for (int i = 0; i < _citernes.Count; i++)
                {
                    var citerne = _citernes[i];
                    var displayName = $"{citerne.MatriculeCiterne ?? "N/A"} - {citerne.Capacite:N0}L";
                    CiternePicker.Items.Add(displayName);

                    if (_camion.CiterneID.HasValue && citerne.ID == _camion.CiterneID.Value)
                    {
                        selectedCiterneIndex = i + 1; // +1 because "Aucune" is at index 0
                    }
                }
            }
            CiternePicker.SelectedIndex = selectedCiterneIndex;

            // Set current values
            MatriculeEntry.Text = _camion.Matricule;
            MarqueEntry.Text = _camion.Marque;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur lors du chargement des données: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(false);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Validate required inputs
        if (string.IsNullOrWhiteSpace(MatriculeEntry.Text))
        {
            ErrorLabel.Text = "Le matricule est obligatoire";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (FournisseurPicker.SelectedIndex < 0)
        {
            ErrorLabel.Text = "Veuillez sélectionner un fournisseur";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            // Check if matricule is unique (excluding current camion)
            var isUnique = await _camionService.IsMatriculeUniqueAsync(MatriculeEntry.Text.Trim(), _camion.ID);
            if (!isUnique)
            {
                ErrorLabel.Text = "Ce matricule existe déjà";
                ErrorLabel.IsVisible = true;
                return;
            }

            // Update camion properties
            _camion.Matricule = MatriculeEntry.Text.Trim();
            _camion.Marque = string.IsNullOrWhiteSpace(MarqueEntry.Text) ? null : MarqueEntry.Text.Trim();
            _camion.FournisseurID = _fournisseurs[FournisseurPicker.SelectedIndex].ID;
            _camion.CiterneID = GetSelectedCiterneId();

            var success = await _camionService.UpdateCamionAsync(_camion);

            if (success)
            {
                await CloseAsync(true);
            }
            else
            {
                ErrorLabel.Text = "Erreur lors de la mise à jour du camion";
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }

    private int? GetSelectedCiterneId()
    {
        if (CiternePicker.SelectedIndex <= 0) // 0 is "Aucune"
        {
            return null;
        }

        // Subtract 1 because first item is "Aucune"
        var citerneIndex = CiternePicker.SelectedIndex - 1;
        if (citerneIndex >= 0 && citerneIndex < _citernes.Count)
        {
            return _citernes[citerneIndex].ID;
        }

        return null;
    }
}
using CommunityToolkit.Maui.Views;
using BombaProMax.Services;
using BombaProMax.Models;

namespace BombaProMax.Views.CamionViews;

public partial class CamionCreatePopup : Popup
{
    private readonly CamionService _camionService;
    private readonly FournisseurService _fournisseurService;
    private readonly CiterneService _citerneService;
    private List<FournisseurDto> _fournisseurs = new();
    private List<CiterneDto> _citernes = new();

    public CamionCreatePopup(CamionService camionService, FournisseurService fournisseurService, CiterneService citerneService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;
        
        _camionService = camionService;
        _fournisseurService = fournisseurService;
        _citerneService = citerneService;
        
        LoadPickerData();
    }

    private async void LoadPickerData()
    {
        try
        {
            // Load active fournisseurs
            _fournisseurs = await _fournisseurService.GetActiveFournisseursAsync();
            if (_fournisseurs.Count > 0)
            {
                foreach (var fournisseur in _fournisseurs)
                {
                    var displayName = $"{fournisseur.Prenom} {fournisseur.Nom}".Trim();
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        displayName = fournisseur.Societe ?? $"ID: {fournisseur.ID}";
                    }
                    FournisseurPicker.Items.Add(displayName);
                }
            }

            // Load all citernes (user can select any)
            _citernes = await _citerneService.GetAllCiternesAsync();
            CiternePicker.Items.Add("Aucune"); // Add "None" option
            if (_citernes.Count > 0)
            {
                foreach (var citerne in _citernes)
                {
                    var displayName = $"{citerne.MatriculeCiterne ?? "N/A"} - {citerne.Capacite:N0}L";
                    CiternePicker.Items.Add(displayName);
                }
            }
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

    private async void OnCreateClicked(object sender, EventArgs e)
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
            // Check if matricule is unique
            var isUnique = await _camionService.IsMatriculeUniqueAsync(MatriculeEntry.Text.Trim());
            if (!isUnique)
            {
                ErrorLabel.Text = "Ce matricule existe déjà";
                ErrorLabel.IsVisible = true;
                return;
            }

            var camion = new CamionDto
            {
                Matricule = MatriculeEntry.Text.Trim(),
                Marque = string.IsNullOrWhiteSpace(MarqueEntry.Text) ? null : MarqueEntry.Text.Trim(),
                FournisseurID = _fournisseurs[FournisseurPicker.SelectedIndex].ID,
                CiterneID = GetSelectedCiterneId()
            };

            var result = await _camionService.CreateCamionAsync(camion);

            if (result != null)
            {
                await CloseAsync(result); // Return the created DTO
            }
            else
            {
                ErrorLabel.Text = "Erreur lors de la création du camion";
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
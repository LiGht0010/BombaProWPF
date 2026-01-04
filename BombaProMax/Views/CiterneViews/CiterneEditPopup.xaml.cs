using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.CiterneViews
{
    public partial class CiterneEditPopup : Popup
    {
        private readonly CiterneService _citerneService;
        private readonly FournisseurService _fournisseurService;
        private List<FournisseurDto> _fournisseurs = new();
        private readonly CiterneDto _citerneToEdit;

        public CiterneEditPopup(CiterneService citerneService, FournisseurService fournisseurService, CiterneDto citerne)
        {
            InitializeComponent();
            CanBeDismissedByTappingOutsideOfPopup = false;

            _citerneService = citerneService;
            _fournisseurService = fournisseurService;
            _citerneToEdit = citerne;

            LoadFournisseurs();
        }

        private async void LoadFournisseurs()
        {
            try
            {
                _fournisseurs = await _fournisseurService.GetAllFournisseursAsync();
                FournisseurPicker.ItemsSource = _fournisseurs;

                // Populate form with existing data after loading fournisseurs
                PopulateForm();
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Erreur",
                        $"Impossible de charger les fournisseurs: {ex.Message}",
                        "OK");
                }
            }
        }

        private void PopulateForm()
        {
            if (_citerneToEdit == null) return;

            // Set basic information
            CapaciteEntry.Text = _citerneToEdit.Capacite?.ToString() ?? string.Empty;
            MatriculeEntry.Text = _citerneToEdit.MatriculeCiterne ?? string.Empty;
            PartitionsEntry.Text = _citerneToEdit.PartitionsNumber?.ToString() ?? string.Empty;

            // Select the correct fournisseur by setting SelectedItem
            if (_citerneToEdit.FournisseurID.HasValue && _fournisseurs.Count > 0)
            {
                var selectedFournisseur = _fournisseurs.FirstOrDefault(f => f.ID == _citerneToEdit.FournisseurID.Value);
                if (selectedFournisseur != null)
                {
                    FournisseurPicker.SelectedItem = selectedFournisseur;
                }
            }
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            try
            {
                // Hide previous error
                ErrorLabel.IsVisible = false;

                // Validate inputs
                if (string.IsNullOrWhiteSpace(CapaciteEntry.Text))
                {
                    ShowError("La capacité est obligatoire");
                    return;
                }

                if (!decimal.TryParse(CapaciteEntry.Text, out decimal capacite) || capacite <= 0)
                {
                    ShowError("La capacité doit être un nombre positif");
                    return;
                }

                if (FournisseurPicker.SelectedItem is not FournisseurDto selectedFournisseur)
                {
                    ShowError("Veuillez sélectionner un fournisseur");
                    return;
                }

                // Validate partitions if provided
                uint? partitions = null;
                if (!string.IsNullOrWhiteSpace(PartitionsEntry.Text))
                {
                    if (!uint.TryParse(PartitionsEntry.Text, out uint partitionsValue))
                    {
                        ShowError("Le nombre de partitions doit être un nombre entier positif");
                        return;
                    }
                    partitions = partitionsValue;
                }

                // Update citerne properties
                _citerneToEdit.Capacite = capacite;
                _citerneToEdit.MatriculeCiterne = string.IsNullOrWhiteSpace(MatriculeEntry.Text)
                    ? null
                    : MatriculeEntry.Text.Trim();
                _citerneToEdit.PartitionsNumber = partitions;
                _citerneToEdit.FournisseurID = selectedFournisseur.ID;

                // Update in database
                var result = await _citerneService.UpdateCiterneAsync(_citerneToEdit);

                if (result)
                {
                    await CloseAsync(true);
                }
                else
                {
                    ShowError("Erreur lors de la modification de la citerne");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Une erreur s'est produite: {ex.Message}");
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close(false);
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }
    }
}
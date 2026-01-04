using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.CiterneViews
{
    public partial class CiterneCreatePopup : Popup
    {
        private readonly CiterneService _citerneService;
        private readonly FournisseurService _fournisseurService;
        private List<FournisseurDto> _fournisseurs = new();

        public CiterneCreatePopup(CiterneService citerneService, FournisseurService fournisseurService)
        {
            InitializeComponent();
            CanBeDismissedByTappingOutsideOfPopup = false;

            _citerneService = citerneService;
            _fournisseurService = fournisseurService;

            LoadFournisseurs();
        }

        private async void LoadFournisseurs()
        {
            try
            {
                _fournisseurs = await _fournisseurService.GetAllFournisseursAsync();
                FournisseurPicker.ItemsSource = _fournisseurs;
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

        private async void OnCreateClicked(object sender, EventArgs e)
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

                // Create new citerne DTO
                var newCiterne = new CiterneDto
                {
                    Capacite = capacite,
                    MatriculeCiterne = string.IsNullOrWhiteSpace(MatriculeEntry.Text)
                        ? null
                        : MatriculeEntry.Text.Trim(),
                    PartitionsNumber = partitions,
                    FournisseurID = selectedFournisseur.ID
                };

                // Save to database
                var result = await _citerneService.CreateCiterneAsync(newCiterne);

                if (result != null)
                {
                    await CloseAsync(result); // Return the created DTO
                }
                else
                {
                    ShowError("Erreur lors de la création de la citerne");
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

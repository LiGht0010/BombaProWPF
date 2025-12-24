using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.EmployeViews;

public partial class EmployeCreatePopup : Popup
{
    private readonly EmployeService _employeService;
    private List<string> _postes = new();

    public EmployeCreatePopup(EmployeService employeService)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _employeService = employeService;

        LoadPostes();
    }

    private async void LoadPostes()
    {
        try
        {
            // Load unique postes from existing employees
            _postes = await _employeService.GetUniquePostesAsync();

            // Add common postes if not already present
            var commonPostes = new List<string>
            {
                "Chauffeur",
                "Mécanicien",
                "Comptable",
                "Gestionnaire",
                "Superviseur",
                "Technicien",
                "Assistant",
                "Opérateur"
            };

            foreach (var poste in commonPostes)
            {
                if (!_postes.Contains(poste))
                {
                    _postes.Add(poste);
                }
            }

            // Sort alphabetically
            _postes = _postes.OrderBy(p => p).ToList();

            PostePicker.ItemsSource = _postes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading postes: {ex.Message}");
            // Set default postes on error
            PostePicker.ItemsSource = new List<string>
            {
                "Chauffeur",
                "Mécanicien",
                "Comptable",
                "Gestionnaire",
                "Superviseur",
                "Technicien",
                "Assistant",
                "Opérateur"
            };
        }
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        try
        {
            // Hide previous error
            ErrorLabel.IsVisible = false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(NomEntry.Text))
            {
                ShowError("Le nom est obligatoire");
                return;
            }

            if (string.IsNullOrWhiteSpace(PrenomEntry.Text))
            {
                ShowError("Le prénom est obligatoire");
                return;
            }

            if (string.IsNullOrWhiteSpace(CINEntry.Text))
            {
                ShowError("Le CIN est obligatoire");
                return;
            }

            if (string.IsNullOrWhiteSpace(AddressEntry.Text))
            {
                ShowError("L'adresse est obligatoire");
                return;
            }

            // Check if CIN already exists
            var cinExists = await _employeService.CINExistsAsync(CINEntry.Text.Trim());
            if (cinExists)
            {
                ShowError("Un employé avec ce CIN existe déjà");
                return;
            }

            // Validate telephone format if provided
            if (!string.IsNullOrWhiteSpace(TelephoneEntry.Text))
            {
                var telephone = TelephoneEntry.Text.Trim();
                if (telephone.Length < 10)
                {
                    ShowError("Le numéro de téléphone doit contenir au moins 10 chiffres");
                    return;
                }
            }

            // Validate and parse salaire if provided
            decimal? salaire = null;
            if (!string.IsNullOrWhiteSpace(SalaireEntry.Text))
            {
                if (!decimal.TryParse(SalaireEntry.Text, out decimal salaireValue) || salaireValue < 0)
                {
                    ShowError("Le salaire doit être un nombre positif");
                    return;
                }
                salaire = salaireValue;
            }

            // Get selected poste
            string? poste = null;
            if (PostePicker.SelectedIndex >= 0)
            {
                poste = _postes[PostePicker.SelectedIndex];
            }

            // Create new employe DTO
            var newEmploye = new EmployeDto
            {
                Nom = NomEntry.Text.Trim(),
                Prenom = PrenomEntry.Text.Trim(),
                CIN = CINEntry.Text.Trim(),
                Telephone = string.IsNullOrWhiteSpace(TelephoneEntry.Text)
                    ? null
                    : TelephoneEntry.Text.Trim(),
                Address = AddressEntry.Text.Trim(),
                Poste = poste,
                Salaire = salaire
            };

            // Save to database (service will set AjoutePar and DateCreation)
            var result = await _employeService.CreateEmployeAsync(newEmploye);

            if (result != null)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Succès",
                    $"Employé {newEmploye.Prenom} {newEmploye.Nom} créé avec succès!",
                    "OK");
                Close(true); // Return true to indicate success
            }
            else
            {
                ShowError("Erreur lors de la création de l'employé");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Une erreur s'est produite: {ex.Message}");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(false); // Return false to indicate cancellation
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
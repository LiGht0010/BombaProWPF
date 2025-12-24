using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.EmployeViews;

public partial class EmployeEditPopup : Popup
{
    private readonly EmployeService _employeService;
    private readonly EmployeDto _employeToEdit;
    private List<string> _postes = new();

    public EmployeEditPopup(EmployeService employeService, EmployeDto employe)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _employeService = employeService;
        _employeToEdit = employe;

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

            // Populate form with existing data
            PopulateForm();
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
            PopulateForm();
        }
    }

    private void PopulateForm()
    {
        if (_employeToEdit == null) return;

        // Set personal information
        NomEntry.Text = _employeToEdit.Nom ?? string.Empty;
        PrenomEntry.Text = _employeToEdit.Prenom ?? string.Empty;
        CINEntry.Text = _employeToEdit.CIN ?? string.Empty;
        TelephoneEntry.Text = _employeToEdit.Telephone ?? string.Empty;
        AddressEntry.Text = _employeToEdit.Address ?? string.Empty;

        // Set employment information
        SalaireEntry.Text = _employeToEdit.Salaire?.ToString() ?? string.Empty;

        // Select the correct poste
        if (!string.IsNullOrWhiteSpace(_employeToEdit.Poste) && _postes != null)
        {
            var posteIndex = _postes.FindIndex(p => p.Equals(_employeToEdit.Poste, StringComparison.OrdinalIgnoreCase));
            if (posteIndex >= 0)
            {
                PostePicker.SelectedIndex = posteIndex;
            }
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
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

            // Check if CIN already exists (excluding current employee)
            var cinExists = await _employeService.CINExistsAsync(CINEntry.Text.Trim(), _employeToEdit.ID);
            if (cinExists)
            {
                ShowError("Un autre employé avec ce CIN existe déjà");
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

            // Update employe DTO properties
            _employeToEdit.Nom = NomEntry.Text.Trim();
            _employeToEdit.Prenom = PrenomEntry.Text.Trim();
            _employeToEdit.CIN = CINEntry.Text.Trim();
            _employeToEdit.Telephone = string.IsNullOrWhiteSpace(TelephoneEntry.Text)
                ? null
                : TelephoneEntry.Text.Trim();
            _employeToEdit.Address = AddressEntry.Text.Trim();
            _employeToEdit.Poste = poste;
            _employeToEdit.Salaire = salaire;

            // Update in database (service will set ModifiePar and DateModification)
            var result = await _employeService.UpdateEmployeAsync(_employeToEdit);

            if (result)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Succès",
                    $"Employé {_employeToEdit.Prenom} {_employeToEdit.Nom} modifié avec succès!",
                    "OK");
                Close(true); // Return true to indicate success
            }
            else
            {
                ShowError("Erreur lors de la modification de l'employé");
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
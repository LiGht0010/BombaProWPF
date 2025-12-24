using System.Text.Json;
using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.User;

public partial class UserCreatePopup : Popup
{
    private readonly UserService _userService;

    public UserCreatePopup(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowError("Le nom est requis");
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            ShowError("L'email est requis");
            return;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            ShowError("Le mot de passe est requis");
            return;
        }

        if (PasswordEntry.Text.Length < 8)
        {
            ShowError("Le mot de passe doit contenir au moins 8 caractères");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            ShowError("Les mots de passe ne correspondent pas");
            return;
        }

        if (RolePicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner un rôle");
            return;
        }

        // Check if email already exists
        if (await _userService.EmailExistsAsync(EmailEntry.Text.Trim()))
        {
            ShowError("Cette adresse email est déjà utilisée");
            return;
        }

        ErrorLabel.IsVisible = false;

        // Determine role flags based on selection
        var selectedRole = RolePicker.SelectedItem.ToString();
        bool isAdmin = selectedRole == "Administrateur" || selectedRole == "Superviseur";
        bool isSuperAdmin = selectedRole == "Superviseur";

        var currentUser = App.CurrentUser;

        // Create user DTO with form data
        var newUser = new UserDto
        {
            Name = NameEntry.Text.Trim(),
            Email = EmailEntry.Text.Trim(),
            Password = PasswordEntry.Text,
            IsAdmin = isAdmin,
            IsSuperAdmin = isSuperAdmin,
            IsActive = StatusSwitch.IsToggled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUser?.UserId ?? 0,
            UpdatedBy = currentUser?.UserId ?? 0,

            // Permissions from checkboxes
            CanViewReports = ViewReportsPermission.IsChecked,
            CanManageUsers = ManageUsersPermission.IsChecked,
            CanManageSettings = SystemSettingsPermission.IsChecked,
            CanManageProducts = ManageProductsPermission.IsChecked,
            CanManageCustomers = ManageClientsPermission.IsChecked,
            CanManageSales = ManageSalesPermission.IsChecked,
            ShowDepenses = ManageExpensesPermission.IsChecked,

            // Default other permissions based on role
            CanManagePromotions = isAdmin,
            CanManageSuppliers = isAdmin,
            CanManageCategories = isAdmin,
            ShowAcceuil = true,
            ShowTableauDeBord = isAdmin,
            ShowVente = true,
            EditLivreur = isAdmin,
            AddAchat = isAdmin,
            EditCiternes = isAdmin,
            EditPistolets = isAdmin,
            EditClients = ManageClientsPermission.IsChecked,
            AddBonFacturation = ManageSalesPermission.IsChecked
        };

        try
        {
            var createdUser = await _userService.CreateAsync(newUser);

            if (createdUser != null)
            {
                await CloseAsync(createdUser);
            }
            else
            {
                ShowError("Erreur lors de la création de l'utilisateur");
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
}
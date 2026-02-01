using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.User;

public partial class UserEditPopup : Popup
{
    private readonly UserService _userService;
    private readonly UserDto _userToEdit;

    public UserEditPopup(UserService userService, UserDto user)
    {
        InitializeComponent();
        _userService = userService;
        _userToEdit = user;

        // Set UserDto as BindingContext for data binding
        BindingContext = _userToEdit;

        // Initialize controls that can't use binding
        InitializeNonBindableControls();
    }

    private void InitializeNonBindableControls()
    {
        // Set role picker based on user flags (can't easily bind to multiple bools)
        if (_userToEdit.IsSuperAdmin)
            RolePicker.SelectedItem = "Superviseur";
        else if (_userToEdit.IsAdmin)
            RolePicker.SelectedItem = "Administrateur";
        else
            RolePicker.SelectedItem = "Utilisateur";

        // Update status label
        UpdateStatusLabel(_userToEdit.IsActive);
    }

    private void UpdateStatusLabel(bool isActive)
    {
        StatusLabel.Text = isActive
            ? "Actif - L'utilisateur peut se connecter"
            : "Inactif - L'utilisateur ne peut pas se connecter";
        StatusLabel.TextColor = isActive ? Color.FromArgb("#4CAF50") : Color.FromArgb("#F44336");
    }

    private void OnChangePasswordToggled(object sender, ToggledEventArgs e)
    {
        PasswordSection.IsVisible = e.Value;
    }

    private void OnStatusToggled(object sender, ToggledEventArgs e)
    {
        UpdateStatusLabel(e.Value);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Validate input (bindings have already updated _userToEdit)
        if (string.IsNullOrWhiteSpace(_userToEdit.Name))
        {
            ShowError("Le nom est requis");
            return;
        }

        if (string.IsNullOrWhiteSpace(_userToEdit.Email))
        {
            ShowError("L'email est requis");
            return;
        }

        if (RolePicker.SelectedItem == null)
        {
            ShowError("Veuillez sélectionner un rôle");
            return;
        }

        // Check if email already exists (excluding current user)
        if (await _userService.EmailExistsAsync(_userToEdit.Email.Trim(), _userToEdit.UserId))
        {
            ShowError("Cette adresse email est déjà utilisée");
            return;
        }

        // If changing password, validate new password
        if (ChangePasswordSwitch.IsToggled)
        {
            if (string.IsNullOrWhiteSpace(NewPasswordEntry.Text))
            {
                ShowError("Le nouveau mot de passe est requis");
                return;
            }

            if (NewPasswordEntry.Text.Length < 4)
            {
                ShowError("Le mot de passe doit contenir au moins 4 caractères");
                return;
            }

            if (NewPasswordEntry.Text != ConfirmPasswordEntry.Text)
            {
                ShowError("Les mots de passe ne correspondent pas");
                return;
            }

            _userToEdit.Password = NewPasswordEntry.Text;
        }

        ErrorLabel.IsVisible = false;

        // Determine role flags based on selection
        var selectedRole = RolePicker.SelectedItem.ToString();
        bool isAdmin = selectedRole == "Administrateur" || selectedRole == "Superviseur";
        bool isSuperAdmin = selectedRole == "Superviseur";

        var currentUser = App.CurrentUser;

        // Update role flags and metadata
        _userToEdit.IsAdmin = isAdmin;
        _userToEdit.IsSuperAdmin = isSuperAdmin;
        _userToEdit.UpdatedAt = DateTime.UtcNow;
        _userToEdit.UpdatedBy = currentUser?.UserId ?? 0;

        // Update other permissions based on role
        _userToEdit.CanManagePromotions = isAdmin;
        _userToEdit.CanManageSuppliers = isAdmin;
        _userToEdit.CanManageCategories = isAdmin;
        _userToEdit.ShowTableauDeBord = isAdmin;
        _userToEdit.EditLivreur = isAdmin;
        _userToEdit.AddAchat = isAdmin;
        _userToEdit.EditCiternes = isAdmin;
        _userToEdit.EditPistolets = isAdmin;
        _userToEdit.EditClients = _userToEdit.CanManageCustomers;
        _userToEdit.AddBonFacturation = _userToEdit.CanManageSales;

        try
        {
            var success = await _userService.UpdateAsync(_userToEdit);

            if (success)
            {
                await CloseAsync(true);
            }
            else
            {
                ShowError("Erreur lors de la mise à jour de l'utilisateur");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur: {ex.Message}");
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
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.AchatViews;

/// <summary>
/// Popup that prompts the user to confirm their password before proceeding with a sensitive operation.
/// </summary>
public partial class PasswordConfirmationPopup : Popup
{
    private readonly ILoginRepository _loginService;
    private readonly string _userEmail;
    private bool _isProcessing;

    public PasswordConfirmationPopup()
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _loginService = new LoginServices();
        _userEmail = App.CurrentUser?.Email ?? App.user?.Email ?? string.Empty;

        if (string.IsNullOrEmpty(_userEmail))
        {
            ShowError("Utilisateur non connecté");
            ConfirmButton.IsEnabled = false;
        }
    }

    private void OnPasswordEntryCompleted(object? sender, EventArgs e)
    {
        // Trigger confirm when user presses Enter/Done on keyboard
        OnConfirmClicked(sender, e);
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (_isProcessing)
            return;

        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Veuillez saisir votre mot de passe");
            return;
        }

        if (string.IsNullOrEmpty(_userEmail))
        {
            ShowError("Utilisateur non connecté");
            return;
        }

        _isProcessing = true;
        SetLoadingState(true);
        HideError();

        try
        {
            // Verify password by attempting login with current user's email
            var user = await _loginService.Login(_userEmail, password);

            if (user != null)
            {
                // Password confirmed successfully
                await CloseAsync(true);
            }
            else
            {
                // Login failed - password incorrect
                // Note: LoginServices already shows an alert, but we also show inline error
                ShowError("Mot de passe incorrect");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PasswordConfirmationPopup] Error: {ex.Message}");
            ShowError("Erreur lors de la vérification");
        }
        finally
        {
            _isProcessing = false;
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        ConfirmButton.IsEnabled = !isLoading;
        CancelButton.IsEnabled = !isLoading;
        PasswordEntry.IsEnabled = !isLoading;
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void HideError()
    {
        ErrorLabel.IsVisible = false;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        if (_isProcessing)
            return;

        Close(false);
    }
}

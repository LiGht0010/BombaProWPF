using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Models;
using FourniPro.Resources;
using FourniPro.Services;

namespace FourniPro.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    partial void OnEmailChanged(string value) => SignInCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => SignInCommand.NotifyCanExecuteChanged();
    partial void OnIsBusyChanged(bool value) => SignInCommand.NotifyCanExecuteChanged();

    private readonly ILoginRepository _loginService = new LoginServices();

    /// <summary>Raised when login succeeds so the host window can navigate.</summary>
    public event Action? LoginSucceeded;

    private bool CanSignIn() =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password);

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    public async Task SignIn()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var user = await _loginService.Login(Email, Password);
            if (user is null)
            {
                ErrorMessage = Strings.InvalidCredentials;
                return;
            }

            App.CurrentUser = user;
            LoginSucceeded?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ForgotPassword()
    {
        // TODO: implement password recovery.
    }
}

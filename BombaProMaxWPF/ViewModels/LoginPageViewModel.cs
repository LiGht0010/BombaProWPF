using BombaProMaxWPF.Models;
using BombaProMaxWPF.Resources;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

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

    readonly ILoginRepository loginservice = new LoginServices();
    private readonly OpeningBalanceOnboardingService _onboardingService;

    /// <summary>
    /// Raised when login succeeds so the host window can navigate.
    /// </summary>
    public event Action? LoginSucceeded;

    public LoginPageViewModel(OpeningBalanceOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// Enables the sign-in button only when both fields are populated and no request is in flight.
    /// </summary>
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
            var user = await loginservice.Login(Email, Password);
            if (user is null)
            {
                ErrorMessage = Strings.InvalidCredentials;
                return;
            }

            App.user = user;
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
        MessageBox.Show("Forgot password not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    public void Signup()
    {
        MessageBox.Show("Signup not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
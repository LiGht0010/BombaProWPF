using BombaProMaxWPF.Models;
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

    [RelayCommand]
    public async Task SignIn()
    {
        try
        {
            if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
            {
                var user = await loginservice.Login(Email, Password);
                if (user != null)
                {
                    // Set BOTH user properties
                    App.user = user;
                    App.CurrentUser = user;

                    // Notify host to navigate
                    LoginSucceeded?.Invoke();
                }
            }
            else
            {
                MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
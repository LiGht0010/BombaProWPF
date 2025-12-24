using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace BombaProMax.ViewModels;

public partial class LoginPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    readonly ILoginRepository loginservice = new LoginServices();

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
                    if (Preferences.ContainsKey(nameof(App.user)))
                    {
                        Preferences.Remove(nameof(App.user));
                    }
                    string userDetails = JsonConvert.SerializeObject(user);
                    Preferences.Set(nameof(App.user), userDetails);
                    
                    // Set BOTH user properties
                    App.user = user;
                    App.CurrentUser = user;

                    // Navigate to HomePage
                    await Shell.Current.GoToAsync("//HomePage");

                    // Then enable the flyout (hamburger menu) - with a small delay to ensure navigation is complete
                    await Task.Delay(100);
                    Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
                    
                    // Update the user display in the sidebar
                    if (Shell.Current is AppShell appShell)
                    {
                        appShell.UpdateUserDisplay();
                    }
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Please enter both email and password.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public async Task ForgotPassword()
    {
        await Shell.Current.DisplayAlert("Info", "Forgot password not implemented yet", "OK");
    }

    [RelayCommand]
    public async Task Signup()
    {
        await Shell.Current.DisplayAlert("Info", "Signup not implemented yet", "OK");
    }
}
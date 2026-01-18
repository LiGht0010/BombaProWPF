using BombaProMax.Models;
using BombaProMax.Services;
using BombaProMax.Views.OnboardingViews;
using CommunityToolkit.Maui.Views;
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
    private readonly OpeningBalanceOnboardingService _onboardingService;

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

                    // Check if Opening Balance onboarding is needed
                    await CheckAndStartOnboardingAsync();
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

    /// <summary>
    /// Checks if any reservoirs need Opening Balance setup and starts the onboarding flow.
    /// </summary>
    private async Task CheckAndStartOnboardingAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[LoginPageViewModel] Checking for Opening Balance onboarding...");
            
            var isOnboardingNeeded = await _onboardingService.IsOnboardingNeededAsync();
            
            if (isOnboardingNeeded)
            {
                var count = _onboardingService.TotalCount;
                System.Diagnostics.Debug.WriteLine(
                    $"[LoginPageViewModel] Opening Balance onboarding needed for {count} reservoir(s)");
                
                // Small delay to let HomePage fully load
                await Task.Delay(300);
                
                // Show the onboarding popup
                var popup = new OpeningBalancePopup(_onboardingService);
                var result = await Shell.Current.CurrentPage.ShowPopupAsync(popup);
                
                if (popup.CompletedSuccessfully)
                {
                    System.Diagnostics.Debug.WriteLine("[LoginPageViewModel] Opening Balance onboarding completed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoginPageViewModel] Opening Balance onboarding was cancelled");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LoginPageViewModel] No Opening Balance onboarding needed");
            }
        }
        catch (Exception ex)
        {
            // Don't block login if onboarding check fails
            System.Diagnostics.Debug.WriteLine(
                $"[LoginPageViewModel] Error during Opening Balance onboarding check: {ex.Message}");
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
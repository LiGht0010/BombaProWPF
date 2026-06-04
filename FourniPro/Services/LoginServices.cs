using FourniPro.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Windows;

namespace FourniPro.Services;

public class LoginServices : ILoginRepository
{
    public async Task<UserDto?> Login(string email, string password)
    {
        try
        {
            var client = HttpClientFactory.Create();
            string loginUrl = $"{ApiConfig.Users}/Login/{email}/{password}";
            Debug.WriteLine($"[LoginServices] Attempting login for: {email}");

            HttpResponseMessage response = await client.GetAsync(loginUrl);
            Debug.WriteLine($"[LoginServices] Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                Debug.WriteLine($"[LoginServices] Login successful for user: {user?.Name}");
                return user;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[LoginServices] Login failed: {response.StatusCode} - {errorContent}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("Email ou mot de passe incorrect", "Erreur de connexion",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show($"Erreur de connexion: {response.ReasonPhrase}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[LoginServices] HttpRequestException: {ex.Message}");
            MessageBox.Show($"Impossible de contacter le serveur.\n\n{ex.Message}", "Erreur réseau",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Debug.WriteLine($"[LoginServices] Timeout: {ex.Message}");
            MessageBox.Show("Le serveur ne répond pas. Veuillez réessayer.", "Délai dépassé",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
    }
}

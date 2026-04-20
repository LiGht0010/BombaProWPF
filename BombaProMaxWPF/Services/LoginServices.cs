using BombaProMaxWPF.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Windows;

namespace BombaProMaxWPF.Services;

public class LoginServices : ILoginRepository
{
    public async Task<UserDto?> Login(string email, string password)
    {
        try
        {
            // Get the HttpClient
            var client = HttpClientFactory.Create();
            
            string loginUrl = $"{ApiConfig.Users}/Login/{email}/{password}";
            Debug.WriteLine($"[LoginServices] Attempting login for: {email}");
            Debug.WriteLine($"[LoginServices] Login URL: {loginUrl}");
            
            HttpResponseMessage response = await client.GetAsync(loginUrl);
            
            Debug.WriteLine($"[LoginServices] Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                Debug.WriteLine($"[LoginServices] Login successful for user: {user?.Name}");
                return user;
            }
            else
            {
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
            Debug.WriteLine($"[LoginServices] TaskCanceledException (timeout): {ex.Message}");
            MessageBox.Show("Le serveur ne répond pas. Veuillez réessayer.", "Délai dépassé",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LoginServices] Exception: {ex.Message}");
            Debug.WriteLine($"[LoginServices] Stack: {ex.StackTrace}");
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }
}
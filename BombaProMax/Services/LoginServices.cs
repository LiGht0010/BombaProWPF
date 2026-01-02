using BombaProMax.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace BombaProMax.Services;

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
                    await Shell.Current.DisplayAlert("Erreur de connexion", 
                        "Email ou mot de passe incorrect", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erreur", 
                        $"Erreur de connexion: {response.ReasonPhrase}", "OK");
                }
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[LoginServices] HttpRequestException: {ex.Message}");
            await Shell.Current.DisplayAlert("Erreur réseau", 
                $"Impossible de contacter le serveur.\n\n{ex.Message}", "OK");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            Debug.WriteLine($"[LoginServices] TaskCanceledException (timeout): {ex.Message}");
            await Shell.Current.DisplayAlert("Délai dépassé", 
                "Le serveur ne répond pas. Veuillez réessayer.", "OK");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LoginServices] Exception: {ex.Message}");
            Debug.WriteLine($"[LoginServices] Stack: {ex.StackTrace}");
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
            return null;
        }
    }
}
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
            // Use HttpClientFactory to ensure tenant header is configured
            var client = HttpClientFactory.Create();
            
            string loginUrl = $"{ApiConfig.Users}/Login/{email}/{password}";
            Debug.WriteLine($"[LoginServices] Attempting login for: {email}");
            Debug.WriteLine($"[LoginServices] Login URL: {loginUrl}");
            Debug.WriteLine($"[LoginServices] Tenant ID: {ApiConfig.TenantId}");
            
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
        catch (Exception ex)
        {
            Debug.WriteLine($"[LoginServices] Exception: {ex.Message}");
            Debug.WriteLine($"[LoginServices] Stack: {ex.StackTrace}");
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
            return null;
        }
    }
}
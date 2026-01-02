using BombaProMax.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace BombaProMax.Services;

public class UserService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Users;

    public UserService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    /// <summary>
    /// Gets all users from the API.
    /// </summary>
    public async Task<List<UserDto>> GetAllAsync()
    {
        try
        {
            Debug.WriteLine($"[UserService] Fetching users from: {BaseUrl}");
            var response = await _httpClient.GetAsync(BaseUrl);
            Debug.WriteLine($"[UserService] Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<UserDto>>(json) ?? [];
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[UserService] Error response: {errorContent}");
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error fetching users: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    public async Task<UserDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error fetching user: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public async Task<UserDto?> CreateAsync(UserDto user)
    {
        try
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error creating user: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    public async Task<bool> UpdateAsync(UserDto user)
    {
        try
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{user.UserId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error updating user: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error deleting user: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    public async Task<UserDto?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/Login/{email}/{password}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Login error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Searches users by name or email.
    /// </summary>
    public async Task<List<UserDto>> SearchAsync(string searchTerm)
    {
        try
        {
            var allUsers = await GetAllAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allUsers;
            }

            searchTerm = searchTerm.ToLower();
            return allUsers.Where(u =>
                (u.Name?.ToLower().Contains(searchTerm) ?? false) ||
                (u.Email?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error searching users: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets a user's name by their ID.
    /// </summary>
    public async Task<string> GetUserNameByIdAsync(int? userId)
    {
        if (!userId.HasValue || userId.Value <= 0)
            return "N/A";

        try
        {
            var user = await GetByIdAsync(userId.Value);
            return user?.Name ?? "Utilisateur inconnu";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error fetching user name for ID {userId}: {ex.Message}");
            return "Utilisateur inconnu";
        }
    }

    /// <summary>
    /// Checks if an email already exists (for validation during create/edit).
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude (for edit scenarios)</param>
    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        try
        {
            var allUsers = await GetAllAsync();
            return allUsers.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.UserId != excludeUserId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error checking email exists: {ex.Message}");
            return false;
        }
    }
}
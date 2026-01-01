using BombaProMax.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace BombaProMax.Services;

public class UserService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Users;
    
    // Cache for user names to avoid repeated API calls
    private static readonly Dictionary<int, string> _userNameCache = new();
    private static DateTime _cacheLastRefresh = DateTime.MinValue;
    private static readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

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
            Debug.WriteLine($"[UserService] Creating user: {user.Name}");
            Debug.WriteLine($"[UserService] Password provided: {!string.IsNullOrEmpty(user.Password)}, Length: {user.Password?.Length ?? 0}");
            
            var json = JsonConvert.SerializeObject(user);
            Debug.WriteLine($"[UserService] JSON payload: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            Debug.WriteLine($"[UserService] Create response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[UserService] Create response: {responseJson}");
                return JsonConvert.DeserializeObject<UserDto>(responseJson);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[UserService] Create error: {errorContent}");
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

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[UserService] Update error: {errorContent}");
            }

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

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[UserService] Delete error: {errorContent}");
            }

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
    /// Gets users filtered by active status.
    /// </summary>
    public async Task<List<UserDto>> GetByStatusAsync(bool isActive)
    {
        try
        {
            var allUsers = await GetAllAsync();
            return allUsers.Where(u => u.IsActive == isActive).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error filtering users: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Checks if an email already exists.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        try
        {
            var allUsers = await GetAllAsync();
            return allUsers.Any(u =>
                u.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) == true &&
                u.UserId != excludeUserId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error checking email: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a user's name by their ID. Uses caching for performance.
    /// </summary>
    public async Task<string> GetUserNameByIdAsync(int? userId)
    {
        if (!userId.HasValue || userId.Value <= 0)
            return "N/A";

        // Check if cache needs refresh
        if (DateTime.Now - _cacheLastRefresh > _cacheExpiration)
        {
            _userNameCache.Clear();
        }

        // Check cache first
        if (_userNameCache.TryGetValue(userId.Value, out var cachedName))
            return cachedName;

        try
        {
            var user = await GetByIdAsync(userId.Value);
            var userName = user?.Name ?? "Utilisateur inconnu";
            
            // Cache the result
            _userNameCache[userId.Value] = userName;
            _cacheLastRefresh = DateTime.Now;
            
            return userName;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error fetching user name for ID {userId}: {ex.Message}");
            return "Utilisateur inconnu";
        }
    }

    /// <summary>
    /// Preloads user names into cache for multiple IDs at once.
    /// </summary>
    public async Task PreloadUserNamesAsync(IEnumerable<int?> userIds)
    {
        var idsToLoad = userIds
            .Where(id => id.HasValue && id.Value > 0 && !_userNameCache.ContainsKey(id.Value))
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (idsToLoad.Count == 0)
            return;

        try
        {
            var allUsers = await GetAllAsync();
            foreach (var user in allUsers.Where(u => idsToLoad.Contains(u.UserId)))
            {
                _userNameCache[user.UserId] = user.Name ?? "Utilisateur inconnu";
            }
            _cacheLastRefresh = DateTime.Now;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserService] Error preloading user names: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the user name cache.
    /// </summary>
    public static void ClearCache()
    {
        _userNameCache.Clear();
        _cacheLastRefresh = DateTime.MinValue;
    }
}
using System.Diagnostics;
using System.Text;
using BombaProMax.Models;
using Newtonsoft.Json;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing expense categories (DepenseCategories).
/// </summary>
public class DepenseCategorieService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.DepenseCategories;

    public DepenseCategorieService()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    #region CRUD Operations

    /// <summary>
    /// Get all active expense categories.
    /// </summary>
    public async Task<List<DepenseCategorieDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[DepenseCategorieService] GetAll failed: {response.StatusCode}");
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DepenseCategorieDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error fetching categories: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Get all expense categories including inactive ones.
    /// </summary>
    public async Task<List<DepenseCategorieDto>> GetAllIncludingInactiveAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/all");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[DepenseCategorieService] GetAllIncludingInactive failed: {response.StatusCode}");
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DepenseCategorieDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error fetching all categories: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Get category names only (for pickers).
    /// </summary>
    public async Task<List<string>> GetCategoryNamesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/names");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[DepenseCategorieService] GetCategoryNames failed: {response.StatusCode}");
                return GetDefaultCategories();
            }

            var json = await response.Content.ReadAsStringAsync();
            var names = JsonConvert.DeserializeObject<List<string>>(json) ?? [];
            
            // If empty, return defaults
            return names.Count > 0 ? names : GetDefaultCategories();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error fetching category names: {ex.Message}");
            return GetDefaultCategories();
        }
    }

    /// <summary>
    /// Get a category by ID.
    /// </summary>
    public async Task<DepenseCategorieDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[DepenseCategorieService] GetById failed: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DepenseCategorieDto>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error fetching category: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create a new expense category.
    /// </summary>
    public async Task<DepenseCategorieDto?> CreateAsync(DepenseCategorieDto dto)
    {
        try
        {
            dto.CreePar = App.CurrentUser?.Name ?? App.user?.Name ?? "System";
            dto.DateCreation = DateTime.UtcNow;

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(dto, settings);
            Debug.WriteLine($"[DepenseCategorieService] Creating category: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[DepenseCategorieService] Create response: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[DepenseCategorieService] Create failed: {responseContent}");
                return null;
            }

            return JsonConvert.DeserializeObject<DepenseCategorieDto>(responseContent);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error creating category: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an existing expense category.
    /// </summary>
    public async Task<bool> UpdateAsync(DepenseCategorieDto dto)
    {
        try
        {
            dto.ModifiePar = App.CurrentUser?.Name ?? App.user?.Name ?? "System";
            dto.DateModification = DateTime.UtcNow;

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(dto, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[DepenseCategorieService] Update failed: {response.StatusCode} - {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error updating category: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete (soft) an expense category.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[DepenseCategorieService] Delete failed: {response.StatusCode}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error deleting category: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Permanently delete an expense category.
    /// </summary>
    public async Task<bool> DeletePermanentAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}/permanent");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[DepenseCategorieService] Permanent delete failed: {response.StatusCode}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error permanently deleting category: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Search categories by name.
    /// </summary>
    public async Task<List<DepenseCategorieDto>> SearchAsync(string searchTerm)
    {
        try
        {
            var allCategories = await GetAllAsync();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return allCategories;

            searchTerm = searchTerm.ToLower();
            return allCategories.Where(c =>
                (c.Nom?.ToLower().Contains(searchTerm) ?? false) ||
                (c.Description?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DepenseCategorieService] Error searching categories: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Get default expense categories (fallback when API fails).
    /// </summary>
    public static List<string> GetDefaultCategories()
    {
        return
        [
            "Carburant",
            "Électricité",
            "Eau",
            "Téléphone",
            "Internet",
            "Loyer",
            "Salaires",
            "Fournitures",
            "Entretien",
            "Réparations",
            "Assurance",
            "Taxes",
            "Transport",
            "Marketing",
            "Divers"
        ];
    }

    #endregion
}

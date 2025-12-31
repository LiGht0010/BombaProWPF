using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

public class ServiceCategorieService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.ServiceCategories;

    public ServiceCategorieService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    // ============================
    // GET ALL (Active only)
    // ============================
    public async Task<List<ServiceCategorieDto>> GetAllCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ServiceCategorieDto>>(json) ?? new List<ServiceCategorieDto>();
            }
            return new List<ServiceCategorieDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching service categories: {ex.Message}");
            return new List<ServiceCategorieDto>();
        }
    }

    // ============================
    // GET ALL (Including inactive)
    // ============================
    public async Task<List<ServiceCategorieDto>> GetAllCategoriesIncludingInactiveAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/all");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ServiceCategorieDto>>(json) ?? new List<ServiceCategorieDto>();
            }
            return new List<ServiceCategorieDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all service categories: {ex.Message}");
            return new List<ServiceCategorieDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ServiceCategorieDto?> GetCategorieByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ServiceCategorieDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching service category: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET CATEGORY NAMES
    // ============================
    public async Task<List<string>> GetCategorieNamesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/names");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            return new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching category names: {ex.Message}");
            return new List<string>();
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ServiceCategorieDto?> CreateCategorieAsync(ServiceCategorieDto categorie)
    {
        try
        {
            categorie.CreePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            categorie.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(categorie);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ServiceCategorieDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating service category: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateCategorieAsync(ServiceCategorieDto categorie)
    {
        try
        {
            categorie.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            categorie.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(categorie);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{categorie.ID}", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating service category: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE (Soft delete)
    // ============================
    public async Task<bool> DeleteCategorieAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting service category: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE PERMANENT
    // ============================
    public async Task<bool> DeleteCategoriePermanentAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}/permanent");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error permanently deleting service category: {ex.Message}");
            return false;
        }
    }

    // ============================
    // SEARCH (Client-side)
    // ============================
    public async Task<List<ServiceCategorieDto>> SearchCategoriesAsync(string searchTerm)
    {
        try
        {
            var allCategories = await GetAllCategoriesAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allCategories;
            }

            searchTerm = searchTerm.ToLower();
            return allCategories.Where(c =>
                (c.Nom?.ToLower().Contains(searchTerm) ?? false) ||
                (c.Description?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching service categories: {ex.Message}");
            return new List<ServiceCategorieDto>();
        }
    }
}

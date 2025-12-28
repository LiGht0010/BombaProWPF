using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

public class ServiceService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Services;

    public ServiceService()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<ServiceDto>> GetAllServicesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ServiceDto>>(json) ?? new List<ServiceDto>();
            }
            return new List<ServiceDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching services: {ex.Message}");
            return new List<ServiceDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ServiceDto?> GetServiceByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ServiceDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching service: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY CATEGORY
    // ============================
    public async Task<List<ServiceDto>> GetServicesByCategoryAsync(int categoryId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/category/{categoryId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ServiceDto>>(json) ?? new List<ServiceDto>();
            }
            return new List<ServiceDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching services by category: {ex.Message}");
            return new List<ServiceDto>();
        }
    }

    // ============================
    // SEARCH (Server-side)
    // ============================
    public async Task<List<ServiceDto>> SearchServicesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllServicesAsync();
            }

            var response = await _httpClient.GetAsync($"{BaseUrl}/search?term={Uri.EscapeDataString(searchTerm)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ServiceDto>>(json) ?? new List<ServiceDto>();
            }

            // Fallback to client-side search
            var allServices = await GetAllServicesAsync();
            searchTerm = searchTerm.ToLower();
            return allServices.Where(s =>
                (s.Numero?.ToLower().Contains(searchTerm) ?? false) ||
                (s.Description?.ToLower().Contains(searchTerm) ?? false) ||
                (s.ServiceCategorieNom?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching services: {ex.Message}");
            return new List<ServiceDto>();
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ServiceDto?> CreateServiceAsync(ServiceDto service)
    {
        try
        {
            service.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            service.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(service);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ServiceDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating service: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateServiceAsync(ServiceDto service)
    {
        try
        {
            service.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            service.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(service);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{service.ID}", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating service: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteServiceAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting service: {ex.Message}");
            return false;
        }
    }
}

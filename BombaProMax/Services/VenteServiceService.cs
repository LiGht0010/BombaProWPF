using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing VenteService (service sales) API operations.
/// </summary>
public class VenteServiceService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.VenteServices;

    public VenteServiceService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<VenteServiceDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteServiceDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching vente services: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<VenteServiceDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VenteServiceDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching vente service: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY DATE RANGE
    // ============================
    public async Task<List<VenteServiceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var url = $"{BaseUrl}/bydate?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteServiceDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching vente services by date: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY SERVICE
    // ============================
    public async Task<List<VenteServiceDto>> GetByServiceAsync(int serviceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/byservice/{serviceId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteServiceDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching vente services by service: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY CATEGORY
    // ============================
    public async Task<List<VenteServiceDto>> GetByCategoryAsync(int categoryId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/bycategory/{categoryId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteServiceDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching vente services by category: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY CLIENT
    // ============================
    public async Task<List<VenteServiceDto>> GetByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/byclient/{clientId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteServiceDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching vente services by client: {ex.Message}");
            return [];
        }
    }

    // ============================
    // SEARCH
    // ============================
    public async Task<List<VenteServiceDto>> SearchAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            var response = await _httpClient.GetAsync($"{BaseUrl}/search?term={Uri.EscapeDataString(searchTerm)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteServiceDto>>(json) ?? [];
            }

            // Fallback to client-side search
            var allVentes = await GetAllAsync();
            searchTerm = searchTerm.ToLower();
            return allVentes.Where(v =>
                (v.NumeroVente?.ToLower().Contains(searchTerm) ?? false) ||
                (v.ServiceDescription?.ToLower().Contains(searchTerm) ?? false) ||
                (v.ClientNom?.ToLower().Contains(searchTerm) ?? false) ||
                (v.Notes?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching vente services: {ex.Message}");
            return [];
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<VenteServiceDto?> CreateAsync(VenteServiceDto vente)
    {
        try
        {
            vente.CreePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            vente.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(vente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VenteServiceDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating vente service: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateAsync(VenteServiceDto vente)
    {
        try
        {
            vente.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            vente.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(vente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{vente.ID}", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating vente service: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting vente service: {ex.Message}");
            return false;
        }
    }
}

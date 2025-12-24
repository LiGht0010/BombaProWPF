using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

public class CiterneService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Citernes;

    public CiterneService()
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
    public async Task<List<CiterneDto>> GetAllCiternesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CiterneDto>>(json) ?? new List<CiterneDto>();
            }
            return new List<CiterneDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching citernes: {ex.Message}");
            return new List<CiterneDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<CiterneDto?> GetCiterneByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CiterneDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching citerne: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY FOURNISSEUR (Server-side)
    // ============================
    public async Task<List<CiterneDto>> GetCiternesByFournisseurAsync(int fournisseurId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/fournisseur/{fournisseurId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CiterneDto>>(json) ?? new List<CiterneDto>();
            }
            return new List<CiterneDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching citernes by fournisseur: {ex.Message}");
            return new List<CiterneDto>();
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<CiterneDto?> CreateCiterneAsync(CiterneDto citerne)
    {
        try
        {
            // Set audit fields
            citerne.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            citerne.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(citerne);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CiterneDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating citerne: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateCiterneAsync(CiterneDto citerne)
    {
        try
        {
            // Set audit fields
            citerne.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            citerne.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(citerne);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{citerne.ID}", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating citerne: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteCiterneAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting citerne: {ex.Message}");
            return false;
        }
    }

    // ============================
    // SEARCH (Client-side)
    // ============================
    public async Task<List<CiterneDto>> SearchCiternesAsync(string searchTerm)
    {
        try
        {
            var allCiternes = await GetAllCiternesAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allCiternes;
            }

            searchTerm = searchTerm.ToLower();
            return allCiternes.Where(c =>
                (c.Capacite?.ToString().Contains(searchTerm) ?? false) ||
                (c.MatriculeCiterne?.ToLower().Contains(searchTerm) ?? false) ||
                (c.FournisseurNom?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching citernes: {ex.Message}");
            return new List<CiterneDto>();
        }
    }

    // ============================
    // STATISTICS
    // ============================
    public async Task<CiterneStatistics> GetCiterneStatisticsAsync()
    {
        try
        {
            var allCiternes = await GetAllCiternesAsync();
            return new CiterneStatistics
            {
                TotalCiternes = allCiternes.Count,
                TotalCapacity = allCiternes.Sum(c => c.Capacite ?? 0)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting citerne statistics: {ex.Message}");
            return new CiterneStatistics();
        }
    }
}

// Helper class for statistics
public class CiterneStatistics
{
    public int TotalCiternes { get; set; }
    public decimal TotalCapacity { get; set; }
}

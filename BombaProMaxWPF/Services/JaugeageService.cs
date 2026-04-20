using BombaProMaxWPF.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Service for managing Jaugeage (tank gauging) records
/// </summary>
public class JaugeageService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSettings;
    private static string BaseUrl => ApiConfig.Jaugeages;

    public JaugeageService()
    {
        _httpClient = HttpClientFactory.Create();
        _jsonSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<JaugeageDto>> GetAllJaugeagesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JaugeageDto>>(json, _jsonSettings) 
                    ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error fetching jaugeages: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<JaugeageDto?> GetJaugeageByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageDto>(json, _jsonSettings);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error fetching jaugeage: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET WITH DETAILS
    // ============================
    public async Task<JaugeageWithDetailsDto?> GetJaugeageWithDetailsAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}/with-details");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageWithDetailsDto>(json, _jsonSettings);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error fetching jaugeage with details: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY DATE
    // ============================
    public async Task<List<JaugeageDto>> GetJaugeagesByDateAsync(DateTime date)
    {
        try
        {
            // Format date as yyyy-MM-dd for the API query
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/by-date?date={date:yyyy-MM-dd}");
            
            Debug.WriteLine($"[JaugeageService] GET by-date: {date:yyyy-MM-dd}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JaugeageDto>>(json, _jsonSettings) 
                    ?? [];
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[JaugeageService] Error response: {response.StatusCode} - {errorContent}");
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error fetching jaugeages by date: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY TEMOIN
    // ============================
    public async Task<List<JaugeageDto>> GetJaugeagesByTemoinAsync(int temoinId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/by-temoin/{temoinId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JaugeageDto>>(json, _jsonSettings) 
                    ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error fetching jaugeages by temoin: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET LATEST
    // ============================
    public async Task<JaugeageWithDetailsDto?> GetLatestJaugeageAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/latest");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageWithDetailsDto>(json, _jsonSettings);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error fetching latest jaugeage: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<JaugeageDto?> CreateJaugeageAsync(JaugeageDto jaugeage)
    {
        try
        {
            // Set audit fields - AjoutePar is the User who created this record
            jaugeage.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId;
            jaugeage.DateCreation = DateTime.UtcNow;
            
            // Ensure DateJaugeage is UTC
            jaugeage.DateJaugeage = EnsureUtc(jaugeage.DateJaugeage);

            var json = JsonConvert.SerializeObject(jaugeage, _jsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageDto>(responseJson, _jsonSettings);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error creating jaugeage: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE WITH DETAILS
    // ============================
    /// <summary>
    /// Creates a Jaugeage with all its details in a single transaction.
    /// NumeroJaugeage is auto-generated by the API if not provided.
    /// </summary>
    public async Task<JaugeageWithDetailsDto?> CreateJaugeageWithDetailsAsync(JaugeageWithDetailsDto jaugeage)
    {
        try
        {
            // Set audit fields - AjoutePar is the User who created this record
            // Note: TemoinID is the Employe who witnessed the jaugeage (set by caller)
            jaugeage.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId;
            jaugeage.DateCreation = DateTime.UtcNow;
            
            // Ensure DateJaugeage is UTC
            jaugeage.DateJaugeage = EnsureUtc(jaugeage.DateJaugeage);

            var json = JsonConvert.SerializeObject(jaugeage, _jsonSettings);
            
            Debug.WriteLine($"[JaugeageService] POST {BaseUrl}/with-details");
            Debug.WriteLine($"[JaugeageService] Request: {json}");
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/with-details", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[JaugeageService] Response: {response.StatusCode}");
            Debug.WriteLine($"[JaugeageService] Body: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<JaugeageWithDetailsDto>(responseContent, _jsonSettings);
            }
            
            Debug.WriteLine($"[JaugeageService] Error: {response.StatusCode} - {responseContent}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Exception: {ex.Message}");
            Debug.WriteLine($"[JaugeageService] Stack: {ex.StackTrace}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateJaugeageAsync(JaugeageDto jaugeage)
    {
        try
        {
            // Set audit fields
            jaugeage.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId;
            jaugeage.DateModification = DateTime.UtcNow;
            
            // Ensure DateJaugeage is UTC
            jaugeage.DateJaugeage = EnsureUtc(jaugeage.DateJaugeage);

            var json = JsonConvert.SerializeObject(jaugeage, _jsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{jaugeage.ID}", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error updating jaugeage: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteJaugeageAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JaugeageService] Error deleting jaugeage: {ex.Message}");
            return false;
        }
    }

    // ============================
    // HELPER
    // ============================
    /// <summary>
    /// Ensures the DateTime has Kind=Utc for PostgreSQL compatibility
    /// </summary>
    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) // Unspecified -> treat as UTC
        };
    }
}

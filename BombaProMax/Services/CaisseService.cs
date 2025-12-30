using BombaProMax.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing cash (Caisse) operations including deposits and cash summary.
/// </summary>
public class CaisseService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.DepotCaisses;

    public CaisseService()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    #region Deposit Operations

    /// <summary>
    /// Gets all cash deposits.
    /// </summary>
    public async Task<List<DepotCaisseDto>> GetAllDepotsAsync()
    {
        try
        {
            Debug.WriteLine($"[CaisseService] GET {BaseUrl}");
            var response = await _httpClient.GetAsync(BaseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DepotCaisseDto>>(json) ?? [];
            }

            Debug.WriteLine($"[CaisseService] Error: {response.StatusCode}");
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CaisseService] Exception: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets a deposit by ID.
    /// </summary>
    public async Task<DepotCaisseDto?> GetDepotByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DepotCaisseDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CaisseService] Error getting depot {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets deposits within a date range.
    /// </summary>
    public async Task<List<DepotCaisseDto>> GetDepotsByDateRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            var startStr = start.ToString("yyyy-MM-dd");
            var endStr = end.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"{BaseUrl}/date-range?start={startStr}&end={endStr}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DepotCaisseDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CaisseService] Error getting depots by date range: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Creates a new cash deposit.
    /// </summary>
    public async Task<DepotCaisseDto?> CreateDepotAsync(DepotCaisseDto dto)
    {
        try
        {
            // Set audit fields
            dto.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            dto.DateCreation = DateTime.UtcNow;
            dto.DateDepot = DateTime.SpecifyKind(dto.DateDepot, DateTimeKind.Utc);

            var json = JsonConvert.SerializeObject(dto);
            Debug.WriteLine($"[CaisseService] POST {BaseUrl}: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[CaisseService] Response: {response.StatusCode} - {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<DepotCaisseDto>(responseBody);
            }

            Debug.WriteLine($"[CaisseService] Error creating depot: {responseBody}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CaisseService] Exception creating depot: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates an existing cash deposit.
    /// </summary>
    public async Task<bool> UpdateDepotAsync(DepotCaisseDto dto)
    {
        try
        {
            // Set audit fields
            dto.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            dto.DateModification = DateTime.UtcNow;
            dto.DateDepot = DateTime.SpecifyKind(dto.DateDepot, DateTimeKind.Utc);

            if (dto.DateCreation.HasValue)
            {
                dto.DateCreation = DateTime.SpecifyKind(dto.DateCreation.Value, DateTimeKind.Utc);
            }

            var json = JsonConvert.SerializeObject(dto);
            Debug.WriteLine($"[CaisseService] PUT {BaseUrl}/{dto.ID}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

            Debug.WriteLine($"[CaisseService] Response: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CaisseService] Exception updating depot: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Deletes a cash deposit.
    /// </summary>
    public async Task<bool> DeleteDepotAsync(int id)
    {
        try
        {
            Debug.WriteLine($"[CaisseService] DELETE {BaseUrl}/{id}");
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            Debug.WriteLine($"[CaisseService] Response: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CaisseService] Exception deleting depot: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Summary Operations

    /// <summary>
    /// Gets the cash summary (all sources and current balance).
    /// </summary>
    public async Task<CaisseSummaryDto?> GetCashSummaryAsync(DateTime? start = null, DateTime? end = null)
    {
        try
        {
            var url = $"{BaseUrl}/summary";
            var queryParams = new List<string>();

            if (start.HasValue)
            {
                queryParams.Add($"start={start.Value:yyyy-MM-dd}");
            }
            if (end.HasValue)
            {
                queryParams.Add($"end={end.Value:yyyy-MM-dd}");
            }

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            Debug.WriteLine($"[CaisseService] GET {url}");
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[CaisseService] Summary response: {json}");
                return JsonConvert.DeserializeObject<CaisseSummaryDto>(json);
            }

            Debug.WriteLine($"[CaisseService] Error getting summary: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CaisseService] Exception getting summary: {ex.Message}");
            return null;
        }
    }

    #endregion
}

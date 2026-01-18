using BombaProMax.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing Periodes (shifts) and their details.
/// Handles both Periode and PeriodeDetails as they are tightly coupled.
/// </summary>
public class PeriodeService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Periodes;
    private static string DetailsBaseUrl => $"{ApiConfig.BaseUrl}/PeriodeDetails";

    public PeriodeService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    #region Periode Operations

    /// <summary>
    /// Gets all periodes.
    /// </summary>
    public async Task<List<PeriodeDto>> GetAllPeriodesAsync()
    {
        try
        {
            Debug.WriteLine($"[PeriodeService] GET {BaseUrl}");
            var response = await _httpClient.GetAsync(BaseUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PeriodeDto>>(json) ?? [];
            }

            Debug.WriteLine($"[PeriodeService] Error: {response.StatusCode}");
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Exception: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets a periode by ID.
    /// </summary>
    public async Task<PeriodeDto?> GetPeriodeByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PeriodeDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Error getting periode {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets periodes by employee ID.
    /// </summary>
    public async Task<List<PeriodeDto>> GetPeriodesByEmployeAsync(int employeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/employe/{employeId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PeriodeDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Error getting periodes for employe {employeId}: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets periodes within a date range.
    /// </summary>
    public async Task<List<PeriodeDto>> GetPeriodesByDateRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            var startStr = start.ToString("yyyy-MM-dd");
            var endStr = end.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"{BaseUrl}/date-range?start={startStr}&end={endStr}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PeriodeDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Error getting periodes by date range: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets the current active periode.
    /// </summary>
    public async Task<PeriodeDto?> GetCurrentPeriodeAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/current");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PeriodeDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Error getting current periode: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new periode.
    /// </summary>
    public async Task<PeriodeDto?> CreatePeriodeAsync(PeriodeDto dto)
    {
        // Set audit fields
        dto.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
        dto.DateCreation = DateTime.UtcNow;

        // Ensure dates are in UTC
        dto.DateDebut = DateTime.SpecifyKind(dto.DateDebut, DateTimeKind.Utc);
        dto.DateFin = DateTime.SpecifyKind(dto.DateFin, DateTimeKind.Utc);

        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[PeriodeService] POST {BaseUrl}: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(BaseUrl, content);

        var responseBody = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode} - {responseBody}");

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<PeriodeDto>(responseBody);
        }

        Debug.WriteLine($"[PeriodeService] Error creating periode: {responseBody}");
        return null;
    }

    /// <summary>
    /// Updates an existing periode.
    /// </summary>
    public async Task<bool> UpdatePeriodeAsync(PeriodeDto dto)
    {
        // Set audit fields
        dto.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
        dto.DateModification = DateTime.UtcNow;

        // Ensure dates are in UTC
        dto.DateDebut = DateTime.SpecifyKind(dto.DateDebut, DateTimeKind.Utc);
        dto.DateFin = DateTime.SpecifyKind(dto.DateFin, DateTimeKind.Utc);

        if (dto.DateCreation.HasValue)
        {
            dto.DateCreation = DateTime.SpecifyKind(dto.DateCreation.Value, DateTimeKind.Utc);
        }

        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[PeriodeService] PUT {BaseUrl}/{dto.PeriodeID}: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.PeriodeID}", content);

        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes a periode and all its details.
    /// </summary>
    public async Task<bool> DeletePeriodeAsync(int id)
    {
        Debug.WriteLine($"[PeriodeService] DELETE {BaseUrl}/{id}");
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region PeriodeDetails Operations

    /// <summary>
    /// Gets all details for a specific periode.
    /// </summary>
    public async Task<List<PeriodeDetailsDto>> GetDetailsByPeriodeAsync(int periodeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{DetailsBaseUrl}/periode/{periodeId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PeriodeDetailsDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Error getting details for periode {periodeId}: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets a specific detail by ID.
    /// </summary>
    public async Task<PeriodeDetailsDto?> GetDetailByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{DetailsBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PeriodeDetailsDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Error getting detail {id}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets details by pompe ID.
    /// </summary>
    public async Task<List<PeriodeDetailsDto>> GetDetailsByPompeAsync(int pompeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{DetailsBaseUrl}/pompe/{pompeId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PeriodeDetailsDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Error getting details for pompe {pompeId}: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Creates a new periode detail.
    /// </summary>
    public async Task<PeriodeDetailsDto?> CreateDetailAsync(PeriodeDetailsDto dto)
    {
        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[PeriodeService] POST {DetailsBaseUrl}: {json}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(DetailsBaseUrl, content);

        var responseBody = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode} - {responseBody}");

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<PeriodeDetailsDto>(responseBody);
        }

        Debug.WriteLine($"[PeriodeService] Error creating detail: {responseBody}");
        return null;
    }

    /// <summary>
    /// Creates multiple periode details in batch.
    /// </summary>
    public async Task<List<PeriodeDetailsDto>> CreateDetailsBatchAsync(List<PeriodeDetailsDto> dtos)
    {
        var json = JsonConvert.SerializeObject(dtos);
        Debug.WriteLine($"[PeriodeService] POST {DetailsBaseUrl}/batch: {dtos.Count} items");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{DetailsBaseUrl}/batch", content);

        var responseBody = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<PeriodeDetailsDto>>(responseBody) ?? [];
        }

        Debug.WriteLine($"[PeriodeService] Error creating details batch: {responseBody}");
        return [];
    }

    /// <summary>
    /// Updates a periode detail.
    /// </summary>
    public async Task<bool> UpdateDetailAsync(PeriodeDetailsDto dto)
    {
        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[PeriodeService] PUT {DetailsBaseUrl}/{dto.PeriodeDetailID}");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{DetailsBaseUrl}/{dto.PeriodeDetailID}", content);

        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes a periode detail.
    /// </summary>
    public async Task<bool> DeleteDetailAsync(int id)
    {
        Debug.WriteLine($"[PeriodeService] DELETE {DetailsBaseUrl}/{id}");
        var response = await _httpClient.DeleteAsync($"{DetailsBaseUrl}/{id}");

        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes all details for a periode.
    /// </summary>
    public async Task<bool> DeleteDetailsByPeriodeAsync(int periodeId)
    {
        Debug.WriteLine($"[PeriodeService] DELETE {DetailsBaseUrl}/periode/{periodeId}");
        var response = await _httpClient.DeleteAsync($"{DetailsBaseUrl}/periode/{periodeId}");

        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region Combined Operations

    /// <summary>
    /// Creates a periode with its details in one atomic operation.
    /// Uses the /with-details endpoint which also triggers FIFO stock consumption.
    /// </summary>
    public async Task<(PeriodeDto? Periode, List<PeriodeDetailsDto> Details)> CreatePeriodeWithDetailsAsync(
        PeriodeDto periode, 
        List<PeriodeDetailsDto> details,
        List<int>? creditTransactionIds = null)
    {
        // Set audit fields on periode
        periode.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
        periode.DateCreation = DateTime.UtcNow;

        // Ensure dates are in UTC
        periode.DateDebut = DateTime.SpecifyKind(periode.DateDebut, DateTimeKind.Utc);
        periode.DateFin = DateTime.SpecifyKind(periode.DateFin, DateTimeKind.Utc);

        // Build the combined DTO
        var dto = new PeriodeWithDetailsDto
        {
            Periode = periode,
            Details = details,
            CreditTransactionIds = creditTransactionIds ?? []
        };

        // Debug logging to trace stock consumption
        Debug.WriteLine($"[PeriodeService] ========== CREATING PERIODE WITH DETAILS ==========");
        Debug.WriteLine($"[PeriodeService] Periode dates: {periode.DateDebut:g} to {periode.DateFin:g}");
        Debug.WriteLine($"[PeriodeService] Details count: {details.Count}");
        foreach (var detail in details)
        {
            var qtyVendue = detail.CompteurElectroniqueFinal - detail.CompteurElectroniqueDebut;
            Debug.WriteLine($"[PeriodeService]   Detail: Pompe={detail.PompeID}, Reservoir={detail.ReservoirID}, Produit={detail.ProduitID}");
            Debug.WriteLine($"[PeriodeService]     CompteurElecDebut={detail.CompteurElectroniqueDebut:F2}, CompteurElecFin={detail.CompteurElectroniqueFinal:F2}");
            Debug.WriteLine($"[PeriodeService]     QuantiteVendue (calculated)={qtyVendue:F2}L, PrixCarburant={detail.PrixCarburant:F2}");
        }
        Debug.WriteLine($"[PeriodeService] CreditTransactionIds count: {dto.CreditTransactionIds.Count}");

        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[PeriodeService] POST {BaseUrl}/with-details");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{BaseUrl}/with-details", content);

        var responseBody = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var result = JsonConvert.DeserializeObject<PeriodeWithDetailsDto>(responseBody);
            if (result != null)
            {
                Debug.WriteLine($"[PeriodeService] SUCCESS: Created Periode {result.Periode.PeriodeID} with {result.Details.Count} details and {result.CreditTransactionIds.Count} CTs");
                Debug.WriteLine($"[PeriodeService] ========== STOCK SHOULD BE CONSUMED ==========");
                return (result.Periode, result.Details);
            }
        }

        // Log error details
        Debug.WriteLine($"[PeriodeService] ERROR creating periode with details: {responseBody}");
        
        // Try to parse error response
        try
        {
            var errorObj = JsonConvert.DeserializeObject<dynamic>(responseBody);
            if (errorObj?.error != null)
            {
                Debug.WriteLine($"[PeriodeService] API Error: {errorObj.error} - {errorObj.message}");
            }
        }
        catch { }

        return (null, []);
    }

    /// <summary>
    /// Updates a periode with its details in one atomic operation.
    /// Uses the /{id}/with-details endpoint which handles stock reversal and re-consumption.
    /// </summary>
    public async Task<(PeriodeDto? Periode, List<PeriodeDetailsDto> Details)> UpdatePeriodeWithDetailsAsync(
        PeriodeDto periode,
        List<PeriodeDetailsDto> details,
        List<int>? creditTransactionIds = null)
    {
        // Set audit fields on periode
        periode.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
        periode.DateModification = DateTime.UtcNow;

        // Ensure dates are in UTC
        periode.DateDebut = DateTime.SpecifyKind(periode.DateDebut, DateTimeKind.Utc);
        periode.DateFin = DateTime.SpecifyKind(periode.DateFin, DateTimeKind.Utc);

        if (periode.DateCreation.HasValue)
        {
            periode.DateCreation = DateTime.SpecifyKind(periode.DateCreation.Value, DateTimeKind.Utc);
        }

        // Build the combined DTO
        var dto = new PeriodeWithDetailsDto
        {
            Periode = periode,
            Details = details,
            CreditTransactionIds = creditTransactionIds ?? []
        };

        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[PeriodeService] PUT {BaseUrl}/{periode.PeriodeID}/with-details: Periode + {details.Count} details + {dto.CreditTransactionIds.Count} CTs");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{BaseUrl}/{periode.PeriodeID}/with-details", content);

        var responseBody = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[PeriodeService] Response: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var result = JsonConvert.DeserializeObject<PeriodeWithDetailsDto>(responseBody);
            if (result != null)
            {
                Debug.WriteLine($"[PeriodeService] Updated Periode {result.Periode.PeriodeID} with {result.Details.Count} details and {result.CreditTransactionIds.Count} CTs (stock adjusted)");
                return (result.Periode, result.Details);
            }
        }

        // Log error details
        Debug.WriteLine($"[PeriodeService] Error updating periode with details: {responseBody}");

        // Try to parse error response
        try
        {
            var errorObj = JsonConvert.DeserializeObject<dynamic>(responseBody);
            if (errorObj?.error != null)
            {
                Debug.WriteLine($"[PeriodeService] API Error: {errorObj.error} - {errorObj.message}");
            }
        }
        catch { }

        return (null, []);
    }

    /// <summary>
    /// Gets a periode with all its details loaded.
    /// </summary>
    public async Task<(PeriodeDto? Periode, List<PeriodeDetailsDto> Details)> GetPeriodeWithDetailsAsync(int periodeId)
    {
        var periode = await GetPeriodeByIdAsync(periodeId);
        if (periode == null)
        {
            return (null, []);
        }

        var details = await GetDetailsByPeriodeAsync(periodeId);
        return (periode, details);
    }

    #endregion

    #region Marge Analysis

    /// <summary>
    /// Gets margin analysis for a specific Periode.
    /// Shows which StockLots were consumed at what PrixAchat and calculates margins.
    /// </summary>
    /// <param name="periodeId">The Periode ID to analyze</param>
    /// <returns>Detailed margin breakdown with FIFO cost tracking, or null if not found</returns>
    public async Task<PeriodeMargeAnalysisDto?> GetPeriodeMargeAnalysisAsync(int periodeId)
    {
        try
        {
            var url = $"{ApiConfig.BaseUrl}/StockLots/periode/{periodeId}/marge";
            Debug.WriteLine($"[PeriodeService] GET {url}");
            
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PeriodeMargeAnalysisDto>(json);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Debug.WriteLine($"[PeriodeService] No marge data for periode {periodeId}");
                return null;
            }

            Debug.WriteLine($"[PeriodeService] Error getting marge: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PeriodeService] Exception getting marge for periode {periodeId}: {ex.Message}");
            return null;
        }
    }

    #endregion
}

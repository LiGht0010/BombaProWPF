using BombaProMaxWPF.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Service for managing JaugeageDetail records with automatic volume calculation from calibration data
/// </summary>
public class JaugeageDetailService
{
    private readonly HttpClient _httpClient;
    private readonly ReservoirCalibrationService _calibrationService;
    private static string BaseUrl => ApiConfig.JaugeageDetails;

    public JaugeageDetailService()
    {
        _httpClient = HttpClientFactory.Create();
        _calibrationService = new ReservoirCalibrationService();
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<JaugeageDetailDto>> GetAllDetailsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JaugeageDetailDto>>(json) 
                    ?? new List<JaugeageDetailDto>();
            }
            return new List<JaugeageDetailDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching jaugeage details: {ex.Message}");
            return new List<JaugeageDetailDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<JaugeageDetailDto?> GetDetailByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageDetailDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching jaugeage detail: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY JAUGEAGE
    // ============================
    public async Task<List<JaugeageDetailDto>> GetDetailsByJaugeageAsync(int jaugeageId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/jaugeage/{jaugeageId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JaugeageDetailDto>>(json) 
                    ?? new List<JaugeageDetailDto>();
            }
            return new List<JaugeageDetailDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching details by jaugeage: {ex.Message}");
            return new List<JaugeageDetailDto>();
        }
    }

    // ============================
    // GET BY RESERVOIR
    // ============================
    public async Task<List<JaugeageDetailDto>> GetDetailsByReservoirAsync(int reservoirId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/reservoir/{reservoirId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<JaugeageDetailDto>>(json) 
                    ?? new List<JaugeageDetailDto>();
            }
            return new List<JaugeageDetailDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching details by reservoir: {ex.Message}");
            return new List<JaugeageDetailDto>();
        }
    }

    // ============================
    // CALCULATE VOLUME FROM HEIGHT (Client-side preview)
    // ============================
    /// <summary>
    /// Calculates the volume for a given height using the reservoir's calibration data.
    /// Use this for real-time preview before saving.
    /// </summary>
    public async Task<VolumeLookupResultDto?> CalculateVolumeAsync(int reservoirId, decimal hauteurCm)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/calculate-volume?reservoirId={reservoirId}&hauteurCm={hauteurCm}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VolumeLookupResultDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating volume: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE (Standard - auto-calculates if volume is 0)
    // ============================
    public async Task<JaugeageDetailDto?> CreateDetailAsync(JaugeageDetailDto detail)
    {
        try
        {
            var json = JsonConvert.SerializeObject(detail);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageDetailDto>(responseJson);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error creating detail: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating jaugeage detail: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE WITH AUTO-VOLUME (Always calculates from calibration)
    // ============================
    /// <summary>
    /// Creates a JaugeageDetail with automatic volume calculation from calibration data.
    /// The volume is always calculated, ignoring any provided VolumeCalcule value.
    /// </summary>
    public async Task<JaugeageDetailDto?> CreateDetailWithAutoVolumeAsync(JaugeageDetailDto detail)
    {
        try
        {
            var json = JsonConvert.SerializeObject(detail);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/with-auto-volume", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageDetailDto>(responseJson);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error creating detail with auto-volume: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating jaugeage detail with auto-volume: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateDetailAsync(JaugeageDetailDto detail)
    {
        try
        {
            var json = JsonConvert.SerializeObject(detail);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{detail.ID}", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating jaugeage detail: {ex.Message}");
            return false;
        }
    }

    // ============================
    // RECALCULATE VOLUME
    // ============================
    /// <summary>
    /// Recalculates the volume for an existing detail based on current calibration data.
    /// Useful if calibration data was updated after the detail was created.
    /// </summary>
    public async Task<JaugeageDetailDto?> RecalculateVolumeAsync(int detailId)
    {
        try
        {
            var response = await _httpClient.PutAsync(
                $"{BaseUrl}/{detailId}/recalculate", 
                new StringContent("", Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JaugeageDetailDto>(responseJson);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error recalculating volume: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recalculating volume: {ex.Message}");
            return null;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteDetailAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting jaugeage detail: {ex.Message}");
            return false;
        }
    }

    // ============================
    // HELPER: Create detail with local volume preview
    // ============================
    /// <summary>
    /// Creates a JaugeageDetailDto with pre-calculated volume for UI preview.
    /// Call this when user enters height to show the calculated volume before saving.
    /// </summary>
    public async Task<JaugeageDetailDto> CreateDetailWithVolumePreviewAsync(
        int jaugeageId, 
        int reservoirId, 
        decimal hauteurMesuree,
        decimal? temperature = null,
        string? notes = null)
    {
        var detail = new JaugeageDetailDto
        {
            JaugeageID = jaugeageId,
            ReservoirID = reservoirId,
            HauteurMesuree = hauteurMesuree,
            Temperature = temperature,
            Notes = notes,
            VolumeCalcule = 0
        };

        // Get volume preview from API
        var volumeResult = await CalculateVolumeAsync(reservoirId, hauteurMesuree);
        if (volumeResult != null)
        {
            detail.VolumeCalcule = volumeResult.VolumeLitres;
            detail.ReservoirNumero = volumeResult.ReservoirNumero;
        }

        return detail;
    }
}

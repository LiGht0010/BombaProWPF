using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing reservoir calibration data (height to volume mapping)
/// </summary>
public class ReservoirCalibrationService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.ReservoirCalibrations;

    public ReservoirCalibrationService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    // ============================
    // GET CALIBRATIONS BY RESERVOIR
    // ============================
    /// <summary>
    /// Gets all calibration entries for a specific reservoir, ordered by height
    /// </summary>
    public async Task<List<ReservoirCalibrationDto>> GetCalibrationsByReservoirAsync(int reservoirId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/reservoir/{reservoirId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReservoirCalibrationDto>>(json) 
                    ?? new List<ReservoirCalibrationDto>();
            }
            return new List<ReservoirCalibrationDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching calibrations: {ex.Message}");
            return new List<ReservoirCalibrationDto>();
        }
    }

    // ============================
    // GET SINGLE CALIBRATION
    // ============================
    public async Task<ReservoirCalibrationDto?> GetCalibrationByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ReservoirCalibrationDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching calibration: {ex.Message}");
            return null;
        }
    }

    // ============================
    // LOOKUP VOLUME BY HEIGHT
    // ============================
    /// <summary>
    /// Looks up the volume for a given height in a reservoir.
    /// Interpolates between calibration points if exact height not found.
    /// </summary>
    public async Task<VolumeLookupResultDto?> LookupVolumeAsync(int reservoirId, decimal hauteurCm)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/reservoir/{reservoirId}/lookup?hauteurCm={hauteurCm}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VolumeLookupResultDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error looking up volume: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE SINGLE CALIBRATION
    // ============================
    public async Task<ReservoirCalibrationDto?> CreateCalibrationAsync(ReservoirCalibrationDto calibration)
    {
        try
        {
            var json = JsonConvert.SerializeObject(calibration);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ReservoirCalibrationDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating calibration: {ex.Message}");
            return null;
        }
    }

    // ============================
    // BULK IMPORT CALIBRATIONS
    // ============================
    /// <summary>
    /// Imports calibration data for a reservoir. Replaces all existing calibration entries.
    /// </summary>
    public async Task<CalibrationImportResultDto?> ImportCalibrationsAsync(
        int reservoirId, 
        List<ReservoirCalibrationImportDto> calibrations)
    {
        try
        {
            var json = JsonConvert.SerializeObject(calibrations);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/reservoir/{reservoirId}/import", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CalibrationImportResultDto>(responseJson);
            }
            
            // Log error response
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Import failed: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing calibrations: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE CALIBRATION
    // ============================
    public async Task<bool> UpdateCalibrationAsync(ReservoirCalibrationDto calibration)
    {
        try
        {
            var json = JsonConvert.SerializeObject(calibration);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{calibration.ID}", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating calibration: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE SINGLE CALIBRATION
    // ============================
    public async Task<bool> DeleteCalibrationAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting calibration: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE ALL CALIBRATIONS FOR RESERVOIR
    // ============================
    public async Task<bool> DeleteAllCalibrationsAsync(int reservoirId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/reservoir/{reservoirId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting all calibrations: {ex.Message}");
            return false;
        }
    }

    // ============================
    // PARSE CSV/EXCEL DATA
    // ============================
    /// <summary>
    /// Parses CSV data into calibration import DTOs.
    /// Expected format: "HauteurCm,VolumeLitres" or "HauteurCm;VolumeLitres"
    /// </summary>
    public List<ReservoirCalibrationImportDto> ParseCsvData(string csvContent)
    {
        var calibrations = new List<ReservoirCalibrationImportDto>();
        var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Skip header line if present
            if (line.StartsWith("Hauteur", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Height", StringComparison.OrdinalIgnoreCase))
                continue;

            // Try comma or semicolon separator
            var parts = line.Contains(';') 
                ? line.Split(';') 
                : line.Split(',');

            if (parts.Length >= 2 &&
                decimal.TryParse(parts[0].Trim(), out var hauteur) &&
                decimal.TryParse(parts[1].Trim(), out var volume))
            {
                calibrations.Add(new ReservoirCalibrationImportDto
                {
                    HauteurCm = hauteur,
                    VolumeLitres = volume
                });
            }
        }

        return calibrations;
    }

    // ============================
    // CHECK IF RESERVOIR HAS CALIBRATION
    // ============================
    public async Task<bool> HasCalibrationDataAsync(int reservoirId)
    {
        var calibrations = await GetCalibrationsByReservoirAsync(reservoirId);
        return calibrations.Count > 0;
    }
}

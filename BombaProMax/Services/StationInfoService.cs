using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing station information (business details for official documents).
/// </summary>
public class StationInfoService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => $"{ApiConfig.BaseUrl}/stationinfo";
    
    // Cache the station info to avoid repeated API calls
    private static StationInfoDto? _cachedStationInfo;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public StationInfoService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    /// <summary>
    /// Gets the station information (uses cache if available).
    /// </summary>
    public async Task<StationInfoDto?> GetStationInfoAsync(bool forceRefresh = false)
    {
        try
        {
            // Return cached if still valid
            if (!forceRefresh && _cachedStationInfo != null && DateTime.Now < _cacheExpiry)
            {
                return _cachedStationInfo;
            }

            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _cachedStationInfo = JsonConvert.DeserializeObject<StationInfoDto>(json);
                _cacheExpiry = DateTime.Now.Add(CacheDuration);
                return _cachedStationInfo;
            }
            
            // Return cached even if expired, if API fails
            return _cachedStationInfo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching station info: {ex.Message}");
            return _cachedStationInfo; // Return cached on error
        }
    }

    /// <summary>
    /// Creates or updates the station information.
    /// </summary>
    public async Task<StationInfoDto?> SaveStationInfoAsync(StationInfoDto stationInfo)
    {
        try
        {
            var json = JsonConvert.SerializeObject(stationInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response;
            if (stationInfo.ID > 0)
            {
                // Update existing
                response = await _httpClient.PutAsync($"{BaseUrl}/{stationInfo.ID}", content);
            }
            else
            {
                // Create new
                response = await _httpClient.PostAsync(BaseUrl, content);
            }

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                _cachedStationInfo = JsonConvert.DeserializeObject<StationInfoDto>(responseJson);
                _cacheExpiry = DateTime.Now.Add(CacheDuration);
                return _cachedStationInfo;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving station info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates only the logo for the station.
    /// </summary>
    public async Task<bool> UpdateLogoAsync(byte[] logoBytes)
    {
        try
        {
            var base64Logo = Convert.ToBase64String(logoBytes);
            var json = JsonConvert.SerializeObject(new { LogoBase64 = base64Logo });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync($"{BaseUrl}/logo", content);
            
            if (response.IsSuccessStatusCode)
            {
                // Invalidate cache
                _cacheExpiry = DateTime.MinValue;
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating logo: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the logo bytes for PDF generation.
    /// </summary>
    public async Task<byte[]?> GetLogoBytes()
    {
        try
        {
            var stationInfo = await GetStationInfoAsync();
            if (stationInfo?.LogoBase64 != null)
            {
                return Convert.FromBase64String(stationInfo.LogoBase64);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting logo bytes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Clears the cache to force a refresh on next call.
    /// </summary>
    public static void ClearCache()
    {
        _cachedStationInfo = null;
        _cacheExpiry = DateTime.MinValue;
    }
}

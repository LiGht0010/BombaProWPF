using BombaProMax.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing payment methods (Moyens de Paiement).
/// </summary>
public class MoyensPaiementService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.MoyensPaiement;

    public MoyensPaiementService()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    /// <summary>
    /// Gets all payment methods.
    /// </summary>
    public async Task<List<MoyensPaiementDto>> GetAllAsync()
    {
        try
        {
            Debug.WriteLine($"[MoyensPaiementService] GET {BaseUrl}");
            var response = await _httpClient.GetAsync(BaseUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[MoyensPaiementService] Response: {json}");
                return JsonConvert.DeserializeObject<List<MoyensPaiementDto>>(json) ?? [];
            }
            else
            {
                Debug.WriteLine($"[MoyensPaiementService] Error: {response.StatusCode}");
                return [];
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MoyensPaiementService] Exception: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets a payment method by ID.
    /// </summary>
    public async Task<MoyensPaiementDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MoyensPaiementDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MoyensPaiementService] Error getting payment method {id}: {ex.Message}");
            return null;
        }
    }
}

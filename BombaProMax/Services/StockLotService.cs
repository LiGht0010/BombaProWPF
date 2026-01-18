using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for StockLot operations including Opening Balance management.
/// </summary>
public class StockLotService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.StockLots;

    public StockLotService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    // ============================
    // OPENING BALANCE OPERATIONS
    // ============================

    /// <summary>
    /// Creates an opening balance for a reservoir.
    /// </summary>
    public async Task<(bool Success, OpeningBalanceResultDto? Result, string? ErrorMessage)> CreateOpeningBalanceAsync(
        OpeningBalanceCreateDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{BaseUrl}/opening-balance", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<OpeningBalanceResultDto>(responseJson);
                return (true, result, null);
            }
            
            // Try to extract error message from response
            var errorMessage = responseJson;
            try
            {
                // API might return error as plain text or JSON
                if (responseJson.StartsWith("{"))
                {
                    var errorObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson);
                    if (errorObj != null && errorObj.TryGetValue("message", out var msg))
                    {
                        errorMessage = msg?.ToString() ?? responseJson;
                    }
                }
            }
            catch
            {
                // Keep original response as error message
            }

            return (false, null, errorMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating opening balance: {ex.Message}");
            return (false, null, $"Erreur de connexion: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the stock status for a reservoir.
    /// </summary>
    public async Task<ReservoirStockStatusDto?> GetReservoirStockStatusAsync(int reservoirId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/reservoir/{reservoirId}/status");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ReservoirStockStatusDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting reservoir stock status: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all reservoirs that need opening balance setup.
    /// </summary>
    public async Task<List<ReservoirStockStatusDto>> GetReservoirsNeedingOpeningBalanceAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiConfig.Reservoirs}/needing-opening-balance");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReservoirStockStatusDto>>(json) 
                    ?? new List<ReservoirStockStatusDto>();
            }
            return new List<ReservoirStockStatusDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting reservoirs needing opening balance: {ex.Message}");
            return new List<ReservoirStockStatusDto>();
        }
    }

    /// <summary>
    /// Validates if stock is available for a sale.
    /// </summary>
    public async Task<(bool IsAvailable, decimal QuantiteDisponible, string Message)> ValidateStockAvailabilityAsync(
        int reservoirId, 
        int produitId, 
        decimal quantiteRequired)
    {
        try
        {
            var url = $"{BaseUrl}/validate-availability?reservoirId={reservoirId}&produitId={produitId}&quantite={quantiteRequired}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                
                if (result != null)
                {
                    var isAvailable = Convert.ToBoolean(result["IsAvailable"]);
                    var disponible = Convert.ToDecimal(result["QuantiteDisponible"]);
                    var message = result["Message"]?.ToString() ?? "";
                    return (isAvailable, disponible, message);
                }
            }
            
            return (false, 0, "Erreur lors de la validation du stock");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating stock availability: {ex.Message}");
            return (false, 0, $"Erreur de connexion: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the available stock for a reservoir.
    /// </summary>
    public async Task<decimal> GetAvailableStockAsync(int reservoirId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/reservoir/{reservoirId}/available");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (result != null && result.TryGetValue("QuantiteDisponible", out var qty))
                {
                    return Convert.ToDecimal(qty);
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting available stock: {ex.Message}");
            return 0;
        }
    }
}

using BombaProMaxWPF.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Service for stock withdrawal operations.
/// Used by super admin to manually remove stock from reservoirs.
/// </summary>
public class StockWithdrawalService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.StockLots;

    public StockWithdrawalService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    /// <summary>
    /// Withdraws stock from a reservoir using FIFO order.
    /// </summary>
    /// <param name="request">Withdrawal request details</param>
    /// <returns>Result with affected lots breakdown</returns>
    public async Task<StockWithdrawalResponseDto> WithdrawStockAsync(StockWithdrawalRequestDto request)
    {
        try
        {
            Debug.WriteLine($"[StockWithdrawalService] Withdrawing {request.Quantite}L from Reservoir {request.ReservoirID}");

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/withdraw", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[StockWithdrawalService] Response: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<StockWithdrawalResponseDto>(responseJson);
                return result ?? new StockWithdrawalResponseDto
                {
                    Success = false,
                    Message = "Réponse invalide du serveur"
                };
            }

            // Try to parse error response
            try
            {
                var errorResult = JsonConvert.DeserializeObject<StockWithdrawalResponseDto>(responseJson);
                if (errorResult != null)
                    return errorResult;
            }
            catch
            {
                // Ignore parse error
            }

            return new StockWithdrawalResponseDto
            {
                Success = false,
                Message = $"Erreur: {responseJson}"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockWithdrawalService] Error: {ex.Message}");
            return new StockWithdrawalResponseDto
            {
                Success = false,
                Message = $"Erreur de connexion: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Deletes a withdrawal and restores stock to the affected lots and reservoir.
    /// </summary>
    /// <param name="withdrawalId">ID of the withdrawal to delete</param>
    /// <returns>Result with restoration details</returns>
    public async Task<StockWithdrawalResponseDto> DeleteWithdrawalAsync(int withdrawalId)
    {
        try
        {
            Debug.WriteLine($"[StockWithdrawalService] Deleting withdrawal {withdrawalId}");

            var response = await _httpClient.DeleteAsync($"{BaseUrl}/withdrawals/{withdrawalId}");
            var responseJson = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[StockWithdrawalService] Delete Response: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<StockWithdrawalResponseDto>(responseJson);
                return result ?? new StockWithdrawalResponseDto
                {
                    Success = false,
                    Message = "Réponse invalide du serveur"
                };
            }

            // Try to parse error response
            try
            {
                var errorResult = JsonConvert.DeserializeObject<StockWithdrawalResponseDto>(responseJson);
                if (errorResult != null)
                    return errorResult;
            }
            catch
            {
                // Ignore parse error
            }

            return new StockWithdrawalResponseDto
            {
                Success = false,
                Message = $"Erreur: {responseJson}"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockWithdrawalService] Error deleting: {ex.Message}");
            return new StockWithdrawalResponseDto
            {
                Success = false,
                Message = $"Erreur de connexion: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gets withdrawal history records.
    /// </summary>
    /// <param name="reservoirId">Optional filter by reservoir</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of withdrawal history records</returns>
    public async Task<List<StockWithdrawalHistoryDto>> GetWithdrawalHistoryAsync(
        int? reservoirId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (reservoirId.HasValue)
                queryParams.Add($"reservoirId={reservoirId.Value}");
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            var url = $"{BaseUrl}/withdrawals";
            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            Debug.WriteLine($"[StockWithdrawalService] GET {url}");

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<StockWithdrawalHistoryDto>>(json) ?? [];
            }

            Debug.WriteLine($"[StockWithdrawalService] Error getting history: {response.StatusCode}");
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockWithdrawalService] Error: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets reservoirs available for withdrawal with stock info.
    /// </summary>
    /// <returns>List of reservoirs with available stock</returns>
    public async Task<List<ReservoirWithdrawalInfoDto>> GetReservoirsForWithdrawalAsync()
    {
        try
        {
            var url = $"{BaseUrl}/reservoirs-for-withdrawal";
            Debug.WriteLine($"[StockWithdrawalService] GET {url}");

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReservoirWithdrawalInfoDto>>(json) ?? [];
            }

            Debug.WriteLine($"[StockWithdrawalService] Error getting reservoirs: {response.StatusCode}");
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockWithdrawalService] Error: {ex.Message}");
            return [];
        }
    }
}

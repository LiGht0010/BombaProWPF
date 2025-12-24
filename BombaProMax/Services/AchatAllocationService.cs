using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing fuel purchase allocations to reservoirs.
/// </summary>
public class AchatAllocationService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.AchatAllocations;

    public AchatAllocationService()
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
    public async Task<List<AchatAllocationDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);

            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AchatAllocationDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching allocations: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<AchatAllocationDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AchatAllocationDto>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching allocation: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY ACHAT ID
    // ============================
    public async Task<List<AchatAllocationDto>> GetByAchatIdAsync(int achatId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/achat/{achatId}");

            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AchatAllocationDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching allocations by achat: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY RESERVOIR ID
    // ============================
    public async Task<List<AchatAllocationDto>> GetByReservoirIdAsync(int reservoirId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/reservoir/{reservoirId}");

            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AchatAllocationDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching allocations by reservoir: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET AVAILABLE RESERVOIRS
    // ============================
    /// <summary>
    /// Gets reservoirs available for allocation for a specific fuel product.
    /// Returns reservoirs that either have the same product or are empty.
    /// </summary>
    public async Task<List<ReservoirAllocationInfoDto>> GetAvailableReservoirsAsync(int produitId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/available-reservoirs/{produitId}");

            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ReservoirAllocationInfoDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching available reservoirs: {ex.Message}");
            return [];
        }
    }

    // ============================
    // CHECK ACHAT ALLOCATION STATUS
    // ============================
    /// <summary>
    /// Checks if an achat has been allocated and returns allocation status.
    /// </summary>
    public async Task<AchatAllocationStatusDto?> CheckAchatAllocationStatusAsync(int achatId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/check-achat-allocated/{achatId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AchatAllocationStatusDto>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking achat allocation status: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE SINGLE ALLOCATION
    // ============================
    public async Task<AchatAllocationDto?> CreateAsync(AchatAllocationDto dto)
    {
        try
        {
            dto.UtilisateurAllocation = App.CurrentUser?.Name ?? App.user?.Name;
            dto.DateAllocation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {error}");
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AchatAllocationDto>(responseJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create error: {ex.Message}");
            return null;
        }
    }

    // ============================
    // BATCH ALLOCATION
    // ============================
    /// <summary>
    /// Allocates fuel from an achat to multiple reservoirs in a single transaction.
    /// </summary>
    public async Task<BatchAllocationResponseDto?> BatchAllocateAsync(BatchAllocationRequestDto request)
    {
        try
        {
            request.UtilisateurAllocation = App.CurrentUser?.Name ?? App.user?.Name;

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/batch", content);

            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Try to parse error response
                var errorResponse = JsonConvert.DeserializeObject<BatchAllocationResponseDto>(responseJson);
                if (errorResponse != null)
                    return errorResponse;

                return new BatchAllocationResponseDto
                {
                    Success = false,
                    Message = $"Erreur serveur: {response.StatusCode}"
                };
            }

            return JsonConvert.DeserializeObject<BatchAllocationResponseDto>(responseJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Batch allocation error: {ex.Message}");
            return new BatchAllocationResponseDto
            {
                Success = false,
                Message = $"Erreur: {ex.Message}"
            };
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateAsync(AchatAllocationDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Update failed ({response.StatusCode}): {err}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update error: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete error: {ex.Message}");
            return false;
        }
    }

    // ============================
    // CANCEL ALLOCATION
    // ============================
    /// <summary>
    /// Cancels an allocation and reverses the reservoir fuel level.
    /// </summary>
    public async Task<bool> CancelAsync(int id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/cancel/{id}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cancel error: {ex.Message}");
            return false;
        }
    }

    // ============================
    // HELPER: Get total allocated for achat
    // ============================
    public async Task<decimal> GetTotalAllocatedForAchatAsync(int achatId)
    {
        var allocations = await GetByAchatIdAsync(achatId);
        return allocations
            .Where(a => a.Statut != "Annulée")
            .Sum(a => a.QuantiteAllouee);
    }
}

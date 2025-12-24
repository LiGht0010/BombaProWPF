using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

public class ReservoirService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Reservoirs;

    public ReservoirService()
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
    public async Task<List<ReservoirDto>> GetAllReservoirsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReservoirDto>>(json) ?? new List<ReservoirDto>();
            }
            return new List<ReservoirDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching reservoirs: {ex.Message}");
            return new List<ReservoirDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ReservoirDto?> GetReservoirByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ReservoirDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching reservoir: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY PRODUCT (Server-side)
    // ============================
    public async Task<List<ReservoirDto>> GetReservoirsByProductIdAsync(int produitId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/product/{produitId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReservoirDto>>(json) ?? new List<ReservoirDto>();
            }
            return new List<ReservoirDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching reservoirs by product: {ex.Message}");
            return new List<ReservoirDto>();
        }
    }

    // ============================
    // GET LOW FUEL (Server-side)
    // ============================
    public async Task<List<ReservoirDto>> GetLowFuelReservoirsAsync(decimal thresholdPercentage = 20m)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/lowfuel?threshold={thresholdPercentage}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReservoirDto>>(json) ?? new List<ReservoirDto>();
            }
            // Fallback to client-side filter
            var allReservoirs = await GetAllReservoirsAsync();
            return allReservoirs
                .Where(r => r.Capacite > 0 && (r.NiveauDeCarburant / r.Capacite * 100) < thresholdPercentage)
                .OrderBy(r => r.NiveauDeCarburant / r.Capacite)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching low fuel reservoirs: {ex.Message}");
            return new List<ReservoirDto>();
        }
    }

    // ============================
    // CHECK NUMERO EXISTS (Server-side)
    // ============================
    public async Task<bool> ReservoirNumberExistsAsync(string numero, int? excludeReservoirId = null)
    {
        try
        {
            var url = $"{BaseUrl}/check-numero?numero={Uri.EscapeDataString(numero)}";
            if (excludeReservoirId.HasValue)
            {
                url += $"&excludeId={excludeReservoirId.Value}";
            }
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            // Fallback to client-side check
            var reservoirs = await GetAllReservoirsAsync();
            return reservoirs.Any(r =>
                r.Numero.Equals(numero, StringComparison.OrdinalIgnoreCase) &&
                r.ID != excludeReservoirId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking reservoir number: {ex.Message}");
            return false;
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ReservoirDto?> CreateReservoirAsync(ReservoirDto reservoir)
    {
        try
        {
            // Set audit fields
            reservoir.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            reservoir.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(reservoir);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ReservoirDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating reservoir: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateReservoirAsync(ReservoirDto reservoir)
    {
        try
        {
            // Set audit fields
            reservoir.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            reservoir.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(reservoir);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{reservoir.ID}", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating reservoir: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteReservoirAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting reservoir: {ex.Message}");
            return false;
        }
    }

    // ============================
    // UPDATE FUEL LEVEL
    // ============================
    public async Task<bool> UpdateFuelLevelAsync(int reservoirId, decimal newLevel)
    {
        try
        {
            var reservoir = await GetReservoirByIdAsync(reservoirId);
            if (reservoir == null)
            {
                return false;
            }

            // Validate fuel level doesn't exceed capacity
            if (newLevel > reservoir.Capacite)
            {
                Console.WriteLine($"Error: Fuel level ({newLevel}) exceeds capacity ({reservoir.Capacite})");
                return false;
            }

            reservoir.NiveauDeCarburant = newLevel;
            return await UpdateReservoirAsync(reservoir);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating fuel level: {ex.Message}");
            return false;
        }
    }

    // ============================
    // GET CAPACITY STATISTICS
    // ============================
    public async Task<Dictionary<string, decimal>> GetCapacityStatisticsAsync()
    {
        try
        {
            var reservoirs = await GetAllReservoirsAsync();
            return new Dictionary<string, decimal>
            {
                { "TotalCapacity", reservoirs.Sum(r => r.Capacite) },
                { "TotalFuelLevel", reservoirs.Sum(r => r.NiveauDeCarburant) },
                { "AverageCapacity", reservoirs.Count > 0 ? reservoirs.Average(r => r.Capacite) : 0 },
                { "AverageFuelLevel", reservoirs.Count > 0 ? reservoirs.Average(r => r.NiveauDeCarburant) : 0 },
                { "TotalAvailableSpace", reservoirs.Sum(r => r.Capacite - r.NiveauDeCarburant) }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating capacity statistics: {ex.Message}");
            return new Dictionary<string, decimal>();
        }
    }

    // ============================
    // SEARCH (Client-side)
    // ============================
    public async Task<List<ReservoirDto>> SearchReservoirsAsync(string searchTerm)
    {
        try
        {
            var allReservoirs = await GetAllReservoirsAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allReservoirs;
            }

            searchTerm = searchTerm.ToLower();
            return allReservoirs.Where(r =>
                (r.Numero?.ToLower().Contains(searchTerm) ?? false) ||
                (r.ProduitNom?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching reservoirs: {ex.Message}");
            return new List<ReservoirDto>();
        }
    }
}
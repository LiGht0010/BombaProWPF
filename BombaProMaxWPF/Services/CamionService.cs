using BombaProMaxWPF.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMaxWPF.Services;

public class CamionService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Camions;

    public CamionService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<CamionDto>> GetAllCamionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CamionDto>>(json) ?? new List<CamionDto>();
            }
            return new List<CamionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching camions: {ex.Message}");
            return new List<CamionDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<CamionDto?> GetCamionByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CamionDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching camion: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY FOURNISSEUR (Server-side)
    // ============================
    public async Task<List<CamionDto>> GetCamionsByFournisseurAsync(int fournisseurId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/fournisseur/{fournisseurId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CamionDto>>(json) ?? new List<CamionDto>();
            }
            return new List<CamionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching camions by fournisseur: {ex.Message}");
            return new List<CamionDto>();
        }
    }

    // ============================
    // GET AVAILABLE (Server-side)
    // ============================
    public async Task<List<CamionDto>> GetAvailableCamionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/available");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CamionDto>>(json) ?? new List<CamionDto>();
            }
            // Fallback to client-side filter
            var all = await GetAllCamionsAsync();
            return all.Where(c => c.CiterneID == null).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching available camions: {ex.Message}");
            return new List<CamionDto>();
        }
    }

    // ============================
    // GET BY CITERNE (Client-side)
    // ============================
    public async Task<List<CamionDto>> GetCamionsByCiterneAsync(int citerneId)
    {
        try
        {
            var allCamions = await GetAllCamionsAsync();
            return allCamions.Where(c => c.CiterneID == citerneId).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching camions by citerne: {ex.Message}");
            return new List<CamionDto>();
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<CamionDto?> CreateCamionAsync(CamionDto camion)
    {
        try
        {
            // Set audit fields
            camion.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            camion.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(camion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CamionDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating camion: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateCamionAsync(CamionDto camion)
    {
        try
        {
            // Set audit fields
            camion.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            camion.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(camion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{camion.ID}", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating camion: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteCamionAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting camion: {ex.Message}");
            return false;
        }
    }

    // ============================
    // ASSIGN CITERNE
    // ============================
    public async Task<bool> AssignCiterneToCamionAsync(int camionId, int citerneId)
    {
        try
        {
            var camion = await GetCamionByIdAsync(camionId);
            if (camion != null)
            {
                camion.CiterneID = citerneId;
                return await UpdateCamionAsync(camion);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error assigning citerne to camion: {ex.Message}");
            return false;
        }
    }

    // ============================
    // UNASSIGN CITERNE
    // ============================
    public async Task<bool> UnassignCiterneFromCamionAsync(int camionId)
    {
        try
        {
            var camion = await GetCamionByIdAsync(camionId);
            if (camion != null)
            {
                camion.CiterneID = null;
                return await UpdateCamionAsync(camion);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unassigning citerne from camion: {ex.Message}");
            return false;
        }
    }

    // ============================
    // SEARCH (Client-side)
    // ============================
    public async Task<List<CamionDto>> SearchCamionsAsync(string searchTerm)
    {
        try
        {
            var allCamions = await GetAllCamionsAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allCamions;
            }

            searchTerm = searchTerm.ToLower();
            return allCamions.Where(c =>
                (c.Matricule?.ToLower().Contains(searchTerm) ?? false) ||
                (c.Marque?.ToLower().Contains(searchTerm) ?? false) ||
                (c.FournisseurNom?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching camions: {ex.Message}");
            return new List<CamionDto>();
        }
    }

    // ============================
    // STATISTICS
    // ============================
    public async Task<CamionStatistics> GetCamionStatisticsAsync()
    {
        try
        {
            var allCamions = await GetAllCamionsAsync();
            return new CamionStatistics
            {
                TotalCamions = allCamions.Count,
                CamionsWithCiterne = allCamions.Count(c => c.CiterneID.HasValue),
                AvailableCamions = allCamions.Count(c => !c.CiterneID.HasValue)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting camion statistics: {ex.Message}");
            return new CamionStatistics();
        }
    }

    // ============================
    // CHECK MATRICULE UNIQUE
    // ============================
    public async Task<bool> IsMatriculeUniqueAsync(string matricule, int? excludeCamionId = null)
    {
        try
        {
            var allCamions = await GetAllCamionsAsync();
            return !allCamions.Any(c => 
                c.Matricule?.Equals(matricule, StringComparison.OrdinalIgnoreCase) == true &&
                c.ID != excludeCamionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking matricule uniqueness: {ex.Message}");
            return true;
        }
    }
}

// Helper class for statistics
public class CamionStatistics
{
    public int TotalCamions { get; set; }
    public int CamionsWithCiterne { get; set; }
    public int AvailableCamions { get; set; }
}

using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class VoyageService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Voyages;

    public async Task<List<VoyageDto>> GetAllVoyagesAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<VoyageDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoyageService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<VoyageDto?> GetVoyageByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<VoyageDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoyageService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<VoyageDto?> CreateVoyageAsync(VoyageDto voyage)
    {
        try
        {
            voyage.AjoutePar    = App.CurrentUser?.UserId;
            voyage.DateCreation = DateTime.UtcNow;

            var json    = JsonConvert.SerializeObject(voyage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[VoyageService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<VoyageDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoyageService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateVoyageAsync(VoyageDto voyage)
    {
        try
        {
            voyage.ModifiePar        = App.CurrentUser?.UserId;
            voyage.DateModification  = DateTime.UtcNow;

            var json    = JsonConvert.SerializeObject(voyage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{voyage.VoyageId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoyageService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteVoyageAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoyageService] Delete error: {ex.Message}");
            return false;
        }
    }
}

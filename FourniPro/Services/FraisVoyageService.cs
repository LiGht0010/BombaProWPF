using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class FraisVoyageService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.FraisVoyages;

    public async Task<List<FraisVoyageDto>> GetAllAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<FraisVoyageDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FraisVoyageService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<List<FraisVoyageDto>> GetByVoyageAsync(int voyageId)
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<FraisVoyageDto>>($"{BaseUrl}?voyageId={voyageId}");
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FraisVoyageService] GetByVoyage error: {ex.Message}");
            return [];
        }
    }

    public async Task<FraisVoyageDto?> CreateAsync(FraisVoyageDto dto)
    {
        try
        {
            var json     = JsonConvert.SerializeObject(dto);
            var content  = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FraisVoyageService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FraisVoyageDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FraisVoyageService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(FraisVoyageDto dto)
    {
        try
        {
            var json     = JsonConvert.SerializeObject(dto);
            var content  = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.FraisVoyageId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FraisVoyageService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FraisVoyageService] Delete error: {ex.Message}");
            return false;
        }
    }
}

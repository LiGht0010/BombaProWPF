using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class StockVoyageService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.StockVoyages;

    public async Task<List<StockVoyageDto>> GetAllAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<StockVoyageDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockVoyageService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<List<StockVoyageDto>> GetByVoyageAsync(int voyageId)
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<StockVoyageDto>>($"{BaseUrl}?voyageId={voyageId}");
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockVoyageService] GetByVoyage error: {ex.Message}");
            return [];
        }
    }

    public async Task<StockVoyageDto?> CreateAsync(StockVoyageDto dto)
    {
        try
        {
            var json     = JsonConvert.SerializeObject(dto);
            var content  = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[StockVoyageService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StockVoyageDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockVoyageService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(StockVoyageDto dto)
    {
        try
        {
            var json     = JsonConvert.SerializeObject(dto);
            var content  = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.StockVoyageId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StockVoyageService] Update error: {ex.Message}");
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
            Debug.WriteLine($"[StockVoyageService] Delete error: {ex.Message}");
            return false;
        }
    }
}

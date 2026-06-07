using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class AvoirService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Avoirs;

    public async Task<List<AvoirDto>> GetAllAsync(int? venteId = null, int? creditId = null)
    {
        try
        {
            var url = BaseUrl;
            if (venteId.HasValue)
                url += $"?venteId={venteId}";
            else if (creditId.HasValue)
                url += $"?creditId={creditId}";

            var list = await _httpClient.GetFromJsonAsync<List<AvoirDto>>(url);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AvoirService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<AvoirDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AvoirDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AvoirService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<AvoirDto?> CreateAsync(AvoirDto dto)
    {
        try
        {
            dto.AjoutePar    = App.CurrentUser?.UserId;
            dto.DateCreation = DateTime.UtcNow;

            var content  = new StringContent(
                JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[AvoirService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AvoirDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AvoirService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(AvoirDto dto)
    {
        try
        {
            dto.ModifiePar       = App.CurrentUser?.UserId;
            dto.DateModification = DateTime.UtcNow;

            var content  = new StringContent(
                JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.AvoirId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AvoirService] Update error: {ex.Message}");
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
            Debug.WriteLine($"[AvoirService] Delete error: {ex.Message}");
            return false;
        }
    }
}

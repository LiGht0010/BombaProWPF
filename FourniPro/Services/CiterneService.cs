using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class CiterneService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Citernes;

    public async Task<List<CiterneDto>> GetAllCiternesAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<CiterneDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CiterneService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<CiterneDto?> GetCiterneByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CiterneDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CiterneService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<CiterneDto?> CreateCiterneAsync(CiterneDto citerne)
    {
        try
        {
            citerne.AjoutePar = App.CurrentUser?.UserId;
            citerne.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(citerne);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[CiterneService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CiterneDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CiterneService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateCiterneAsync(CiterneDto citerne)
    {
        try
        {
            citerne.ModifiePar = App.CurrentUser?.UserId;
            citerne.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(citerne);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{citerne.CiterneId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CiterneService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteCiterneAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CiterneService] Delete error: {ex.Message}");
            return false;
        }
    }
}

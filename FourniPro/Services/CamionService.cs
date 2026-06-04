using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class CamionService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Camions;

    public async Task<List<CamionDto>> GetAllCamionsAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<CamionDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CamionService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<CamionDto?> GetCamionByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CamionDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CamionService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<CamionDto?> CreateCamionAsync(CamionDto camion)
    {
        try
        {
            camion.AjoutePar = App.CurrentUser?.UserId;
            camion.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(camion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[CamionService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CamionDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CamionService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateCamionAsync(CamionDto camion)
    {
        try
        {
            camion.ModifiePar = App.CurrentUser?.UserId;
            camion.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(camion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{camion.CamionId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CamionService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteCamionAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CamionService] Delete error: {ex.Message}");
            return false;
        }
    }
}

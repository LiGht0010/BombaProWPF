using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class AchatService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Achats;

    public async Task<List<AchatDto>> GetAllAchatsAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<AchatDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AchatService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<AchatDto?> GetAchatByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AchatDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AchatService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<AchatDto?> CreateAchatAsync(AchatDto achat)
    {
        try
        {
            achat.AjoutePar = App.CurrentUser?.UserId;
            achat.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(achat);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[AchatService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AchatDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AchatService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateAchatAsync(AchatDto achat)
    {
        try
        {
            achat.ModifiePar = App.CurrentUser?.UserId;
            achat.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(achat);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{achat.AchatId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AchatService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAchatAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AchatService] Delete error: {ex.Message}");
            return false;
        }
    }
}

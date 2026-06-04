using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class ChauffeurService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Chauffeurs;

    public async Task<List<ChauffeurDto>> GetAllChauffeursAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<ChauffeurDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ChauffeurService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<ChauffeurDto?> GetChauffeurByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ChauffeurDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ChauffeurService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<ChauffeurDto?> CreateChauffeurAsync(ChauffeurDto chauffeur)
    {
        try
        {
            chauffeur.AjoutePar = App.CurrentUser?.UserId;
            chauffeur.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(chauffeur);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[ChauffeurService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ChauffeurDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ChauffeurService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateChauffeurAsync(ChauffeurDto chauffeur)
    {
        try
        {
            chauffeur.ModifiePar = App.CurrentUser?.UserId;
            chauffeur.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(chauffeur);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{chauffeur.ChauffeurId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ChauffeurService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteChauffeurAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ChauffeurService] Delete error: {ex.Message}");
            return false;
        }
    }
}

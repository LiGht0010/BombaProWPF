using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class PaiementFournisseurService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.PaiementsFournisseur;

    public async Task<List<PaiementFournisseurDto>> GetAllAsync(int? creditFournisseurId = null)
    {
        try
        {
            var url  = creditFournisseurId.HasValue
                ? $"{BaseUrl}?creditFournisseurId={creditFournisseurId}"
                : BaseUrl;
            var list = await _httpClient.GetFromJsonAsync<List<PaiementFournisseurDto>>(url);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementFournisseurService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<PaiementFournisseurDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PaiementFournisseurDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementFournisseurService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<PaiementFournisseurDto?> CreateAsync(PaiementFournisseurDto dto)
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
                Debug.WriteLine($"[PaiementFournisseurService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PaiementFournisseurDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementFournisseurService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(PaiementFournisseurDto dto)
    {
        try
        {
            dto.ModifiePar       = App.CurrentUser?.UserId;
            dto.DateModification = DateTime.UtcNow;

            var content  = new StringContent(
                JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(
                $"{BaseUrl}/{dto.PaiementFournisseurId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementFournisseurService] Update error: {ex.Message}");
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
            Debug.WriteLine($"[PaiementFournisseurService] Delete error: {ex.Message}");
            return false;
        }
    }
}

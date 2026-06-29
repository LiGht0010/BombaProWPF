using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class CreditFournisseurService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.CreditsFournisseur;

    public async Task<List<CreditFournisseurDto>> GetAllAsync(int? fournisseurId = null)
    {
        try
        {
            var url  = fournisseurId.HasValue ? $"{BaseUrl}?fournisseurId={fournisseurId}" : BaseUrl;
            var list = await _httpClient.GetFromJsonAsync<List<CreditFournisseurDto>>(url);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditFournisseurService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<CreditFournisseurDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CreditFournisseurDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditFournisseurService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<CreditFournisseurDto?> CreateCreditFournisseurAsync(CreditFournisseurDto dto)
    {
        try
        {
            dto.AjoutePar    = App.CurrentUser?.UserId;
            dto.DateCreation = DateTime.UtcNow;

            var content  = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[CreditFournisseurService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CreditFournisseurDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditFournisseurService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateCreditFournisseurAsync(CreditFournisseurDto dto)
    {
        try
        {
            dto.ModifiePar       = App.CurrentUser?.UserId;
            dto.DateModification = DateTime.UtcNow;

            var content  = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.CreditFournisseurId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditFournisseurService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteCreditFournisseurAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditFournisseurService] Delete error: {ex.Message}");
            return false;
        }
    }
}

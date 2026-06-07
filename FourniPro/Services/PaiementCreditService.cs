using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class PaiementCreditService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.PaiementsCredit;

    public async Task<List<PaiementCreditDto>> GetAllAsync(int? creditId = null)
    {
        try
        {
            var url  = creditId.HasValue ? $"{BaseUrl}?creditId={creditId}" : BaseUrl;
            var list = await _httpClient.GetFromJsonAsync<List<PaiementCreditDto>>(url);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementCreditService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<PaiementCreditDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PaiementCreditDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementCreditService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<PaiementCreditDto?> CreateAsync(PaiementCreditDto dto)
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
                Debug.WriteLine($"[PaiementCreditService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PaiementCreditDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementCreditService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(PaiementCreditDto dto)
    {
        try
        {
            dto.ModifiePar       = App.CurrentUser?.UserId;
            dto.DateModification = DateTime.UtcNow;

            var content  = new StringContent(
                JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(
                $"{BaseUrl}/{dto.PaiementCreditId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PaiementCreditService] Update error: {ex.Message}");
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
            Debug.WriteLine($"[PaiementCreditService] Delete error: {ex.Message}");
            return false;
        }
    }
}

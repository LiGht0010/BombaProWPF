using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class CreditService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Credits;

    public async Task<List<CreditDto>> GetAllCreditsAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<CreditDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<CreditDto?> GetCreditByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CreditDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<CreditDto?> CreateCreditAsync(CreditDto credit)
    {
        try
        {
            credit.AjoutePar   = App.CurrentUser?.UserId;
            credit.DateCreation = DateTime.UtcNow;

            var json    = JsonConvert.SerializeObject(credit);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[CreditService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CreditDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateCreditAsync(CreditDto credit)
    {
        try
        {
            credit.ModifiePar       = App.CurrentUser?.UserId;
            credit.DateModification = DateTime.UtcNow;

            var json    = JsonConvert.SerializeObject(credit);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{credit.CreditId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteCreditAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreditService] Delete error: {ex.Message}");
            return false;
        }
    }
}

using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class EmployeService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Employes;

    public async Task<List<EmployeDto>> GetAllEmployesAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<EmployeDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmployeService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<EmployeDto?> GetEmployeByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EmployeDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmployeService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<EmployeDto?> CreateEmployeAsync(EmployeDto employe)
    {
        try
        {
            employe.AjoutePar    = App.CurrentUser?.UserId;
            employe.DateCreation = DateTime.UtcNow;

            var json    = JsonConvert.SerializeObject(employe);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[EmployeService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EmployeDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmployeService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateEmployeAsync(EmployeDto employe)
    {
        try
        {
            employe.ModifiePar        = App.CurrentUser?.UserId;
            employe.DateModification  = DateTime.UtcNow;

            var json    = JsonConvert.SerializeObject(employe);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{employe.EmployeId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmployeService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteEmployeAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmployeService] Delete error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> HasRelatedRecordsAsync(int id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<bool>($"{BaseUrl}/{id}/hasrelatedrecords");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EmployeService] HasRelated error: {ex.Message}");
            return true; // safe default — block deletion on error
        }
    }
}

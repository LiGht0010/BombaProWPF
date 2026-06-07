using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class VenteService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Ventes;

    public async Task<List<VenteDto>> GetAllVentesAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<VenteDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteService] GetAll error: {ex.Message}");
            return [];
        }
    }

    public async Task<VenteDto?> GetVenteByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<VenteDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteService] GetById error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<VenteDto>> GetByVoyageAsync(int voyageId)
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<VenteDto>>($"{BaseUrl}?voyageId={voyageId}");
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteService] GetByVoyage error: {ex.Message}");
            return [];
        }
    }

    public async Task<VenteDto?> CreateVenteAsync(VenteDto vente)
    {
        try
        {
            vente.AjoutePar = App.CurrentUser?.UserId;
            vente.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(vente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[VenteService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<VenteDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteService] Create error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> UpdateVenteAsync(VenteDto vente)
    {
        try
        {
            vente.ModifiePar = App.CurrentUser?.UserId;
            vente.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(vente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{vente.VenteId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteService] Update error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteVenteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VenteService] Delete error: {ex.Message}");
            return false;
        }
    }
}

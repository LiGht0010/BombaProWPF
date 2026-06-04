using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class FournisseurService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Fournisseurs;

    // ============================
    // GET ALL
    // ============================
    public async Task<List<FournisseurDto>> GetAllFournisseursAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<FournisseurDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FournisseurService] GetAll error: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<FournisseurDto?> GetFournisseurByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FournisseurDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FournisseurService] GetById error: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<FournisseurDto?> CreateFournisseurAsync(FournisseurDto fournisseur)
    {
        try
        {
            fournisseur.AjoutePar = App.CurrentUser?.UserId;
            fournisseur.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(fournisseur);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FournisseurService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FournisseurDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FournisseurService] Create error: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateFournisseurAsync(FournisseurDto fournisseur)
    {
        try
        {
            fournisseur.ModifiePar = App.CurrentUser?.UserId;
            fournisseur.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(fournisseur);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{fournisseur.FournisseurId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FournisseurService] Update error: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteFournisseurAsync(int fournisseurId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{fournisseurId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FournisseurService] Delete error: {ex.Message}");
            return false;
        }
    }
}

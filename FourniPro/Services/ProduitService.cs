using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class ProduitService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Produits;

    // ============================
    // GET ALL
    // ============================
    public async Task<List<ProduitDto>> GetAllProduitsAsync()
    {
        try
        {
            var produits = await _httpClient.GetFromJsonAsync<List<ProduitDto>>(BaseUrl);
            return produits ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProduitService] GetAll error: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ProduitDto?> GetProduitByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProduitDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProduitService] GetById error: {ex.Message}");
            return null;
        }
    }

    // ============================
    // SEARCH (client-side)
    // ============================
    public async Task<List<ProduitDto>> SearchProduitsAsync(string searchTerm)
    {
        var all = await GetAllProduitsAsync();
        if (string.IsNullOrWhiteSpace(searchTerm)) return all;

        searchTerm = searchTerm.ToLower();
        return all.Where(p =>
            (p.NumeroProduit?.ToLower().Contains(searchTerm) ?? false) ||
            (p.Description?.ToLower().Contains(searchTerm) ?? false))
            .ToList();
    }

    // ============================
    // LOW STOCK (client-side filter)
    // ============================
    public async Task<List<ProduitDto>> GetLowStockProduitsAsync()
    {
        var all = await GetAllProduitsAsync();
        return all.Where(p => p.IsLowStock).ToList();
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ProduitDto?> CreateProduitAsync(ProduitDto produit)
    {
        try
        {
            produit.AjoutePar = App.CurrentUser?.UserId;
            produit.DateCreation = DateTime.UtcNow;
            CalculatePrixTTC(produit);

            var json = JsonConvert.SerializeObject(produit);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[ProduitService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProduitDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProduitService] Create error: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateProduitAsync(ProduitDto produit)
    {
        try
        {
            produit.ModifiePar = App.CurrentUser?.UserId;
            produit.DateModification = DateTime.UtcNow;
            CalculatePrixTTC(produit);

            var json = JsonConvert.SerializeObject(produit);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{produit.ProduitId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProduitService] Update error: {ex.Message}");
            return false;
        }
    }

    // ============================
    // UPDATE STOCK
    // ============================
    public async Task<bool> UpdateStockAsync(int produitId, int delta)
    {
        var produit = await GetProduitByIdAsync(produitId);
        if (produit is null) return false;

        produit.Stock = (produit.Stock ?? 0) + delta;
        return await UpdateProduitAsync(produit);
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteProduitAsync(int produitId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{produitId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProduitService] Delete error: {ex.Message}");
            return false;
        }
    }

    // ============================
    // HELPER
    // ============================
    private static void CalculatePrixTTC(ProduitDto p)
    {
        if (p.PrixHT.HasValue && p.TVA.HasValue)
            p.PrixTTC = Math.Round(p.PrixHT.Value * (1 + p.TVA.Value / 100), 2);
    }
}

using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

public class ProduitService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Produits;

    public ProduitService()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<ProduitDto>> GetAllProduitsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProduitDto>>(json) ?? new List<ProduitDto>();
            }
            return new List<ProduitDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching products: {ex.Message}");
            return new List<ProduitDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ProduitDto?> GetProduitByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProduitDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching product: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY CATEGORY (Server-side)
    // ============================
    public async Task<List<ProduitDto>> GetProduitsByCategoryAsync(int categoryId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/category/{categoryId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProduitDto>>(json) ?? new List<ProduitDto>();
            }
            return new List<ProduitDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching products by category: {ex.Message}");
            return new List<ProduitDto>();
        }
    }

    // ============================
    // GET LOW STOCK (Server-side)
    // ============================
    public async Task<List<ProduitDto>> GetLowStockProduitsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/lowstock");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProduitDto>>(json) ?? new List<ProduitDto>();
            }
            // Fallback to client-side filter
            var allProduits = await GetAllProduitsAsync();
            return allProduits.Where(p => 
                p.Stock.HasValue && 
                p.StockMinimum.HasValue && 
                p.Stock.Value <= p.StockMinimum.Value)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching low stock products: {ex.Message}");
            return new List<ProduitDto>();
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ProduitDto?> CreateProduitAsync(ProduitDto produit)
    {
        try
        {
            // Set audit fields
            produit.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            produit.DateCreation = DateTime.UtcNow;

            // Calculate PrixTTC if needed
            CalculatePrixTTC(produit);

            var json = JsonConvert.SerializeObject(produit);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProduitDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating product: {ex.Message}");
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
            // Set audit fields
            produit.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            produit.DateModification = DateTime.UtcNow;

            // Calculate PrixTTC if needed
            CalculatePrixTTC(produit);

            var json = JsonConvert.SerializeObject(produit);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{produit.ID}", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating product: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteProduitAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting product: {ex.Message}");
            return false;
        }
    }

    // ============================
    // UPDATE STOCK
    // ============================
    public async Task<bool> UpdateStockAsync(int produitId, int quantity)
    {
        try
        {
            var produit = await GetProduitByIdAsync(produitId);
            if (produit != null)
            {
                produit.Stock = (produit.Stock ?? 0) + quantity;
                return await UpdateProduitAsync(produit);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating stock: {ex.Message}");
            return false;
        }
    }

    // ============================
    // SEARCH (Client-side)
    // ============================
    public async Task<List<ProduitDto>> SearchProduitsAsync(string searchTerm)
    {
        try
        {
            var allProduits = await GetAllProduitsAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allProduits;
            }

            searchTerm = searchTerm.ToLower();
            return allProduits.Where(p =>
                (p.NumeroProduit?.ToLower().Contains(searchTerm) ?? false) ||
                (p.Description?.ToLower().Contains(searchTerm) ?? false) ||
                (p.CategorieNom?.ToLower().Contains(searchTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching products: {ex.Message}");
            return new List<ProduitDto>();
        }
    }

    // ============================
    // HELPER: Calculate PrixTTC
    // ============================
    private static void CalculatePrixTTC(ProduitDto produit)
    {
        if (produit.PrixHT.HasValue && produit.TVA.HasValue)
        {
            produit.PrixTTC = Math.Round(produit.PrixHT.Value * (1 + produit.TVA.Value / 100), 2);
        }
    }
}

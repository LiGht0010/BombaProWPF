using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BombaProMax.Models;
using Newtonsoft.Json;

namespace BombaProMax.Services
{
    /// <summary>
    /// Service for managing lubricants and articles sales (non-fuel products).
    /// </summary>
    public class VenteLubrifiantsEtArticlesService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.VenteLubrifiantsEtArticles;

        public VenteLubrifiantsEtArticlesService()
        {
            _httpClient = HttpClientFactory.Create();
        }

        #region CRUD Operations

        /// <summary>
        /// Get all sales.
        /// </summary>
        public async Task<List<VenteLubrifiantsEtArticlesDto>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[VenteLubService] GetAll failed: {response.StatusCode}");
                    return new List<VenteLubrifiantsEtArticlesDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteLubrifiantsEtArticlesDto>>(json) ?? new List<VenteLubrifiantsEtArticlesDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error fetching sales: {ex.Message}");
                return new List<VenteLubrifiantsEtArticlesDto>();
            }
        }

        /// <summary>
        /// Get a sale by ID.
        /// </summary>
        public async Task<VenteLubrifiantsEtArticlesDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[VenteLubService] GetById failed: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VenteLubrifiantsEtArticlesDto>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error fetching sale: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get sales within a date range.
        /// </summary>
        public async Task<List<VenteLubrifiantsEtArticlesDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var url = $"{BaseUrl}/bydate?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[VenteLubService] GetByDateRange failed: {response.StatusCode}");
                    return new List<VenteLubrifiantsEtArticlesDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteLubrifiantsEtArticlesDto>>(json) ?? new List<VenteLubrifiantsEtArticlesDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error fetching sales by date: {ex.Message}");
                return new List<VenteLubrifiantsEtArticlesDto>();
            }
        }

        /// <summary>
        /// Get sales by category (lubrifiant or articles).
        /// </summary>
        public async Task<List<VenteLubrifiantsEtArticlesDto>> GetByCategoryAsync(string categoryName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/bycategory/{categoryName}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[VenteLubService] GetByCategory failed: {response.StatusCode}");
                    return new List<VenteLubrifiantsEtArticlesDto>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VenteLubrifiantsEtArticlesDto>>(json) ?? new List<VenteLubrifiantsEtArticlesDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error fetching sales by category: {ex.Message}");
                return new List<VenteLubrifiantsEtArticlesDto>();
            }
        }

        /// <summary>
        /// Create a new sale.
        /// </summary>
        public async Task<VenteLubrifiantsEtArticlesDto?> CreateAsync(VenteLubrifiantsEtArticlesDto dto)
        {
            try
            {
                // Set audit fields
                dto.CreePar = App.CurrentUser?.Name ?? App.user?.Name ?? "System";
                dto.DateCreation = DateTime.UtcNow;

                // Serialize with settings to handle nulls properly
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };
                
                var json = JsonConvert.SerializeObject(dto, settings);
                Debug.WriteLine($"[VenteLubService] Creating sale with JSON: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BaseUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"[VenteLubService] Create response: {response.StatusCode}");
                Debug.WriteLine($"[VenteLubService] Response content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[VenteLubService] Create failed: {response.StatusCode} - {responseContent}");
                    return null;
                }

                return JsonConvert.DeserializeObject<VenteLubrifiantsEtArticlesDto>(responseContent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error creating sale: {ex.Message}");
                Debug.WriteLine($"[VenteLubService] Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Update an existing sale.
        /// </summary>
        public async Task<bool> UpdateAsync(VenteLubrifiantsEtArticlesDto dto)
        {
            try
            {
                // Set audit fields
                dto.ModifiePar = App.CurrentUser?.Name ?? App.user?.Name ?? "System";
                dto.DateModification = DateTime.UtcNow;

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };
                
                var json = JsonConvert.SerializeObject(dto, settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[VenteLubService] Update failed: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error updating sale: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a sale.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[VenteLubService] Delete failed: {response.StatusCode}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error deleting sale: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Business Logic

        /// <summary>
        /// Search sales by product name, client name, or sale number.
        /// </summary>
        public async Task<List<VenteLubrifiantsEtArticlesDto>> SearchAsync(string searchTerm)
        {
            try
            {
                var allSales = await GetAllAsync();
                
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return allSales;

                searchTerm = searchTerm.ToLower();
                return allSales.Where(v =>
                    (v.NumeroVente?.ToLower().Contains(searchTerm) ?? false) ||
                    (v.ProduitNom?.ToLower().Contains(searchTerm) ?? false) ||
                    (v.ClientNom?.ToLower().Contains(searchTerm) ?? false) ||
                    (v.CategorieNom?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VenteLubService] Error searching sales: {ex.Message}");
                return new List<VenteLubrifiantsEtArticlesDto>();
            }
        }

        /// <summary>
        /// Get today's sales.
        /// </summary>
        public async Task<List<VenteLubrifiantsEtArticlesDto>> GetTodaySalesAsync()
        {
            var today = DateTime.Today;
            return await GetByDateRangeAsync(today, today.AddDays(1).AddTicks(-1));
        }

        /// <summary>
        /// Get sales statistics for a date range.
        /// </summary>
        public async Task<VenteSummary> GetSalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await GetByDateRangeAsync(startDate, endDate);
            
            return new VenteSummary
            {
                TotalVentes = sales.Count,
                TotalQuantite = sales.Sum(v => v.QuantiteVendue),
                TotalMontantTTC = sales.Sum(v => v.MontantTotalTTC),
                TotalMontantHT = sales.Sum(v => v.MontantTotalHT),
                TotalTVA = sales.Sum(v => v.MontantTVA),
                TotalMarge = sales.Sum(v => v.MargeBeneficiaire ?? 0),
                VentesParCategorie = sales
                    .GroupBy(v => v.CategorieNom)
                    .ToDictionary(g => g.Key, g => g.Sum(v => v.MontantTotalTTC))
            };
        }

        #endregion
    }

    /// <summary>
    /// Summary of sales for reporting.
    /// </summary>
    public class VenteSummary
    {
        public int TotalVentes { get; set; }
        public int TotalQuantite { get; set; }
        public decimal TotalMontantTTC { get; set; }
        public decimal TotalMontantHT { get; set; }
        public decimal TotalTVA { get; set; }
        public decimal TotalMarge { get; set; }
        public Dictionary<string, decimal> VentesParCategorie { get; set; } = new Dictionary<string, decimal>();
    }
}

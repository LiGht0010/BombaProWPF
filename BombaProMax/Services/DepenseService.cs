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
    /// Service for managing expenses (Depenses).
    /// </summary>
    public class DepenseService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.Depenses;

        public DepenseService()
        {
            // Create handler that ignores SSL certificate errors for development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        #region CRUD Operations

        /// <summary>
        /// Get all expenses.
        /// </summary>
        public async Task<List<DepenseDto>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[DepenseService] GetAll failed: {response.StatusCode}");
                    return [];
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DepenseDto>>(json) ?? [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error fetching depenses: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Get an expense by ID.
        /// </summary>
        public async Task<DepenseDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[DepenseService] GetById failed: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DepenseDto>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error fetching depense: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get expenses within a date range.
        /// </summary>
        public async Task<List<DepenseDto>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            try
            {
                var url = $"{BaseUrl}/bydate?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[DepenseService] GetByDateRange failed: {response.StatusCode}");
                    return [];
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DepenseDto>>(json) ?? [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error fetching depenses by date: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Get expenses by category.
        /// </summary>
        public async Task<List<DepenseDto>> GetByCategoryAsync(string categorie)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/bycategory/{categorie}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[DepenseService] GetByCategory failed: {response.StatusCode}");
                    return [];
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DepenseDto>>(json) ?? [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error fetching depenses by category: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Get all unique categories.
        /// </summary>
        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/categories");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[DepenseService] GetCategories failed: {response.StatusCode}");
                    return GetDefaultCategories();
                }

                var json = await response.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<string>>(json) ?? [];
                
                // Merge with default categories
                var allCategories = categories.Union(GetDefaultCategories()).Distinct().OrderBy(c => c).ToList();
                return allCategories;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error fetching categories: {ex.Message}");
                return GetDefaultCategories();
            }
        }

        /// <summary>
        /// Create a new expense.
        /// </summary>
        public async Task<DepenseDto?> CreateAsync(DepenseDto dto)
        {
            try
            {
                dto.CreePar = App.CurrentUser?.Name ?? App.user?.Name ?? "System";
                dto.DateCreation = DateTime.UtcNow;

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var json = JsonConvert.SerializeObject(dto, settings);
                Debug.WriteLine($"[DepenseService] Creating depense: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"[DepenseService] Create response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[DepenseService] Create failed: {responseContent}");
                    return null;
                }

                return JsonConvert.DeserializeObject<DepenseDto>(responseContent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error creating depense: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update an existing expense.
        /// </summary>
        public async Task<bool> UpdateAsync(DepenseDto dto)
        {
            try
            {
                dto.ModifiePar = App.CurrentUser?.Name ?? App.user?.Name ?? "System";
                dto.DateModification = DateTime.UtcNow;

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                var json = JsonConvert.SerializeObject(dto, settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[DepenseService] Update failed: {response.StatusCode} - {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error updating depense: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete an expense.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[DepenseService] Delete failed: {response.StatusCode}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error deleting depense: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Business Logic

        /// <summary>
        /// Search expenses by numero, category, or description.
        /// </summary>
        public async Task<List<DepenseDto>> SearchAsync(string searchTerm)
        {
            try
            {
                var allDepenses = await GetAllAsync();

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return allDepenses;

                searchTerm = searchTerm.ToLower();
                return allDepenses.Where(d =>
                    (d.Numero?.ToLower().Contains(searchTerm) ?? false) ||
                    (d.Categorie?.ToLower().Contains(searchTerm) ?? false) ||
                    (d.Description?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DepenseService] Error searching depenses: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Get today's expenses.
        /// </summary>
        public async Task<List<DepenseDto>> GetTodayDepensesAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await GetByDateRangeAsync(today, today);
        }

        /// <summary>
        /// Get this month's expenses.
        /// </summary>
        public async Task<List<DepenseDto>> GetThisMonthDepensesAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateOnly(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            return await GetByDateRangeAsync(startOfMonth, endOfMonth);
        }

        /// <summary>
        /// Get default expense categories.
        /// </summary>
        public static List<string> GetDefaultCategories()
        {
            return
            [
                "Carburant",
                "Électricité",
                "Eau",
                "Téléphone",
                "Internet",
                "Loyer",
                "Salaires",
                "Fournitures",
                "Entretien",
                "Réparations",
                "Assurance",
                "Taxes",
                "Transport",
                "Marketing",
                "Divers"
            ];
        }

        #endregion
    }
}

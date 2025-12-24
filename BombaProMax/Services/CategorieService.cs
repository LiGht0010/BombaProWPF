using BombaProMax.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BombaProMax.Services
{
    public class CategorieService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.Categories;

        public CategorieService()
        {
            // Create handler that ignores SSL certificate errors for development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        // Get all categories
        public async Task<List<CategorieDto>> GetAllCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CategorieDto>>(json) ?? new List<CategorieDto>();
                }
                return new List<CategorieDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories: {ex.Message}");
                return new List<CategorieDto>();
            }
        }

        // Get category by ID
        public async Task<CategorieDto?> GetCategorieByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CategorieDto>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching category: {ex.Message}");
                return null;
            }
        }

        // Create new category
        public async Task<CategorieDto?> CreateCategorieAsync(CategorieDto categorie)
        {
            try
            {
                var json = JsonConvert.SerializeObject(categorie);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CategorieDto>(responseJson);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating category: {ex.Message}");
                return null;
            }
        }

        // Update existing category
        public async Task<bool> UpdateCategorieAsync(CategorieDto categorie)
        {
            try
            {
                var json = JsonConvert.SerializeObject(categorie);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{categorie.ID}", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                return false;
            }
        }

        // Delete category
        public async Task<bool> DeleteCategorieAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category: {ex.Message}");
                return false;
            }
        }

        // Search categories by name
        public async Task<List<CategorieDto>> SearchCategoriesAsync(string searchTerm)
        {
            try
            {
                var allCategories = await GetAllCategoriesAsync();
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return allCategories;
                }

                searchTerm = searchTerm.ToLower();
                return allCategories.Where(c =>
                    c.Nom?.ToLower().Contains(searchTerm) ?? false)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching categories: {ex.Message}");
                return new List<CategorieDto>();
            }
        }
    }
}

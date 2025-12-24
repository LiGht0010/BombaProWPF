using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services
{
    public class FournisseurService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.Fournisseurs;

        public FournisseurService()
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
        public async Task<List<FournisseurDto>> GetAllFournisseursAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<FournisseurDto>>(json) ?? new List<FournisseurDto>();
                }
                return new List<FournisseurDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching fournisseurs: {ex.Message}");
                return new List<FournisseurDto>();
            }
        }

        // ============================
        // GET BY ID
        // ============================
        public async Task<FournisseurDto?> GetFournisseurByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FournisseurDto>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching fournisseur: {ex.Message}");
                return null;
            }
        }

        // ============================
        // GET ACTIVE (Server-side filter)
        // ============================
        public async Task<List<FournisseurDto>> GetActiveFournisseursAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/status/Actif");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<FournisseurDto>>(json) ?? new List<FournisseurDto>();
                }
                // Fallback to client-side filter
                var all = await GetAllFournisseursAsync();
                return all.Where(f => f.Statut == "Actif").ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching active fournisseurs: {ex.Message}");
                return new List<FournisseurDto>();
            }
        }

        // ============================
        // GET BY STATUS (Server-side filter)
        // ============================
        public async Task<List<FournisseurDto>> GetFournisseursByStatusAsync(string statut)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/status/{statut}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<FournisseurDto>>(json) ?? new List<FournisseurDto>();
                }
                return new List<FournisseurDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching fournisseurs by status: {ex.Message}");
                return new List<FournisseurDto>();
            }
        }

        // ============================
        // CREATE
        // ============================
        public async Task<FournisseurDto?> CreateFournisseurAsync(FournisseurDto fournisseur)
        {
            try
            {
                // Set audit fields
                fournisseur.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                fournisseur.DateCreation = DateTime.UtcNow;

                // Set default status if not provided
                if (string.IsNullOrWhiteSpace(fournisseur.Statut))
                {
                    fournisseur.Statut = "Actif";
                }

                var json = JsonConvert.SerializeObject(fournisseur);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FournisseurDto>(responseJson);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating fournisseur: {ex.Message}");
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
                // Set audit fields
                fournisseur.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                fournisseur.DateModification = DateTime.UtcNow;

                var json = JsonConvert.SerializeObject(fournisseur);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{fournisseur.ID}", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating fournisseur: {ex.Message}");
                return false;
            }
        }

        // ============================
        // DELETE
        // ============================
        public async Task<bool> DeleteFournisseurAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting fournisseur: {ex.Message}");
                return false;
            }
        }

        // ============================
        // UPDATE STATUS
        // ============================
        public async Task<bool> UpdateFournisseurStatusAsync(int fournisseurId, string statut)
        {
            try
            {
                var fournisseur = await GetFournisseurByIdAsync(fournisseurId);
                if (fournisseur != null)
                {
                    fournisseur.Statut = statut;
                    return await UpdateFournisseurAsync(fournisseur);
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating fournisseur status: {ex.Message}");
                return false;
            }
        }

        // ============================
        // SEARCH (Client-side)
        // ============================
        public async Task<List<FournisseurDto>> SearchFournisseursAsync(string searchTerm)
        {
            try
            {
                var allFournisseurs = await GetAllFournisseursAsync();
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return allFournisseurs;
                }

                searchTerm = searchTerm.ToLower();
                return allFournisseurs.Where(f =>
                    (f.Nom?.ToLower().Contains(searchTerm) ?? false) ||
                    (f.Prenom?.ToLower().Contains(searchTerm) ?? false) ||
                    (f.Societe?.ToLower().Contains(searchTerm) ?? false) ||
                    (f.Contact?.ToLower().Contains(searchTerm) ?? false) ||
                    (f.Email?.ToLower().Contains(searchTerm) ?? false) ||
                    (f.Telephone?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching fournisseurs: {ex.Message}");
                return new List<FournisseurDto>();
            }
        }

        // ============================
        // STATISTICS
        // ============================
        public async Task<FournisseurStatistics> GetFournisseurStatisticsAsync()
        {
            try
            {
                var allFournisseurs = await GetAllFournisseursAsync();
                return new FournisseurStatistics
                {
                    TotalFournisseurs = allFournisseurs.Count,
                    ActiveFournisseurs = allFournisseurs.Count(f => f.Statut == "Actif"),
                    InactiveFournisseurs = allFournisseurs.Count(f => f.Statut != "Actif"),
                    FournisseursWithEmail = allFournisseurs.Count(f => !string.IsNullOrWhiteSpace(f.Email)),
                    FournisseursWithRIB = allFournisseurs.Count(f => !string.IsNullOrWhiteSpace(f.RIB))
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting fournisseur statistics: {ex.Message}");
                return new FournisseurStatistics();
            }
        }
    }

    // Helper class for statistics
    public class FournisseurStatistics
    {
        public int TotalFournisseurs { get; set; }
        public int ActiveFournisseurs { get; set; }
        public int InactiveFournisseurs { get; set; }
        public int FournisseursWithEmail { get; set; }
        public int FournisseursWithRIB { get; set; }
    }
}

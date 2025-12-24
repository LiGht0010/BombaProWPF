using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services
{
    public class AchatService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.Achats;

        public AchatService()
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
        public async Task<List<AchatDto>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);

                if (!response.IsSuccessStatusCode)
                    return new List<AchatDto>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AchatDto>>(json) ?? new List<AchatDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching achats: {ex.Message}");
                return new List<AchatDto>();
            }
        }

        // ============================
        // GET BY ID
        // ============================
        public async Task<AchatDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AchatDto>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching achat: {ex.Message}");
                return null;
            }
        }

        // ============================
        // CREATE
        // ============================
        public async Task<AchatDto?> CreateAsync(AchatDto dto)
        {
            try
            {
                // Set the user who created this achat
                dto.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                dto.DateCreation = DateTime.UtcNow;

                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BaseUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {error}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AchatDto>(responseJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create error: {ex.Message}");
                return null;
            }
        }

        // ============================
        // UPDATE
        // ============================
        public async Task<bool> UpdateAsync(AchatDto dto)
        {
            try
            {
                // Set the user who modified this achat
                dto.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                dto.DateModification = DateTime.UtcNow;

                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Update failed ({response.StatusCode}): {err}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update error: {ex.Message}");
                return false;
            }
        }

        // ============================
        // DELETE
        // ============================
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete error: {ex.Message}");
                return false;
            }
        }

        // ============================
        // SERVER-SIDE FILTER ENDPOINTS
        // ============================

        public async Task<List<AchatDto>> GetByFournisseurIdAsync(int fournisseurId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/fournisseur/{fournisseurId}");

                if (!response.IsSuccessStatusCode)
                    return new List<AchatDto>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AchatDto>>(json) ?? new List<AchatDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching achats by fournisseur: {ex.Message}");
                return new List<AchatDto>();
            }
        }

        public async Task<List<AchatDto>> GetByProduitIdAsync(int produitId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/produit/{produitId}");

                if (!response.IsSuccessStatusCode)
                    return new List<AchatDto>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AchatDto>>(json) ?? new List<AchatDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching achats by produit: {ex.Message}");
                return new List<AchatDto>();
            }
        }

        public async Task<List<AchatDto>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/daterange?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

                if (!response.IsSuccessStatusCode)
                    return new List<AchatDto>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AchatDto>>(json) ?? new List<AchatDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching achats by date range: {ex.Message}");
                return new List<AchatDto>();
            }
        }

        // ============================
        // BUSINESS LOGIC FUNCTIONS
        // ============================

        public async Task<bool> AchatNumberExistsAsync(string numero, int? excludeAchatId = null)
        {
            var achats = await GetAllAsync();
            return achats.Any(a =>
                a.Numero != null &&
                a.Numero.Equals(numero, StringComparison.OrdinalIgnoreCase) &&
                a.ID != excludeAchatId);
        }

        public async Task<List<AchatDto>> GetByChauffeurIdAsync(int chauffeurId)
        {
            var achats = await GetAllAsync();
            return achats.Where(a => a.ChauffeurID == chauffeurId).ToList();
        }

        public async Task<List<AchatDto>> GetByCamionIdAsync(int camionId)
        {
            var achats = await GetAllAsync();
            return achats.Where(a => a.CamionID == camionId).ToList();
        }

        public async Task<List<AchatDto>> GetDefectiveDeliveriesAsync()
        {
            var achats = await GetAllAsync();
            return achats
                .Where(a => a.LivraisonDefectueuse == true)
                .OrderByDescending(a => a.Date)
                .ToList();
        }

        public async Task<decimal> GetTotalCostByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            var achats = await GetByDateRangeAsync(startDate, endDate);
            return achats
                .Where(a => a.Cout.HasValue)
                .Sum(a => a.Cout!.Value);
        }

        public async Task<int> GetTotalQuantityByProduitAsync(int produitId)
        {
            var achats = await GetByProduitIdAsync(produitId);
            return achats
                .Where(a => a.Quantite.HasValue)
                .Sum(a => a.Quantite!.Value);
        }

        public async Task<List<AchatDto>> GetRecentAchatsAsync(int count = 10)
        {
            var achats = await GetAllAsync();
            return achats
                .OrderByDescending(a => a.Date)
                .Take(count)
                .ToList();
        }

        public async Task<Dictionary<string, decimal>> GetTotalCostByFournisseurAsync()
        {
            var achats = await GetAllAsync();
            return achats
                .Where(a => a.FournisseurNom != null && a.Cout.HasValue)
                .GroupBy(a => a.FournisseurNom!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.Cout!.Value));
        }

        public async Task<string> GenerateNextNumeroAsync()
        {
            var achats = await GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var prefix = $"ACH-{today:yyyyMMdd}-";

            var todayNumbers = achats
                .Where(a => a.Numero != null && a.Numero.StartsWith(prefix))
                .Select(a =>
                {
                    var suffix = a.Numero!.Replace(prefix, "");
                    return int.TryParse(suffix, out var num) ? num : 0;
                })
                .ToList();

            var nextNumber = todayNumbers.Count > 0 ? todayNumbers.Max() + 1 : 1;
            return $"{prefix}{nextNumber:D3}";
        }
    }
}
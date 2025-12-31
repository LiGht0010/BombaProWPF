using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services
{
    public class EmployeService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.Employes;

        public EmployeService()
        {
            _httpClient = HttpClientFactory.Create();
        }

        // Get all employes
        public async Task<List<EmployeDto>> GetAllEmployesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<EmployeDto>>(json) ?? new List<EmployeDto>();
                }
                return new List<EmployeDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employes: {ex.Message}");
                return new List<EmployeDto>();
            }
        }

        // Get employe by ID
        public async Task<EmployeDto?> GetEmployeByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EmployeDto>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employe: {ex.Message}");
                return null;
            }
        }

        // Get employes by poste
        public async Task<List<EmployeDto>> GetEmployesByPosteAsync(string poste)
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                if (string.IsNullOrWhiteSpace(poste))
                {
                    return allEmployes;
                }
                return allEmployes.Where(e => e.Poste?.Equals(poste, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employes by poste: {ex.Message}");
                return new List<EmployeDto>();
            }
        }

        // Get employes with salary range
        public async Task<List<EmployeDto>> GetEmployesBySalaryRangeAsync(decimal minSalary, decimal maxSalary)
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                return allEmployes.Where(e => 
                    e.Salaire.HasValue && 
                    e.Salaire.Value >= minSalary && 
                    e.Salaire.Value <= maxSalary).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employes by salary range: {ex.Message}");
                return new List<EmployeDto>();
            }
        }

        // Create new employe
        public async Task<EmployeDto?> CreateEmployeAsync(EmployeDto employe)
        {
            try
            {
                // Set the user who created this employe
                employe.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                employe.DateCreation = DateTime.UtcNow;

                var json = JsonConvert.SerializeObject(employe);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EmployeDto>(responseJson);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating employe: {ex.Message}");
                return null;
            }
        }

        // Update existing employe
        public async Task<bool> UpdateEmployeAsync(EmployeDto employe)
        {
            try
            {
                // Set the user who modified this employe
                employe.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                employe.DateModification = DateTime.UtcNow;

                var json = JsonConvert.SerializeObject(employe);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{employe.ID}", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating employe: {ex.Message}");
                return false;
            }
        }

        // Delete employe
        public async Task<bool> DeleteEmployeAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting employe: {ex.Message}");
                return false;
            }
        }

        // Search employes
        public async Task<List<EmployeDto>> SearchEmployesAsync(string searchTerm)
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return allEmployes;
                }

                searchTerm = searchTerm.ToLower();
                return allEmployes.Where(e =>
                    (e.Nom?.ToLower().Contains(searchTerm) ?? false) ||
                    (e.Prenom?.ToLower().Contains(searchTerm) ?? false) ||
                    (e.CIN?.ToLower().Contains(searchTerm) ?? false) ||
                    (e.Telephone?.ToLower().Contains(searchTerm) ?? false) ||
                    (e.Poste?.ToLower().Contains(searchTerm) ?? false) ||
                    (e.Address?.ToLower().Contains(searchTerm) ?? false))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching employes: {ex.Message}");
                return new List<EmployeDto>();
            }
        }

        // Check if CIN already exists
        public async Task<bool> CINExistsAsync(string cin, int? excludeEmployeId = null)
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                return allEmployes.Any(e => 
                    e.CIN?.Equals(cin, StringComparison.OrdinalIgnoreCase) == true && 
                    e.ID != excludeEmployeId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking CIN existence: {ex.Message}");
                return false;
            }
        }

        // Get employe statistics
        public async Task<EmployeStatistics> GetEmployeStatisticsAsync()
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                return new EmployeStatistics
                {
                    TotalEmployes = allEmployes.Count,
                    EmployesWithSalary = allEmployes.Count(e => e.Salaire.HasValue),
                    TotalSalaryExpense = allEmployes.Where(e => e.Salaire.HasValue).Sum(e => e.Salaire.Value),
                    AverageSalary = allEmployes.Where(e => e.Salaire.HasValue).Any() 
                        ? allEmployes.Where(e => e.Salaire.HasValue).Average(e => e.Salaire.Value) 
                        : 0,
                    EmployesByPoste = allEmployes
                        .Where(e => !string.IsNullOrWhiteSpace(e.Poste))
                        .GroupBy(e => e.Poste)
                        .ToDictionary(g => g.Key!, g => g.Count())
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employe statistics: {ex.Message}");
                return new EmployeStatistics();
            }
        }

        // Check if employe has related records
        public async Task<bool> HasRelatedRecordsAsync(int employeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{employeId}/hasrelatedrecords");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking related records: {ex.Message}");
                return false;
            }
        }

        // Get employes with credit balance
        public async Task<List<EmployeDto>> GetEmployesWithCreditAsync()
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                return allEmployes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employes with credit: {ex.Message}");
                return new List<EmployeDto>();
            }
        }

        // Get employes who witnessed jaugeages
        public async Task<List<EmployeDto>> GetEmployesWithJaugeagesAsync()
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                return allEmployes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employes with jaugeages: {ex.Message}");
                return new List<EmployeDto>();
            }
        }

        // Get unique postes
        public async Task<List<string>> GetUniquePostesAsync()
        {
            try
            {
                var allEmployes = await GetAllEmployesAsync();
                return allEmployes
                    .Where(e => !string.IsNullOrWhiteSpace(e.Poste))
                    .Select(e => e.Poste!)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching unique postes: {ex.Message}");
                return new List<string>();
            }
        }
    }

    // Helper class for statistics
    public class EmployeStatistics
    {
        public int TotalEmployes { get; set; }
        public int EmployesWithSalary { get; set; }
        public decimal TotalSalaryExpense { get; set; }
        public decimal AverageSalary { get; set; }
        public Dictionary<string, int> EmployesByPoste { get; set; } = new Dictionary<string, int>();
    }
}

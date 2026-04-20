using BombaProMaxWPF;
using BombaProMaxWPF.Models;  // PompeDto namespace
using Newtonsoft.Json;
using System.Text;

namespace BombaProMaxWPF.Services
{
    public class PompeService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.Pompes;

        public PompeService()
        {
            _httpClient = HttpClientFactory.Create();
        }

        // ============================
        // GET ALL
        // ============================
        public async Task<List<PompeDto>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);

                if (!response.IsSuccessStatusCode)
                    return new List<PompeDto>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PompeDto>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pompes: {ex.Message}");
                return new List<PompeDto>();
            }
        }

        // ============================
        // GET BY ID
        // ============================
        public async Task<PompeDto> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PompeDto>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pompe: {ex.Message}");
                return null;
            }
        }

        // ============================
        // CREATE
        // ============================
        public async Task<PompeDto> CreateAsync(PompeDto dto)
        {
            try
            {
                dto.AjoutePar =
                    App.CurrentUser?.UserId ??
                    App.user?.UserId ??
                    5;

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
                return JsonConvert.DeserializeObject<PompeDto>(responseJson);
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
        public async Task<bool> UpdateAsync(PompeDto dto)
        {
            try
            {
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
        // BUSINESS LOGIC FUNCTIONS
        // ============================

        public async Task<bool> PompeNumberExistsAsync(string numero, int? excludePompeId = null)
        {
            var pompes = await GetAllAsync();
            return pompes.Any(p =>
                p.Numero.Equals(numero, StringComparison.OrdinalIgnoreCase) &&
                p.ID != excludePompeId);
        }

        public async Task<List<string>> GetUniquePompeNumbersAsync()
        {
            var pompes = await GetAllAsync();
            return pompes
                .Where(p => !string.IsNullOrWhiteSpace(p.Numero))
                .Select(p => p.Numero)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }

        public async Task<List<PompeDto>> GetByReservoirIdAsync(int reservoirId)
        {
            var pompes = await GetAllAsync();
            return pompes.Where(p => p.ReservoirAssocieID == reservoirId).ToList();
        }

        public async Task<List<PompeDto>> GetByStatusAsync(string statut)
        {
            var pompes = await GetAllAsync();
            return pompes.Where(p =>
                p.Statut.Equals(statut, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<bool> UpdateElectronicMeterAsync(int pompeId, decimal newReading)
        {
            var pompe = await GetByIdAsync(pompeId);
            if (pompe == null)
                return false;

            if (pompe.CompteurElectroniqueActuel.HasValue &&
                newReading < pompe.CompteurElectroniqueActuel.Value)
                return false;

            pompe.CompteurElectroniqueActuel = newReading;
            return await UpdateAsync(pompe);
        }

        public async Task<bool> UpdateMechanicalMeterAsync(int pompeId, decimal newReading)
        {
            var pompe = await GetByIdAsync(pompeId);
            if (pompe == null)
                return false;

            if (pompe.CompteurMecaniqueActuel.HasValue &&
                newReading < pompe.CompteurMecaniqueActuel.Value)
                return false;

            pompe.CompteurMecaniqueActuel = newReading;
            return await UpdateAsync(pompe);
        }

        public async Task<Dictionary<int, decimal>> GetMeterDiscrepanciesAsync()
        {
            var pompes = await GetAllAsync();
            var result = new Dictionary<int, decimal>();

            foreach (var p in pompes)
            {
                if (p.CompteurElectroniqueActuel.HasValue &&
                    p.CompteurMecaniqueActuel.HasValue)
                {
                    var diff = Math.Abs(
                        p.CompteurElectroniqueActuel.Value -
                        p.CompteurMecaniqueActuel.Value);

                    if (diff > 0)
                        result[p.ID] = diff;
                }
            }

            return result;
        }

        public async Task<bool> UpdateStatusAsync(int pompeId, string newStatus)
        {
            var pompe = await GetByIdAsync(pompeId);
            if (pompe == null)
                return false;

            pompe.Statut = newStatus;
            return await UpdateAsync(pompe);
        }

        public async Task<List<PompeDto>> GetWithHighDiscrepancyAsync(decimal threshold = 10m)
        {
            var pompes = await GetAllAsync();

            return pompes
                .Where(p =>
                    p.CompteurElectroniqueActuel.HasValue &&
                    p.CompteurMecaniqueActuel.HasValue &&
                    Math.Abs(
                        p.CompteurElectroniqueActuel.Value -
                        p.CompteurMecaniqueActuel.Value) > threshold)
                .OrderByDescending(p =>
                    Math.Abs(
                        p.CompteurElectroniqueActuel.Value -
                        p.CompteurMecaniqueActuel.Value))
                .ToList();
        }
    }
}

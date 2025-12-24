using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services
{
    public class ChauffeurService
    {
        private readonly HttpClient _httpClient;
        private static string BaseUrl => ApiConfig.Chauffeurs;

        public ChauffeurService()
        {
            // Create handler that ignores SSL certificate errors for development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<List<ChauffeurDto>> GetAllChauffeursAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ChauffeurDto>>(json) ?? new List<ChauffeurDto>();
                }
                return new List<ChauffeurDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chauffeurs: {ex.Message}");
                return new List<ChauffeurDto>();
            }
        }

        public async Task<ChauffeurDto?> GetChauffeurByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ChauffeurDto>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chauffeur: {ex.Message}");
                return null;
            }
        }

        public async Task<ChauffeurDto?> CreateChauffeurAsync(ChauffeurDto chauffeur)
        {
            try
            {
                // Set the user who created this chauffeur
                chauffeur.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                chauffeur.DateCreation = DateTime.UtcNow;

                var json = JsonConvert.SerializeObject(chauffeur);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ChauffeurDto>(responseJson);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating chauffeur: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateChauffeurAsync(ChauffeurDto chauffeur)
        {
            try
            {
                // Set the user who modified this chauffeur
                chauffeur.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
                chauffeur.DateModification = DateTime.UtcNow;

                var json = JsonConvert.SerializeObject(chauffeur);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{chauffeur.ID}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating chauffeur: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteChauffeurAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting chauffeur: {ex.Message}");
                return false;
            }
        }

        public async Task<List<ChauffeurDto>> GetChauffeursByFournisseurAsync(int fournisseurId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/fournisseur/{fournisseurId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ChauffeurDto>>(json) ?? new List<ChauffeurDto>();
                }
                return new List<ChauffeurDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chauffeurs by fournisseur: {ex.Message}");
                return new List<ChauffeurDto>();
            }
        }
    }
}

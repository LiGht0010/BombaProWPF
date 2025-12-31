using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

public class ClientService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Clients;

    public ClientService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<ClientDto>> GetAllClientsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ClientDto>>(json) ?? new List<ClientDto>();
            }
            return new List<ClientDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching clients: {ex.Message}");
            return new List<ClientDto>();
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ClientDto?> GetClientByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ClientDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching client: {ex.Message}");
            return null;
        }
    }

    // ============================
    // SEARCH (Server-side)
    // ============================
    public async Task<List<ClientDto>> SearchClientsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllClientsAsync();
            }

            var response = await _httpClient.GetAsync($"{BaseUrl}/search?term={Uri.EscapeDataString(searchTerm)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ClientDto>>(json) ?? new List<ClientDto>();
            }
            // Fallback to client-side search
            var allClients = await GetAllClientsAsync();
            var lowerTerm = searchTerm.ToLower();
            return allClients.Where(c =>
                (c.Nom?.ToLower().Contains(lowerTerm) ?? false) ||
                (c.NumeroClient?.ToLower().Contains(lowerTerm) ?? false) ||
                (c.Contact?.ToLower().Contains(lowerTerm) ?? false) ||
                (c.NomSociete?.ToLower().Contains(lowerTerm) ?? false) ||
                (c.CIN?.ToLower().Contains(lowerTerm) ?? false))
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching clients: {ex.Message}");
            return new List<ClientDto>();
        }
    }

    // ============================
    // CHECK NUMERO EXISTS (Server-side)
    // ============================
    public async Task<bool> ClientNumberExistsAsync(string numero, int? excludeClientId = null)
    {
        try
        {
            var url = $"{BaseUrl}/check-numero?numero={Uri.EscapeDataString(numero)}";
            if (excludeClientId.HasValue)
            {
                url += $"&excludeId={excludeClientId.Value}";
            }
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            // Fallback to client-side check
            var clients = await GetAllClientsAsync();
            return clients.Any(c =>
                c.NumeroClient.Equals(numero, StringComparison.OrdinalIgnoreCase) &&
                c.ID != excludeClientId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking client number: {ex.Message}");
            return false;
        }
    }

    // ============================
    // CHECK CIN EXISTS (Server-side)
    // ============================
    public async Task<bool> ClientCINExistsAsync(string cin, int? excludeClientId = null)
    {
        try
        {
            var url = $"{BaseUrl}/check-cin?cin={Uri.EscapeDataString(cin)}";
            if (excludeClientId.HasValue)
            {
                url += $"&excludeId={excludeClientId.Value}";
            }
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<bool>(json);
            }
            // Fallback to client-side check
            var clients = await GetAllClientsAsync();
            return clients.Any(c =>
                c.CIN.Equals(cin, StringComparison.OrdinalIgnoreCase) &&
                c.ID != excludeClientId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking client CIN: {ex.Message}");
            return false;
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ClientDto?> CreateClientAsync(ClientDto client)
    {
        try
        {
            // Set audit fields
            client.userID = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            client.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ClientDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating client: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateClientAsync(ClientDto client)
    {
        try
        {
            // Set audit fields
            client.userID = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            client.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{client.ID}", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating client: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteClientAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting client: {ex.Message}");
            return false;
        }
    }

    // ============================
    // GET CREDIT BALANCE
    // ============================
    public async Task<BilanCreditDto?> GetClientCreditBalanceAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{clientId}/credit-balance");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BilanCreditDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching client credit balance: {ex.Message}");
            return null;
        }
    }
}
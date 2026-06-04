using FourniPro.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace FourniPro.Services;

public class ClientService
{
    private readonly HttpClient _httpClient = HttpClientFactory.Create();
    private static string BaseUrl => ApiConfig.Clients;

    // ============================
    // GET ALL
    // ============================
    public async Task<List<ClientDto>> GetAllClientsAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<ClientDto>>(BaseUrl);
            return list ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ClientService] GetAll error: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ClientDto?> GetClientByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ClientDto>($"{BaseUrl}/{id}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ClientService] GetById error: {ex.Message}");
            return null;
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ClientDto?> CreateClientAsync(ClientDto client)
    {
        try
        {
            client.AjoutePar = App.CurrentUser?.UserId;
            client.DateCreation = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[ClientService] Create failed: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ClientDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ClientService] Create error: {ex.Message}");
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
            client.ModifiePar = App.CurrentUser?.UserId;
            client.DateModification = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{client.ClientId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ClientService] Update error: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{clientId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ClientService] Delete error: {ex.Message}");
            return false;
        }
    }
}

using BombaProMax.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing client credit payments (reglements).
/// Automatically triggers bilan recalculation after mutations.
/// </summary>
public class ReglementCreditService
{
    private readonly HttpClient _httpClient;
    private readonly BilanCreditService _bilanService;
    private static string BaseUrl => ApiConfig.ReglementCredits;

    public ReglementCreditService()
    {
        _httpClient = HttpClientFactory.Create();
        _bilanService = new BilanCreditService();
    }

    // ============================
    // GET ALL
    // ============================
    public async Task<List<ReglementCreditDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReglementCreditDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ReglementCreditService] Error fetching reglements: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<ReglementCreditDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ReglementCreditDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ReglementCreditService] Error fetching reglement {id}: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY CLIENT
    // ============================
    public async Task<List<ReglementCreditDto>> GetByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReglementCreditDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ReglementCreditService] Error fetching reglements for client {clientId}: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET TOTAL BY CLIENT
    // ============================
    public async Task<decimal> GetTotalByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}/total");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<decimal>(json);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ReglementCreditService] Error fetching total reglements for client {clientId}: {ex.Message}");
            return 0;
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<ReglementCreditDto?> CreateAsync(ReglementCreditDto dto)
    {
        // Set audit fields
        dto.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
        dto.DateCreation = DateTime.UtcNow;

        // Set validator
        dto.ValidePar = App.CurrentUser?.Name ?? App.user?.Name ?? "System";

        // Ensure DateReglement is set and in UTC
        if (dto.DateReglement == default)
        {
            dto.DateReglement = DateTime.UtcNow;
        }
        else
        {
            // Convert to UTC if not already
            dto.DateReglement = DateTime.SpecifyKind(dto.DateReglement, DateTimeKind.Utc);
        }

        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[ReglementCreditService] Sending POST: {json}");
        
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(BaseUrl, content);

        var responseBody = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[ReglementCreditService] Response Status: {response.StatusCode}");
        Debug.WriteLine($"[ReglementCreditService] Response Body: {responseBody}");

        if (response.IsSuccessStatusCode)
        {
            var created = JsonConvert.DeserializeObject<ReglementCreditDto>(responseBody);

            // Recalculate bilan after creating payment
            if (created != null)
            {
                await _bilanService.RecalculateAsync(created.ClientID);
            }

            return created;
        }
        else
        {
            Debug.WriteLine($"[ReglementCreditService] Error creating reglement: {response.StatusCode} - {responseBody}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateAsync(ReglementCreditDto dto)
    {
        // Set audit fields
        dto.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
        dto.DateModification = DateTime.UtcNow;

        // Update validator
        dto.ValidePar = App.CurrentUser?.Name ?? App.user?.Name ?? dto.ValidePar;

        // Ensure DateReglement is in UTC
        dto.DateReglement = DateTime.SpecifyKind(dto.DateReglement, DateTimeKind.Utc);
        
        if (dto.DateCreation.HasValue)
        {
            dto.DateCreation = DateTime.SpecifyKind(dto.DateCreation.Value, DateTimeKind.Utc);
        }

        var json = JsonConvert.SerializeObject(dto);
        Debug.WriteLine($"[ReglementCreditService] Sending PUT: {json}");
        
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ReglementID}", content);

        var responseBody = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"[ReglementCreditService] Response Status: {response.StatusCode}");
        Debug.WriteLine($"[ReglementCreditService] Response Body: {responseBody}");

        if (response.IsSuccessStatusCode)
        {
            // Recalculate bilan after updating payment
            await _bilanService.RecalculateAsync(dto.ClientID);
            return true;
        }
        else
        {
            Debug.WriteLine($"[ReglementCreditService] Error updating reglement: {response.StatusCode} - {responseBody}");
            return false;
        }
    }

    // ============================
    // DELETE
    // ============================
    public async Task<bool> DeleteAsync(int id)
    {
        // Get the reglement first to know which client's bilan to recalculate
        var reglement = await GetByIdAsync(id);
        if (reglement == null)
        {
            Debug.WriteLine($"[ReglementCreditService] Reglement {id} not found for deletion");
            return false;
        }

        Debug.WriteLine($"[ReglementCreditService] Sending DELETE for reglement {id}");
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

        Debug.WriteLine($"[ReglementCreditService] Response Status: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            // Recalculate bilan after deleting payment
            await _bilanService.RecalculateAsync(reglement.ClientID);
            return true;
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[ReglementCreditService] Error deleting reglement: {response.StatusCode} - {responseBody}");
            return false;
        }
    }

    // ============================
    // GET BY DATE RANGE
    // ============================
    public async Task<List<ReglementCreditDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var reglements = await GetAllAsync();
        return reglements
            .Where(r => r.DateReglement >= startDate && r.DateReglement <= endDate)
            .OrderByDescending(r => r.DateReglement)
            .ToList();
    }

    // ============================
    // GET BY PAYMENT MODE
    // ============================
    public async Task<List<ReglementCreditDto>> GetByPaymentModeAsync(int modePaiementId)
    {
        var reglements = await GetAllAsync();
        return reglements
            .Where(r => r.ModePaiementID == modePaiementId)
            .OrderByDescending(r => r.DateReglement)
            .ToList();
    }

    // ============================
    // GET RECENT PAYMENTS
    // ============================
    public async Task<List<ReglementCreditDto>> GetRecentAsync(int count = 10)
    {
        var reglements = await GetAllAsync();
        return reglements
            .OrderByDescending(r => r.DateReglement)
            .Take(count)
            .ToList();
    }

    // ============================
    // GET TOTAL PAYMENTS BY DATE RANGE
    // ============================
    public async Task<decimal> GetTotalByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var reglements = await GetByDateRangeAsync(startDate, endDate);
        return reglements.Sum(r => r.MontantPaye);
    }
}

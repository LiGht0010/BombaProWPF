using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BombaProMaxWPF.Models;
using Newtonsoft.Json;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Service for managing client credit balance (BilanCredit).
/// This is a lightweight service focused on reading and recalculating balances.
/// </summary>
public class BilanCreditService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.BilanCredits;

    public BilanCreditService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    /// <summary>
    /// Gets the credit balance for a specific client.
    /// </summary>
    public async Task<BilanCreditDto?> GetByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BilanCreditDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching bilan for client {clientId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all credit balances.
    /// </summary>
    public async Task<List<BilanCreditDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BilanCreditDto>>(json) ?? new List<BilanCreditDto>();
            }
            return new List<BilanCreditDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all bilans: {ex.Message}");
            return new List<BilanCreditDto>();
        }
    }

    /// <summary>
    /// Recalculates the credit balance for a specific client based on transactions and payments.
    /// This should be called after any transaction or payment is created/updated/deleted.
    /// </summary>
    public async Task<BilanCreditDto?> RecalculateAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/recalculate/{clientId}", null);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BilanCreditDto>(json);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error recalculating bilan: {response.StatusCode} - {error}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recalculating bilan for client {clientId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets clients with outstanding balance (Balance > 0).
    /// </summary>
    public async Task<List<BilanCreditDto>> GetClientsWithOutstandingBalanceAsync()
    {
        var bilans = await GetAllAsync();
        return bilans.Where(b => b.Balance > 0).OrderByDescending(b => b.Balance).ToList();
    }

    /// <summary>
    /// Gets total outstanding balance across all clients.
    /// </summary>
    public async Task<decimal> GetTotalOutstandingBalanceAsync()
    {
        var bilans = await GetAllAsync();
        return bilans.Sum(b => b.Balance);
    }
}

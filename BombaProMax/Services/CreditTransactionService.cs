using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

/// <summary>
/// Service for managing client credit transactions.
/// Automatically triggers bilan recalculation after mutations.
/// </summary>
public class CreditTransactionService
{
    private readonly HttpClient _httpClient;
    private readonly BilanCreditService _bilanService;
    private static string BaseUrl => ApiConfig.CreditTransactions;

    public CreditTransactionService()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
        _bilanService = new BilanCreditService();
    }

    // ════════════════════════════════════════════════════════════════
    // GET ALL
    // ════════════════════════════════════════════════════════════════
    public async Task<List<CreditTransactionDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CreditTransactionDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching credit transactions: {ex.Message}");
            return [];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET BY ID
    // ════════════════════════════════════════════════════════════════
    public async Task<CreditTransactionDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CreditTransactionDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching transaction {id}: {ex.Message}");
            return null;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET BY CLIENT
    // ════════════════════════════════════════════════════════════════
    public async Task<List<CreditTransactionDto>> GetByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CreditTransactionDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching transactions for client {clientId}: {ex.Message}");
            return [];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET NON-INVOICED BY CLIENT
    // ════════════════════════════════════════════════════════════════
    public async Task<List<CreditTransactionDto>> GetNonInvoicedByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}/non-invoiced");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CreditTransactionDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching non-invoiced transactions for client {clientId}: {ex.Message}");
            return [];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET AVAILABLE BY CLIENT (⭐ NEW - not in BL and not invoiced)
    // ════════════════════════════════════════════════════════════════
    public async Task<List<CreditTransactionDto>> GetAvailableByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}/available");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CreditTransactionDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching available transactions for client {clientId}: {ex.Message}");
            return [];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET IN-BL BY CLIENT (⭐ NEW - in BL but not yet invoiced)
    // ════════════════════════════════════════════════════════════════
    public async Task<List<CreditTransactionDto>> GetInBLByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}/in-bl");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CreditTransactionDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching in-BL transactions for client {clientId}: {ex.Message}");
            return [];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET INVOICED BY CLIENT (⭐ NEW - already invoiced)
    // ════════════════════════════════════════════════════════════════
    public async Task<List<CreditTransactionDto>> GetInvoicedByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}/invoiced");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CreditTransactionDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching invoiced transactions for client {clientId}: {ex.Message}");
            return [];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET TOTAL BY CLIENT
    // ════════════════════════════════════════════════════════════════
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
            Console.WriteLine($"Error fetching total for client {clientId}: {ex.Message}");
            return 0;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET NON-INVOICED TOTAL BY CLIENT
    // ════════════════════════════════════════════════════════════════
    public async Task<decimal> GetNonInvoicedTotalByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}/non-invoiced/total");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<decimal>(json);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching non-invoiced total for client {clientId}: {ex.Message}");
            return 0;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GET AVAILABLE TOTAL BY CLIENT (⭐ NEW)
    // ════════════════════════════════════════════════════════════════
    public async Task<decimal> GetAvailableTotalByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}/available/total");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<decimal>(json);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching available total for client {clientId}: {ex.Message}");
            return 0;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE
    // ════════════════════════════════════════════════════════════════
    public async Task<CreditTransactionDto?> CreateAsync(CreditTransactionDto dto)
    {
        try
        {
            // Set audit fields
            dto.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            dto.DateCreation = DateTime.UtcNow;

            // Set default date if not provided
            if (dto.DateCredit == default)
            {
                dto.DateCredit = DateTime.UtcNow;
            }

            // Calculate MontantTotal
            if (dto.MontantTotal == 0)
            {
                dto.MontantTotal = dto.PrixTTC * dto.Quantite;
            }

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            // Always read the response body
            var responseBody = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[CreateAsync] Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[CreateAsync] Response Body: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                var created = JsonConvert.DeserializeObject<CreditTransactionDto>(responseBody);

                // Recalculate bilan after creating transaction
                if (created != null)
                {
                    await _bilanService.RecalculateAsync(created.ClientID);
                }

                return created;
            }
            else
            {
                // Log the error response body which now contains detailed error from API
                Console.WriteLine($"Error creating transaction: {response.StatusCode} - {responseBody}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating transaction: {ex.Message}");
            return null;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // UPDATE
    // ════════════════════════════════════════════════════════════════
    public async Task<bool> UpdateAsync(CreditTransactionDto dto)
    {
        try
        {
            // Set audit fields
            dto.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;
            dto.DateModification = DateTime.UtcNow;

            // Recalculate MontantTotal
            dto.MontantTotal = dto.PrixTTC * dto.Quantite;

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.CreditID}", content);

            if (response.IsSuccessStatusCode)
            {
                // Recalculate bilan after updating transaction
                await _bilanService.RecalculateAsync(dto.ClientID);
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error updating transaction: {response.StatusCode} - {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating transaction: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // DELETE
    // ════════════════════════════════════════════════════════════════
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            // Get the transaction first to know which client's bilan to recalculate
            var transaction = await GetByIdAsync(id);
            if (transaction == null)
            {
                return false;
            }

            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Recalculate bilan after deleting transaction
                await _bilanService.RecalculateAsync(transaction.ClientID);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting transaction {id}: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // MARK AS INVOICED
    // ════════════════════════════════════════════════════════════════
    public async Task<bool> MarkAsInvoicedAsync(int transactionId, int factureId)
    {
        try
        {
            var response = await _httpClient.PutAsync(
                $"{BaseUrl}/{transactionId}/mark-invoiced/{factureId}",
                null);

            if (response.IsSuccessStatusCode)
            {
                // Get transaction to recalculate bilan
                var transaction = await GetByIdAsync(transactionId);
                if (transaction != null)
                {
                    await _bilanService.RecalculateAsync(transaction.ClientID);
                }
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error marking transaction as invoiced: {response.StatusCode} - {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking transaction {transactionId} as invoiced: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // MARK AS IN BL (⭐ NEW)
    // ════════════════════════════════════════════════════════════════
    public async Task<bool> MarkAsInBLAsync(int transactionId, int bonLivraisonId)
    {
        try
        {
            var response = await _httpClient.PutAsync(
                $"{BaseUrl}/{transactionId}/mark-in-bl/{bonLivraisonId}",
                null);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking transaction {transactionId} as in BL: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // GENERATE TRANSACTION NUMBER
    // ════════════════════════════════════════════════════════════════
    public async Task<string> GenerateNextNumeroAsync()
    {
        var transactions = await GetAllAsync();
        var today = DateTime.UtcNow;
        var prefix = $"CRD-{today:yyyyMMdd}-";

        var todayNumbers = transactions
            .Where(t => t.NumeroTransaction != null && t.NumeroTransaction.StartsWith(prefix))
            .Select(t =>
            {
                var suffix = t.NumeroTransaction!.Replace(prefix, "");
                return int.TryParse(suffix, out var num) ? num : 0;
            })
            .ToList();

        var nextNumber = todayNumbers.Count > 0 ? todayNumbers.Max() + 1 : 1;
        return $"{prefix}{nextNumber:D3}";
    }
}

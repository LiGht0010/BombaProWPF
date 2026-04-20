using BombaProMaxWPF.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMaxWPF.Services;

public class FactureService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Factures;
    private static string ElementsBaseUrl => $"{ApiConfig.BaseUrl}/ElementsFacture";
    private static string FactureBLBaseUrl => $"{ApiConfig.BaseUrl}/FactureBonLivraison";

    public FactureService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    // ════════════════════════════════════════════════════════════════
    // FACTURE CRUD
    // ════════════════════════════════════════════════════════════════

    // ============================
    // GET ALL
    // ============================
    public async Task<List<FactureDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FactureDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching factures: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<FactureDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FactureDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching facture: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY ID WITH DETAILS
    // ============================
    public async Task<FactureDto?> GetByIdWithDetailsAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}/details");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FactureDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching facture with details: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY NUMERO
    // ============================
    public async Task<FactureDto?> GetByNumeroAsync(string numeroFacture)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/numero/{Uri.EscapeDataString(numeroFacture)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FactureDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching facture by numero: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY CLIENT
    // ============================
    public async Task<List<FactureDto>> GetByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FactureDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching factures by client: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY STATUT
    // ============================
    public async Task<List<FactureDto>> GetByStatutAsync(string statut)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/statut/{Uri.EscapeDataString(statut)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FactureDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching factures by statut: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY DATE RANGE
    // ============================
    public async Task<List<FactureDto>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/date-range?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FactureDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching factures by date range: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET NEXT NUMERO
    // ============================
    public async Task<string> GetNextNumeroAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/next-numero");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(json) ?? $"FAC-{DateTime.Now.Year}-00001";
            }
            return $"FAC-{DateTime.Now.Year}-00001";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching next numero: {ex.Message}");
            return $"FAC-{DateTime.Now.Year}-00001";
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<FactureDto?> CreateAsync(FactureDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FactureDto>(responseJson);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating facture: {error}");
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating facture: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateAsync(FactureDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error updating facture: {error}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating facture: {ex.Message}");
            return false;
        }
    }

    // ============================
    // UPDATE STATUT
    // ============================
    public async Task<bool> UpdateStatutAsync(int id, string statut)
    {
        try
        {
            var json = JsonConvert.SerializeObject(statut);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{id}/statut", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error updating facture statut: {error}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating facture statut: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE (also unlocks linked BLs)
    // ============================
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error deleting facture: {error}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting facture: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE FACTURE FROM CREDIT TRANSACTIONS (⭐ NEW - Direct invoice)
    // ════════════════════════════════════════════════════════════════
    public async Task<CTConversionResultDto> CreateFromCreditTransactionsAsync(CreateFactureFromCTsDto request)
    {
        try
        {
            request.CreatedByUserId = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/from-credit-transactions", content);

            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CTConversionResultDto>(responseJson) 
                    ?? new CTConversionResultDto { Success = false, Message = "Erreur de désérialisation" };
            }
            else
            {
                var errorResult = JsonConvert.DeserializeObject<CTConversionResultDto>(responseJson);
                return errorResult ?? new CTConversionResultDto 
                { 
                    Success = false, 
                    Message = "Erreur lors de la création de la facture",
                    Errors = [responseJson]
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating facture from CTs: {ex.Message}");
            return new CTConversionResultDto
            {
                Success = false,
                Message = "Erreur de connexion",
                Errors = [ex.Message]
            };
        }
    }

    // ════════════════════════════════════════════════════════════════
    // ELEMENTS FACTURE
    // ════════════════════════════════════════════════════════════════

    // ============================
    // GET ELEMENTS BY FACTURE
    // ============================
    public async Task<List<ElementsFactureDto>> GetElementsByFactureAsync(int factureId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ElementsBaseUrl}/facture/{factureId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ElementsFactureDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching elements facture: {ex.Message}");
            return [];
        }
    }

    // ============================
    // ADD ELEMENT
    // ============================
    public async Task<ElementsFactureDto?> AddElementAsync(ElementsFactureDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ElementsBaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ElementsFactureDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding element facture: {ex.Message}");
            return null;
        }
    }

    // ============================
    // ADD ELEMENTS BATCH
    // ============================
    public async Task<List<ElementsFactureDto>> AddElementsBatchAsync(List<ElementsFactureDto> dtos)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dtos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{ElementsBaseUrl}/batch", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ElementsFactureDto>>(responseJson) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding elements facture batch: {ex.Message}");
            return [];
        }
    }

    // ============================
    // UPDATE ELEMENT
    // ============================
    public async Task<bool> UpdateElementAsync(ElementsFactureDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{ElementsBaseUrl}/{dto.ID}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating element facture: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE ELEMENT
    // ============================
    public async Task<bool> DeleteElementAsync(int elementId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{ElementsBaseUrl}/{elementId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting element facture: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE ALL ELEMENTS BY FACTURE
    // ============================
    public async Task<bool> DeleteAllElementsAsync(int factureId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{ElementsBaseUrl}/facture/{factureId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting all elements facture: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // LINKED BLS (via FactureBonLivraison)
    // ════════════════════════════════════════════════════════════════

    // ============================
    // GET LINKED BL RECORDS
    // ============================
    public async Task<List<FactureBonLivraisonDto>> GetLinkedBLsAsync(int factureId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FactureBLBaseUrl}/facture/{factureId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FactureBonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching linked BLs: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET FULL LINKED BLS
    // ============================
    public async Task<List<BonLivraisonDto>> GetFullLinkedBLsAsync(int factureId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FactureBLBaseUrl}/facture/{factureId}/bls");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching full linked BLs: {ex.Message}");
            return [];
        }
    }

    // ════════════════════════════════════════════════════════════════
    // STATISTICS / HELPERS
    // ════════════════════════════════════════════════════════════════

    // ============================
    // GET UNPAID FACTURES
    // ============================
    public async Task<List<FactureDto>> GetUnpaidFacturesAsync()
    {
        return await GetByStatutAsync("Non Payée");
    }

    // ============================
    // GET PAID FACTURES
    // ============================
    public async Task<List<FactureDto>> GetPaidFacturesAsync()
    {
        return await GetByStatutAsync("Payée");
    }

    // ============================
    // MARK AS PAID
    // ============================
    public async Task<bool> MarkAsPaidAsync(int factureId)
    {
        return await UpdateStatutAsync(factureId, "Payée");
    }

    // ============================
    // MARK AS UNPAID
    // ============================
    public async Task<bool> MarkAsUnpaidAsync(int factureId)
    {
        return await UpdateStatutAsync(factureId, "Non Payée");
    }

    // ════════════════════════════════════════════════════════════════
    // MERGE FACTURES (⭐ NEW - Consolidate multiple Factures into one)
    // ════════════════════════════════════════════════════════════════
    public async Task<MergeFacturesResultDto> MergeFacturesAsync(MergeFacturesDto request)
    {
        try
        {
            request.CreatedByUserId = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/merge", content);

            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<MergeFacturesResultDto>(responseJson)
                    ?? new MergeFacturesResultDto { Success = false, Message = "Erreur de désérialisation" };
            }
            else
            {
                var errorResult = JsonConvert.DeserializeObject<MergeFacturesResultDto>(responseJson);
                return errorResult ?? new MergeFacturesResultDto
                {
                    Success = false,
                    Message = "Erreur lors de la fusion des factures",
                    Errors = [responseJson]
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error merging factures: {ex.Message}");
            return new MergeFacturesResultDto
            {
                Success = false,
                Message = "Erreur de connexion",
                Errors = [ex.Message]
            };
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE FACTURE FROM BLs (⭐ BL → Facture conversion)
    // ════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Creates a Facture from selected BonLivraisons.
    /// This is the main workflow: CT → BL → Facture
    /// </summary>
    public async Task<FacturationResultDto> CreateFactureFromBLsAsync(CreateFactureFromBLsDto request)
    {
        try
        {
            request.CreatedByUserId = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/from-bons-livraison", content);

            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<FacturationResultDto>(responseJson)
                    ?? new FacturationResultDto { Success = false, Message = "Erreur de désérialisation" };
            }
            else
            {
                var errorResult = JsonConvert.DeserializeObject<FacturationResultDto>(responseJson);
                return errorResult ?? new FacturationResultDto
                {
                    Success = false,
                    Message = "Erreur lors de la création de la facture",
                    Errors = [responseJson]
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating facture from BLs: {ex.Message}");
            return new FacturationResultDto
            {
                Success = false,
                Message = "Erreur de connexion",
                Errors = [ex.Message]
            };
        }
    }
}

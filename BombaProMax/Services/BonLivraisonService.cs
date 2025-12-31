using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BombaProMax.Models;
using Newtonsoft.Json;
using System.Text;

namespace BombaProMax.Services;

public class BonLivraisonService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.BonLivraisons;
    private static string DetailsBaseUrl => $"{ApiConfig.BaseUrl}/BonLivraisonDetails";
    private static string FacturationBaseUrl => $"{ApiConfig.BaseUrl}/FactureBonLivraison";

    public BonLivraisonService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    // ════════════════════════════════════════════════════════════════
    // BON LIVRAISON CRUD
    // ════════════════════════════════════════════════════════════════

    // ============================
    // GET ALL
    // ============================
    public async Task<List<BonLivraisonDto>> GetAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(BaseUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching bons livraison: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY ID
    // ============================
    public async Task<BonLivraisonDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BonLivraisonDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching bon livraison: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY NUMERO
    // ============================
    public async Task<BonLivraisonDto?> GetByNumeroAsync(string numeroBL)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/numero/{Uri.EscapeDataString(numeroBL)}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BonLivraisonDto>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching bon livraison by numero: {ex.Message}");
            return null;
        }
    }

    // ============================
    // GET BY CLIENT
    // ============================
    public async Task<List<BonLivraisonDto>> GetByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/client/{clientId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching bons livraison by client: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET NON FACTURES (⭐ For facturation)
    // ============================
    public async Task<List<BonLivraisonDto>> GetNonFacturesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/non-factures");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching non-factured bons livraison: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET NON FACTURES BY CLIENT (⭐ For facturation)
    // ============================
    public async Task<List<BonLivraisonDto>> GetNonFacturesByClientAsync(int clientId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/non-factures/client/{clientId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching non-factured bons livraison by client: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET BY DATE RANGE
    // ============================
    public async Task<List<BonLivraisonDto>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/date-range?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching bons livraison by date range: {ex.Message}");
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
                return JsonConvert.DeserializeObject<string>(json) ?? $"BL-{DateTime.Now.Year}-00001";
            }
            return $"BL-{DateTime.Now.Year}-00001";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching next numero: {ex.Message}");
            return $"BL-{DateTime.Now.Year}-00001";
        }
    }

    // ============================
    // CREATE
    // ============================
    public async Task<BonLivraisonDto?> CreateAsync(CreateBonLivraisonDto dto)
    {
        try
        {
            // Set audit fields
            dto.AjoutePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(BaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BonLivraisonDto>(responseJson);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error creating bon livraison: {error}");
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating bon livraison: {ex.Message}");
            return null;
        }
    }

    // ============================
    // UPDATE
    // ============================
    public async Task<bool> UpdateAsync(UpdateBonLivraisonDto dto
)
    {
        try
        {
            // Set audit fields
            dto.ModifiePar = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/{dto.ID}", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error updating bon livraison: {error}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating bon livraison: {ex.Message}");
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
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error deleting bon livraison: {error}");
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting bon livraison: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // BON LIVRAISON DETAILS
    // ════════════════════════════════════════════════════════════════

    // ============================
    // GET DETAILS BY BL
    // ============================
    public async Task<List<BonLivraisonDetailsDto>> GetDetailsByBLAsync(int bonLivraisonId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{DetailsBaseUrl}/bonlivraison/{bonLivraisonId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDetailsDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching bon livraison details: {ex.Message}");
            return [];
        }
    }

    // ============================
    // ADD DETAIL
    // ============================
    public async Task<BonLivraisonDetailsDto?> AddDetailAsync(BonLivraisonDetailsDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(DetailsBaseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BonLivraisonDetailsDto>(responseJson);
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding bon livraison detail: {ex.Message}");
            return null;
        }
    }

    // ============================
    // ADD DETAILS BATCH
    // ============================
    public async Task<List<BonLivraisonDetailsDto>> AddDetailsBatchAsync(List<BonLivraisonDetailsDto> dtos)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dtos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{DetailsBaseUrl}/batch", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDetailsDto>>(responseJson) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding bon livraison details batch: {ex.Message}");
            return [];
        }
    }

    // ============================
    // UPDATE DETAIL
    // ============================
    public async Task<bool> UpdateDetailAsync(BonLivraisonDetailsDto dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{DetailsBaseUrl}/{dto.ID}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating bon livraison detail: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE DETAIL
    // ============================
    public async Task<bool> DeleteDetailAsync(int detailId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{DetailsBaseUrl}/{detailId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting bon livraison detail: {ex.Message}");
            return false;
        }
    }

    // ============================
    // DELETE ALL DETAILS BY BL
    // ============================
    public async Task<bool> DeleteAllDetailsAsync(int bonLivraisonId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{DetailsBaseUrl}/bonlivraison/{bonLivraisonId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting all bon livraison details: {ex.Message}");
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CREATE BL FROM CREDIT TRANSACTIONS (⭐ NEW)
    // ════════════════════════════════════════════════════════════════
    public async Task<CTConversionResultDto> CreateFromCreditTransactionsAsync(CreateBLFromCTsDto request)
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
                    Message = "Erreur lors de la création du BL",
                    Errors = [responseJson]
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating BL from CTs: {ex.Message}");
            return new CTConversionResultDto
            {
                Success = false,
                Message = "Erreur de connexion",
                Errors = [ex.Message]
            };
        }
    }

    // ════════════════════════════════════════════════════════════════
    // MERGE BLs (⭐ NEW - Consolidate multiple BLs into one)
    // ════════════════════════════════════════════════════════════════
    public async Task<MergeBLsResultDto> MergeBLsAsync(MergeBLsDto request)
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
                return JsonConvert.DeserializeObject<MergeBLsResultDto>(responseJson)
                    ?? new MergeBLsResultDto { Success = false, Message = "Erreur de désérialisation" };
            }
            else
            {
                var errorResult = JsonConvert.DeserializeObject<MergeBLsResultDto>(responseJson);
                return errorResult ?? new MergeBLsResultDto
                {
                    Success = false,
                    Message = "Erreur lors de la fusion des BLs",
                    Errors = [responseJson]
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error merging BLs: {ex.Message}");
            return new MergeBLsResultDto
            {
                Success = false,
                Message = "Erreur de connexion",
                Errors = [ex.Message]
            };
        }
    }

    // ════════════════════════════════════════════════════════════════
    // FACTURATION (⭐ Core invoicing from BLs)
    // ════════════════════════════════════════════════════════════════

    // ============================
    // CREATE FACTURE FROM BLs (⭐⭐⭐)
    // ============================
    public async Task<FacturationResultDto> CreateFactureFromBLsAsync(CreateFactureFromBLsDto request)
    {
        try
        {
            request.CreatedByUserId = App.CurrentUser?.UserId ?? App.user?.UserId ?? 5;

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{FacturationBaseUrl}/from-bls", content);

            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<FacturationResultDto>(responseJson) 
                    ?? new FacturationResultDto { Success = false, Message = "Erreur de désérialisation" };
            }
            else
            {
                // Try to deserialize error response
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

    // ============================
    // GET NEXT NUMERO FACTURE
    // ============================
    public async Task<string> GetNextNumeroFactureAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FacturationBaseUrl}/next-numero-facture");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(json) ?? $"FAC-{DateTime.Now.Year}-00001";
            }
            return $"FAC-{DateTime.Now.Year}-00001";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching next numero facture: {ex.Message}");
            return $"FAC-{DateTime.Now.Year}-00001";
        }
    }

    // ============================
    // GET BLs BY FACTURE
    // ============================
    public async Task<List<BonLivraisonDto>> GetBLsByFactureAsync(int factureId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FacturationBaseUrl}/facture/{factureId}/bls");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching BLs by facture: {ex.Message}");
            return [];
        }
    }

    // ============================
    // GET FACTURE-BL LINKS
    // ============================
    public async Task<List<FactureBonLivraisonDto>> GetFactureBLLinksAsync(int factureId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{FacturationBaseUrl}/facture/{factureId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FactureBonLivraisonDto>>(json) ?? [];
            }
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching facture-BL links: {ex.Message}");
            return [];
        }
    }
}

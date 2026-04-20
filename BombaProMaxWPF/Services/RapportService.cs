using System.Diagnostics;
using BombaProMaxWPF.Models;
using Newtonsoft.Json;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Service for fetching report data from the API.
/// </summary>
public class RapportService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Rapports;

    public RapportService()
    {
        _httpClient = HttpClientFactory.Create();
    }

    /// <summary>
    /// Get sales report (Ventes Carburant + Lubrifiants/Articles).
    /// </summary>
    public async Task<RapportVentesDto> GetRapportVentesAsync(DateOnly? date = null, string? month = null)
    {
        try
        {
            var queryParams = BuildQueryParams(date, month);
            var url = $"{BaseUrl}/ventes{queryParams}";

            Debug.WriteLine($"[RapportService] Fetching ventes: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[RapportService] Ventes request failed: {response.StatusCode}");
                return new RapportVentesDto();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RapportVentesDto>(json);
            return result ?? new RapportVentesDto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RapportService] Error fetching ventes: {ex.Message}");
            return new RapportVentesDto();
        }
    }

    /// <summary>
    /// Get expenses report (Depenses by category).
    /// </summary>
    public async Task<RapportDepensesDto> GetRapportDepensesAsync(DateOnly? date = null, string? month = null)
    {
        try
        {
            var queryParams = BuildQueryParams(date, month);
            var url = $"{BaseUrl}/depenses{queryParams}";

            Debug.WriteLine($"[RapportService] Fetching depenses: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[RapportService] Depenses request failed: {response.StatusCode}");
                return new RapportDepensesDto();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RapportDepensesDto>(json);
            return result ?? new RapportDepensesDto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RapportService] Error fetching depenses: {ex.Message}");
            return new RapportDepensesDto();
        }
    }

    /// <summary>
    /// Get stock report (Reservoirs + Products + Achats movements).
    /// </summary>
    public async Task<RapportStockDto> GetRapportStockAsync(DateOnly? date = null, string? month = null)
    {
        try
        {
            var queryParams = BuildQueryParams(date, month);
            var url = $"{BaseUrl}/stock{queryParams}";

            Debug.WriteLine($"[RapportService] Fetching stock: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[RapportService] Stock request failed: {response.StatusCode}");
                return new RapportStockDto();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RapportStockDto>(json);
            return result ?? new RapportStockDto();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RapportService] Error fetching stock: {ex.Message}");
            return new RapportStockDto();
        }
    }

    /// <summary>
    /// Get all reports at once.
    /// </summary>
    public async Task<RapportCompletDto> GetRapportCompletAsync(DateOnly? date = null, string? month = null)
    {
        var result = new RapportCompletDto();

        // Fetch all reports in parallel
        var ventesTask = GetRapportVentesAsync(date, month);
        var depensesTask = GetRapportDepensesAsync(date, month);
        var stockTask = GetRapportStockAsync(date, month);

        await Task.WhenAll(ventesTask, depensesTask, stockTask);

        result.Ventes = await ventesTask;
        result.Depenses = await depensesTask;
        result.Stock = await stockTask;

        // Build period label
        if (date.HasValue)
        {
            result.PeriodeLabel = date.Value.ToString("dd/MM/yyyy");
        }
        else if (!string.IsNullOrEmpty(month))
        {
            var parts = month.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out var m))
            {
                var monthNames = new[] { "", "Janvier", "Fevrier", "Mars", "Avril", "Mai", "Juin",
                    "Juillet", "Aout", "Septembre", "Octobre", "Novembre", "Decembre" };
                result.PeriodeLabel = $"{monthNames[m]} {parts[0]}";
            }
        }
        else
        {
            result.PeriodeLabel = "Toutes les periodes";
        }

        return result;
    }

    /// <summary>
    /// Build query string from filter parameters.
    /// </summary>
    private static string BuildQueryParams(DateOnly? date, string? month)
    {
        var parts = new List<string>();

        if (date.HasValue)
        {
            parts.Add($"date={date.Value:yyyy-MM-dd}");
        }
        else if (!string.IsNullOrEmpty(month))
        {
            parts.Add($"month={month}");
        }

        return parts.Count > 0 ? "?" + string.Join("&", parts) : "";
    }

    /// <summary>
    /// Get Jaugeage analysis comparing jaugeages around the selected date vs actual sales.
    /// </summary>
    public async Task<RapportJaugeageAnalyseDto> GetRapportJaugeageAnalyseAsync(DateOnly? date = null, string? month = null)
    {
        try
        {
            var queryParams = BuildQueryParams(date, month);
            var url = $"{BaseUrl}/jaugeage-analyse{queryParams}";

            Debug.WriteLine($"[RapportService] Fetching jaugeage-analyse: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[RapportService] Jaugeage-analyse request failed: {response.StatusCode}");
                return new RapportJaugeageAnalyseDto { HasData = false, Message = "Erreur de chargement" };
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RapportJaugeageAnalyseDto>(json);
            return result ?? new RapportJaugeageAnalyseDto { HasData = false };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RapportService] Error fetching jaugeage-analyse: {ex.Message}");
            return new RapportJaugeageAnalyseDto { HasData = false, Message = ex.Message };
        }
    }
}

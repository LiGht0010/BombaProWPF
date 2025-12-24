using BombaProMax.Models.Dashboard;
using Newtonsoft.Json;

namespace BombaProMax.Services;

/// <summary>
/// Service for fetching dashboard analytics data.
/// Returns raw rows that can be grouped/aggregated client-side.
/// </summary>
public class DashboardService
{
    private readonly HttpClient _httpClient;
    private static string BaseUrl => ApiConfig.Dashboard;

    public DashboardService()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    /// <summary>
    /// Get Achat analytics rows with optional filters.
    /// </summary>
    /// <param name="startDate">Start date for range filter</param>
    /// <param name="endDate">End date for range filter</param>
    /// <param name="date">Specific date filter</param>
    /// <param name="year">Year filter</param>
    /// <param name="month">Month filter (yyyy-MM format)</param>
    public async Task<List<AchatAnalyticsRowDto>> GetAchatsAnalyticsAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        DateOnly? date = null,
        int? year = null,
        string? month = null)
    {
        try
        {
            var queryParams = BuildQueryString(startDate, endDate, date, year, month);
            var url = $"{BaseUrl}/achats{queryParams}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AchatAnalyticsRowDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching achats analytics: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Get Vente (Lubrifiants et Articles) analytics rows with optional filters.
    /// </summary>
    /// <param name="startDate">Start date for range filter</param>
    /// <param name="endDate">End date for range filter</param>
    /// <param name="date">Specific date filter</param>
    /// <param name="year">Year filter</param>
    /// <param name="month">Month filter (yyyy-MM format)</param>
    public async Task<List<VenteAnalyticsRowDto>> GetVentesAnalyticsAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        DateOnly? date = null,
        int? year = null,
        string? month = null)
    {
        try
        {
            var queryParams = BuildQueryString(startDate, endDate, date, year, month);
            var url = $"{BaseUrl}/ventes{queryParams}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<VenteAnalyticsRowDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching ventes analytics: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Get Vente Carburant (fuel sales from Periode/PeriodeDetails) analytics rows with optional filters.
    /// </summary>
    /// <param name="startDate">Start date for range filter</param>
    /// <param name="endDate">End date for range filter</param>
    /// <param name="date">Specific date filter</param>
    /// <param name="year">Year filter</param>
    /// <param name="month">Month filter (yyyy-MM format)</param>
    public async Task<List<VenteCarburantAnalyticsRowDto>> GetVentesCarburantAnalyticsAsync(
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        DateOnly? date = null,
        int? year = null,
        string? month = null)
    {
        try
        {
            var queryParams = BuildQueryString(startDate, endDate, date, year, month);
            var url = $"{BaseUrl}/ventes-carburant{queryParams}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<VenteCarburantAnalyticsRowDto>>(json) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching ventes carburant analytics: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Builds query string from filter parameters.
    /// </summary>
    private static string BuildQueryString(
        DateOnly? startDate,
        DateOnly? endDate,
        DateOnly? date,
        int? year,
        string? month)
    {
        var parameters = new List<string>();

        if (date.HasValue)
        {
            parameters.Add($"date={date.Value:yyyy-MM-dd}");
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            parameters.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            parameters.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        }
        else if (!string.IsNullOrEmpty(month))
        {
            parameters.Add($"month={month}");
        }
        else if (year.HasValue)
        {
            parameters.Add($"year={year.Value}");
        }

        return parameters.Count > 0 ? "?" + string.Join("&", parameters) : string.Empty;
    }
}

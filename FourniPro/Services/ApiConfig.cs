using System.Text.Json;

namespace FourniPro.Services;

/// <summary>
/// Centralized API configuration for FourniPro.
/// </summary>
public static class ApiConfig
{
    private static string _baseUrl = "http://localhost:5007/api";

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FourniPro",
        "settings.json");

    /// <summary>Gets or sets the API base URL.</summary>
    public static string BaseUrl
    {
        get => _baseUrl;
        set => _baseUrl = value?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Endpoint for user authentication.</summary>
    public static string Users => $"{_baseUrl}/Users";

    /// <summary>Endpoint for product management.</summary>
    public static string Produits => $"{_baseUrl}/Produits";

    /// <summary>Endpoint for fournisseur management.</summary>
    public static string Fournisseurs => $"{_baseUrl}/Fournisseurs";

    /// <summary>Endpoint for client management.</summary>
    public static string Clients => $"{_baseUrl}/Clients";

    /// <summary>Endpoint for chauffeur management.</summary>
    public static string Chauffeurs => $"{_baseUrl}/Chauffeurs";

    /// <summary>Endpoint for camion management.</summary>
    public static string Camions => $"{_baseUrl}/Camions";

    /// <summary>Endpoint for citerne management.</summary>
    public static string Citernes => $"{_baseUrl}/Citernes";

    /// <summary>Endpoint for achat management.</summary>
    public static string Achats => $"{_baseUrl}/Achats";

    /// <summary>Endpoint for vente management.</summary>
    public static string Ventes => $"{_baseUrl}/Ventes";

    /// <summary>Endpoint for employe management.</summary>
    public static string Employes => $"{_baseUrl}/Employes";

    /// <summary>Endpoint for credit management.</summary>
    public static string Credits => $"{_baseUrl}/Credits";

    /// <summary>Endpoint for voyage management.</summary>
    public static string Voyages => $"{_baseUrl}/Voyages";

    /// <summary>Endpoint for stock-voyage management.</summary>
    public static string StockVoyages => $"{_baseUrl}/StockVoyages";

    /// <summary>Endpoint for frais-voyage management.</summary>
    public static string FraisVoyages => $"{_baseUrl}/FraisVoyages";

    /// <summary>Initializes the API configuration from saved settings or defaults.</summary>
    public static void Initialize()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return;
            var json = File.ReadAllText(SettingsFilePath);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("ApiBaseUrl", out var el) &&
                el.GetString() is { Length: > 0 } saved)
            {
                _baseUrl = saved.TrimEnd('/');
            }
        }
        catch { /* use default */ }

        System.Diagnostics.Debug.WriteLine($"[ApiConfig] Initialized - BaseUrl: {_baseUrl}");
    }
}

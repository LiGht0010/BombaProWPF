using System.IO;
using System.Text.Json;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Centralized API configuration. BaseUrl can be configured per client installation.
/// Uses file-based settings storage (WPF-compatible replacement for MAUI Preferences).
/// </summary>
public static class ApiConfig
{
    // ===========================================
    // CHANGE THIS URL TO SWITCH ENVIRONMENTS
    // ===========================================

    // Production (Debian Server)
    //private static string _baseUrl = "http://62.84.189.17:5002/api";

    // Development (Local) - Matches Kestrel config in appsettings.json
    private static string _baseUrl = "http://localhost:5006/api";

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BombaProMaxWPF",
        "settings.json");

    /// <summary>
    /// Gets or sets the API base URL. Set this at app startup based on client configuration.
    /// </summary>
    public static string BaseUrl
    {
        get => _baseUrl;
        set => _baseUrl = value?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Initializes the API configuration from saved settings or defaults.
    /// Call this in App.xaml.cs at startup.
    /// </summary>
    public static void Initialize()
    {
        var savedUrl = GetSetting("ApiBaseUrl");
        if (!string.IsNullOrWhiteSpace(savedUrl))
        {
            _baseUrl = savedUrl.TrimEnd('/');
        }

        System.Diagnostics.Debug.WriteLine($"[ApiConfig] Initialized - BaseUrl: {_baseUrl}");
    }

    /// <summary>
    /// Updates the API base URL and saves it to settings.
    /// </summary>
    /// <param name="newBaseUrl">The new API base URL</param>
    public static void SetAndSaveBaseUrl(string newBaseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newBaseUrl);
        _baseUrl = newBaseUrl.TrimEnd('/');
        SaveSetting("ApiBaseUrl", _baseUrl);
    }

    private static string? GetSetting(string key)
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
                return null;

            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return settings?.GetValueOrDefault(key);
        }
        catch
        {
            return null;
        }
    }

    private static void SaveSetting(string key, string value)
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsFilePath)!;
            Directory.CreateDirectory(dir);

            Dictionary<string, string> settings = [];
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            }

            settings[key] = value;
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiConfig] Error saving setting: {ex.Message}");
        }
    }

    // ===========================================
    // API Endpoints
    // ===========================================
    public static string Users => $"{BaseUrl}/Users";
    public static string Clients => $"{BaseUrl}/Clients";
    public static string Produits => $"{BaseUrl}/Produits";
    public static string Categories => $"{BaseUrl}/Categories";
    public static string Fournisseurs => $"{BaseUrl}/Fournisseurs";
    public static string Achats => $"{BaseUrl}/Achats";
    public static string AchatAllocations => $"{BaseUrl}/AchatAllocations";
    public static string Factures => $"{BaseUrl}/Factures";
    public static string BonLivraisons => $"{BaseUrl}/BonLivraisons";
    public static string Employes => $"{BaseUrl}/Employes";
    public static string Chauffeurs => $"{BaseUrl}/Chauffeurs";
    public static string Camions => $"{BaseUrl}/Camions";
    public static string Citernes => $"{BaseUrl}/Citernes";
    public static string Pompes => $"{BaseUrl}/Pompes";
    public static string Reservoirs => $"{BaseUrl}/Reservoirs";
    public static string ReservoirCalibrations => $"{BaseUrl}/ReservoirCalibrations";
    public static string Jaugeages => $"{BaseUrl}/Jaugeages";
    public static string JaugeageDetails => $"{BaseUrl}/JaugeageDetails";
    public static string Depenses => $"{BaseUrl}/Depenses";
    public static string DepenseCategories => $"{BaseUrl}/DepenseCategories";
    public static string MoyensPaiement => $"{BaseUrl}/MoyensPaiements";
    public static string BilanCredits => $"{BaseUrl}/BilanCredits";
    public static string CreditTransactions => $"{BaseUrl}/CreditTransactions";
    public static string ReglementCredits => $"{BaseUrl}/ReglementCredits";
    public static string Periodes => $"{BaseUrl}/Periodes";
    public static string Services => $"{BaseUrl}/Services";
    public static string ServiceCategories => $"{BaseUrl}/ServiceCategories";
    public static string VenteLubrifiantsEtArticles => $"{BaseUrl}/VenteLubrifiantsEtArticles";
    public static string VenteServices => $"{BaseUrl}/VenteServices";
    public static string Dashboard => $"{BaseUrl}/Dashboard";
    public static string Rapports => $"{BaseUrl}/Rapports";
    public static string Home => $"{BaseUrl}";
    public static string DepotCaisses => $"{BaseUrl}/DepotCaisses";
    public static string StockLots => $"{BaseUrl}/StockLots";
    public static string StockCalibration => $"{BaseUrl}/StockCalibration";
}

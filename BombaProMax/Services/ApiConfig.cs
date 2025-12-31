namespace BombaProMax.Services;

/// <summary>
/// Centralized API configuration. BaseUrl and TenantId can be configured per client installation.
/// </summary>
public static class ApiConfig
{
    // ===========================================
    // CONFIGURATION (can be overridden at runtime)
    // ===========================================


    // ===========================================
    // CHANGE THIS URL TO SWITCH ENVIRONMENTS
    // ===========================================

    // Production (Debian Server)
    //private static string _baseUrl = "http://62.84.189.17:5000/api";

    // Development (Local)
    private static string _baseUrl = "https://localhost:7100/api";
    private static string _tenantId = "sidikacem";

    /// <summary>
    /// HTTP header name used to identify the tenant. Must match the API.
    /// </summary>
    public const string TenantHeaderName = "X-Tenant-ID";

    /// <summary>
    /// Gets or sets the API base URL. Set this at app startup based on client configuration.
    /// </summary>
    public static string BaseUrl
    {
        get => _baseUrl;
        set => _baseUrl = value?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the Tenant ID for multi-tenant API. Each client has their own TenantId.
    /// </summary>
    public static string TenantId
    {
        get => _tenantId;
        set => _tenantId = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets the friendly display name for the current tenant.
    /// </summary>
    public static string TenantDisplayName => _tenantId switch
    {
        "sidikacem" => "Sidi Kacem",
        "qserkber" => "Qser Kber",
        "sidiaddi" => "Sidi Addi",
        _ => _tenantId
    };

    /// <summary>
    /// Gets the database name for the current tenant.
    /// </summary>
    public static string TenantDatabaseName => _tenantId switch
    {
        "sidikacem" => "SidiKacemDB",
        "qserkber" => "QserKberDB",
        "sidiaddi" => "SidiAddiDB",
        _ => _tenantId
    };

    /// <summary>
    /// Gets the full display string with tenant name and database.
    /// </summary>
    public static string TenantFullDisplayName => $"{TenantDisplayName} ({TenantDatabaseName})";

    /// <summary>
    /// Initializes the API configuration from saved preferences or defaults.
    /// Call this in MauiProgram.cs or App.xaml.cs at startup.
    /// </summary>
    public static void Initialize()
    {
        var savedUrl = Preferences.Get("ApiBaseUrl", _baseUrl);
        if (!string.IsNullOrWhiteSpace(savedUrl))
        {
            _baseUrl = savedUrl.TrimEnd('/');
        }

        var savedTenantId = Preferences.Get("TenantId", _tenantId);
        if (!string.IsNullOrWhiteSpace(savedTenantId))
        {
            _tenantId = savedTenantId;
        }
    }

    /// <summary>
    /// Updates the API base URL and saves it to preferences.
    /// </summary>
    /// <param name="newBaseUrl">The new API base URL</param>
    public static void SetAndSaveBaseUrl(string newBaseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newBaseUrl);
        _baseUrl = newBaseUrl.TrimEnd('/');
        Preferences.Set("ApiBaseUrl", _baseUrl);
    }

    /// <summary>
    /// Updates the Tenant ID and saves it to preferences.
    /// </summary>
    /// <param name="newTenantId">The new tenant ID (e.g., "client1", "sidikassem")</param>
    public static void SetAndSaveTenantId(string newTenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newTenantId);
        _tenantId = newTenantId;
        Preferences.Set("TenantId", _tenantId);
    }

    /// <summary>
    /// Clears the tenant configuration. Call this when logging out.
    /// </summary>
    public static void ClearTenant()
    {
        _tenantId = string.Empty;
        Preferences.Remove("TenantId");
    }

    /// <summary>
    /// Configures an HttpClient with the tenant header.
    /// Call this on every HttpClient before making API requests.
    /// </summary>
    /// <param name="client">The HttpClient to configure</param>
    public static void ConfigureHttpClient(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        
        // Remove existing header if present, then add the current tenant
        client.DefaultRequestHeaders.Remove(TenantHeaderName);
        client.DefaultRequestHeaders.Add(TenantHeaderName, _tenantId);
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
}

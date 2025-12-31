namespace BombaProMaxApi.MultiTenancy;

/// <summary>
/// Configuration for a single tenant (client).
/// </summary>
public class TenantSettings
{
    /// <summary>
    /// Unique identifier for the tenant (e.g., "client1", "sidikassem", etc.)
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Database connection string for this tenant.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Root configuration object for multi-tenancy settings.
/// </summary>
public class MultiTenantSettings
{
    public const string SectionName = "Tenants";

    /// <summary>
    /// Default tenant ID to use if none is specified.
    /// </summary>
    public string DefaultTenant { get; set; } = string.Empty;

    /// <summary>
    /// List of all configured tenants.
    /// </summary>
    public List<TenantSettings> Tenants { get; set; } = [];
}
